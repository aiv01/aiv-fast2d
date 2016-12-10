using System;
using OpenTK;
#if !__MOBILE__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES30;
using OpenTK.Platform.Android;
using Android.Views;
using Android.Content;
using Android.OS;
using Android.Content.Res;
#endif
using System.Diagnostics;
using OpenTK.Input;
#if !__MOBILE__
using System.Drawing;
#endif
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Linq;
using System.IO;

namespace Aiv.Fast2D
{

#if !__MOBILE__
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
#endif

    public class Window
    {
        private Matrix4 orthoMatrix;
        private float _aspectRatio;

        public Matrix4 OrthoMatrix
        {
            get
            {
                return this.orthoMatrix;
            }
        }

        public float aspectRatio
        {
            get
            {
                return this._aspectRatio;
            }
        }

        private int width;
        private int height;

        private Vector2 viewportPosition;
        private Vector2 viewportSize;

        public Vector2 CurrentViewportPosition
        {
            get
            {
                return viewportPosition;
            }
        }

        public Vector2 CurrentViewportSize
        {
            get
            {
                return viewportSize;
            }
        }

        private float currentOrthographicSize;

        public float CurrentOrthoGraphicSize
        {
            get
            {
                return currentOrthographicSize;
            }
        }

        public int Width
        {
            get
            {
                return this.width;
            }
        }

        public int Height
        {
            get
            {
                return this.height;
            }
        }

        public float orthoWidth
        {
            get
            {
                if (this.currentOrthographicSize > 0)
                    return this.currentOrthographicSize * this._aspectRatio;
                return this.width;
            }
        }

