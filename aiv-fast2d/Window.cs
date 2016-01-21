using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using OpenTK.Input;
using System.Drawing;

namespace Aiv.Fast2D
{

	public enum KeyCode
	{
		A = Key.A,
		B = Key.B,
		C = Key.C,
		D = Key.D,
		E = Key.E,
		F = Key.F,
		G = Key.G,
		H = Key.H,
		I = Key.I,
		J = Key.J,
		K = Key.K,
		L = Key.L,
		M = Key.M,
		N = Key.N,
		O = Key.O,
		P = Key.P,
		Q = Key.Q,
		R = Key.R,
		S = Key.S,
		T = Key.T,
		U = Key.U,
		V = Key.V,
		W = Key.W,
		X = Key.X,
		Y = Key.Y,
		Z = Key.Z,

		Space = Key.Space,
		Return = Key.Enter,
		Esc = Key.Escape,

		Up = Key.Up,
		Down = Key.Down,
		Left = Key.Left,
		Right = Key.Right,
	}

	public class Window
	{
		private Matrix4 orthoMatrix;
		private float aspectRatio;

		public Matrix4 OrthoMatrix {
			get {
				return this.orthoMatrix;
			}
		}

		private int width;
		private int height;

		public int Width {
			get {
				return this.width;
			}
		}

		public int Height {
			get {
				return this.height;
			}
		}

		private GameWindow window;

		private Stopwatch watch;

		public bool opened = true;

		private KeyboardState _keyboardState;
		private MouseState _mouseState;



		// used for dpi management
		private float scaleX;
		private float scaleY;

		private float _deltaTime;

		public float deltaTime {
			get {
				return _deltaTime;
			}
		}

		public Window (int width, int height, string title, bool fullScreen = false)
		{
			this.aspectRatio = (float)width / (float)height;

			// TODO support centered matrix for unit-based development
			//this.orthoMatrix = Matrix4.CreateOrthographic ((float)width, (float)height, -1, 1);

			this.orthoMatrix = Matrix4.CreateOrthographicOffCenter (0, (float)width, (float)height, 0, -1, 1);

			this.width = width;
			this.height = height;


			// force opengl 3.3 this is a good compromise
			this.window = new GameWindow (width, height, OpenTK.Graphics.GraphicsMode.Default, title, 
				fullScreen ? GameWindowFlags.Fullscreen : GameWindowFlags.FixedWindow,
				DisplayDevice.Default, 3, 3, OpenTK.Graphics.GraphicsContextFlags.Default);

			this.scaleX = (float)this.window.Width / (float)this.width;
			this.scaleY = (float)this.window.Height / (float)this.height;

			// setup viewport
			this.SetViewport (0, 0, width, height);
			// required for updating context !
			this.window.Context.Update (this.window.WindowInfo);
			GL.Clear (ClearBufferMask.ColorBufferBit);
			this.window.SwapBuffers ();

			this.window.Closed += new EventHandler<EventArgs> (this.Close);
			this.window.Visible = true;

			watch = new Stopwatch ();


			// enable alpha blending
			GL.Enable (EnableCap.Blend);
			GL.BlendFunc (BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			Context.currentWindow = this;

		}

		private void Close (object sender, EventArgs args)
		{
			this.opened = false;
		}

		public void Update ()
		{
			if (!this.watch.IsRunning)
				this.watch.Start ();


			this._keyboardState = Keyboard.GetState ();
			this._mouseState = Mouse.GetCursorState ();


			// redraw
			this.window.SwapBuffers ();

			// get next events
			this.window.ProcessEvents ();

			this._deltaTime = (float)this.watch.Elapsed.TotalSeconds;


			this.watch.Reset ();
			this.watch.Start ();
		
			// destroy useless resources
			// use for for avoiding "changing while iterating
			for (int i = 0; i < Context.bufferGC.Count; i++) {
				int _id = Context.bufferGC [i];
				Console.WriteLine ("deleting " + _id);
				GL.DeleteBuffer (_id);
			}
			Context.bufferGC.Clear ();

			for (int i = 0; i < Context.vaoGC.Count; i++) {
				int _id = Context.vaoGC [i];
				Console.WriteLine ("deleting " + _id);
				GL.DeleteVertexArray (_id);
			}
			Context.vaoGC.Clear ();

			for (int i = 0; i < Context.textureGC.Count; i++) {
				int _id = Context.textureGC [i];
				Console.WriteLine ("deleting " + _id);
				GL.DeleteTexture (_id);
			}
			Context.textureGC.Clear ();

			for (int i = 0; i < Context.shaderGC.Count; i++) {
				int _id = Context.shaderGC [i];
				Console.WriteLine ("deleting " + _id);
				GL.DeleteProgram (_id);
			}
			Context.shaderGC.Clear ();

			GL.Clear (ClearBufferMask.ColorBufferBit);

		}

		public int mouseX {
			get {
				Point p = new Point (this._mouseState.X, this._mouseState.Y);
				return (int)((float)this.window.PointToClient (p).X / this.scaleX);
			}
		}

		public int mouseY {
			get {
				Point p = new Point (this._mouseState.X, this._mouseState.Y);
				return (int)((float)this.window.PointToClient (p).Y / this.scaleY);
			}
		}

		public bool mouseLeft {
			get {
				return this._mouseState.IsButtonDown (MouseButton.Left);
			}
		}

		public bool mouseRight {
			get {
				return this._mouseState.IsButtonDown (MouseButton.Right);
			}
		}

		public bool mouseMiddle {
			get {
				return this._mouseState.IsButtonDown (MouseButton.Middle);
			}
		}

		public bool GetKey (KeyCode key)
		{
			return this._keyboardState.IsKeyDown ((Key)key);
		}

		public void SetViewport (int x, int y, int width, int height)
		{
			GL.Viewport ((int)(x * this.scaleX),
				(int)(y * this.scaleY),
				(int)(width * this.scaleX),
				(int)(height * this.scaleY));
		}
			
	}
}

