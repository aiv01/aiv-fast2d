using System;
using OpenTK.Graphics.OpenGL;

namespace Aiv.Fast2D
{
	public static class Context
	{
		public static Window currentWindow;

		public static string GetError ()
		{
			return GL.GetError ().ToString ();
		}


	}
}

