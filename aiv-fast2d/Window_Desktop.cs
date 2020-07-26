﻿using System;
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

        internal static byte[] LoadImage(string fileName, bool premultiplied, out int width, out int height)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            Stream imageStream = null;

            // if the file in included in the resources, load it as stream
            if (assembly.GetManifestResourceNames().Contains<string>(fileName))
            {
                imageStream = assembly.GetManifestResourceStream(fileName);
            }

            if (imageStream == null)
            {
                imageStream = new FileStream(fileName, FileMode.Open);
            }

            return LoadImage(imageStream, premultiplied, out width, out height);

        }
        internal static byte[] LoadImage(Stream imageStream, bool premultiplied, out int width, out int height)
        {
            {
                byte[] bitmap = null;
                Bitmap image = null;


                image = new Bitmap(imageStream);

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
                        _image = image.Clone(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
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


                imageStream.Close();

                return bitmap;
            }
        }


        internal GameWindow Context { get; set; }
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

            this.Context.Closed += new EventHandler<EventArgs>(this.CloseHandler);
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


        public void Update()
        {
            

            // apply postprocessing (if required)
            ApplyPostProcessingEffects();

            // redraw
            this.Context.SwapBuffers();

            // cleanup graphics resources
            RunGraphicsGC();

            // update input
            this._keyboardState = Keyboard.GetState();
            this._mouseState = Mouse.GetCursorState();

            // get next events
            this.Context.ProcessEvents();

            this.DeltaTime = (float)this.timer.Elapsed.TotalSeconds;
            this.timer.Restart();

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
                return ((float)this.Context.PointToClient(p).X / this.scaleX - this.CurrentViewportPosition.X) / (this.CurrentViewportSize.X / this.OrthoWidth);
            }
        }

        public float mouseY
        {
            get
            {
                Point p = new Point(this._mouseState.X, this._mouseState.Y);
                return ((float)this.Context.PointToClient(p).Y / this.scaleY - this.CurrentViewportPosition.Y) / (this.CurrentViewportSize.Y / this.OrthoHeight);
            }
        }

        public Vector2 mousePosition
        {
            get
            {
                return new Vector2(mouseX, mouseY);
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

        public float MouseWheel
        {
            get
            {
                return this._mouseState.WheelPrecise;
            }
        }

        public bool MouseButton1
        {
            get
            {
                return this._mouseState.IsButtonDown(MouseButton.Button1);
            }
        }

        public bool MouseButton2
        {
            get
            {
                return this._mouseState.IsButtonDown(MouseButton.Button2);
            }
        }

        public bool MouseButton3
        {
            get
            {
                return this._mouseState.IsButtonDown(MouseButton.Button3);
            }
        }

        public bool MouseButton4
        {
            get
            {
                return this._mouseState.IsButtonDown(MouseButton.Button4);
            }
        }

        public bool MouseButton5
        {
            get
            {
                return this._mouseState.IsButtonDown(MouseButton.Button5);
            }
        }

        public bool MouseButton6
        {
            get
            {
                return this._mouseState.IsButtonDown(MouseButton.Button6);
            }
        }

        public bool MouseButton7
        {
            get
            {
                return this._mouseState.IsButtonDown(MouseButton.Button7);
            }
        }

        public bool MouseButton8
        {
            get
            {
                return this._mouseState.IsButtonDown(MouseButton.Button8);
            }
        }

        public bool MouseButton9
        {
            get
            {
                return this._mouseState.IsButtonDown(MouseButton.Button9);
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

        private void CloseHandler(object sender, EventArgs args)
        {
            this.opened = false;
        }

    }
}