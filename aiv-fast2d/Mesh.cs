using System;
using System.Collections.Generic;
using OpenTK;

namespace Aiv.Fast2D
{
	public class Mesh : IDisposable
	{

		private int vertexArrayId;

		public float[] v;
		public float[] uv;
		public float[] vc;

		private int vBufferId;
		private int uvBufferId;
		private int vcBufferId;

		private struct VertexAttrib
		{
			public int bufferId;
			public int elementSize;

			public VertexAttrib(int bufferId, int elementSize)
			{
				this.bufferId = bufferId;
				this.elementSize = elementSize;
			}
		}

		private Dictionary<int, VertexAttrib> customBuffers;

		private bool disposed;

		public bool hasVertexColors;



		public Shader shader;

		public Vector2 position = Vector2.Zero;
		public Vector2 scale = Vector2.One;

		public Vector2 pivot = Vector2.Zero;

		public Camera Camera;

		private float rotation;

		public float Rotation
		{
			get
			{
				return this.rotation;
			}
			set
			{
				this.rotation = value;
			}
		}

		public float EulerRotation
		{
			get
			{
				return this.rotation * 180f / (float)Math.PI;
			}
			set
			{
				this.rotation = value * (float)Math.PI / 180f;
			}
		}

		// this is called to set uniform in shaders
		public delegate void ShaderSetupHook(Mesh mesh);
		protected ShaderSetupHook shaderSetupHook;

		protected int numberOfAxis;

		public bool noMatrix;

		protected bool requireUseTexture;

		protected int instances;
		public int Instances
		{
			get
			{
				return instances;
			}
		}


		private static string simpleVertexShader = @"
#version 330 core

layout(location = 0) in vec2 vertex;
layout(location = 1) in vec2 uv;
layout(location = 2) in vec4 vc;

uniform mat4 mvp;
out vec2 uvout;
out vec4 vertex_color;

void main(){
        gl_Position = mvp * vec4(vertex.xy, 0.0, 1.0);
        uvout = uv;
        vertex_color = vc;
}";
		private static string simpleFragmentShader = @"
#version 330 core

precision highp float;

uniform vec4 color;
uniform float use_texture;
uniform float use_wireframe;
uniform sampler2D tex;

in vec2 uvout;
in vec4 vertex_color;

out vec4 out_color;

void main(){
    if (use_texture > 0.0) {
		out_color = texture(tex, uvout);
        out_color += vec4(vertex_color.xyz * out_color.a, vertex_color.a);
    }
    else if (use_wireframe > 0.0) {
        if(any(lessThan(vertex_color.xyz, vec3(use_wireframe)))) {
            out_color = color;
        }
        else {
            out_color = vec4(0, 0, 0, 0);    
        }
        return;
    }
    else {
        out_color = vertex_color;
    }
    out_color += color;
}";

		private static string simpleVertexShaderObsolete = @"
attribute vec2 vertex;
attribute vec2 uv;
attribute vec4 vc;

uniform mat4 mvp;
varying vec2 uvout;
varying vec4 vertex_color;

void main(){
        gl_Position = mvp * vec4(vertex.xy, 0.0, 1.0);
        uvout = uv;
        vertex_color = vc;
}";
		private static string simpleFragmentShaderObsolete = @"
precision mediump float;

uniform vec4 color;
uniform float use_texture;
uniform float use_wireframe;
uniform sampler2D tex;

varying vec2 uvout;
varying vec4 vertex_color;


void main(){
    if (use_texture > 0.0) {
        gl_FragColor = texture2D(tex, uvout);
        gl_FragColor += vec4(vertex_color.xyz * gl_FragColor.a, vertex_color.a);
    }
    else if (use_wireframe > 0.0) {
        if(any(lessThan(vertex_color.xyz, vec3(use_wireframe)))) {
            gl_FragColor = color;
        }
        else {
            gl_FragColor = vec4(0, 0, 0, 0);    
        }
        return;
    }
    else {
        gl_FragColor = vertex_color;
    }
    gl_FragColor += color;
}";
		internal static Shader simpleShader = new Shader(simpleVertexShader, simpleFragmentShader, simpleVertexShaderObsolete, simpleFragmentShaderObsolete, new string[] { "vertex", "uv", "vc" });



