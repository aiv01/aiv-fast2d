using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using OpenTK.Platform;
using OpenTK.Platform.Android;
using Android.Views;
using AndroidContent = Android.Content;
using Android.Util;

using Aiv.Fast2D;

namespace Aiv.Fast2D.Android
{
    public class MobileGame : AndroidGameView
    {
        private Window window;

        public delegate void Setup(Window window);
        public delegate void Update(Window window);

        private Setup setupDelegate;
        private Update updateDelegate;

        public MobileGame(AndroidContent.Context context, Setup setupDelegate, Update updateDelegate) : base(context)
        {
            window = new Window(this);
            this.setupDelegate = setupDelegate;
            this.updateDelegate = updateDelegate;
        }

        // This gets called when the drawing surface is ready
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.setupDelegate(window);

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
                Log.Verbose("Aiv.Fast2D", "Loading with default settings");

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
                Log.Verbose("Aiv.Fast2D", "Loading with custom Android settings (low mode)");
                GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);

                // if you don't call this, the context won't be created
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("Aiv.Fast2D", "{0}", ex);
            }
            throw new Exception("Can't load egl, aborting");
        }

        // This gets called on each frame render
        protected override void OnRenderFrame(FrameEventArgs e)
        {

            base.OnRenderFrame(e);

            this.updateDelegate(window);

        }

    }
}
