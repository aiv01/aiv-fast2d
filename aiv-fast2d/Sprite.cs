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

in vec2 uvout;

uniform vec4 mul_tint;
uniform vec4 add_tint;
uniform sampler2D tex;
uniform float use_texture;

out vec4 color;

void main(){
        if (use_texture > 0.0) {
            color = texture(tex, uvout) * mul_tint;
            color += vec4(add_tint.xyz * color.a, add_tint.a);
        }
        else {
            color = add_tint;
        }
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


		private float width;
		private float height;

		public float Width
		{
			get
			{
				return this.width;
			}
		}

		public float Height
		{
			get
			{
				return this.height;
			}
		}

		private Vector4 multiplyTint = Vector4.One;
		private Vector4 additiveTint = Vector4.Zero;

		public Sprite(float width, float height) : base(spriteShader)
		{
			this.hasVertexColors = false;
			this.width = width;
			this.height = height;
			this.v = new float[] {
				0, 0,
				width, 0,
				0, height,
				width, 0,
				width, height,
				0, height
			};
			this.uv = new float[] {
				0, 1,
				1, 1,
				0, 0,
				1, 1,
				1, 0,
				0, 0
			};
			this.Update();

			this.shaderSetupHook = (mesh) =>
			{
				mesh.shader.SetUniform("tex", 0);
				mesh.shader.SetUniform("use_texture", 1f);
				mesh.shader.SetUniform("mul_tint", multiplyTint);
				mesh.shader.SetUniform("add_tint", additiveTint);
			};

		}

		public void SetAdditiveTint(int r, int g, int b, int a)
		{
			this.additiveTint = new Vector4(r / 255, g / 255, b / 255, a / 255);
		}

		public void SetAdditiveTint(float r, float g, float b, float a)
		{
			this.additiveTint = new Vector4(r, g, b, a);
		}

		public void SetAdditiveTint(Vector4 color)
		{
			this.additiveTint = color;
		}

		public void SetMultiplyTint(float r, float g, float b, float a)
		{
			this.multiplyTint = new Vector4(r, g, b, a);
		}

		public void SetMultiplyTint(Vector4 color)
		{
			this.multiplyTint = color;
		}

		public void DrawSolidColor(float r, float g, float b, float a = 1)
		{
			this.Draw((mesh) =>
			{
				mesh.shader.SetUniform("use_texture", -1f);
				mesh.shader.SetUniform("add_tint", new Vector4(r, g, b, a));
			});
		}

		public void DrawSolidColor(int r, int g, int b, int a = 255)
		{
			DrawSolidColor(r / 255f, g / 255f, b / 255f, a / 255f);
		}

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

			// OPTIMIZATION: re-upload uv's only if they are different from previous run
			if (top != this.uv[1] || bottom != this.uv[5] || left != this.uv[0] || right != this.uv[2])
			{
				this.uv = new float[] {
					left, top,
					right, top,
					left, bottom,
					right, top,
					right, bottom,
					left, bottom};

				this.UpdateUV();
			}
			base.DrawTexture(tex);
		}

		public void DrawTexture(Texture tex, int xOffset, int yOffset)
		{
			this.DrawTexture(tex, xOffset, yOffset, tex.Width, tex.Height);
		}

		public override void DrawTexture(Texture tex)
		{
			this.DrawTexture(tex, 0, 0, tex.Width, tex.Height);
		}
	}
}

