using System;
using SharpDX;
using SharpDX.Direct3D11;

namespace Aiv.Fast2D
{
	public static class Graphics
	{
        private static DeviceContext3 currentContext;
        private static RenderTargetView currentTargetView;
        private static Color currentClearColor;

		public static void SetContext(Window window)
		{
            currentContext = window.GetDeviceContext();
            currentTargetView = window.GetRenderTargetView();
            currentClearColor = Color.Black;
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
            return -1;
		}

		public static int NewArray()
		{
            return -1;
		}


		public static void DrawArrays(int amount)
		{
		}

		public static void MapBufferToArray(int bufferId, int index, int elementSize)
		{
		}

		public static void BufferData(float[] data)
		{
		}

		public static void BufferData(int bufferId, float[] data)
		{
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
		}

		public static void BindBuffer(int id)
		{
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
	}
}
