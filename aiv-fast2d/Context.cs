using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Input;
using System.Collections.Generic;

namespace Aiv.Fast2D
{
	public static class Context
	{

		public static float orthographicSize;

		public static Window currentWindow;

		public static Camera mainCamera;

		public static List<Vector2> Resolutions {
			get {
				List<Vector2> resolutions = new List<Vector2> ();
				foreach (DisplayResolution resolution in DisplayDevice.Default.AvailableResolutions) {
					resolutions.Add (new Vector2 (resolution.Width, resolution.Height));
				}
				return resolutions;
			}
		}



		public static string GetError ()
		{
			return GL.GetError ().ToString ();
		}

		public static List<int> textureGC = new List<int> ();
		public static List<int> bufferGC = new List<int> ();
		public static List<int> vaoGC = new List<int> ();
		public static List<int> shaderGC = new List<int> ();

	}


}

