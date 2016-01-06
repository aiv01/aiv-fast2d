using System;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Aiv.Fast2D
{
	public static class Context
	{
		public static Window currentWindow;

		public static string GetError ()
		{
			return GL.GetError ().ToString ();
		}

		public static List<int> textureGC = new List<int>();
		public static List<int> bufferGC = new List<int>();
		public static List<int> vaoGC = new List<int>();
		public static List<int> shaderGC = new List<int>();
	}
}

