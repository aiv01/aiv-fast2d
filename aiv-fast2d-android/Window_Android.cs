using System;
namespace Aiv.Fast2D
{
	public partial class Window
	{

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
			Vibrator vibrator = (Vibrator)this.window.Context.GetSystemService(global::Android.Content.Context.VibratorService);
			vibrator.Vibrate(amount);
		}

		public void CancelVibration()
		{
			Vibrator vibrator = (Vibrator)this.window.Context.GetSystemService(global::Android.Content.Context.VibratorService);
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
			this.window = gameView;
			// required for accessing assets
			assets = gameView.Context.Assets;
			this.scaleX = 1;
			this.scaleY = 1;
			// on mobile refresh is capped to 60hz
			this._deltaTime = 1f / 60f;

			this.window.Resize += (sender, e) =>
			{
				this.FixMobileViewport();
			};

			this.window.Touch += (sender, e) =>
			{
				switch (e.Event.Action)
				{
					case MotionEventActions.Move:
						touchX = e.Event.GetX() - this.viewportPosition.X / (this.viewportSize.X / this.orthoWidth);
						touchY = e.Event.GetY() - this.viewportPosition.Y / (this.viewportSize.Y / this.orthoHeight);
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

	}
}
