using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using SharpDX;
using D3D11 = SharpDX.Direct3D11;
using D3D = SharpDX.Direct3D;
using SharpDX.DXGI;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Gaming.Input;

namespace Aiv.Fast2D
{
    public enum KeyCode
    {
        /*
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
        */
    }

    public partial class Window
    {

        private Stopwatch watch;

        private D3D11.Device2 device;
        private D3D11.DeviceContext2 deviceContext;
        private SwapChain2 swapChain;

        private SwapChainPanel context;

        private D3D11.RenderTargetView renderTargetView;

        private Aiv.Fast2D.UWP.IGame game;

        public D3D11.DeviceContext2 GetDeviceContext()
        {
            return this.deviceContext;
        }
        public D3D11.RenderTargetView GetRenderTargetView()
        {
            return this.renderTargetView;
        }

        public Window(SwapChainPanel panel, Aiv.Fast2D.UWP.IGame game)
        {

            this.context = panel;
            this.game = game;

            using (D3D11.Device defaultDevice = new D3D11.Device(D3D.DriverType.Hardware, D3D11.DeviceCreationFlags.Debug))
            {
                this.device = defaultDevice.QueryInterface<D3D11.Device2>();
            }

            // Save the context instance
            this.deviceContext = this.device.ImmediateContext2;

            // Properties of the swap chain
            SwapChainDescription1 swapChainDescription = new SwapChainDescription1()
            {
                // No transparency.
                AlphaMode = AlphaMode.Ignore,
                // Double buffer.
                BufferCount = 2,
                // BGRA 32bit pixel format.
                Format = Format.R8G8B8A8_UNorm,
                // Unlike in CoreWindow swap chains, the dimensions must be set.
                Height = (int)(this.context.RenderSize.Height),
                Width = (int)(this.context.RenderSize.Width),
                // Default multisampling.
                SampleDescription = new SampleDescription(1, 0),
                // In case the control is resized, stretch the swap chain accordingly.
                Scaling = Scaling.Stretch,
                // No support for stereo display.
                Stereo = false,
                // Sequential displaying for double buffering.
                SwapEffect = SwapEffect.FlipSequential,
                // This swapchain is going to be used as the back buffer.
                Usage = Usage.BackBuffer | Usage.RenderTargetOutput,
            };

            // Retrive the DXGI device associated to the Direct3D device.
            using (Device3 dxgiDevice3 = this.device.QueryInterface<Device3>())
            {
                // Get the DXGI factory automatically created when initializing the Direct3D device.
                using (Factory3 dxgiFactory3 = dxgiDevice3.Adapter.GetParent<Factory5>())
                {
                    // Create the swap chain and get the highest version available.
                    using (SwapChain1 swapChain1 = new SwapChain1(dxgiFactory3, this.device, ref swapChainDescription))
                    {
                        this.swapChain = swapChain1.QueryInterface<SwapChain2>();
                    }
                }
            }

            // Obtain a reference to the native COM object of the SwapChainPanel.
            using (ISwapChainPanelNative nativeObject = ComObject.As<ISwapChainPanelNative>(this.context))
            {
                // Set its swap chain.
                nativeObject.SwapChain = this.swapChain;
            }

            // Create a Texture2D from the existing swap chain to use as 
            D3D11.Texture2D backBufferTexture = D3D11.Texture2D.FromSwapChain<D3D11.Texture2D>(this.swapChain, 0);
            this.renderTargetView = new D3D11.RenderTargetView(this.device, backBufferTexture);

            FinalizeSetup();

            width = (int)this.context.RenderSize.Width;
            height = (int)this.context.RenderSize.Height;

            scaleX = 1;
            scaleY = 1;

            this.SetViewport(0, 0, width, height);

            // for now disable only backface culling
            D3D11.RasterizerStateDescription rasterizerDescription = D3D11.RasterizerStateDescription.Default();
            rasterizerDescription.CullMode = D3D11.CullMode.None;
            this.deviceContext.Rasterizer.State = new SharpDX.Direct3D11.RasterizerState(this.device, rasterizerDescription);

            vsync = 1;

            CompositionTarget.Rendering += this.Update;
            this.game.GameSetup(this);

            watch = new Stopwatch();
        }


        public void SetFullScreen(bool enable)
        {
            this.swapChain.SetFullscreenState(enable, null);
        }

        private int vsync;

        public void SetVSync(bool enable)
        {
            if (enable)
            {
                vsync = 1;
            }
            else
            {
                vsync = 0;
            }
        }


        public void Update(object sender, object e)
        {
            Debug.WriteLine(this.deltaTime);

            // Set the active back buffer and clear it.
            this.deviceContext.OutputMerger.SetRenderTargets(this.renderTargetView);
            this.ClearColor();

            this.game.GameUpdate(this);

            // apply postprocessing (if required)
            ApplyPostProcessingEffects();

            // Tell the swap chain to present the buffer.
            this.swapChain.Present(vsync, PresentFlags.None);

            // cleanup graphics resources
            RunGraphicsGC();

            // avoid negative values
            this._deltaTime = this.watch.Elapsed.TotalSeconds > 0 ? (float)this.watch.Elapsed.TotalSeconds : 0f;

            this.watch.Reset();
            this.watch.Start();
        }

