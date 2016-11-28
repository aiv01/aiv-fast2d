```cs
using Android.App;
using Android.OS;
using Android.Content.PM;

using Aiv.Fast2D;
using Aiv.Fast2D.Android;


namespace ExampleAivMobileGame
{
	
	[Activity(Label = "ExampleAivMobileGame",
				ConfigurationChanges = ConfigChanges.KeyboardHidden,
				ScreenOrientation = ScreenOrientation.SensorLandscape,
				MainLauncher = true,
				Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{

		Sprite sprite002;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// MobileGame is exposed by Aiv.Fast2D.Android
			MobileGame view = new MobileGame(this, Setup, Update);
			SetContentView(view);

		}

		// load textures, create meshes and so on...
		private void Setup(Window window)
		{
			sprite002 = new Sprite(400, 200);
		}

		// rendering cycle
		private void Update(Window window)
		{
			sprite002.position = window.TouchPosition;
			sprite002.DrawSolidColor(0, 1, 1, 1f);

			window.Update();
		}
	}
}
```
