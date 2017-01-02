using System;
using System.IO;
using OpenTK;
using OpenGLES;
using OpenTK.Platform.iPhoneOS;
using GLKit;
using UIKit;
using System.Runtime.InteropServices;
using CoreGraphics;

namespace Aiv.Fast2D
{
	public partial class Window
	{

		#region touch

		private float touchX;
		private float touchY;

		public float TouchX
		{
			get
			{
				return touchX;
			}
		}

		public float TouchY
		{
			get
			{
				return touchY;
			}
		}

		public Vector2 TouchPosition
		{
			get
			{
				return new Vector2(touchX, touchY);
			}
		}

		private bool isTouching;
		public bool IsTouching
		{
			get
			{
				return isTouching;
			}
		}
		#endregion


		private GLKView context;

		public void FixMobileViewport()
		{
			// get the size in pixels
			this.width = (int)(context.Bounds.Size.Width * UIKit.UIScreen.MainScreen.Scale);
			this.height = (int)(context.Bounds.Size.Height * UIKit.UIScreen.MainScreen.Scale);

			this.SetViewport(0, 0, this.width, this.height);

			Graphics.Setup();
		}

		public void TouchBegan(float x, float y)
		{
			isTouching = true;
			touchX = x / this.scaleX - this.viewportPosition.X / (this.viewportSize.X / this.OrthoWidth);
			touchY = y / this.scaleY - this.viewportPosition.Y / (this.viewportSize.Y / this.OrthoHeight);
		}

		public void TouchEnded(float x, float y)
		{
			isTouching = false;
			touchX = x / this.scaleX - this.viewportPosition.X / (this.viewportSize.X / this.OrthoWidth);
			touchY = y / this.scaleY - this.viewportPosition.Y / (this.viewportSize.Y / this.OrthoHeight);
		}

		public void TouchMoved(float x, float y)
		{
			touchX = x / this.scaleX - this.viewportPosition.X / (this.viewportSize.X / this.OrthoWidth);
			touchY = y / this.scaleY - this.viewportPosition.Y / (this.viewportSize.Y / this.OrthoHeight);
		}

		public Window(GLKView view)
		{
			context = view;

			// compute pixel density scale
			this.scaleX = 1f / (float)UIKit.UIScreen.MainScreen.Scale;
			this.scaleY = 1f / (float)UIKit.UIScreen.MainScreen.Scale;


			// on mobile refresh is capped to 60hz
			this._deltaTime = 1f / 60f;

			FixMobileViewport();

			FinalizeSetup();
		}

		public void Update()
		{

			// apply postprocessing (if required)
			ApplyPostProcessingEffects();

			// cleanup graphics resources
			RunGraphicsGC();
		}

		public static byte[] LoadImage(string fileName, bool premultiplied, out int width, out int height)
		{
			premultiplied = false;
			UIImage image = UIImage.FromBundle(fileName);
			width = (int)image.CGImage.Width;
			height = (int)image.CGImage.Height;

			byte[] bitmap = new byte[width * height * 4];

			using (var colorSpace = CGColorSpace.CreateDeviceRGB())
			{
				IntPtr rawData = Marshal.AllocHGlobal(width * height * 4);
				using (var cgContext = new CGBitmapContext(rawData, width, height, 8, 4 * width, colorSpace, CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast))
				{
					cgContext.DrawImage(new CGRect(0, 0, width, height), image.CGImage);
					Marshal.Copy(rawData, bitmap, 0, bitmap.Length);
				}
				Marshal.FreeHGlobal(rawData);
			}

			if (!premultiplied)
			{
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						int position = (y * width * 4) + (x * 4);

						byte r = bitmap[position];
						byte g = bitmap[position + 1];
						byte b = bitmap[position + 2];
						byte a = bitmap[position + 3];


						bitmap[position] = (byte)(r * (255f / a));
						bitmap[position + 1] = (byte)(g * (255f / a));
						bitmap[position + 2] = (byte)(b * (255f / a));

					}

				}

			}

			return bitmap;
		}

	}
}
