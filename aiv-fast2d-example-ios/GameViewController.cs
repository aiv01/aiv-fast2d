using System;
using System.Diagnostics;

using Foundation;
using GLKit;
using OpenGLES;

using Aiv.Fast2D.iOS;
using OpenTK;

namespace Aiv.Fast2D.Example.iOS
{
	[Register("GameViewController")]
	public class GameViewController : MobileGame
	{
		Sprite sprite;
		Texture alien;

		[Export("initWithCoder:")]
		public GameViewController(NSCoder coder) : base(coder)
		{
		}

		protected override void GameSetup(Window window)
		{
			window.SetClearColor(1f, 0, 0, 1f);
			sprite = new Sprite(100, 100);

			alien = new Texture("2.png");
		}

		protected override void GameUpdate(Window window)
		{
			sprite.scale = Vector2.One;
			sprite.position = window.TouchPosition;

			sprite.DrawSolidColor(0f, 1f, 1f, 1f);

			sprite.scale = new Vector2(2, 2);
			sprite.position = Vector2.Zero;

			sprite.DrawTexture(alien);
		}
	}
}