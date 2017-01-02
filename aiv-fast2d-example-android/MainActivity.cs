using System;
using Android.App;
using Android.OS;
using Android.Content.PM;
using Aiv.Fast2D.Android;
using Aiv.Fast2D.Example;
using OpenTK;

namespace Aiv.Fast2D.Android.Example
{
	[Activity(Label = "aiv_fast2d_example_android",
		MainLauncher = true,
		Icon = "@drawable/icon",
		ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden
#if __ANDROID_11__
		, HardwareAccelerated = false
#endif
		, ScreenOrientation = ScreenOrientation.Landscape
		)]
	public class MainActivity : MobileGame
	{

		private Sprite sprite001;
		private ParticleSystem particleSystem001;
		private Texture alienTexture;
		private Sprite alien;
		private Segment lineDrawer;

		protected override void GameSetup(Window window)
		{
			window.SetClearColor(0f, 0f, 1f);
			sprite001 = new Sprite(300, 300);
			sprite001.position = new Vector2(window.Width / 2 - 150, window.Height / 2 - 150);
			particleSystem001 = new ParticleSystem(10, 10, 100);
			particleSystem001.position = new Vector2(window.Width / 2, window.Height / 2);

			alienTexture = new Texture("Assets/2.png");
			alien = new Sprite(alienTexture.Width, alienTexture.Height);

			lineDrawer = new Segment(0, 0, window.Width, window.Height, 4);
		}

		protected override void GameUpdate(Window window)
		{
			if (window.IsTouching)
			{
				alien.position = window.TouchPosition;
				if ((sprite001.position - window.TouchPosition).Length < 100)
				{
					window.Vibrate(1000);
				}
			}
			else
			{
				window.CancelVibration();
			}
			alien.DrawTexture(alienTexture);

			particleSystem001.Update(window);

			sprite001.position.X += 10f * window.deltaTime;
			sprite001.DrawSolidColor(1, 0, 0, 0.5f);

			lineDrawer.Point2 = window.TouchPosition;
			lineDrawer.DrawSolidColor(1f, 1f, 0f, 1f);
		}
	}
}