        public float orthoHeight
        {
            get
            {
                if (this.currentOrthographicSize > 0)
                    return this.currentOrthographicSize;
                return this.height;
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

        private float defaultOrthographicSize;

        public void SetDefaultOrthographicSize(float value)
        {
            defaultOrthographicSize = value;
            this.SetViewport(0, 0, this.width, this.height);
        }

        private static Window current;

        public static Window Current
        {
            get
            {
                return Window.current;
            }
        }

        public static void SetCurrent(Window targetWindow)
        {
            current = targetWindow;
            // on mobile devices, multiple contexts are not available
#if !__MOBILE__
            current.window.MakeCurrent();
#endif
        }

        public void SetCurrent()
        {
            Window.SetCurrent(this);
        }

#if !__MOBILE__
        private GameWindow window;
        private Stopwatch watch;

        public bool opened = true;

        private KeyboardState _keyboardState;
        private MouseState _mouseState;
#else
        private AndroidGameView window;
#endif

        // used for dpi management
        private float scaleX;
        private float scaleY;

        private float _deltaTime;

        public float deltaTime
        {
            get
            {
                return _deltaTime;
            }
        }

        public delegate void GameLoop(Window window);

        private List<PostProcessingEffect> postProcessingEffects;

        public string GetError()
        {
#if !__MOBILE__
            return GL.GetError ().ToString ();
#else
            return GL.GetErrorCode().ToString();
#endif
        }

        private ILogger logger;

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void Log(string message)
        {
            if (logger == null)
                return;
            logger.Log(message);
        }

        public List<int> textureGC = new List<int>();
        public List<int> bufferGC = new List<int>();
        public List<int> vaoGC = new List<int>();
        public List<int> shaderGC = new List<int>();

        private Camera currentCamera;

        public void SetCamera(Camera camera)
        {
            this.currentCamera = camera;
        }

        public Camera CurrentCamera
        {
            get
            {
                return this.currentCamera;
            }
        }

#if !__MOBILE__
        public Window(string title) : this(DisplayDevice.Default.Width, DisplayDevice.Default.Height, title, true)
        {
        }

        public Window(int width, int height, string title, bool fullScreen = false)
        {

            // force opengl 3.3 this is a good compromise
            this.window = new GameWindow(width, height, OpenTK.Graphics.GraphicsMode.Default, title,
                fullScreen ? GameWindowFlags.Fullscreen : GameWindowFlags.FixedWindow,
                DisplayDevice.Default, 3, 3, OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible);

            if (fullScreen)
            {
                this.window.Location = new Point(0, 0);
            }

            // enable vsync by default
            this.SetVSync(true);

            FixDimensions(width, height, true);

            this.window.Closed += new EventHandler<EventArgs>(this.Close);
            this.window.Visible = true;

            watch = new Stopwatch();

            SetupOpenGL();
#else
        public void FixMobileViewport()
        {
            this.width = window.Holder.SurfaceFrame.Width();
            this.height = window.Holder.SurfaceFrame.Height();

            this.SetViewport(0, 0, this.width, this.height);

            SetupOpenGL();
        }

        private float touchX;
        private float touchY;

        public float TouchX
        {
            get
            {
                return touchX;
            }
        }

        public float TouchY
        {
            get
            {
                return touchY;
            }
        }

        public Vector2 TouchPosition
        {
            get
            {
                return new Vector2(touchX, touchY);
            }
        }

        private bool isTouching;
        public bool IsTouching
        {
            get
            {
                return isTouching;
            }
        }

        public void Vibrate(long amount)
        {
            Vibrator vibrator = (Vibrator)this.window.Context.GetSystemService(global::Android.Content.Context.VibratorService);
            vibrator.Vibrate(amount);
        }

        public void CancelVibration()
        {
            Vibrator vibrator = (Vibrator)this.window.Context.GetSystemService(global::Android.Content.Context.VibratorService);
            vibrator.Cancel();
        }

        private static AssetManager assets;

        public static AssetManager Assets
        {
            get
            {
                return assets;
            }
        }

        public Window(AndroidGameView gameView)
        {
            this.window = gameView;
            // required for accessing assets
            assets = gameView.Context.Assets;
            this.scaleX = 1;
            this.scaleY = 1;
            // on mobile refresh is capped to 60hz
            this._deltaTime = 1f / 60f;

            this.window.Resize += (sender, e) =>
            {
                this.FixMobileViewport();
            };

            this.window.Touch += (sender, e) =>
            {
                switch (e.Event.Action)
                {
                    case MotionEventActions.Move:
                        touchX = e.Event.GetX() - this.viewportPosition.X / (this.viewportSize.X / this.orthoWidth);
                        touchY = e.Event.GetY() - this.viewportPosition.Y / (this.viewportSize.Y / this.orthoHeight);
                        break;
                    case MotionEventActions.Up:
                        isTouching = false;
                        break;
                    case MotionEventActions.Down:
                        isTouching = true;
                        break;
                    default:
                        break;
                }
            };
#endif

            // more gentle GC
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;

            postProcessingEffects = new List<PostProcessingEffect>();

            Window.SetCurrent(this);
        }

        public PostProcessingEffect AddPostProcessingEffect(PostProcessingEffect effect)
        {
            effect.Setup(this);
            postProcessingEffects.Add(effect);
            return effect;
        }

        public void ClearPostProcessingEffects()
        {
            postProcessingEffects.Clear();
        }

        public PostProcessingEffect[] PostProcessingEffects
        {
            get
            {
                return postProcessingEffects.ToArray();
            }
        }

        public PostProcessingEffect SetPostProcessingEffect(int index, PostProcessingEffect effect)
        {
            effect.Setup(this);
            postProcessingEffects.Insert(index, effect);
            return effect;
        }


        public void SetAlphaBlending()
        {
            // enable alpha blending
            GL.Enable(EnableCap.Blend);
            GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
            GL.BlendFuncSeparate(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
            GL.ColorMask(true, true, true, true);
        }

        public void SetMaskedBlending()
        {
            // enable alpha blending
            GL.Enable(EnableCap.Blend);
            GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
            GL.BlendFuncSeparate(BlendingFactorSrc.DstAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.OneMinusSrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.ColorMask(true, true, true, true);
        }

        private void SetupOpenGL()
        {
            SetAlphaBlending();

            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.ScissorTest);

#if !__MOBILE__
            GL.Disable(EnableCap.Multisample);
#endif

            GL.Disable(EnableCap.DepthTest);
        }

#if !__MOBILE__
        public void SetVSync(bool enable)
        {
            if (enable)
            {
                this.window.VSync = VSyncMode.On;
            }
            else
            {
                this.window.VSync = VSyncMode.Off;
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
                return this.window.Focused;
            }
        }

        private void FixDimensions(int width, int height, bool first = false)
        {
            this.width = width;
            this.height = height;

            if (!first)
            {
                this.window.Width = (int)(this.width * this.scaleX);
                this.window.Height = (int)(this.height * this.scaleY);
            }
            this.scaleX = (float)this.window.Width / (float)this.width;
            this.scaleY = (float)this.window.Height / (float)this.height;
           
            // setup viewport
            this.SetViewport(0, 0, width, height);

            // required for updating context !
            this.window.Context.Update(this.window.WindowInfo);
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            this.window.SwapBuffers();
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
            else {
                icon = new Icon(fileName);
            }
            this.window.Icon = icon;
        }

        public void SetFullScreen(bool enable)
        {
            this.window.WindowState = enable ? WindowState.Fullscreen : WindowState.Normal;
            this.FixDimensions(width, height);
        }

        public void SetCursor(bool enable)
        {
            this.window.CursorVisible = enable;
        }
#endif

        public string Title
        {
            get
            {
                return this.window.Title;
            }
            set
            {
                this.window.Title = value;
            }
        }



        public void SetClearColor(float r, float g, float b, float a = 1)
        {
            GL.ClearColor(r, g, b, a);
        }

        public void SetClearColor(int r, int g, int b, int a = 255)
        {
            GL.ClearColor(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        public void ClearColor()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }


#if !__MOBILE__

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
#endif

        public void SetScissorTest(bool enabled)
        {
            if (enabled)
            {
                GL.Enable(EnableCap.ScissorTest);
            }
            else
            {
                GL.Disable(EnableCap.ScissorTest);
            }
        }

        public void SetScissorTest(int x, int y, int width, int height)
        {
            SetScissorTest(true);
            GL.Scissor(x, (this.height - y) - height, width, height);
        }

        public void SetScissorTest(float x, float y, float width, float height)
        {
            SetScissorTest((int)x, (int)y, (int)width, (int)height);
        }

        private PostProcessingEffect GetFirstPostProcessingEffect()
        {
            foreach (PostProcessingEffect effect in postProcessingEffects)
            {
                if (effect != null && effect.enabled)
                    return effect;
            }
            return null;
        }

        private int GetDefaultFrameBuffer()
        {
            foreach (PostProcessingEffect effect in postProcessingEffects)
            {
                if (effect != null && effect.enabled)
                    return effect.RenderTexture.FrameBuffer;
            }
            return 0;
        }

        public int ActivePostProcessingEffectsCount
        {
            get
            {
                int i = 0;
                foreach (PostProcessingEffect effect in postProcessingEffects)
                {
                    if (effect != null && effect.enabled)
                        i++;
                }
                return i;
            }
        }

        // 0 means, render to the real screen
        private int GetNextPostProcessingEffectFramebuffer(int current)
        {
            for (int i = current + 1; i < postProcessingEffects.Count; i++)
            {
                if (postProcessingEffects[i] != null && postProcessingEffects[i].enabled)
                {
                    return postProcessingEffects[i].RenderTexture.FrameBuffer;
                }

            }
            return 0;
        }

        private void ApplyPostProcessingEffects()
        {

            for (int i = 0; i < postProcessingEffects.Count; i++)
            {
                if (postProcessingEffects[i] != null && postProcessingEffects[i].enabled)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, GetNextPostProcessingEffectFramebuffer(i));
                    GL.Clear(ClearBufferMask.ColorBufferBit);
                    // custom update cycle
                    postProcessingEffects[i].Update(this);
                    // blit to the next render destination
                    postProcessingEffects[i].Apply(this);

                }
            }

        }

        public void BindTextureToUnit(Texture texture, int unit)
        {
            BindTextureToUnit(texture.Id, unit);
        }

        public void BindTextureToUnit(int textureId, int unit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + unit);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
        }

        public void Update()
        {

            // apply postprocessing (if required)
            ApplyPostProcessingEffects();

            // redraw
            this.window.SwapBuffers();

            // destroy useless resources
            // use for for avoiding "changing while iterating
            for (int i = 0; i < this.bufferGC.Count; i++)
            {
                int _id = this.bufferGC[i];
#if !__MOBILE__
                GL.DeleteBuffer(_id);
#else
                GL.DeleteBuffers(1, new int[] { _id });
#endif
                this.Log(string.Format("buffer {0} deleted", _id));
            }
            this.bufferGC.Clear();

            for (int i = 0; i < this.vaoGC.Count; i++)
            {
                int _id = this.vaoGC[i];
#if !__MOBILE__
                GL.DeleteVertexArray(_id);
#else
                GL.DeleteVertexArrays(1, new int[] { _id });
#endif
                this.Log(string.Format("vertexArray {0} deleted", _id));
            }
            this.vaoGC.Clear();

            for (int i = 0; i < this.textureGC.Count; i++)
            {
                int _id = this.textureGC[i];

                GL.DeleteTexture(_id);
                this.Log(string.Format("texture {0} deleted", _id));
            }
            this.textureGC.Clear();

            for (int i = 0; i < this.shaderGC.Count; i++)
            {
                int _id = this.shaderGC[i];

                GL.DeleteProgram(_id);
                this.Log(string.Format("shader {0} deleted", _id));
            }
            this.shaderGC.Clear();

#if !__MOBILE__
            this._keyboardState = Keyboard.GetState();
            this._mouseState = Mouse.GetCursorState();

            // get next events
            this.window.ProcessEvents();




            // avoid negative values
            this._deltaTime = this.watch.Elapsed.TotalSeconds > 0 ? (float)this.watch.Elapsed.TotalSeconds : 0f;

            this.watch.Reset();
            this.watch.Start();
#endif

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, GetDefaultFrameBuffer());

            GL.Clear(ClearBufferMask.ColorBufferBit);

        }



#if !__MOBILE__
        public float mouseX
        {
            get
            {
                Point p = new Point(this._mouseState.X, this._mouseState.Y);
                return ((float)this.window.PointToClient(p).X / this.scaleX - this.viewportPosition.X)/(this.viewportSize.X/this.orthoWidth);
            }
        }

        public float mouseY
        {
            get
            {
                Point p = new Point(this._mouseState.X, this._mouseState.Y);
                return ((float)this.window.PointToClient(p).Y / this.scaleY - this.viewportPosition.Y)/(this.viewportSize.Y/this.orthoHeight);
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

        public Vector2 JoystickAxisLeft(int index)
        {
            return GamePad.GetState(index).ThumbSticks.Left;
        }

        public Vector2 JoystickAxisRight(int index)
        {
            return GamePad.GetState(index).ThumbSticks.Right;
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
#endif

        public void SetViewport(int x, int y, int width, int height, float orthoSize = 0, bool virtualScreen = false)
        {
            // store values before changes
            this.viewportPosition = new Vector2(x, y);
            this.viewportSize = new Vector2(width, height);
            // fix y as it is downsided in OpenGL
            y = (this.height - y) - height;
            if (virtualScreen)
            {
                GL.Viewport(0,
                0,
                width,
                height);
            }
            else {
                GL.Viewport((int)(x * this.scaleX),
                    (int)(y * this.scaleY),
                    (int)(width * this.scaleX),
                    (int)(height * this.scaleY));
            }

            this._aspectRatio = (float)width / (float)height;

            if (orthoSize == 0)
                orthoSize = this.defaultOrthographicSize;

            // use units instead of pixels ?
            if (orthoSize > 0)
            {
                this.orthoMatrix = Matrix4.CreateOrthographicOffCenter(0, orthoSize * this._aspectRatio, orthoSize, 0, -1.0f, 1.0f);
            }
            else {
                this.orthoMatrix = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1.0f, 1.0f);
            }

            this.currentOrthographicSize = orthoSize;

        }

        public void RenderTo(RenderTexture renderTexture, bool clear = true, float orthoSize = 0)
        {
            if (renderTexture == null)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, GetDefaultFrameBuffer());
                SetViewport(0, 0, this.width, this.height);
                return;
            }
            else {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, renderTexture.FrameBuffer);
                // unscaled,virtual viewport
                SetViewport(0, 0, renderTexture.Width, renderTexture.Height, orthoSize, true);
            }

            if (clear)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }
        }
    }
}

