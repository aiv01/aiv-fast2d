using System;
using System.Collections.Generic;
using OpenTK;
using System.Reflection;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenTK.Input;
using System.Runtime.InteropServices;

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

	public partial class Window
	{

		public GameWindow context;

		private Stopwatch watch;

		public static string[] Displays
		{
			get
			{
				List<string> displays = new List<string>();
				for (int i = 0; i <= (int)DisplayIndex.Sixth; i++)
				{
					DisplayDevice device = DisplayDevice.GetDisplay((DisplayIndex)i);
					if (device != null)
						displays.Add(device.ToString());
				}
				return displays.ToArray();
			}
		}

		public static Vector2[] Resolutions
		{
			get
			{
				List<Vector2> resolutions = new List<Vector2>();
				foreach (DisplayResolution resolution in DisplayDevice.Default.AvailableResolutions)
				{
					resolutions.Add(new Vector2(resolution.Width, resolution.Height));
				}
				return resolutions.ToArray();
			}
		}

		public bool SetResolution(int screenWidth, int screenHeight)
		{
			return SetResolution(new Vector2(screenWidth, screenHeight));
		}

		public bool SetResolution(Vector2 newResolution)
		{
			foreach (DisplayResolution resolution in DisplayDevice.Default.AvailableResolutions)
			{
				if (resolution.Width == newResolution.X && resolution.Height == newResolution.Y)
				{
					DisplayDevice.Default.ChangeResolution(resolution);
					this.FixDimensions(resolution.Width, resolution.Height);
					return true;
				}
			}
			return false;
		}

		public Window(string title) : this(DisplayDevice.Default.Width, DisplayDevice.Default.Height, title, true)
		{
		}

		public Window(int width, int height, string title, bool fullScreen = false)
		{
			// force opengl 3.3 this is a good compromise
			int major = 3;
			int minor = 3;
			if (obsoleteMode)
			{
				major = 2;
				minor = 2;
			}
			this.context = new GameWindow(width, height, OpenTK.Graphics.GraphicsMode.Default, title,
				fullScreen ? GameWindowFlags.Fullscreen : GameWindowFlags.FixedWindow,
				DisplayDevice.Default, major, minor, OpenTK.Graphics.GraphicsContextFlags.Default);

			if (fullScreen)
			{
				this.context.Location = new Point(0, 0);
			}

			// enable vsync by default
			this.SetVSync(true);

			FixDimensions(width, height, true);

			this.context.Closed += new EventHandler<EventArgs>(this.Close);
			this.context.Visible = true;

			watch = new Stopwatch();

			// initialize graphics subsystem
			Graphics.Setup();

			FinalizeSetup();
		}


		public void SetVSync(bool enable)
		{
			if (enable)
			{
				this.context.VSync = VSyncMode.On;
			}
			else
			{
				this.context.VSync = VSyncMode.Off;
			}
		}

		private void Close(object sender, EventArgs args)
		{
			this.opened = false;
		}

		public bool HasFocus
		{
			get
			{
				return this.context.Focused;
			}
		}

		private void FixDimensions(int width, int height, bool first = false)
		{
			this.width = width;
			this.height = height;

			if (!first)
			{
				this.context.Width = (int)(this.width * this.scaleX);
				this.context.Height = (int)(this.height * this.scaleY);
			}
			this.scaleX = (float)this.context.Width / (float)this.width;
			this.scaleY = (float)this.context.Height / (float)this.height;

			// setup viewport
			this.SetViewport(0, 0, width, height);

			// required for updating context !
			this.context.Context.Update(this.context.WindowInfo);
			this.SetClearColor(0f, 0f, 0f, 1f);
			this.ClearColor();
			this.context.SwapBuffers();
		}

		public void SetIcon(string fileName)
		{
			Icon icon = null;

			Assembly assembly = Assembly.GetEntryAssembly();

			// if the file in included in the resources, load it as stream
			if (assembly.GetManifestResourceNames().Contains<string>(fileName))
			{
				Stream iconStream = assembly.GetManifestResourceStream(fileName);
				icon = new Icon(iconStream);

			}
			else
			{
				icon = new Icon(fileName);
			}
			this.context.Icon = icon;
		}

		public void SetFullScreen(bool enable)
		{
			this.context.WindowState = enable ? WindowState.Fullscreen : WindowState.Normal;
			this.FixDimensions(width, height);
		}

		public void SetCursor(bool enable)
		{
			this.context.CursorVisible = enable;
		}

		public string Title
		{
			get
			{
				return this.context.Title;
			}
			set
			{
				this.context.Title = value;
			}
		}


		public void Update()
		{

			// apply postprocessing (if required)
			ApplyPostProcessingEffects();

			// redraw
			this.context.SwapBuffers();

			// cleanup graphics resources
			RunGraphicsGC();

			// update input
			this._keyboardState = Keyboard.GetState();
			this._mouseState = Mouse.GetCursorState();

			// get next events
			this.context.ProcessEvents();

			// avoid negative values
			this._deltaTime = this.watch.Elapsed.TotalSeconds > 0 ? (float)this.watch.Elapsed.TotalSeconds : 0f;

			this.watch.Reset();
			this.watch.Start();

			// reset and clear
			ResetFrameBuffer();
		}

		#region input

		private KeyboardState _keyboardState;
		private MouseState _mouseState;

		public float mouseX
		{
			get
			{
				Point p = new Point(this._mouseState.X, this._mouseState.Y);
				return ((float)this.context.PointToClient(p).X / this.scaleX - this.viewportPosition.X) / (this.viewportSize.X / this.OrthoWidth);
			}
		}

		public float mouseY
		{
			get
			{
				Point p = new Point(this._mouseState.X, this._mouseState.Y);
				return ((float)this.context.PointToClient(p).Y / this.scaleY - this.viewportPosition.Y) / (this.viewportSize.Y / this.OrthoHeight);
			}
		}

		public Vector2 mousePosition
		{
			get
			{
				return new Vector2(mouseX, mouseY);
			}
		}

		public bool mouseLeft
		{
			get
			{
				return this._mouseState.IsButtonDown(MouseButton.Left);
			}
		}

		public bool mouseRight
		{
			get
			{
				return this._mouseState.IsButtonDown(MouseButton.Right);
			}
		}

		public bool mouseMiddle
		{
			get
			{
				return this._mouseState.IsButtonDown(MouseButton.Middle);
			}
		}


		public bool GetKey(KeyCode key)
		{
			return this._keyboardState.IsKeyDown((Key)key);
		}

        public string[] Joysticks
		{
			get
			{
				string[] joysticks = new string[4];
				for (int i = 0; i < 4; i++)
				{
					if (GamePad.GetState(i).IsConnected)
						joysticks[i] = GamePad.GetName(i);
					else
						joysticks[i] = null;
				}
				return joysticks;
			}
		}

		public Vector2 JoystickAxisLeftRaw(int index)
		{
			return GamePad.GetState(index).ThumbSticks.Left;
		}

		public Vector2 JoystickAxisRightRaw(int index)
		{
			return GamePad.GetState(index).ThumbSticks.Right;
		}

		private Vector2 SanitizeJoystickVector(Vector2 axis, float threshold)
		{
			if (Math.Abs(axis.X) < threshold)
				axis.X = 0;
			if (Math.Abs(axis.X) > 1f - threshold)
				axis.X = Math.Sign(axis.X);
			if (Math.Abs(axis.Y) < threshold)
				axis.Y = 0;
			if (Math.Abs(axis.Y) > 1f - threshold)
				axis.Y = Math.Sign(axis.Y);
			axis.Y *= -1;
			return axis;
		}

		public Vector2 JoystickAxisLeft(int index, float threshold = 0.1f)
		{
			Vector2 axis = GamePad.GetState(index).ThumbSticks.Left;
			return SanitizeJoystickVector(axis, threshold);
		}

		public Vector2 JoystickAxisRight(int index, float threshold = 0.1f)
		{
			Vector2 axis = GamePad.GetState(index).ThumbSticks.Right;
			return SanitizeJoystickVector(axis, threshold);
		}

		public float JoystickTriggerLeftRaw(int index)
		{
			return GamePad.GetState(index).Triggers.Left;
		}

		public float JoystickTriggerRightRaw(int index)
		{
			return GamePad.GetState(index).Triggers.Right;
		}

		private float SanitizeJoystickTrigger(float value, float threshold)
		{
			if (value < threshold)
				return 0;
			if (value > 1f - threshold)
				return 1f;
			return value;
		}

		public float JoystickTriggerLeft(int index, float threshold = 0.1f)
		{
			float trigger = GamePad.GetState(index).Triggers.Left;
			return SanitizeJoystickTrigger(trigger, threshold);
		}

		public float JoystickTriggerRight(int index, float threshold = 0.1f)
		{
			float trigger = GamePad.GetState(index).Triggers.Right;
			return SanitizeJoystickTrigger(trigger, threshold);
		}

		public bool JoystickShoulderLeft(int index)
		{
			return GamePad.GetState(index).Buttons.LeftShoulder == ButtonState.Pressed;
		}

		public bool JoystickShoulderRight(int index)
		{
			return GamePad.GetState(index).Buttons.RightShoulder == ButtonState.Pressed;
		}

		public bool JoystickLeftStick(int index)
		{
			return GamePad.GetState(index).Buttons.LeftStick == ButtonState.Pressed;
		}

		public bool JoystickRightStick(int index)
		{
			return GamePad.GetState(index).Buttons.RightStick == ButtonState.Pressed;
		}

		public bool JoystickUp(int index)
		{
			return GamePad.GetState(index).DPad.IsUp;
		}

		public bool JoystickDown(int index)
		{
			return GamePad.GetState(index).DPad.IsDown;
		}

		public bool JoystickRight(int index)
		{
			return GamePad.GetState(index).DPad.IsRight;
		}

		public bool JoystickLeft(int index)
		{
			return GamePad.GetState(index).DPad.IsLeft;
		}

		public void JoystickVibrate(int index, float left, float right)
		{
			GamePad.SetVibration(index, left, right);
		}

		public bool JoystickA(int index)
		{
			return GamePad.GetState(index).Buttons.A == ButtonState.Pressed;
		}

		public bool JoystickB(int index)
		{
			return GamePad.GetState(index).Buttons.B == ButtonState.Pressed;
		}

		public bool JoystickX(int index)
		{
			return GamePad.GetState(index).Buttons.X == ButtonState.Pressed;
		}

		public bool JoystickBack(int index)
		{
			return GamePad.GetState(index).Buttons.Back == ButtonState.Pressed;
		}

		public bool JoystickStart(int index)
		{
			return GamePad.GetState(index).Buttons.Start == ButtonState.Pressed;
		}

		public bool JoystickBigButton(int index)
		{
			return GamePad.GetState(index).Buttons.BigButton == ButtonState.Pressed;
		}

		public bool JoystickY(int index)
		{
			return GamePad.GetState(index).Buttons.Y == ButtonState.Pressed;
		}

		public string JoystickDebug(int index)
		{
			return GamePad.GetState(index).ToString();
		}

		#endregion

		public static byte[] LoadImage(string fileName, bool premultiplied, out int width, out int height)
		{
			byte[] bitmap = null;
			Stream imageStream = null;
			Bitmap image = null;

			Assembly assembly = Assembly.GetEntryAssembly();

			// if the file in included in the resources, load it as stream
			if (assembly.GetManifestResourceNames().Contains<string>(fileName))
			{
				imageStream = assembly.GetManifestResourceStream(fileName);
			}

			if (imageStream != null)
			{
				image = new Bitmap(imageStream);
			}
			else {
				image = new Bitmap(fileName);
			}

			using (image)
			{
				// to avoid losing a ref
				Bitmap _image = image;
				bitmap = new byte[image.Width * image.Height * 4];
				width = image.Width;
				height = image.Height;
				// if the image is not rgba, let's fix it
				if (image.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
				{
					Bitmap tmpBitmap = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					using (System.Drawing.Graphics gfx = System.Drawing.Graphics.FromImage(tmpBitmap))
					{
						gfx.DrawImageUnscaled(image, 0, 0);
					}
					_image = tmpBitmap;
				}

				System.Drawing.Imaging.BitmapData data = _image.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				Marshal.Copy(data.Scan0, bitmap, 0, bitmap.Length);
				_image.UnlockBits(data);

				for (int y = 0; y < _image.Height; y++)
				{
					for (int x = 0; x < _image.Width; x++)
					{
						int position = (y * _image.Width * 4) + (x * 4);
						// bgra -> rgba
						byte b = bitmap[position];
						byte r = bitmap[position + 2];
						bitmap[position] = r;
						bitmap[position + 2] = b;
						// premultiply
						if (premultiplied)
						{
							byte a = bitmap[position + 3];
							bitmap[position] = (byte)(bitmap[position] * (a / 255f));
							bitmap[position + 1] = (byte)(bitmap[position + 1] * (a / 255f));
							bitmap[position + 2] = (byte)(bitmap[position + 2] * (a / 255f));
						}
					}
				}
			}
			return bitmap;
		}
	}
}