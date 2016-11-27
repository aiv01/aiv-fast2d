using System;
#if !__MOBILE__
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
#else
using OpenTK.Graphics.ES30;
using OpenTK.Platform.Android;
using Android.Content.Res;
#endif
using OpenTK;
using System.Collections.Generic;

namespace Aiv.Fast2D
{
	public static class Context
	{

		public static float orthographicSize;

        public static Window currentWindow;

        public static Camera mainCamera;

#if __MOBILE__
        public static AssetManager assets;
#endif

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
#if !__MOBILE__
            return GL.GetError ().ToString ();
#else
            return GL.GetErrorCode().ToString();
#endif
        }

		public static List<int> textureGC = new List<int> ();
		public static List<int> bufferGC = new List<int> ();
		public static List<int> vaoGC = new List<int> ();
		public static List<int> shaderGC = new List<int> ();

	}


}

