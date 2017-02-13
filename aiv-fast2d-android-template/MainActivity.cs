using System;
using Android.App;
using Android.Content.PM;
using Aiv.Fast2D;
using Aiv.Fast2D.Android;
using OpenTK;

namespace $safeprojectname$ 
{
    [Activity(Label = "$safeprojectname$ ",
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