		public Mesh(Shader shader = null, int numberOfAxis = 2)
		{

			this.customBuffers = new Dictionary<int, VertexAttrib>();
			this.numberOfAxis = numberOfAxis;

			// use VAO if possible
			this.vertexArrayId = Graphics.NewArray();
			if (this.vertexArrayId > -1)
			{
				this.Bind();
			}

			// vertex
			this.vBufferId = Graphics.NewBuffer();
			Graphics.MapBufferToArray(this.vBufferId, 0, numberOfAxis);

			// uv
			this.uvBufferId = Graphics.NewBuffer();
			Graphics.MapBufferToArray(this.uvBufferId, 1, 2);

			// vc
			this.vcBufferId = Graphics.NewBuffer();
			Graphics.MapBufferToArray(this.vcBufferId, 2, 4);

			if (shader == null)
			{
				shader = simpleShader;
				shader.SetUniform("tex", 0);
			}

			this.shader = shader;

			this.noMatrix = false;
			this.hasVertexColors = true;
			this.requireUseTexture = true;
		}

		protected int NewFloatBuffer(int attribArrayId, int elementSize, float[] data, int divisor = 0)
		{
			this.Bind();

			int bufferId = Graphics.NewBuffer();
			Graphics.MapBufferToArray(bufferId, attribArrayId, elementSize);

			if (divisor > 0)
			{
				Graphics.SetArrayDivisor(attribArrayId, divisor);
			}

			Graphics.BufferData(data);
			this.customBuffers[attribArrayId] = new VertexAttrib(bufferId, elementSize);
			return bufferId;
		}

		protected void UpdateFloatBuffer(int bufferId, float[] data, int offset = 0)
		{
			Graphics.BufferSubData(bufferId, data, offset);
		}

		public void Update()
		{
			this.UpdateVertex();
			this.UpdateUV();
			this.UpdateVertexColor();
		}

		public void UpdateVertex()
		{
			if (this.v == null)
				return;

			Graphics.BufferData(this.vBufferId, this.v);
		}

		public void UpdateUV()
		{
			if (this.uv == null)
				return;

			Graphics.BufferData(this.uvBufferId, this.uv);
		}

		public void UpdateVertexColor()
		{
			if (this.vc == null)
				return;

			Graphics.BufferData(this.vcBufferId, this.vc);
		}

		public void Bind()
		{
			if (this.vertexArrayId > -1)
			{
				Graphics.BindArray(this.vertexArrayId);
			}
			else
			{
				// vertex
				Graphics.MapBufferToArray(this.vBufferId, 0, numberOfAxis);
				// uv
				Graphics.MapBufferToArray(this.uvBufferId, 1, 2);
				// vc
				Graphics.MapBufferToArray(this.vcBufferId, 2, 4);

				// custom buffers
				foreach (int customAttribId in this.customBuffers.Keys)
				{
					int customBufferId = this.customBuffers[customAttribId].bufferId;
					int customBufferElementSize = this.customBuffers[customAttribId].elementSize;
					Graphics.MapBufferToArray(customBufferId, customAttribId, customBufferElementSize);
				}
			}
		}

		// here we update translations, scaling and rotations
		protected virtual void ApplyMatrix()
		{
			if (this.noMatrix)
				return;

			// WARNING !!! OpenTK uses row-major while OpenGL uses column-major
			Matrix4 m =
				Matrix4.CreateTranslation(-this.pivot.X, -this.pivot.Y, 0) *

				Matrix4.CreateScale(this.scale.X, this.scale.Y, 1) *

				Matrix4.CreateRotationZ(this.rotation) *
				// here we do not re-add the pivot, so translation is pivot based too
				Matrix4.CreateTranslation(this.position.X, this.position.Y, 0);


			Matrix4 projectionMatrix = Window.Current.ProjectionMatrix;

			if (this.Camera != null)
			{
				m *= this.Camera.Matrix();
				if (this.Camera.HasProjection)
				{
					projectionMatrix = this.Camera.ProjectionMatrix();
				}
			}
			else if (Window.Current.CurrentCamera != null)
			{
				m *= Window.Current.CurrentCamera.Matrix();
				if (Window.Current.CurrentCamera.HasProjection)
				{
					projectionMatrix = Window.Current.CurrentCamera.ProjectionMatrix();
				}
			}

			Matrix4 mvp = m * projectionMatrix;

			// pass the matrix to the shader
			this.shader.SetUniform("mvp", mvp);
		}

