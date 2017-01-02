using System;
#if !__MOBILE__
using OpenTK.Graphics.OpenGL;
using System.Drawing;
#else
using OpenTK.Graphics.ES30;
#if __ANDROID__
using Android.Graphics;
#endif
#endif
using System.Reflection;
using System.Text;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

namespace Aiv.Fast2D
{
	public class Texture : IDisposable
	{

		private int textureId;
		private int width;
		private int height;
		private byte[] bitmap;
		private bool premultiplied;

		public bool flipped = false;

		public int Id
		{
			get
			{
				return textureId;
			}
		}

		private bool disposed;

		public int Width
		{
			get
			{
				return this.width;
			}
		}

		public int Height
		{
			get
			{
				return this.height;
			}
		}

		public float Ratio
		{
			get
			{
				return (float)this.Width / (float)this.Height;
			}
		}

		public byte[] Bitmap
		{
			get
			{
				return this.bitmap;
			}
		}

		public bool IsPremultiplied
		{
			get
			{
				return premultiplied;
			}
		}

		public Texture(bool nearest = false, bool repeatX = false, bool repeatY = false, bool mipMap = false)
		{

			GL.ActiveTexture(TextureUnit.Texture0);
			this.textureId = GL.GenTexture();

			this.Bind();

			if (nearest)
			{
				this.SetNearest(mipMap);
			}
			else {
				this.SetLinear(mipMap);
			}

			this.SetRepeatX(repeatX);

			this.SetRepeatY(repeatY);
		}

		public Texture(int width, int height, bool nearest = false, bool repeatX = false, bool repeatY = false, bool mipMap = false) : this(nearest, repeatX, repeatY, mipMap)
		{
			this.width = width;
			this.height = height;
			this.bitmap = new byte[this.width * this.height * 4];
			this.Update();
		}




		public Texture(string fileName, bool nearest = false, bool repeatX = false, bool repeatY = false, bool mipMap = false) : this(nearest, repeatX, repeatY, mipMap)
		{
			this.bitmap = Window.LoadImage(fileName, premultiplied, out this.width, out this.height);
			this.Update();
		}

		public void Update(byte[] bitmap, int mipMap = 0)
		{
			this.Bind();
			if (mipMap == 0)
				this.bitmap = bitmap;
#if !__MOBILE__
			GL.TexImage2D<byte>(TextureTarget.Texture2D, mipMap, PixelInternalFormat.Rgba8, this.width / (int)Math.Pow(2, mipMap), this.height / (int)Math.Pow(2, mipMap), 0, PixelFormat.Rgba, PixelType.UnsignedByte, this.bitmap);
#else
            GL.TexImage2D(TextureTarget.Texture2D, mipMap, PixelInternalFormat.Rgba, this.width / (int)Math.Pow(2, mipMap), this.height / (int)Math.Pow(2, mipMap), 0, OpenTK.Graphics.ES30.PixelFormat.Rgba, PixelType.UnsignedByte, this.bitmap);
#endif
		}

		public void Update(int mipMap = 0)
		{
			this.Update(this.bitmap, mipMap);
		}

		public void AddMipMap(int mipMap, string fileName)
		{
			int width;
			int height;
			byte[] bitmap = Window.LoadImage(fileName, premultiplied, out width, out height);
			int expectedWidth = this.width / (int)Math.Pow(2, mipMap);
			int expectedHeight = this.height / (int)Math.Pow(2, mipMap);

			if (width != expectedWidth || height != expectedHeight)
				throw new Exception("invalid mipmap size");

			this.Update(bitmap, mipMap);
		}

		public void Bind()
		{
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, this.textureId);
		}

		public void Dispose()
		{
			if (disposed)
				return;
			GL.DeleteTexture(this.textureId);
			Window.Current.Log(string.Format("texture {0} deleted", this.textureId));
			disposed = true;
		}

		public void SetRepeatX(bool repeat = true)
		{
			this.Bind();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, repeat ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
		}

		public void SetRepeatY(bool repeat = true)
		{
			this.Bind();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, repeat ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
		}

		public void SetLinear(bool mipMap = false)
		{
			this.Bind();
			if (mipMap)
			{
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
			}
			else {
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			}
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
		}

		public void SetNearest(bool mipMap = false)
		{
			this.Bind();
			if (mipMap)
			{
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
			}
			else {
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			}
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
		}

		~Texture()
		{
			if (disposed)
				return;
			Window.Current.textureGC.Add(this.textureId);
			disposed = true;
		}
	}
}

