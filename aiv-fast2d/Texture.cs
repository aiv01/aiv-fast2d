using System;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Aiv.Fast2D
{
	public class Texture : IDisposable
	{

		private int textureId;
		private int width;
		private int height;
		private byte[] bitmap;

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

		public byte[] Bitmap {
			get {
				return this.bitmap;
			}
		}

		public Texture (bool linear = false, bool repeatX = false, bool repeatY = false, bool mipMap = false)
		{
			GL.ActiveTexture (TextureUnit.Texture0);
			this.textureId = GL.GenTexture ();

			this.Bind ();

			if (linear) {
				this.SetLinear (mipMap);
			} else {
				this.SetNearest (mipMap);
			}

			this.SetRepeatX (repeatX);

			Console.WriteLine (Context.GetError ());


			this.SetRepeatY (repeatY);

			Console.WriteLine (Context.GetError ());
		}

		public Texture (int width, int height, bool linear = false, bool repeatX = false, bool repeatY = false, bool mipMap = false) : this (linear, repeatX, repeatY, mipMap)
		{
			this.width = width;
			this.height = height;
			this.bitmap = new byte[this.width * this.height * 4];
			this.Update ();
		}

		private byte[] LoadImage (string fileName, out int width, out int height)
		{
			byte[] bitmap = null;
			using (Bitmap image = new Bitmap (fileName)) {
				// to avoid losing a ref
				Bitmap _image = image;
				bitmap = new byte[image.Width * image.Height * 4];
				width = image.Width;
				height = image.Height;
				// if the image is not rgba, let's fix it
				if (image.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb) {
					Bitmap tmpBitmap = new Bitmap (image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					using (Graphics gfx = Graphics.FromImage (tmpBitmap)) {
						gfx.DrawImageUnscaled (image, 0, 0);
					}
					_image = tmpBitmap;
				}

				System.Drawing.Imaging.BitmapData data = _image.LockBits (new Rectangle (0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				Marshal.Copy (data.Scan0, bitmap, 0, bitmap.Length);
				_image.UnlockBits (data);

				for (int y = 0; y < _image.Height; y++) {
					for (int x = 0; x < _image.Width; x++) {
						int position = (y * _image.Width * 4) + (x * 4);
						// bgra -> rgba
						byte b = bitmap[position];
						byte r = bitmap [position + 2];
						bitmap [position] = r;
						bitmap [position+2] = b;
					}
				}
			}

			return bitmap;
		}

		public Texture (string fileName, bool linear = false, bool repeatX = false, bool repeatY = false, bool mipMap = false) : this (linear, repeatX, repeatY, mipMap)
		{
			this.bitmap = this.LoadImage (fileName, out this.width, out this.height);
			this.Update ();
		}

		public void Update (byte[]bitmap, int mipMap = 0)
		{
			this.Bind ();
			if (mipMap == 0)
				this.bitmap = bitmap;
			GL.TexImage2D<byte> (TextureTarget.Texture2D, mipMap, PixelInternalFormat.Rgba, this.width / (int)Math.Pow (2, mipMap), this.height / (int)Math.Pow (2, mipMap), 0, PixelFormat.Rgba, PixelType.UnsignedByte, this.bitmap);
		}

		public void Update (int mipMap = 0)
		{
			this.Update (this.bitmap, mipMap);
		}

		public void AddMipMap (int mipMap, string fileName)
		{
			int width;
			int height;
			byte[] bitmap = this.LoadImage (fileName, out width, out height);
			int expectedWidth = this.width / (int)Math.Pow (2, mipMap);
			int expectedHeight = this.height / (int)Math.Pow (2, mipMap);

			if (width != expectedWidth || height != expectedHeight)
				throw new Exception ("invalid mipmap size");

			this.Update (bitmap, mipMap);
		}

		public void Bind ()
		{
			GL.ActiveTexture (TextureUnit.Texture0);
			GL.BindTexture (TextureTarget.Texture2D, this.textureId);
		}

		public void Dispose ()
		{
			GL.DeleteTexture (this.textureId);
		}

		public void SetRepeatX (bool repeat = true)
		{
			this.Bind ();
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapS, repeat ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToBorder);
		}

		public void SetRepeatY (bool repeat = true)
		{
			this.Bind ();
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapT, repeat ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToBorder);
		}

		public void SetLinear (bool mipMap = false)
		{
			this.Bind ();
			if (mipMap) {
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.LinearMipmapLinear);
			} else {
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
			}
		}

		public void SetNearest (bool mipMap = false)
		{
			this.Bind ();
			if (mipMap) {
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.NearestMipmapNearest);
			} else {
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
			}
		}
	}
}

