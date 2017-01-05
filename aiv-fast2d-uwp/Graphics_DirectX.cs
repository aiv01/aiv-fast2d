using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using Matrix4 = SharpDX.Matrix;
using SharpDX.D3DCompiler;

namespace Aiv.Fast2D
{
    public static class Graphics
    {
        private static int internalCounter;

        private static int GetNextId()
        {
            return internalCounter++;
        }

        private static DeviceContext3 currentContext;
        private static RenderTargetView currentTargetView;
        private static Color currentClearColor;

        private static SharpDX.Direct3D11.Buffer currentBuffer;

        private static Dictionary<int, SharpDX.Direct3D11.Buffer> buffers;

        private class DirectXShader
        {
            private VertexShader vs;
            private PixelShader ps;
            private InputLayout inputLayout;

            public DirectXShader(VertexShader vs, PixelShader ps, InputLayout layout)
            {
                this.vs = vs;
                this.ps = ps;
                this.inputLayout = layout;
            }

            public void Use()
            {
                currentContext.VertexShader.Set(vs);
                currentContext.PixelShader.Set(ps);
                currentContext.InputAssembler.InputLayout = inputLayout;

                for (int i = 0; i < currentArray.Buffers.Count; i++)
                {
                    currentContext.InputAssembler.SetVertexBuffers(i, currentArray.Buffers[i]);
                }

                currentContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            }
        }

        private class DirectXArray
        {
            private List<VertexBufferBinding> buffers;

            public DirectXArray()
            {
                buffers = new List<VertexBufferBinding>();
            }

            public void SetBuffer(int index, VertexBufferBinding buffer)
            {
                buffers.Insert(index, buffer);
            }

            public List<VertexBufferBinding> Buffers
            {
                get
                {
                    return buffers;
                }
            }
        }

        private static DirectXArray currentArray;

        private static Dictionary<int, DirectXShader> shaders;

        private static Dictionary<int, DirectXArray> arrays;

        static Graphics()
        {
            buffers = new Dictionary<int, SharpDX.Direct3D11.Buffer>();
            shaders = new Dictionary<int, DirectXShader>();
            arrays = new Dictionary<int, DirectXArray>();

            internalCounter = 0;
        }

        public static void SetContext(Window window)
        {
            currentContext = window.GetDeviceContext();
            currentTargetView = window.GetRenderTargetView();
            currentClearColor = Color.Black;
            currentContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
        }

        public static void BindFrameBuffer(int frameBufferId)
        {
        }

        public static string GetError()
        {
            return "";
        }

        public static void SetAlphaBlending()
        {
        }

        public static void SetMaskedBlending()
        {
        }

        public static void Setup()
        {
            SetAlphaBlending();
        }

        public static void ClearColor()
        {
            currentContext.ClearRenderTargetView(currentTargetView, currentClearColor);
        }

        public static void DeleteBuffer(int id)
        {
        }

        public static void DeleteTexture(int id)
        {
        }

        public static void DeleteShader(int id)
        {
        }

        public static void DeleteArray(int id)
        {
        }

        public static void Viewport(int x, int y, int width, int height)
        {
            currentContext.Rasterizer.SetViewport(x, y, width, height);
        }

        public static void EnableScissorTest()
        {
        }

        public static void DisableScissorTest()
        {
        }

        public static void Scissor(int x, int y, int width, int height)
        {
        }

        public static void SetClearColor(float r, float g, float b, float a)
        {
            currentClearColor = new Color(r, g, b, a);
        }

        public static int GetDefaultFrameBuffer()
        {
            return -1;
        }

        public static void BindTextureToUnit(int textureId, int unit)
        {
        }

        public static int NewBuffer()
        {
            float[] data = new float[]
            {
                0, 1,
                -1, -1,
                1, 1
            };
            SharpDX.Direct3D11.Buffer buffer = SharpDX.Direct3D11.Buffer.Create(currentContext.Device, BindFlags.VertexBuffer, data);
            int id = GetNextId();
            buffers[id] = buffer;
            return id;
        }

        public static int NewArray()
        {
            DirectXArray array = new DirectXArray();
            int id = GetNextId();
            arrays[id] = array;
            return id;
        }


