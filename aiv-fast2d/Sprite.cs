using System;
using OpenTK;

namespace Aiv.Fast2D
{
	public class Sprite : Mesh
	{

		private static string spriteVertexShader = @"
#version 330 core

layout(location = 0) in vec2 vertex;
layout(location = 1) in vec2 uv;

uniform mat4 mvp;

out vec2 uvout;

void main(){
        gl_Position = mvp * vec4(vertex.xy, 0.0, 1.0);
        uvout = uv;
}";
		private static string spriteFragmentShader = @"
#version 330 core
precision highp float;

uniform float use_texture;
uniform sampler2D tex;
uniform vec4 color;
uniform vec4 mul_tint;
uniform vec4 add_tint;

in vec2 uvout;
out vec4 frag_color;

void main(){
        if (use_texture > 0.0) {
            frag_color = texture(tex, uvout);
        }
        else {
			frag_color = color;
        }
		frag_color *= mul_tint;
        frag_color += vec4(add_tint.xyz * frag_color.a, add_tint.a);
}";

		private static string spriteVertexShaderObsolete = @"
attribute vec2 vertex;
attribute vec2 uv;
uniform mat4 mvp;

varying vec2 uvout;

void main(){
        gl_Position = mvp * vec4(vertex.xy, 0.0, 1.0);
        uvout = uv;
}";
		private static string spriteFragmentShaderObsolete = @"
precision mediump float;

varying vec2 uvout;

uniform vec4 mul_tint;
uniform vec4 add_tint;
uniform sampler2D tex;
uniform float use_texture;

void main(){
        if (use_texture > 0.0) {
            gl_FragColor = texture2D(tex, uvout) * mul_tint;
            gl_FragColor += vec4(add_tint.xyz * gl_FragColor.a, add_tint.a);
        }
        else {
            gl_FragColor = add_tint;
        }
}";

		private static Shader spriteShader = new Shader(spriteVertexShader, spriteFragmentShader, spriteVertexShaderObsolete, spriteFragmentShaderObsolete, new string[] { "vertex", "uv" });

		/// <summary>
		/// Get the Width of this sprite
		/// </summary>
		public float Width { get; }
		/// <summary>
		/// Get the Height of this sprite
		/// </summary>
		public float Height { get; }

		/// <summary>
		/// Get / Set the orizontal orientation of this Sprite
		/// </summary>
		public bool FlipX { get; set; }
		/// <summary>
		/// Get / Set the verical orientation of this Sprite
		/// </summary>
		public bool FlipY { get; set; }


		private Vector4 multiplyTint;
		private Vector4 additiveTint;

		private void SetArrayData(float[] data, float left, float right, float top, float bottom)
		{
			data[0] = left; data[1] = top;      //top-left
			data[2] = left; data[3] = bottom;   //bottom-left
			data[4] = right; data[5] = top;      //top-right
			data[6] = right; data[7] = top;      //top-right
			data[8] = left; data[9] = bottom;   //bottom-left
			data[10] = right; data[11] = bottom;  //bottom-right
		}

		/// <summary>
		/// Sprite class is a Quad (mesh made by two triangles)
		/// </summary>
		/// <param name="width">the width of the sprite</param>
		/// <param name="height">the height of the sprite</param>
		public Sprite(float width, float height) : base(spriteShader)
		{
			this.hasVertexColors = false;
			this.Width = width;
			this.Height = height;
			this.v = new float[12];
			this.uv = new float[12];
			//Vertices and Uvs default set to Pixel Coordinate (0,0 is top-left)  
			SetArrayData(this.v, 0, width, 0, height);
			SetArrayData(uv, 0, 1, 0, 1);
			base.Update();

			multiplyTint = Vector4.One;
			additiveTint = Vector4.Zero;

			//By default Sprite is configured for DrawTexture
			base.shaderSetupHook = (mesh) =>
			{
				mesh.shader.SetUniform("tex", 0);
				mesh.shader.SetUniform("use_texture", 1f);
				mesh.shader.SetUniform("mul_tint", multiplyTint);
				mesh.shader.SetUniform("add_tint", additiveTint);
			};
		}

		/// <summary>
		/// Color tint added during Draw phase, after base color (or texture) and multiply tint.
		/// </summary>
		/// <param name="r">red channel in space [0, 255]</param>
		/// <param name="g">green channel in space [0, 255]</param>
		/// <param name="b">blue channelin space [0, 255]</param>
		/// <param name="a">alpha channel in space [0, 255]</param>
		public void SetAdditiveTint(int r, int g, int b, int a)
		{
			SetAdditiveTint(r / 255f, g / 255f, b / 255f, a / 255f);
		}

		/// <summary>
		/// Color tint added during Draw phase, after base color (or texture) and multiply tint.
		/// </summary>
		/// <param name="r">red channel in space [0.0, 1.0]</param>
		/// <param name="g">green channel in space [0.0, 1.0]</param>
		/// <param name="b">blue channelin space [0.0, 1.0]</param>
		/// <param name="a">alpha channel in space [0.0, 1.0]</param>
		public void SetAdditiveTint(float r, float g, float b, float a)
		{
			SetAdditiveTint(new Vector4(r, g, b, a));
		}

