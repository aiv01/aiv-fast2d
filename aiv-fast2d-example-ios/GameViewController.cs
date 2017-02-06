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
		RenderTexture fakeScreenTexture;
		Sprite fakeScreen;

		[Export("initWithCoder:")]
		public GameViewController(NSCoder coder) : base(coder)
		{
		}

		protected override void GameSetup(Window window)
		{
			window.SetClearColor(1f, 0, 0, 1f);
			sprite = new Sprite(100, 100);

			alien = new Texture("2.png");

			fakeScreenTexture = new RenderTexture(window.Width, window.Height);
			fakeScreen = new Sprite(window.Width, window.Height);
		}

		protected override void GameUpdate(Window window)
		{

			window.RenderTo(fakeScreenTexture);

			sprite.scale = Vector2.One;
			sprite.position = window.TouchPosition;

			sprite.DrawSolidColor(0f, 1f, 1f, 1f);

			sprite.scale = new Vector2(2, 2);
			sprite.position = Vector2.Zero;

			sprite.DrawTexture(alien);

			window.RenderTo(null);

			fakeScreen.scale = Vector2.One;
			fakeScreen.position = Vector2.Zero;
			fakeScreen.DrawTexture(fakeScreenTexture);

			fakeScreen.scale = new Vector2(0.25f, 0.25f);
			fakeScreen.position = new Vector2(window.Width - fakeScreenTexture.Width * 0.25f, window.Height - fakeScreenTexture.Height * 0.25f);
			fakeScreen.DrawTexture(fakeScreenTexture);

		}
	}
}