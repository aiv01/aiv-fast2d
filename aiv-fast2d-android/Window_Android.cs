using System;
using System.IO;
using OpenTK;
using OpenTK.Platform.Android;
using Android.OS;
using Android.Content.Res;
using Android.Views;
using Android.Graphics;

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


		private AndroidGameView context;

		public void FixMobileViewport()
		{
			this.width = context.Holder.SurfaceFrame.Width();
			this.height = context.Holder.SurfaceFrame.Height();

			this.SetViewport(0, 0, this.width, this.height);

			Graphics.Setup();
		}



		public void Vibrate(long amount)
		{
			Vibrator vibrator = (Vibrator)this.context.Context.GetSystemService(global::Android.Content.Context.VibratorService);
			vibrator.Vibrate(amount);
		}

		public void CancelVibration()
		{
			Vibrator vibrator = (Vibrator)this.context.Context.GetSystemService(global::Android.Content.Context.VibratorService);
			vibrator.Cancel();
		}

		private static AssetManager assets;

		public static AssetManager Assets
		{
			get
			{
				return assets;
			}
		}


		public Window(AndroidGameView gameView)
		{
			this.context = gameView;
			// required for accessing assets
			assets = gameView.Context.Assets;
			this.scaleX = 1;
			this.scaleY = 1;
			// on mobile refresh is capped to 60hz
			this._deltaTime = 1f / 60f;

			this.context.Resize += (sender, e) =>
			{
				this.FixMobileViewport();
			};

			this.context.Touch += (sender, e) =>
			{
				switch (e.Event.Action)
				{
					case MotionEventActions.Move:
						touchX = e.Event.GetX() - this.viewportPosition.X / (this.viewportSize.X / this.OrthoWidth);
						touchY = e.Event.GetY() - this.viewportPosition.Y / (this.viewportSize.Y / this.OrthoHeight);
						break;
					case MotionEventActions.Up:
						isTouching = false;
						break;
					case MotionEventActions.Down:
						isTouching = true;
						break;
					default:
						break;
				}
			};

			FinalizeSetup();
		}

		public void Update()
		{

			// apply postprocessing (if required)
			ApplyPostProcessingEffects();

			// redraw
			this.context.SwapBuffers();

			// cleanup graphics resources
			RunGraphicsGC();

			// reset and clear
			ResetFrameBuffer();
		}

		public static byte[] LoadImage(string fileName, bool premultiplied, out int width, out int height)
		{
			byte[] bitmap = null;
			Stream imageStream = null;
			Bitmap image = null;
			if (imageStream == null)
			{
				if (fileName.StartsWith("Assets/"))
				{
					string newFileName = fileName.Substring(7);
					imageStream = Window.Assets.Open(newFileName);
				}
			}

			BitmapFactory.Options bitmapOptions = new BitmapFactory.Options();
			bitmapOptions.InPreferredConfig = global::Android.Graphics.Bitmap.Config.Argb8888;
			image = BitmapFactory.DecodeStream(imageStream, new global::Android.Graphics.Rect(0, 0, 0, 0), bitmapOptions);

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
					if (premultiplied)
					{
						bitmap[position] = (byte)(r * (a / 255f));
						bitmap[position + 1] = (byte)(g * (a / 255f));
						bitmap[position + 2] = (byte)(b * (a / 255f));
					}
					else {
						bitmap[position] = r;
						bitmap[position + 1] = g;
						bitmap[position + 2] = b;
					}
					bitmap[position + 3] = a;

				}
			}
			return bitmap;
		}

	}
}
