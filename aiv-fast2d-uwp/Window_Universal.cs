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

        private D3D11.Device4 device;
        private D3D11.DeviceContext3 deviceContext;
        private SwapChain4 swapChain;

        private SwapChainPanel context;

        private D3D11.RenderTargetView renderTargetView;

        private Aiv.Fast2D.UWP.IGame game;

        public D3D11.DeviceContext3 GetDeviceContext()
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
                this.device = defaultDevice.QueryInterface<D3D11.Device4>();
            }

            // Save the context instance
            this.deviceContext = this.device.ImmediateContext3;

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
            using (Device4 dxgiDevice4 = this.device.QueryInterface<Device4>())
            {
                // Get the DXGI factory automatically created when initializing the Direct3D device.
                using (Factory5 dxgiFactory5 = dxgiDevice4.Adapter.GetParent<Factory5>())
                {
                    // Create the swap chain and get the highest version available.
                    using (SwapChain1 swapChain1 = new SwapChain1(dxgiFactory5, this.device, ref swapChainDescription))
                    {
                        this.swapChain = swapChain1.QueryInterface<SwapChain4>();
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

            CompositionTarget.Rendering += this.Update;
            this.game.GameSetup(this);
        }




        public void Update(object sender, object e)
        {

            // Set the active back buffer and clear it.
            this.deviceContext.OutputMerger.SetRenderTargets(this.renderTargetView);
            this.ClearColor();

            this.game.GameUpdate(this);

            // apply postprocessing (if required)
            ApplyPostProcessingEffects();

            // Tell the swap chain to present the buffer.
            this.swapChain.Present(1, PresentFlags.None, new PresentParameters());
         
            // cleanup graphics resources
            RunGraphicsGC();
        }



        public static byte[] LoadImage(string fileName, bool premultiplied, out int width, out int height)
        {
            width = 0;
            height = 0;
            return null;
        }
    }
}