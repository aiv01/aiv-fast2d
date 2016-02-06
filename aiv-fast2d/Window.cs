using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using OpenTK.Input;
using System.Drawing;
using System.Collections.Generic;

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

		ShiftRight = Key.ShiftRight,
		ShiftLeft = Key.ShiftLeft,

		Keypad0 = Key.Keypad0,
		Keypad1 = Key.Keypad1,
		Keypad2 = Key.Keypad2,
		Keypad3 = Key.Keypad3,
		Keypad4 = Key.Keypad4,
		Keypad5 = Key.Keypad5,
		Keypad6 = Key.Keypad6,
		Keypad7 = Key.Keypad7,
		Keypad8 = Key.Keypad8,
		Keypad9 = Key.Keypad9,

		Num0 = Key.Number0,
		Num1 = Key.Number1,
		Num2 = Key.Number2,
		Num3 = Key.Number3,
		Num4 = Key.Number4,
		Num5 = Key.Number5,
		Num6 = Key.Number6,
		Num7 = Key.Number7,
		Num8 = Key.Number8,
		Num9 = Key.Number9,

		F1 = Key.F1,
		F2 = Key.F2,
		F3 = Key.F3,
		F4 = Key.F4,
		F5 = Key.F5,
		F6 = Key.F6,
		F7 = Key.F7,
		F8 = Key.F8,
		F9 = Key.F9,
		F10 = Key.F10,

	}

	public class Window
	{
		private Matrix4 orthoMatrix;
		private float _aspectRatio;

		public Matrix4 OrthoMatrix {
			get {
				return this.orthoMatrix;
			}
		}

		public float aspectRatio {
			get {
				return this._aspectRatio;
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

		public float orthoWidth {
			get {
				if (Context.orthographicSize > 0)
					return Context.orthographicSize * 2 * this._aspectRatio;
				return this.Width;
			}
		}

		public float orthoHeight {
			get {
				if (Context.orthographicSize > 0)
					return Context.orthographicSize * 2;
				return this.Height;
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

		public Window (string title) : this (DisplayDevice.Default.Width, DisplayDevice.Default.Height, title, true)
		{
		}

		public Window (int width, int height, string title, bool fullScreen = false)
		{
			
			// force opengl 3.3 this is a good compromise
			this.window = new GameWindow (width, height, OpenTK.Graphics.GraphicsMode.Default, title, 
				fullScreen ? GameWindowFlags.Fullscreen : GameWindowFlags.FixedWindow,
				DisplayDevice.Default, 3, 3, OpenTK.Graphics.GraphicsContextFlags.Default);

			FixDimensions (width, height, true);



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

		private void FixDimensions (int width, int height, bool first = false)
		{
			this.width = width;
			this.height = height;

			if (!first) {
				this.window.Width = (int)(this.width * this.scaleX);
				this.window.Height = (int)(this.height * this.scaleY);
			}
			this.scaleX = (float)this.window.Width / (float)this.width;
			this.scaleY = (float)this.window.Height / (float)this.height;

			this.window.Location = new Point (0, 0);

			this._aspectRatio = (float)width / (float)height;

			// use units instead of pixels ?
			if (Context.orthographicSize > 0) {
				this.orthoMatrix = Matrix4.CreateOrthographic (Context.orthographicSize * 2f * this._aspectRatio, -Context.orthographicSize * 2f, -1, 1);
			} else {
				this.orthoMatrix = Matrix4.CreateOrthographicOffCenter (0, (float)width, (float)height, 0, -1, 1);
			}

			// setup viewport
			this.SetViewport (0, 0, width, height);

			// required for updating context !
			this.window.Context.Update (this.window.WindowInfo);
			GL.Clear (ClearBufferMask.ColorBufferBit);
			this.window.SwapBuffers ();
		}

		public bool SetResolution (Vector2 newResolution)
		{
			foreach (DisplayResolution resolution in DisplayDevice.Default.AvailableResolutions) {
				if (resolution.Width == newResolution.X && resolution.Height == newResolution.Y) {
					DisplayDevice.Default.ChangeResolution (resolution);
					this.FixDimensions (resolution.Width, resolution.Height);
					Console.WriteLine (resolution.Width + " / " + this.window.Width);
					Console.WriteLine (resolution.Height + " / " + this.window.Height);
					return true;
				}
			}
			return false;
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
				//Console.WriteLine ("deleting " + _id);
				GL.DeleteBuffer (_id);
			}
			Context.bufferGC.Clear ();

			for (int i = 0; i < Context.vaoGC.Count; i++) {
				int _id = Context.vaoGC [i];
				//Console.WriteLine ("deleting " + _id);
				GL.DeleteVertexArray (_id);
			}
			Context.vaoGC.Clear ();

			for (int i = 0; i < Context.textureGC.Count; i++) {
				int _id = Context.textureGC [i];
				//Console.WriteLine ("deleting " + _id);
				GL.DeleteTexture (_id);
			}
			Context.textureGC.Clear ();

			for (int i = 0; i < Context.shaderGC.Count; i++) {
				int _id = Context.shaderGC [i];
				//Console.WriteLine ("deleting " + _id);
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

		public string[] Joysticks {
			get {
				string[] joysticks = new string[4];
				for (int i = 0; i < 4; i++) {
					if (GamePad.GetState (i).IsConnected)
						joysticks [i] = GamePad.GetName (i);
					else
						joysticks [i] = null;
				}
				return joysticks;
			}
		}

		public Vector2 JoystickAxisLeft (int index)
		{
			return GamePad.GetState (index).ThumbSticks.Left;
		}

		public Vector2 JoystickAxisRight (int index)
		{
			return GamePad.GetState (index).ThumbSticks.Right;
		}

		public bool JoystickUp (int index)
		{
			return GamePad.GetState (index).DPad.IsUp;
		}

		public bool JoystickDown (int index)
		{
			return GamePad.GetState (index).DPad.IsDown;
		}

		public bool JoystickRight (int index)
		{
			return GamePad.GetState (index).DPad.IsRight;
		}

		public bool JoystickLeft (int index)
		{
			return GamePad.GetState (index).DPad.IsLeft;
		}

		public void JoystickVibrate (int index, float left, float right)
		{
			GamePad.SetVibration (index, left, right);
		}

		public bool JoystickA(int index) {
			return GamePad.GetState (index).Buttons.A == ButtonState.Pressed;
		}

		public bool JoystickB(int index) {
			return GamePad.GetState (index).Buttons.B == ButtonState.Pressed;
		}

		public bool JoystickX(int index) {
			return GamePad.GetState (index).Buttons.X == ButtonState.Pressed;
		}

		public bool JoystickY(int index) {
			return GamePad.GetState (index).Buttons.Y == ButtonState.Pressed;
		}

		public string JoystickDebug (int index)
		{
			return GamePad.GetState (index).ToString();
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

