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
        private bool premultiplied;

        public bool flipped = false;


        /// <summary>
        /// The Id associated to this texture on the GPU
        /// </summary>
        public int Id { get; }

        private bool disposed;

        /// <summary>
        /// The width of this texture
        /// </summary>
        public int Width { get; internal set; }
        
        /// <summary>
        /// The Height of this texture
        /// </summary>
        public int Height { get; internal set; }
       
        /// <summary>
        /// The ratio of the size of this texture (width / height)
        /// </summary>
        public float Ratio
        {
            get
            {
                return (float)Width / (float)Height;
            }
        }

        /// <summary>
        /// Data in bytes representing pixels
        /// </summary>
        public byte[] Bitmap { get; private set; }

        private byte[] _downloadableData; //Works as byte array "instance" cache (to avoid to call "new" every time

        public bool IsPremultiplied
        {
            get
            {
                return premultiplied;
            }
        }

        public Texture(bool nearest = false, bool repeatX = false, bool repeatY = false, bool mipMap = false)
        {
            this.Id = Graphics.NewTexture();

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
            Width = width;
            Height = height;
            this.Bitmap = new byte[Width * Height * 4];
            this.Update();
        }


        public Texture(string fileName, bool nearest = false, bool repeatX = false, bool repeatY = false, bool mipMap = false) : this(nearest, repeatX, repeatY, mipMap)
        {

            this.premultiplied = true;
            this.Bitmap = LoadImage(fileName, premultiplied, out int width, out int height);


            Width = width;
            Height = height;
            this.Update();
        }

        public Texture(Stream stream, bool nearest = false, bool repeatX = false, bool repeatY = false, bool mipMap = false) : this(nearest, repeatX, repeatY, mipMap)
        {
            this.premultiplied = true;
            this.Bitmap = LoadImage(stream, premultiplied, out int width, out int height);

            Width = width;
            Height = height;
            this.Update();
        }

        public void Update(byte[] bitmap, int mipMap = 0)
        {
            this.Bind();
            if (mipMap == 0)
                this.Bitmap = bitmap;
            Graphics.TextureBitmap(Width, Height, bitmap, mipMap);
        }

        public void Update(int mipMap = 0)
        {
            this.Update(this.Bitmap, mipMap);
        }

        public void AddMipMap(int mipMap, string fileName)
        {
            int mipMapWidth;
            int mipMapHeight;
            this.premultiplied = true;
            byte[] mipMapBitmap = LoadImage(fileName, premultiplied, out mipMapWidth, out mipMapHeight);
            int expectedWidth = Width / (int)Math.Pow(2, mipMap);
            int expectedHeight = Height / (int)Math.Pow(2, mipMap);

            if (Width != expectedWidth || Height != expectedHeight)
                throw new Exception("invalid mipmap size");

            this.Update(mipMapBitmap, mipMap);
        }

        public void Bind(int unit = 0)
        {
            Graphics.BindTextureToUnit(this.Id, unit);
        }

        public void Dispose()
        {
            if (disposed)
                return;
            Graphics.DeleteTexture(this.Id);
            Window.Current.Log(string.Format("texture {0} deleted", this.Id));
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
            //Avoid reinstantiating every time Download is called.
            //For some reason byte array is not garbage collected in time
            //and with many call to Download application throws OutOfMemoryException
            //byte[] data = new byte[Width * Height * 4];
            if (_downloadableData == null) 
                _downloadableData = new byte[Width * Height * 4];

            this.Bind();
            Graphics.TextureGetPixels(mipMap, _downloadableData);
            return _downloadableData;
        }

        ~Texture()
        {
            if (disposed)
                return;
            Window.Current.textureGC.Add(this.Id);
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

