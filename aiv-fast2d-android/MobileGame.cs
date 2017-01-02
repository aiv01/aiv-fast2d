using System;
using OpenTK;
using OpenTK.Platform.Android;
using OpenTK.Graphics;
using Android.Util;
using Android.App;
using Android.OS;

namespace Aiv.Fast2D.Android
{

	public class MobileGame : Activity
	{
		private class View : AndroidGameView
		{

			private Window window;

			private MobileGame mobileGame;

			public View(MobileGame mobileGame) : base(mobileGame)
			{
				window = new Window(this);
				this.mobileGame = mobileGame;
			}

			// This gets called when the drawing surface is ready
			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);

				window.FixMobileViewport();
				mobileGame.GameSetup(window);
				// Run the render loop
				Run();
			}

			// This method is called everytime the context needs
			// to be recreated. Use it to set any egl-specific settings
			// prior to context creation
			//
			// In this particular case, we demonstrate how to set
			// the graphics mode and fallback in case the device doesn't
			// support the defaults
			protected override void CreateFrameBuffer()
			{

				ContextRenderingApi = GLVersion.ES3;

				// the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
				try
				{
					Log.Verbose("Aiv.Fast2D", "Trying OpenGL ES3");

					// if you don't call this, the context won't be created
					base.CreateFrameBuffer();
					return;
				}
				catch (Exception ex)
				{
					Log.Verbose("Aiv.Fast2D", "{0}", ex);
				}

				// this is a graphics setting that sets everything to the lowest mode possible so
				// the device returns a reliable graphics setting.
				try
				{
					ContextRenderingApi = GLVersion.ES2;
					Aiv.Fast2D.Window.SetObsoleteMode();

					// a framebuffer could be already created in previous attempt
					try
					{
						base.DestroyFrameBuffer();
					}
					catch
					{

					}

					Log.Verbose("Aiv.Fast2D", "Trying OpenGL ES2");

					// if you don't call this, the context won't be created
					base.CreateFrameBuffer();
					return;
				}
				catch (Exception ex)
				{
					Log.Verbose("Aiv.Fast2D", "{0}", ex);
				}
				throw new Exception("Can't setup OpenGL ES, aborting");
			}

			// This gets called on each frame render
			protected override void OnRenderFrame(FrameEventArgs e)
			{
				if (this.mobileGame.requestedExit)
				{
					this.mobileGame.FinishActivity(0);
					return;
				}
				base.OnRenderFrame(e);
				mobileGame.GameUpdate(window);
				window.Update();
			}

		}

		private View view;
		private bool requestedExit;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			// Create our OpenGL view, and display it
			view = new View(this);
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

		protected void Exit()
		{
			requestedExit = true;
		}

		protected virtual void GameSetup(Aiv.Fast2D.Window window)
		{
		}

		protected virtual void GameUpdate(Aiv.Fast2D.Window window)
		{
		}
	}

}
