using System;
using Aiv.Fast2D;
using System.Collections.Generic;

namespace SimpleGame
{
	public class Program
	{

		public static void Main (string[]args)
		{
			Window window = new Window (1024, 576, "Game");

			Texture vim = new Texture ("/Users/roberto/vim.png");

			Sprite ship = new Sprite (vim.Width, vim.Height);

			List<Sprite> foos = new List<Sprite> ();

			foos.Add (new Sprite (vim.Width, vim.Height));
			foos.Add (new Sprite (vim.Width, vim.Height));
			foos.Add (new Sprite (vim.Width, vim.Height));

			while (window.opened) {
				ship.DrawTexture (vim);
				window.Update ();

				if (foos.Count > 0) {

					foos [0].Dispose ();

					foos.RemoveAt (0);

				}
				foos.Add (new Sprite (vim.Width, vim.Height));
				foos.Add (new Sprite (vim.Width, vim.Height));
				foos.Add (new Sprite (vim.Width, vim.Height));

				vim.Dispose ();
				vim = new Texture ("/Users/roberto/vim.png");

				Console.WriteLine (GC.GetTotalMemory (false));
			}
		}
	}
}
