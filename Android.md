```cs
using System;
using Android.App;
using Android.OS;
using Android.Content.PM;
using Aiv.Fast2D.Android;
using OpenTK;

namespace ExampleApp
{
	[Activity(Label = "ExampleApp",
		MainLauncher = true,
		Icon = "@drawable/icon",
		ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
                ScreenOrientation = ScreenOrientation.Landscape
		)]
	public class MainActivity : MobileGame
	{

		protected override void GameSetup(Window window)
		{	
		}

		protected override void GameUpdate(Window window)
		{
		}
	}
}

```