		public virtual void DrawTexture(Texture tex)
		{
			if (this.v == null || this.uv == null)
				return;
			this.Bind();
			// upload fake vcs (if required) to avoid crashes
			if (this.vc == null && this.hasVertexColors)
			{
				this.vc = new float[(this.v.Length / numberOfAxis) * 4];
				this.UpdateVertexColor();
			}
			tex.Bind();
			if (this.requireUseTexture)
				this.shader.SetUniform("use_texture", 1f);
			this.shader.Use();
			if (this.shaderSetupHook != null)
			{
				this.shaderSetupHook(this);
			}
			this.ApplyMatrix();
			if (instances <= 1)
			{
				Graphics.DrawArrays(this.v.Length / numberOfAxis);
			}
			else
			{
				Graphics.DrawArraysInstanced(this.v.Length / numberOfAxis, instances);
			}
		}

		// fast version of drawtexture without UV re-upload
		public virtual void DrawTexture(int textureId)
		{
			if (this.v == null || this.uv == null)
				return;
			this.Bind();
			// upload fake vcs (if required) to avoid crashes
			if (this.vc == null && this.hasVertexColors)
			{
				this.vc = new float[(this.v.Length / numberOfAxis) * 4];
				this.UpdateVertexColor();
			}
			Graphics.BindTextureToUnit(textureId, 0);
			if (this.requireUseTexture)
				this.shader.SetUniform("use_texture", 1f);
			this.shader.Use();
			if (this.shaderSetupHook != null)
			{
				this.shaderSetupHook(this);
			}
			this.ApplyMatrix();
			if (instances <= 1)
			{
				Graphics.DrawArrays(this.v.Length / numberOfAxis);
			}
			else
			{
				Graphics.DrawArraysInstanced(this.v.Length / numberOfAxis, instances);
			}
		}


		/// <summary>
		/// Draw the sprite filling it with this color
		/// </summary>
		/// <param name="r">red channel in space [0.0, 1.0]</param>
		/// <param name="g">green channel in space [0.0, 1.0]</param>
		/// <param name="b">blue channelin space [0.0, 1.0]</param>
		/// <param name="a">alpha channel in space [0.0, 1.0]</param>
		public virtual void DrawColor(float r, float g, float b, float a = 1)
		{
			DrawColor(new Vector4(r, g, b, a));
		}

		/// <summary>
		/// Draw the sprite filling it with this color
		/// </summary>
		/// <param name="r">red channel in space [0, 255]</param>
		/// <param name="g">green channel in space [0, 255]</param>
		/// <param name="b">blue channelin space [0, 255]</param>
		/// <param name="a">alpha channel in space [0, 255]</param>
		public void DrawColor(int r, int g, int b, int a = 255)
		{
			DrawColor(r / 255f, g / 255f, b / 255f, a / 255f);
		}

		/// <summary>
		/// Draw the sprite filling it with this color
		/// </summary>
		/// <param name="color">color channel as vector of 4 float</param>
		public virtual void DrawColor(Vector4 color)
		{
			this.Bind();
			this.shader.SetUniform("color", color);
			this.Draw();
			// always reset the color
			this.shader.SetUniform("color", Vector4.Zero);
		}

		private float[] wireframceCache;
		public void DrawWireframe(float r, float g, float b, float a = 1, float tickness = 0.02f)
		{
			DrawWireframe(new Vector4(r, g, b, a), tickness);
		}

		public void DrawWireframe(int r, int g, int b, int a = 255, float tickness = 0.02f)
		{
			DrawWireframe(r / 255f, g / 255f, b / 255f, a / 255f, tickness);
		}

		public virtual void DrawWireframe(Vector4 color, float tickness = 0.02f)
		{
			if (this.v == null)
				return;

			// check if vcs neet to be temporarily stored
			// and check if the shader supports them
			if (!this.hasVertexColors)
				return;

			this.Bind();

			if (wireframceCache == null)
			{

				int numVcs = (this.v.Length / numberOfAxis) * 4;
				wireframceCache = new float[numVcs];
				for (int i = 0; i < numVcs; i += 12)
				{
					wireframceCache[i] = 1f;
					wireframceCache[i + 1] = 0f;
					wireframceCache[i + 2] = 0f;

					wireframceCache[i + 4] = 0f;
					wireframceCache[i + 5] = 1f;
					wireframceCache[i + 6] = 0f;

					wireframceCache[i + 8] = 0f;
					wireframceCache[i + 9] = 0f;
					wireframceCache[i + 10] = 1f;
				}
			}

			if (this.vc != wireframceCache)
			{
				this.vc = wireframceCache;
				this.UpdateVertexColor();
			}

			this.shader.SetUniform("color", color);
			this.shader.SetUniform("use_wireframe", tickness);
			this.Draw();
			this.shader.SetUniform("use_wireframe", -1f);
			// always reset the color
			this.shader.SetUniform("color", Vector4.Zero);
		}

