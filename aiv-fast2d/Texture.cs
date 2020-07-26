using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;

namespace Aiv.Fast2D
{
    public class Texture : IDisposable
    {

        protected int textureId;
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
            else
            {
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
            this.premultiplied = true;
            this.bitmap = LoadImage(fileName, premultiplied, out this.width, out this.height);
            this.Update();
        }

        public Texture(Stream stream, bool nearest = false, bool repeatX = false, bool repeatY = false, bool mipMap = false) : this(nearest, repeatX, repeatY, mipMap)
        {
            this.premultiplied = true;
            this.bitmap = LoadImage(stream, premultiplied, out this.width, out this.height);
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
            this.premultiplied = true;
            byte[] mipMapBitmap = LoadImage(fileName, premultiplied, out mipMapWidth, out mipMapHeight);
            int expectedWidth = this.width / (int)Math.Pow(2, mipMap);
            int expectedHeight = this.height / (int)Math.Pow(2, mipMap);

            if (width != expectedWidth || height != expectedHeight)
                throw new Exception("invalid mipmap size");

            this.Update(mipMapBitmap, mipMap);
        }

        public void Bind(int unit = 0)
        {
            Graphics.BindTextureToUnit(this.textureId, unit);
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

        public virtual byte[] Download(int mipMap = 0)
        {
            byte[] data = new byte[this.Width * this.Height * 4];
            this.Bind();
            Graphics.TextureGetPixels(mipMap, data);
            return data;
        }

        ~Texture()
        {
            if (disposed)
                return;
            Window.Current.textureGC.Add(this.textureId);
            disposed = true;
        }

        private static byte[] LoadImage(string fileName, bool premultiplied, out int width, out int height)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            Stream imageStream = null;

            // if the file in included in the resources, load it as stream
            if (assembly.GetManifestResourceNames().Contains<string>(fileName))
            {
                imageStream = assembly.GetManifestResourceStream(fileName);
            }

            if (imageStream == null)
            {
                imageStream = new FileStream(fileName, FileMode.Open);
            }

            return LoadImage(imageStream, premultiplied, out width, out height);

        }
        private static byte[] LoadImage(Stream imageStream, bool premultiplied, out int width, out int height)
        {
            {
                byte[] bitmap = null;
                Bitmap image = new Bitmap(imageStream);

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
                        _image = image.Clone(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
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
                            if (premultiplied)
                            {
                                byte a = bitmap[position + 3];
                                bitmap[position] = (byte)(bitmap[position] * (a / 255f));
                                bitmap[position + 1] = (byte)(bitmap[position + 1] * (a / 255f));
                                bitmap[position + 2] = (byte)(bitmap[position + 2] * (a / 255f));
                            }
                        }
                    }
                }


                imageStream.Close();

                return bitmap;
            }
        }
    }
}

