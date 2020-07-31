using System;
using System.Collections.Generic;
using OpenTK;
using System.Reflection;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenTK.Input;
using System.Runtime.CompilerServices;

namespace Aiv.Fast2D
{
    public partial class Window
    {
        /// <summary>
        /// Return the available displays for this computer.
        /// </summary>
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

        /// <summary>
        /// Return the available resolutions for this computer.
        /// </summary>
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

        /// <summary>
        /// Return the available joysticks for this computer.
        /// </summary>
        public static string[] Joysticks {
            get {
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

        public GameWindow Context { get; set; }
        private readonly Stopwatch timer;


        /// <summary>
        /// Create a window using primary device width and height.
        /// </summary>
        /// <param name="title">the title for this window</param>
        /// <param name="depthSize">number of bit for the depth buffer (e.g. 16bit, 32bit, 64bit). Default is 16</param>
        /// <param name="antialiasingSamples">number of samples for antialiasing. Default is 0</param>
        /// <param name="stencilSize">number of bit for the stencil buffer. Default is 0</param>
        public Window(string title, int depthSize = 16, int antialiasingSamples = 0, int stencilSize = 0) 
            : this(DisplayDevice.Default.Width, DisplayDevice.Default.Height, title, true, depthSize, antialiasingSamples, stencilSize)
        { }

        /// <summary>
        /// Create a window with a specific width and height
        /// </summary>
        /// <param name="width">the width of the window</param>
        /// <param name="height">the height of the window</param>
        /// <param name="title">the title for this window</param>
        /// <param name="fullScreen">if window has to be in full screen mode. Default is false.</param>
        /// <param name="depthSize">number of bit for the depth buffer (e.g. 16bit, 32bit, 64bit). Default is 16</param>
        /// <param name="antialiasingSamples">number of samples for antialiasing. Default is 0</param>
        /// <param name="stencilSize">number of bit for the stencil buffer. Default is 0</param>
        public Window(int width, int height, string title, 
            bool fullScreen = false, int depthSize = 16, int antialiasingSamples = 0, int stencilSize = 0)
        {
            // force opengl 3.3 this is a good compromise
            int major = 3;
            int minor = 3;
            if (obsoleteMode)
            {
                major = 2;
                minor = 2;
            }
            this.Context = new GameWindow(
                                width, height,
                                new OpenTK.Graphics.GraphicsMode(32, depthSize, stencilSize, antialiasingSamples),
                                title,
                                fullScreen ? GameWindowFlags.Fullscreen : GameWindowFlags.FixedWindow,
                                DisplayDevice.Default,
                                major, minor,
                                OpenTK.Graphics.GraphicsContextFlags.Default);

            if (fullScreen)
            {
                this.Context.Location = new Point(0, 0);
            }

            timer = new Stopwatch();

            // enable vsync by default
            this.SetVSync(true);

            FixDimensions(width, height, true);

            this.Context.Closed += CloseHandler;
            this.Context.Move += (sender, e) =>
            {
                // avoid deltaTime to became huge while moving the window
                this.timer.Stop();
            };
            this.Context.Visible = true;

            // initialize graphics subsystem
            Graphics.Setup();

            if (antialiasingSamples > 0)
                Graphics.EnableMultisampling();

            FinalizeSetup();

            // hack for getting the default framebuffer
            ResetFrameBuffer();

            IsOpened = true;
        }

        /// <summary>
        /// Set resolution for this window.
        /// </summary>
        /// <param name="screenWidth"></param>
        /// <param name="screenHeight"></param>
        /// <returns></returns>
        public bool SetResolution(int screenWidth, int screenHeight)
        {
            return SetResolution(new Vector2(screenWidth, screenHeight));
        }

        /// <summary>
        /// Set resolution for this window.
        /// </summary>
        /// <param name="newResolution">a vector2 representing x = width and y = height</param>
        /// <returns></returns>
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

        /// <summary>
        /// Get / Set window position
        /// </summary>
        public Vector2 Position
        {
            get
            {
                Point location = this.Context.Location;
                return new Vector2(location.X, location.Y);
            }

            set
            {
                this.Context.Location = new Point((int)value.X, (int)value.Y);
            }
        }


        /// <summary>
        /// Enable or Disable Vertical Sync
        /// </summary>
        /// <param name="enabled"><c>true</c> to enable VSync, <c>false</c> otherwise</param>
        public void SetVSync(bool enable)
        {
            if (enable)
            {
                this.Context.VSync = VSyncMode.On;
            }
            else
            {
                this.Context.VSync = VSyncMode.Off;
            }
        }

        /// <summary>
        /// Check if this window has focus.
        /// </summary>
        public bool HasFocus
        {
            get
            {
                return this.Context.Focused;
            }
        }

        /// <summary>
		/// Sets Window's Icon
		/// </summary>
		/// <param name="path">path to the icon. Should be an .ico file. Path could be either a filesystem path or a resource path</param>
        public void SetIcon(string path)
        {
            Assembly assembly = Assembly.GetEntryAssembly();

            // if the file in included in the resources, load it as stream
            if (assembly.GetManifestResourceNames().Contains<string>(path))
            {
                Stream iconStream = assembly.GetManifestResourceStream(path);
                this.Context.Icon = new Icon(iconStream);
            }
            else
            {
                this.Context.Icon = new Icon(path);
            }
        }

        /// <summary>
        /// Change Full screen mode for this window
        /// </summary>
        /// <param name="enabled"><c>true</c> for full screen, <c>false</c> otherwise</param>
        public void SetFullScreen(bool enable)
        {
            this.Context.WindowState = enable ? WindowState.Fullscreen : WindowState.Normal;
            this.FixDimensions(Width, Height);
        }

        /// <summary>
        /// Change size for the this window
        /// </summary>
        /// <param name="width">the new width of the window</param>
        /// <param name="height">the new height of the window</param>
        public void SetSize(int width, int height)
        {
            this.Context.WindowState = WindowState.Normal;
            this.FixDimensions(width, height);
        }


        /// <summary>
		/// Sets mouse cursor visibility
		/// </summary>
		/// <param name="enabled"><c>true</c> to show mouse, <c>false</c> otherwise</param>
        public void SetMouseVisible(bool enable)
        {
            this.Context.CursorVisible = enable;
        }

        /// <summary>
        /// Set the title for the window
        /// </summary>
        public void SetTitle(string text) 
        {
              this.Context.Title = text;
        }

        /// <summary>
        /// Present the rendered scene to the user, update events and delta time since last update.
        /// </summary>
        public void Update()
        {
            // apply postprocessing (if required)
            ApplyPostProcessingEffects();

            // redraw
            this.Context.SwapBuffers();

            // cleanup graphics resources
            RunGraphicsGC();

            // update input
            this.keyboardState = Keyboard.GetState();
            this.mouseState = Mouse.GetCursorState();

            // get next events
            this.Context.ProcessEvents();

            this.DeltaTime = (float)this.timer.Elapsed.TotalSeconds;
            this.timer.Restart();

            // reset and clear
            ResetFrameBuffer();
        }

        /// <summary>
        /// Close the window
        /// </summary>
        public void Close()
        {
            if (!IsOpened) return;

            Context.Close();
            Context.Dispose();
            IsOpened = false;
        }

        /// <summary>
        /// Close the window and kill the application
        /// </summary>
        /// <param name="code">the exit code. Default is 0.</param>
        public void Exit(int code = 0)
        {
            Close();
            System.Environment.Exit(code);
        }


        #region input

        private KeyboardState keyboardState;
        private MouseState mouseState;

        /// <summary>
		/// Returns mouse X position relative to the window
		/// </summary>
        public float MouseX
        {
            get
            {
                Point p = new Point(this.mouseState.X, this.mouseState.Y);
                return ((float)this.Context.PointToClient(p).X / this.scaleX - this.CurrentViewportPosition.X) / (this.CurrentViewportSize.X / this.OrthoWidth);
            }
        }

        /// <summary>
        /// Returns mouse Y position relative to the window
        /// </summary>
        public float MouseY
        {
            get
            {
                Point p = new Point(this.mouseState.X, this.mouseState.Y);
                return ((float)this.Context.PointToClient(p).Y / this.scaleY - this.CurrentViewportPosition.Y) / (this.CurrentViewportSize.Y / this.OrthoHeight);
            }
        }

        /// <summary>
        /// Returns mouse position relative to the window as a vector2
        /// </summary>
        public Vector2 MousePosition
        {
            get
            {
                return new Vector2(MouseX, MouseY);
            }
        }

        public float RawMouseX
        {
            get
            {
                return OpenTK.Input.Mouse.GetState().X;
            }
        }

        public float RawMouseY
        {
            get
            {
                return OpenTK.Input.Mouse.GetState().Y;
            }
        }

        public Vector2 RawMousePosition
        {
            get
            {
                return new Vector2(RawMouseX, RawMouseY);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if mouse left button is pressed, otherwise <c>false</c>
        /// </summary>
        public bool MouseLeft
        {
            get
            {
                return this.mouseState.IsButtonDown(MouseButton.Left);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if mouse right button is pressed, otherwise <c>false</c>
        /// </summary>
        public bool MouseRight
        {
            get
            {
                return this.mouseState.IsButtonDown(MouseButton.Right);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if mouse middle button is pressed, otherwise <c>false</c>
        /// </summary>
        public bool MouseMiddle
        {
            get
            {
                return this.mouseState.IsButtonDown(MouseButton.Middle);
            }
        }

        /// <summary>
        /// Returns mouse wheel position
        /// </summary>
        public float MouseWheel
        {
            get
            {
                return this.mouseState.WheelPrecise;
            }
        }

        public bool MouseButton1
        {
            get
            {
                return this.mouseState.IsButtonDown(MouseButton.Button1);
            }
        }

        public bool MouseButton2
        {
            get
            {
                return this.mouseState.IsButtonDown(MouseButton.Button2);
            }
        }

        public bool MouseButton3
        {
            get
            {
                return this.mouseState.IsButtonDown(MouseButton.Button3);
            }
        }

        public bool MouseButton4
        {
            get
            {
                return this.mouseState.IsButtonDown(MouseButton.Button4);
            }
        }

        public bool MouseButton5
        {
            get
            {
                return this.mouseState.IsButtonDown(MouseButton.Button5);
            }
        }

        public bool MouseButton6
        {
            get
            {
                return this.mouseState.IsButtonDown(MouseButton.Button6);
            }
        }

        public bool MouseButton7
        {
            get
            {
                return this.mouseState.IsButtonDown(MouseButton.Button7);
            }
        }

        public bool MouseButton8
        {
            get
            {
                return this.mouseState.IsButtonDown(MouseButton.Button8);
            }
        }

        public bool MouseButton9
        {
            get
            {
                return this.mouseState.IsButtonDown(MouseButton.Button9);
            }
        }

        /// <summary>
		/// Returns true when <c>key</c> is pressed
		/// </summary>
		/// <param name="key">key to check if a <see cref="KeyCode"/> is pressed</param>
        public bool GetKey(KeyCode key)
        {
            return this.keyboardState.IsKeyDown((Key)key);
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

        private void CloseHandler(object sender, EventArgs args)
        {
            IsOpened = false;
        }

        private void FixDimensions(int width, int height, bool first = false)
        {
            this.Width = width;
            this.Height = height;

            if (!first)
            {
                this.Context.Width = (int)(this.Width * this.scaleX);
                this.Context.Height = (int)(this.Height * this.scaleY);
            }
            this.scaleX = (float)this.Context.Width / (float)this.Width;
            this.scaleY = (float)this.Context.Height / (float)this.Height;

            // setup viewport
            this.SetViewport(0, 0, width, height);

            // required for updating context !
            this.Context.Context.Update(this.Context.WindowInfo);
            this.SetClearColor(0f, 0f, 0f, 1f);
            this.ClearColor();
            this.Context.SwapBuffers();
        }


    }
}