		/// <summary>
		/// Color tint added during Draw phase, after base color (or texture) and multiply tint.
		/// </summary>
		/// <param name="color">color channel as vector of 4 float</param>
		public void SetAdditiveTint(Vector4 color)
		{
			this.additiveTint = color;
		}

		/// <summary>
		/// Color tint used as multiplier during Draw phase, after base color (or texture).
		/// </summary>
		/// <param name="r">red channel in space [0.0, 1.0]</param>
		/// <param name="g">green channel in space [0.0, 1.0]</param>
		/// <param name="b">blue channelin space [0.0, 1.0]</param>
		/// <param name="a">alpha channel in space [0.0, 1.0]</param>
		public void SetMultiplyTint(float r, float g, float b, float a)
		{
			SetMultiplyTint(new Vector4(r, g, b, a));
		}

		/// <summary>
		/// Color tint used as multiplier during Draw phase, after base color (or texture).
		/// </summary>
		/// <param name="r">red channel in space [0, 255]</param>
		/// <param name="g">green channel in space [0, 255]</param>
		/// <param name="b">blue channelin space [0, 255]</param>
		/// <param name="a">alpha channel in space [0, 255]</param>
		public void SetMultiplyTint(int r, int g, int b, int a)
		{
			SetMultiplyTint(r / 255f, g / 255f, b / 255f, a / 255f);
		}

		/// <summary>
		/// Color tint added during Draw phase, after base color (or texture).
		/// </summary>
		/// <param name="color">color channel as vector of 4 float</param>
		public void SetMultiplyTint(Vector4 color)
		{
			this.multiplyTint = color;
		}

		/// <summary>
		/// Draw the sprite filling it with this color
		/// </summary>
		/// <param name="color">color channel as vector of 4 float</param>
		override public void DrawColor(Vector4 color)
		{
			//Shader shaderSetupHook is overridden by this configuration
			this.Draw((mesh) =>
			{
				mesh.shader.SetUniform("use_texture", -1f);
				mesh.shader.SetUniform("color", color);
				mesh.shader.SetUniform("mul_tint", multiplyTint);
				mesh.shader.SetUniform("add_tint", additiveTint);
			});
		}

		/// <summary>
		/// Draw the whole texture
		/// </summary>
		/// <param name="tex">the texture used as source</param>
		public override void DrawTexture(Texture tex)
		{
			this.DrawTexture(tex, 0, 0, tex.Width, tex.Height);
		}

		/// <summary>
		/// Draw a texture starting at specific offset to the full size of the texture
		/// </summary>
		/// <param name="tex">the texture used as source</param>
		/// <param name="xOffset">offset on x axis</param>
		/// <param name="yOffset">offset on y axis</param>
		public void DrawTexture(Texture tex, int xOffset, int yOffset)
		{
			this.DrawTexture(tex, xOffset, yOffset, tex.Width, tex.Height);
		}

		/// <summary>
		/// Draw a texture starting at specific offset to a specific size
		/// </summary>
		/// <remarks>
		/// Passing width and height exceeding Texture.Width and Texture.Height will produce
		/// effects based on Texture filter adopted (repeat or clamp)
		/// </remarks>
		/// <param name="tex">the texture used as source</param>
		/// <param name="xOffset">offset on x axis</param>
		/// <param name="yOffset">offset on y axis</param>
		/// <param name="width">width to take into account</param>
		/// <param name="height">height to take into account</param>
		public void DrawTexture(Texture tex, int xOffset, int yOffset, int width, int height)
		{
			float deltaW = 1f / tex.Width;
			float deltaH = 1f / tex.Height;
			float left = xOffset * deltaW;
			float right = (xOffset + width) * deltaW;
			float top = yOffset * deltaH;
			float bottom = (yOffset + height) * deltaH;

			if (tex.flipped)
			{
				float tmp = bottom;
				bottom = top;
				top = tmp;
			}

			if (FlipX)
			{
				float tmp = left;
				left = right;
				right = tmp;
			}

			if (FlipY)
			{
				float tmp = bottom;
				bottom = top;
				top = tmp;
			}

			// OPTIMIZATION: re-upload uv's only if they are different from previous run
			if (top != this.uv[1] || bottom != this.uv[3] || left != this.uv[0] || right != this.uv[4])
			{
				SetArrayData(uv, left, right, top, bottom);
				this.UpdateUV();
			}
			base.DrawTexture(tex);
		}


		public override void DrawWireframe(Vector4 color, float tickness = 0.02F)
		{
			//Mesh / Sprite design is a little "spaghetti" dish
			//In order to make DrawWireframe compliant with Sprite 
			//has been applied the following workaround:
			//1. Make a backup of Sprite specific configuration
			//2. Apply Mesh configuration 
			//3. Execute DrawWireframe call
			//4. Restore Sprite configuration

			//1. Sprite Backup
			Shader shaderBkp = shader;
			ShaderSetupHook hookBkp = shaderSetupHook;

			//2. Set Mesh configuration
			shader = Mesh.simpleShader;
			shader.Use();
			hasVertexColors = true;
			shaderSetupHook = null;

			//3. Call 
			base.DrawWireframe(color, tickness);

			//4. Restore backup
			hasVertexColors = false;
			shader = shaderBkp;
			shader.Use();
			shaderSetupHook = hookBkp;
		}
	}
}

