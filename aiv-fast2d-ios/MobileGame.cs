using System;
using Foundation;
using GLKit;
using OpenGLES;
using UIKit;

namespace Aiv.Fast2D.iOS
{

	public class MobileGame : GLKViewController, IGLKViewDelegate
	{
		private Window window;
		private GLKView view;

		EAGLContext context { get; set; }

		public MobileGame(NSCoder coder) : base(coder)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			context = new EAGLContext(EAGLRenderingAPI.OpenGLES3);

			if (context == null)
			{
				context = new EAGLContext(EAGLRenderingAPI.OpenGLES2);
				if (context == null)
				{
					throw new Exception("Unable to create OpenGL ES2 Context");
				}
				Window.SetObsoleteMode();
			}

			view = (GLKView)View;
			view.Context = context;
			view.DrawableDepthFormat = GLKViewDrawableDepthFormat.Format24;

			EAGLContext.SetCurrentContext(context);

			window = new Window(view);
			GameSetup(window);
		}


		protected virtual void GameSetup(Window window)
		{
		}

		protected virtual void GameUpdate(Window window)
		{
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			throw new Exception("Dispose()");
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			throw new Exception("DidReceiveMemoryWarning()");
		}

		public override bool PrefersStatusBarHidden()
		{
			return true;
		}

		void IGLKViewDelegate.DrawInRect(GLKView view, CoreGraphics.CGRect rect)
		{
			window.ResetFrameBuffer();
			GameUpdate(window);
			window.Update();
		}

		public override void TouchesBegan(NSSet touches, UIKit.UIEvent evt)
		{
			base.TouchesBegan(touches, evt);
			UITouch touch = (UITouch)touches.AnyObject;
			if (touch != null)
			{
				window.TouchBegan((float)touch.LocationInView(view).X, (float)touch.LocationInView(view).Y);
			}
		}

		public override void TouchesEnded(NSSet touches, UIKit.UIEvent evt)
		{
			base.TouchesEnded(touches, evt);
			UITouch touch = (UITouch)touches.AnyObject;
			if (touch != null)
			{
				window.TouchEnded((float)touch.LocationInView(view).X, (float)touch.LocationInView(view).Y);
			}
		}

		public override void TouchesMoved(NSSet touches, UIKit.UIEvent evt)
		{
			base.TouchesMoved(touches, evt);
			UITouch touch = (UITouch)touches.AnyObject;
			if (touch != null)
			{
				window.TouchMoved((float)touch.LocationInView(view).X, (float)touch.LocationInView(view).Y);
			}
		}
	}
}
