using System;

namespace Aiv.Fast2D
{
	public class Game
	{
		private Window window;
		private bool requestedExit;

		public Game(string title)
		{
			window = new Window(title);
		}

		public Game(int width, int height, string title, bool fullScreen = false)
		{
			window = new Window(width, height, title, fullScreen);
		}

		public virtual void GameSetup(Window window)
		{

		}

		public virtual void GameUpdate(Window window)
		{

		}

		public void Run()
		{
			this.GameSetup(window);
			while (window.IsOpened && !requestedExit)
			{
				this.GameUpdate(window);
				window.Update();
			}
		}

		public void Exit()
		{
			requestedExit = true;
		}
	}
}
