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
in vec2 uvout;
out vec4 color;
uniform sampler2D tex;
void main(){
        color = texture(tex, uvout);
}";

		private static Shader spriteShader = new Shader (spriteVertexShader, spriteFragmentShader);


		private int width;
		private int height;

		public int Width {
			get {
				return this.width;
			}
		}

		public int Height {
			get {
				return this.height;
			}
		}

		public Sprite (int width, int height) : base (spriteShader)
		{
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
			this.Update ();
			this.shader.SetUniform ("tex", 0);
		}

		public void DrawTexture (Texture tex, int x, int y, int width, int height)
		{
			float deltaW = 1f / tex.Width;
			float deltaH = 1f / tex.Height;
			float left = x * deltaW;
			float right = (x + width) * deltaW;
			float top = y * deltaH;
			float bottom = (y + height) * deltaH;
			this.uv = new float[] {
				left, top,
				right, top,
				left, bottom,
				right, top,
				right, bottom,
				left, bottom
			};
			this.UpdateUV ();
			base.DrawTexture (tex);
		}

		public void DrawTexture (Texture tex, int x, int y)
		{
			this.DrawTexture (tex, x, y, tex.Width, tex.Height);
		}

		public void DrawTexture (Texture tex)
		{
			this.DrawTexture (tex, 0, 0, tex.Width, tex.Height);
		}
	}
}

