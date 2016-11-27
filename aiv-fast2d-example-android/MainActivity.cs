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
		,HardwareAccelerated=false
#endif
        )]
    public class MainActivity : Activity
    {
        MobileGame view;
        Sprite sprite001;

        ParticleSystem particleSystem001;

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
            window.SetClearColor(0f, 0f, 1f);
            sprite001 = new Sprite(300, 300);
            particleSystem001 = new ParticleSystem(10, 10, 100);
        }

        private void GameUpdate(Window window)
        {
            sprite001.position.X += 10f * window.deltaTime;
            sprite001.DrawSolidColor(0, 1, 0, 1);

            particleSystem001.Update(window);
            window.Update();
        }
    }
}