        public static void DrawArrays(int amount)
        {
            currentContext.Draw(amount, 0);
        }

        public static void MapBufferToArray(int bufferId, int index, int elementSize)
        {
            BindBuffer(bufferId);
            currentArray.SetBuffer(index, new VertexBufferBinding(currentBuffer, elementSize * sizeof(float), 0));
        }

        public static void BufferData(float[] data)
        {
            //currentContext.UpdateSubresource<float>(data, currentBuffer);
        }

        public static void BufferData(int bufferId, float[] data)
        {
            BindBuffer(bufferId);
            BufferData(data);
        }


        public static void BufferSubData(float[] data, int offset = 0)
        {
        }

        public static void BufferSubData(int bufferId, float[] data, int offset = 0)
        {
        }

        public static string Version
        {
            get
            {
                return "";
            }
        }

        public static string Vendor
        {
            get
            {
                return "";
            }
        }

        public static string SLVersion
        {
            get
            {
                return "";
            }
        }

        public static string Renderer
        {
            get
            {
                return "";
            }
        }

        public static string Extensions
        {
            get
            {
                return "";
            }
        }

        public static void SetArrayDivisor(int id, int divisor)
        {
        }

        public static void DrawArraysInstanced(int amount, int instances)
        {
            currentContext.DrawInstanced(amount, instances, 0, 0);
        }

        public static int NewFrameBuffer()
        {
            return -1;
        }


        public static void FrameBufferTexture(int id)
        {
        }

        public static void BindArray(int id)
        {
            currentArray = arrays[id];
        }

        public static void BindBuffer(int id)
        {
            currentBuffer = buffers[id];
        }

        public static int NewTexture()
        {
            return -1;
        }

        public static void TextureBitmap(int width, int height, byte[] bitmap, int mipMap = 0)
        {
        }

        public static void TextureSetRepeatX(bool repeat = true)
        {

        }

        public static void TextureSetRepeatY(bool repeat = true)
        {

        }

        public static void TextureSetLinear(bool mipMap = false)
        {

        }

        public static void TextureSetNearest(bool mipMap = false)
        {

        }

        public static int CompileShader(string vertexModern, string fragmentModern, string vertexObsolete = null, string fragmentObsolete = null, string[] attribs = null, int[] attribsSizes = null)
        {

            VertexShader vertexShader;
            PixelShader fragmentShader;
            InputLayout inputLayout;

            using (var vertexShaderByteCode = ShaderBytecode.Compile(vertexModern, "main", "vs_4_0", ShaderFlags.Debug))
            {
                vertexShader = new VertexShader(currentContext.Device, vertexShaderByteCode);
                List<InputElement> elements = new List<InputElement>();
                for (int i = 0; i < attribs.Length; i++)
                {
                    SharpDX.DXGI.Format format = SharpDX.DXGI.Format.R32G32_Float;
                    if (attribsSizes[i] == 3)
                        format = SharpDX.DXGI.Format.R32G32B32_Float;
                    else if (attribsSizes[i] == 4)
                        format = SharpDX.DXGI.Format.R32G32B32A32_Float;
                    elements.Add(new InputElement(attribs[i].ToUpper(), 0, format, i));
                }
                inputLayout = new InputLayout(currentContext.Device, vertexShaderByteCode, elements.ToArray());
            }

            using (var fragmentShaderByteCode = ShaderBytecode.Compile(fragmentModern, "main", "ps_4_0", ShaderFlags.Debug))
            {
                fragmentShader = new PixelShader(currentContext.Device, fragmentShaderByteCode);
            }

            int id = GetNextId();

            shaders[id] = new DirectXShader(vertexShader, fragmentShader, inputLayout);

            return id;
        }

        public static void BindShader(int shaderId)
        {
            shaders[shaderId].Use();
        }

        public static int GetShaderUniformId(int shaderId, string name)
        {
            return -1;
        }

        public static void SetShaderUniform(int uid, int value)
        {
        }

        public static void SetShaderUniform(int uid, float value)
        {
        }

        public static void SetShaderUniform(int uid, Vector4 value)
        {
        }

        public static void SetShaderUniform(int uid, Matrix4 value)
        {
        }
    }
}
