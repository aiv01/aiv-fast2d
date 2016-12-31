using System;
using System.Diagnostics;

using Foundation;
using GLKit;
using OpenGLES;

using Aiv.Fast2D.iOS;

namespace Aiv.Fast2D.Example.iOS
{
	[Register("GameViewController")]
	public class GameViewController : MobileGame
	{
		Sprite sprite;

		[Export("initWithCoder:")]
		public GameViewController(NSCoder coder) : base(coder)
		{
		}

		protected override void GameSetup(Window window)
		{
			window.SetClearColor(1f, 0, 0, 1f);
			sprite = new Sprite(100, 100);
		}

		protected override void GameUpdate(Window window)
		{
			sprite.position = window.TouchPosition;

			sprite.DrawSolidColor(0f, 1f, 1f, 1f);
		}
	}
}