using System;
using System.Collections.Generic;

#if __SHARPDX__
using SharpDX;
using Matrix4 = SharpDX.Matrix;
#else
using OpenTK;
#endif

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

		private static string simpleVertexShaderDirectX = @"

cbuffer b0 : register (b0) {
    float4x4 mvp;
}

struct vs_out {
    float4 position : SV_POSITION;
    float2 uvout : TEXCOORD;
    float4 vertex_color : COLOR;
};

vs_out main(float2 position : VERTEX, float2 uv : UV, float4 vc : VC)
{
    vs_out o;
    o.position = mul(float4(position.x, position.y, 0.0, 1.0), mvp);
    o.uvout = uv;
    o.vertex_color = vc;
	return o;
}";
		private static string simpleFragmentShaderDirectX = @"
cbuffer b1 : register (b1) {
    float use_texture;
}

cbuffer b2 : register (b2) {
    float use_wireframe;
}

cbuffer b3 : register (b3) {
    float4 color;
}

struct vs_out {
    float4 position : SV_POSITION;
    float2 uvout : TEXCOORD;
    float4 vertex_color : COLOR;
};

float4 main(vs_out i) : SV_TARGET
{
	return i.vertex_color + color;
}";

#if __SHARPDX__
        private static Shader simpleShader = new Shader(simpleVertexShaderDirectX, simpleFragmentShaderDirectX, null, null, new string[] { "VERTEX", "UV", "VC" }, new int[] { 2, 2, 4 }, new string[] { "mvp" }, new string[] { "use_texture", "use_wireframe", "color", "tex" });
#else
		private static Shader simpleShader = new Shader(simpleVertexShader, simpleFragmentShader, simpleVertexShaderObsolete, simpleFragmentShaderObsolete, new string[] { "vertex", "uv", "vc" });
#endif



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
#if __SHARPDX__
            Matrix4 m = Matrix4.Translation(-this.pivot.X, -this.pivot.Y, 0) *
                Matrix4.Scaling(this.scale.X, this.scale.Y, 1) *
                Matrix4.RotationZ(this.rotation) *
                Matrix4.Translation(this.position.X, this.position.Y, 0);
#else
			// WARNING !!! OpenTK uses row-major while OpenGL uses column-major
			Matrix4 m =
				Matrix4.CreateTranslation(-this.pivot.X, -this.pivot.Y, 0) *
#if !__MOBILE__
				Matrix4.CreateScale(this.scale.X, this.scale.Y, 1) *
#else
                Matrix4.Scale(this.scale.X, this.scale.Y, 1) *
#endif
				Matrix4.CreateRotationZ(this.rotation) *
				// here we do not re-add the pivot, so translation is pivot based too
				Matrix4.CreateTranslation(this.position.X, this.position.Y, 0);
#endif

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
#if __SHARPDX__
            // transpose the matrix for DirectX
            mvp.Transpose();
#endif

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


		// simply set the 'color' uniform of the shader
		public virtual void DrawColor(float r, float g, float b, float a = 1)
		{
			this.Bind();
			this.shader.SetUniform("color", new Vector4(r, g, b, a));
			this.Draw();
			// always reset the color
			this.shader.SetUniform("color", Vector4.Zero);
		}

		public virtual void DrawColor(int r, int g, int b, int a = 255)
		{
			DrawColor(r / 255f, g / 255f, b / 255f, a / 255f);
		}

		public virtual void DrawColor(Vector4 color)
		{
			DrawColor(color.X, color.Y, color.Z, color.W);
		}

		public virtual void DrawWireframe(float r, float g, float b, float a = 1, float tickness = 0.02f)
		{
			if (this.v == null)
				return;

			// check if vcs neet to be temporarily stored
			// and check if the shader supports them
			if (!this.hasVertexColors)
				return;

			this.Bind();

			// store original vcs
			float[] vcs_storage = this.vc;

			int numVcs = (this.v.Length / numberOfAxis) * 4;
			this.vc = new float[numVcs];
			for (int i = 0; i < numVcs; i += 12)
			{
				this.vc[i] = 1f;
				this.vc[i + 1] = 0f;
				this.vc[i + 2] = 0f;

				this.vc[i + 4] = 0f;
				this.vc[i + 5] = 1f;
				this.vc[i + 6] = 0f;

				this.vc[i + 8] = 0f;
				this.vc[i + 9] = 0f;
				this.vc[i + 10] = 1f;
			}
			this.UpdateVertexColor();

			this.shader.SetUniform("color", new Vector4(r, g, b, a));
			this.shader.SetUniform("use_wireframe", tickness);
			this.Draw();
			this.shader.SetUniform("use_wireframe", -1f);
			// always reset the color
			this.shader.SetUniform("color", Vector4.Zero);
			// reset old vcs (could be null)
			this.vc = vcs_storage;
		}

		public virtual void DrawWireframe(int r, int g, int b, int a = 255, float tickness = 0.02f)
		{
			DrawWireframe(r / 255f, g / 255f, b / 255f, a / 255f, tickness);
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

		public void Dispose()
		{
			if (disposed)
				return;

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

