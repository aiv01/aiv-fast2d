using System;
using Android.App;
using Android.OS;
using Android.Content.PM;
using Aiv.Fast2D.Android;
using OpenTK;

namespace Aiv.Fast2D.Android.Example.Obsolete
{
    [Activity(Label = "Aiv.Fast2D.Android.Example.Obsolete",
        MainLauncher = true,
        Icon = "@drawable/icon",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden
        , ScreenOrientation = ScreenOrientation.Landscape
        )]
    public class MainActivity : MobileGame
    {

        private Mesh mesh001;
        private Texture alienTexture;
        private Sprite alien;


        protected override void GameSetup(Window window)
        {
            window.SetDefaultOrthographicSize(10);
            window.SetClearColor(0f, 1f, 1f);
            mesh001 = new Mesh();
            mesh001.v = new float[]
            {
                5, 5,
                2.5f, 10,
                7.5f, 10
            };
            mesh001.UpdateVertex();

            alienTexture = new Texture("Assets/2.png");
            alien = new Sprite(3 * alienTexture.Ratio, 3);

            window.AddPostProcessingEffect(new GrayscaleEffect());

        }

        protected override void GameUpdate(Window window)
        {
            if (window.IsTouching)
            {

                window.Vibrate(1000);
            }
            else
            {
                window.CancelVibration();
            }

            mesh001.DrawColor(0f, 1f, 0f, 1f);

            alien.SetAdditiveTint(0f, 0, 0, 0);
            alien.position = window.TouchPosition;
            alien.DrawTexture(alienTexture);

            alien.SetAdditiveTint(1f, 0, 0, 0);
            alien.position = new Vector2(5, 5);
            alien.DrawTexture(alienTexture);
        }
    }
}

