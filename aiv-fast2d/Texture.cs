using System;
#if !__MOBILE__
using OpenTK.Graphics.OpenGL;
using System.Drawing;
#else
using OpenTK.Graphics.ES30;
using Android.Graphics;
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


        private byte[] LoadImage(string fileName, out int width, out int height)
        {
            byte[] bitmap = null;


#if !__MOBILE__
            Bitmap image = null;

            Assembly assembly = Assembly.GetEntryAssembly();

            // if the file in included in the resources, load it as stream
            if (assembly.GetManifestResourceNames().Contains<string>(fileName))
            {
                Stream imageStream = assembly.GetManifestResourceStream(fileName);
                image = new Bitmap(imageStream);
            }
            else {
                image = new Bitmap(fileName);
            }

            using (image)
            {
                // to avoid losing a ref
                Bitmap _image = image;
                bitmap = new byte[image.Width * image.Height * 4];
                width = image.Width;
                height = image.Height;
                // if the image is not rgba, let's fix it
                if (image.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    Bitmap tmpBitmap = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (Graphics gfx = Graphics.FromImage(tmpBitmap))
                    {
                        gfx.DrawImageUnscaled(image, 0, 0);
                    }
                    _image = tmpBitmap;
                }

                System.Drawing.Imaging.BitmapData data = _image.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Marshal.Copy(data.Scan0, bitmap, 0, bitmap.Length);
                _image.UnlockBits(data);

                for (int y = 0; y < _image.Height; y++)
                {
                    for (int x = 0; x < _image.Width; x++)
                    {
                        int position = (y * _image.Width * 4) + (x * 4);
                        // bgra -> rgba
                        byte b = bitmap[position];
                        byte r = bitmap[position + 2];
                        bitmap[position] = r;
                        bitmap[position + 2] = b;
                        // premultiply
                        byte a = bitmap[position + 3];
                        bitmap[position] = (byte) (bitmap[position] * (a / 255f));
                        bitmap[position+1] = (byte)(bitmap[position+1] * (a / 255f));
                        bitmap[position+2] = (byte)(bitmap[position+2] * (a / 255f));
                    }
                }
                premultiplied = true;
            }
#else
            Bitmap image = null;
            if (fileName.StartsWith("Assets/"))
            {
                string newFileName = fileName.Substring(7);
                Stream stream = Context.assets.Open(newFileName);
                BitmapFactory.Options bitmapOptions = new BitmapFactory.Options();
                bitmapOptions.InPreferredConfig = global::Android.Graphics.Bitmap.Config.Argb8888;
                image = BitmapFactory.DecodeStream(stream, new global::Android.Graphics.Rect(0, 0, 0, 0), bitmapOptions);
            }
            width = image.Width;
            height = image.Height;

            bitmap = new byte[width * height * 4];

        
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixel = image.GetPixel(x, y);

                    byte a = (byte)((pixel >> 24) & 0xff);
                    byte r = (byte)((pixel >> 16) & 0xff);
                    byte g = (byte)((pixel >> 8) & 0xff);
                    byte b = (byte)(pixel & 0xff);
                    // premultiply;
                    int position = (y * width * 4) + (x * 4);
                    bitmap[position] = (byte)(r * (a / 255f));
                    bitmap[position + 1] = (byte)(g * (a / 255f));
                    bitmap[position + 2] = (byte)(b * (a / 255f));
                    bitmap[position + 3] = a;

                }
            }
            premultiplied = true;
#endif

            return bitmap;
        }

        public Texture(string fileName, bool nearest = false, bool repeatX = false, bool repeatY = false, bool mipMap = false) : this(nearest, repeatX, repeatY, mipMap)
        {
            this.bitmap = this.LoadImage(fileName, out this.width, out this.height);
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
            byte[] bitmap = this.LoadImage(fileName, out width, out height);
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
            Context.textureGC.Add(this.textureId);
            disposed = true;
        }
    }
}

