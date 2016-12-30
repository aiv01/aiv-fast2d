using System;
using Android.App;
using Android.OS;
using Android.Content.PM;
using Aiv.Fast2D.Android;
using OpenTK;

namespace Aiv.Fast2D.Android.Example.Obsolete
{
    [Activity(Label = "aiv_fast2d_example_android_obsolete",
        MainLauncher = true,
        Icon = "@drawable/icon",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden
#if __ANDROID_11__
        , HardwareAccelerated = false
#endif
        , ScreenOrientation = ScreenOrientation.Landscape
        )]
    public class MainActivity : Activity
    {
        private MobileGame view;

        private Mesh mesh001;
        private Texture alienTexture;
        private Sprite alien;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create our OpenGL view, and display it
            view = new MobileGame(this, GameSetup, GameUpdate);
            SetContentView(view);
        }

        protected override void OnPause()
        {
            base.OnPause();
            view.Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            view.Resume();
        }

        private void GameSetup(Window window)
        {
            window.SetClearColor(0f, 1f, 1f);
            mesh001 = new Mesh();
            mesh001.v = new float[]
            {
                100, 100,
                50, 200,
                150, 200
            };
            mesh001.UpdateVertex();

            alienTexture = new Texture("Assets/2.png");
            alien = new Sprite(alienTexture.Width, alienTexture.Height);

            window.AddPostProcessingEffect(new GrayscaleEffect());

        }

        private void GameUpdate(Window window)
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
            alien.position = new Vector2(100, 100);
            alien.DrawTexture(alienTexture);

            window.Update();
        }
    }
}