		// simple draw without textures (useful for subclasses)
		public void Draw(ShaderSetupHook hook = null)
		{
			if (this.v == null)
				return;
			this.Bind();
			// upload fake uvs (if required) to avoid crashes
			if (this.uv == null)
			{
				this.uv = new float[this.v.Length];
				this.UpdateUV();
			}
			// upload fake vcs (if required) to avoid crashes
			if (this.vc == null && this.hasVertexColors)
			{
				this.vc = new float[(this.v.Length / numberOfAxis) * 4];
				this.UpdateVertexColor();
			}
			// clear current texture
			Graphics.BindTextureToUnit(0, 0);
			if (this.requireUseTexture)
				this.shader.SetUniform("use_texture", -1f);

			this.shader.Use();
			if (hook != null)
			{
				hook(this);
			}
			else if (this.shaderSetupHook != null)
			{
				this.shaderSetupHook(this);
			}

			this.ApplyMatrix();
			if (instances <= 1)
			{
				Graphics.DrawArrays(this.v.Length / numberOfAxis);
			}
			else
			{
				Graphics.DrawArraysInstanced(this.v.Length / numberOfAxis, instances);
			}
		}



		private Texture workaroundForRenderTexture;
		/// <summary>
		/// Allow to draw a <see cref="RenderTexture"/> object.
		/// </summary>
		/// <param name="rt">the render texture to draw</param>
		public void DrawRenderTexture(RenderTexture rt)
		{
			/* This is a WORKAROUND to address the issue:
			 * https://github.com/aiv01/aiv-fast2d/issues/53
			 * 
			 * It's a workaround because the only way found to avoid the "artifacts" problem
			 * is to download from GPU the RenderTexture data and then utilize a temporary
			 * Texture to upload data and draw them properly.
			 * During this data migration, data need to be flipped, otherwise will be shown upside down
			 * 
			 * In order to avoid to create multiple temporary texture during each Draw pass,
			 * we make sure to create just one.
			 */
			if (workaroundForRenderTexture == null)
			{
				workaroundForRenderTexture = new Texture(rt.Width, rt.Height);
				workaroundForRenderTexture.flipped = true;
			}

			byte[] data = rt.Download();
			workaroundForRenderTexture.Update(data);

			DrawTexture(workaroundForRenderTexture);
		}

		public void Dispose()
		{
			if (disposed)
				return;

			if (workaroundForRenderTexture != null)
				workaroundForRenderTexture.Dispose();

			Graphics.DeleteBuffer(this.vBufferId);
			Graphics.DeleteBuffer(this.uvBufferId);
			Graphics.DeleteBuffer(this.vcBufferId);
			if (this.vertexArrayId > -1)
				Graphics.DeleteArray(this.vertexArrayId);

			Window.Current.Log(string.Format("buffer {0} deleted", this.vBufferId));
			Window.Current.Log(string.Format("buffer {0} deleted", this.uvBufferId));
			Window.Current.Log(string.Format("buffer {0} deleted", this.vcBufferId));

			foreach (VertexAttrib customAttrib in this.customBuffers.Values)
			{

				Graphics.DeleteBuffer(customAttrib.bufferId);

				Window.Current.Log(string.Format("buffer {0} deleted", customAttrib.bufferId));
			}

			Window.Current.Log(string.Format("vertexArray {0} deleted", this.vertexArrayId));
			disposed = true;
		}

		~Mesh()
		{
			if (disposed)
				return;
			Window.Current.bufferGC.Add(this.vBufferId);
			Window.Current.bufferGC.Add(this.uvBufferId);
			Window.Current.bufferGC.Add(this.vcBufferId);
			foreach (VertexAttrib customAttrib in this.customBuffers.Values)
			{
				Window.Current.bufferGC.Add(customAttrib.bufferId);
			}
			Window.Current.vaoGC.Add(this.vertexArrayId);

			disposed = true;
		}

	}
}

