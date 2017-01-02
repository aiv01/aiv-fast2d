using System;

namespace Aiv.Fast2D
{
	public class Texture : IDisposable
	{

		private int textureId;
		private int width;
		private int height;
		private byte[] bitmap;
		protected bool premultiplied;

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
			this.textureId = Graphics.NewTexture();

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
			Graphics.TextureBitmap(width, height, bitmap, mipMap);
		}

		public void Update(int mipMap = 0)
		{
			this.Update(this.bitmap, mipMap);
		}

		public void AddMipMap(int mipMap, string fileName)
		{
			int mipMapWidth;
			int mipMapHeight;
			byte[] mipMapBitmap = Window.LoadImage(fileName, premultiplied, out mipMapWidth, out mipMapHeight);
			int expectedWidth = this.width / (int)Math.Pow(2, mipMap);
			int expectedHeight = this.height / (int)Math.Pow(2, mipMap);

			if (width != expectedWidth || height != expectedHeight)
				throw new Exception("invalid mipmap size");

			this.Update(mipMapBitmap, mipMap);
		}

		public void Bind()
		{
			Graphics.BindTextureToUnit(this.textureId, 0);
		}

		public void Dispose()
		{
			if (disposed)
				return;
			Graphics.DeleteTexture(this.textureId);
			Window.Current.Log(string.Format("texture {0} deleted", this.textureId));
			disposed = true;
		}

		public void SetRepeatX(bool repeat = true)
		{
			this.Bind();
			Graphics.TextureSetRepeatX(repeat);
		}

		public void SetRepeatY(bool repeat = true)
		{
			this.Bind();
			Graphics.TextureSetRepeatY(repeat);
		}

		public void SetLinear(bool mipMap = false)
		{
			this.Bind();
			Graphics.TextureSetLinear(mipMap);
		}

		public void SetNearest(bool mipMap = false)
		{
			this.Bind();
			Graphics.TextureSetNearest(mipMap);
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