        #region input
        public string[] Joysticks
        {
            get
            {
                string[] joysticks = new string[4];
                for (int i = 0; i < 4; i++)
                {
                    try
                    {
                        joysticks[i] = Gamepad.Gamepads[i].ToString();
                    }
                    catch
                    {
                        joysticks[i] = null;
                    }
                }
                return joysticks;
            }
        }

        private GamepadReading JoystickGetState(int index)
        {
            try
            {
                return Gamepad.Gamepads[index].GetCurrentReading();
            }
            catch
            {
                return new GamepadReading();
            }
        }

        public Vector2 JoystickAxisLeftRaw(int index)
        {
            var state = JoystickGetState(index);
            return new Vector2((float)state.LeftThumbstickX, (float)state.LeftThumbstickY);
        }

        public Vector2 JoystickAxisRightRaw(int index)
        {
            var state = JoystickGetState(index);
            return new Vector2((float)state.RightThumbstickX, (float)state.RightThumbstickY);
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
            Vector2 axis = JoystickAxisLeftRaw(index);
            return SanitizeJoystickVector(axis, threshold);
        }

        public Vector2 JoystickAxisRight(int index, float threshold = 0.1f)
        {
            Vector2 axis = JoystickAxisRightRaw(index);
            return SanitizeJoystickVector(axis, threshold);
        }


        public float JoystickTriggerLeftRaw(int index)
        {
            var state = JoystickGetState(index);
            return (float)state.LeftTrigger;
        }

        public float JoystickTriggerRightRaw(int index)
        {
            var state = JoystickGetState(index);
            return (float)state.RightTrigger;
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
            float trigger = JoystickTriggerLeftRaw(index);
            return SanitizeJoystickTrigger(trigger, threshold);
        }

        public float JoystickTriggerRight(int index, float threshold = 0.1f)
        {
            float trigger = JoystickTriggerRightRaw(index);
            return SanitizeJoystickTrigger(trigger, threshold);
        }

        public bool JoystickShoulderLeft(int index)
        {
            var state = JoystickGetState(index);
            return (state.Buttons & GamepadButtons.LeftShoulder) != 0;
        }

        public bool JoystickShoulderRight(int index)
        {
            var state = JoystickGetState(index);
            return (state.Buttons & GamepadButtons.RightShoulder) != 0;
        }

        public bool JoystickLeftStick(int index)
        {
            var state = JoystickGetState(index);
            return (state.Buttons & GamepadButtons.LeftThumbstick) != 0;
        }

        public bool JoystickRightStick(int index)
        {
            var state = JoystickGetState(index);
            return (state.Buttons & GamepadButtons.RightThumbstick) != 0;
        }

        public bool JoystickUp(int index)
        {
            var state = JoystickGetState(index);
            return (state.Buttons & GamepadButtons.DPadUp) != 0;
        }

        public bool JoystickDown(int index)
        {
            var state = JoystickGetState(index);
            return (state.Buttons & GamepadButtons.DPadDown) != 0;
        }

        public bool JoystickRight(int index)
        {
            var state = JoystickGetState(index);
            return (state.Buttons & GamepadButtons.DPadRight) != 0;
        }

        public bool JoystickLeft(int index)
        {
            var state = JoystickGetState(index);
            return (state.Buttons & GamepadButtons.DPadLeft) != 0;
        }

        public void JoystickVibrate(int index, float left, float right)
        {
            try
            {
                GamepadVibration vibration = new GamepadVibration();
                vibration.LeftMotor = (double)left;
                vibration.RightMotor = (double)right;
                Gamepad.Gamepads[index].Vibration = vibration;
            }
            catch { }

        }

        public bool JoystickA(int index)
        {
            var state = JoystickGetState(index);
            return (state.Buttons & GamepadButtons.A) != 0;
        }

        public bool JoystickB(int index)
        {
            var state = JoystickGetState(index);
            return (state.Buttons & GamepadButtons.B) != 0;
        }

        public bool JoystickX(int index)
        {
            var state = JoystickGetState(index);
            return (state.Buttons & GamepadButtons.X) != 0;
        }

        public bool JoystickBack(int index)
        {
            var state = JoystickGetState(index);
            return (state.Buttons & GamepadButtons.View) != 0;
        }

        public bool JoystickStart(int index)
        {
            var state = JoystickGetState(index);
            return (state.Buttons & GamepadButtons.Menu) != 0;
        }

        public bool JoystickY(int index)
        {
            var state = JoystickGetState(index);
            return (state.Buttons & GamepadButtons.Y) != 0;
        }

        public string JoystickDebug(int index)
        {
            var state = JoystickGetState(index);
            return state.ToString();
        }
        #endregion

        public static byte[] LoadImage(string fileName, bool premultiplied, out int width, out int height)
        {
            width = 0;
            height = 0;
            return null;
        }
    }
}