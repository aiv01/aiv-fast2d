using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Aiv.Fast2D
{

	using Vector2 = OpenTK.Vector2;

	public static class Graphics
	{
		public static void SetContext(Window window)
		{
			window.Context.MakeCurrent();
		}

		public static void BindFrameBuffer(int frameBufferId)
		{
			if (Window.IsObsolete)
			{
				GL.Ext.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferId);
				return;
			}

			GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferId);
		}

		public static string GetError()
		{
			return GL.GetError().ToString();
		}

		public static void SetAlphaBlending()
		{
			// enable alpha blending
			GL.Enable(EnableCap.Blend);
			GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
			GL.BlendFuncSeparate(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
			GL.ColorMask(true, true, true, true);
		}

		public static void SetMaskedBlending()
		{
			// enable alpha blending
			GL.Enable(EnableCap.Blend);
			GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
			GL.BlendFuncSeparate(BlendingFactorSrc.DstAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.OneMinusSrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			GL.ColorMask(true, true, true, true);
		}

		public static void Setup()
		{
			SetAlphaBlending();

			GL.Disable(EnableCap.CullFace);
			GL.Disable(EnableCap.ScissorTest);
			GL.Disable(EnableCap.Multisample);
			GL.Disable(EnableCap.DepthTest);
		}

		private static bool depthTestEnabled;

		public static void EnableDepthTest()
		{
			GL.Enable(EnableCap.DepthTest);
			depthTestEnabled = true;
		}

		public static void DisableDepthTest()
		{
			GL.Disable(EnableCap.DepthTest);
			depthTestEnabled = false;
		}

        public static void TextureGetPixels(int mipMap, byte[] data)
        {
            
            GL.GetTexImage<byte>(TextureTarget.Texture2D, mipMap, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }

        public static void DepthTextureGetPixels(int id, int mipMap, float[] data)
        {

            GL.GetTexImage<float>(TextureTarget.Texture2D, mipMap, PixelFormat.Rgba, PixelType.Float, data);
        }

		public static void CullBackFaces()
		{
			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);
		}

		public static void CullFrontFaces()
		{
			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Front);
		}

		public static void EnableMultisampling()
		{
			GL.Enable(EnableCap.Multisample);
		}

		public static void CullFacesDisable()
		{
			GL.Disable(EnableCap.CullFace);
		}

		public static void ClearColor()
		{
			if (!depthTestEnabled)
			{
				GL.Clear(ClearBufferMask.ColorBufferBit);
			}
			else
			{
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			}
		}

		public static void DeleteBuffer(int id)
		{
			GL.DeleteBuffer(id);
		}

		public static void DeleteTexture(int id)
		{
			GL.DeleteTexture(id);
		}

		public static void DeleteShader(int id)
		{
			GL.DeleteProgram(id);
		}

		public static void DeleteArray(int id)
		{
			GL.DeleteVertexArray(id);
		}

		public static void Viewport(int x, int y, int width, int height)
		{
			GL.Viewport(x, y, width, height);
		}

		public static void EnableScissorTest()
		{
			GL.Enable(EnableCap.ScissorTest);
		}

		public static void DisableScissorTest()
		{
			GL.Disable(EnableCap.ScissorTest);
		}

        public static void Scissor(int x, int y, int width, int height)
		{
			GL.Scissor(x, y, width, height);
		}

		public static void SetClearColor(float r, float g, float b, float a)
		{
			GL.ClearColor(r, g, b, a);
		}

		public static int GetDefaultFrameBuffer()
		{
			int id = 0;
			// iOS does not use default framebuffer 0
			GL.GetInteger(GetPName.FramebufferBinding, out id);
			return id;
		}

		public static void BindTextureToUnit(int textureId, int unit)
		{
			GL.ActiveTexture(TextureUnit.Texture0 + unit);
			GL.BindTexture(TextureTarget.Texture2D, textureId);
		}

		public static int NewBuffer()
		{
			return GL.GenBuffer();
		}

		public static int NewArray()
		{
			int vao = -1;
			// use VAO on modern OpenGL
			if (!Window.IsObsolete)
			{
				vao = GL.GenVertexArray();
			}
			return vao;
		}


		public static void DrawArrays(int amount)
		{
			GL.DrawArrays(PrimitiveType.Triangles, 0, amount);
		}

		public static void MapBufferToArray(int bufferId, int index, int elementSize)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
			GL.EnableVertexAttribArray(index);
			GL.VertexAttribPointer(index, elementSize, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
		}

        public static void MapBufferToIntArray(int bufferId, int index, int elementSize)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
            GL.EnableVertexAttribArray(index);
            GL.VertexAttribIPointer(index, elementSize, VertexAttribIntegerType.Int, 0, IntPtr.Zero);
        }

        public static void BufferData(float[] data)
		{
			GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * sizeof(float)), data, BufferUsageHint.DynamicDraw);
		}

        public static void BufferData(int[] data)
        {
            GL.BufferData<int>(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * sizeof(int)), data, BufferUsageHint.DynamicDraw);
        }

        public static void BufferData(int bufferId, float[] data)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
			BufferData(data);
		}

        public static void BufferData(int bufferId, int[] data)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
            BufferData(data);
        }

        public static void BufferSubData(float[] data, int offset = 0)
		{
			GL.BufferSubData<float>(BufferTarget.ArrayBuffer, (IntPtr)(offset * sizeof(float)), data.Length * sizeof(float), data);
		}

        public static void BufferSubData(int[] data, int offset = 0)
        {
            GL.BufferSubData<int>(BufferTarget.ArrayBuffer, (IntPtr)(offset * sizeof(int)), data.Length * sizeof(int), data);
        }

        public static void BufferSubData(int bufferId, float[] data, int offset = 0)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
			BufferSubData(data, offset);
		}

        public static void BufferSubData(int bufferId, int[] data, int offset = 0)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
            BufferSubData(data, offset);
        }

        public static string Version
		{
			get
			{
				return GL.GetString(StringName.Version);
			}
		}

		public static string Vendor
		{
			get
			{
				return GL.GetString(StringName.Vendor);
			}
		}

		public static string SLVersion
		{
			get
			{
				return GL.GetString(StringName.ShadingLanguageVersion);
			}
		}

		public static string Renderer
		{
			get
			{
				return GL.GetString(StringName.Renderer);
			}
		}

		public static string Extensions
		{
			get
			{
				return GL.GetString(StringName.Extensions);
			}
		}

		public static void SetArrayDivisor(int id, int divisor)
		{
			GL.VertexAttribDivisor(id, divisor);
		}

		public static void DrawArraysInstanced(int amount, int instances)
		{
			GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, amount, instances);
		}

		public static int NewFrameBuffer()
		{
			return GL.GenFramebuffer();
		}


		public static void FrameBufferTexture(int id)
		{
			if (Window.IsObsolete)
			{
				GL.Ext.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, id, 0);
			}
			else
			{
				GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, id, 0);
			}
		}

		public static void FrameBufferDepthTexture(int id)
		{
			if (Window.IsObsolete)
			{
				GL.Ext.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, id, 0);
			}
			else
			{
				GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, id, 0);
			}
		}

		public static void FrameBufferDisableDraw()
		{
			GL.DrawBuffer(DrawBufferMode.None);
			GL.ReadBuffer(ReadBufferMode.None);
		}

		public static void BindArray(int id)
		{
			GL.BindVertexArray(id);
		}

		public static void BindBuffer(int id)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, id);
		}

		public static int NewTexture()
		{
			return GL.GenTexture();
		}

		public static void TextureBitmap(int width, int height, byte[] bitmap, int mipMap = 0)
		{
			GL.TexImage2D<byte>(TextureTarget.Texture2D, mipMap, PixelInternalFormat.Rgba8, width / (int)Math.Pow(2, mipMap), height / (int)Math.Pow(2, mipMap), 0, PixelFormat.Rgba, PixelType.UnsignedByte, bitmap);
		}

		public static void DepthTexture(int width, int height, int depthSize = 16)
		{
			PixelInternalFormat format = PixelInternalFormat.DepthComponent16;
			if (depthSize == 24)
			{
				format = PixelInternalFormat.DepthComponent24;
			}
			else if (depthSize == 32)
			{
				format = PixelInternalFormat.DepthComponent32;
			}
			GL.TexImage2D(TextureTarget.Texture2D, 0, format, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
		}

		public static void TextureSetRepeatX(bool repeat = true)
		{
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, repeat ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
		}

		public static void TextureSetRepeatY(bool repeat = true)
		{
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, repeat ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
		}

		public static void TextureClampToBorderX(float r, float g, float b, float a = 1)
		{
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, new float[] { r, g, b, a });
		}

		public static void TextureClampToBorderY(float r, float g, float b, float a = 1)
		{
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, new float[] { r, g, b, a });
		}

		public static void TextureSetLinear(bool mipMap = false)
		{
			if (mipMap)
			{
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
			}
			else
			{
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			}
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
		}

		public static void TextureSetNearest(bool mipMap = false)
		{
			if (mipMap)
			{
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
			}
			else
			{
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			}
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
		}

		public static int CompileShader(string vertexModern, string fragmentModern, string vertexObsolete = null, string fragmentObsolete = null, string[] attribs = null, int[] attribsSizes = null, string[] vertexUniforms = null, string[] fragmentUniforms = null)
		{
			string vertex = vertexModern;
			string fragment = fragmentModern;

			if (Window.IsObsolete)
			{
				if (vertexObsolete == null || fragmentObsolete == null || attribs == null)
				{
					throw new Shader.UnsupportedVersionException("Unsupported OpenGL version for this shader");
				}
				vertex = vertexObsolete;
				fragment = fragmentObsolete;

				// obsolete Desktop OpenGL does not have medium precision
				fragment = fragment.Replace("precision mediump float;", "");
			}

			int vertexShaderId = GL.CreateShader(ShaderType.VertexShader);
			int fragmentShaderId = GL.CreateShader(ShaderType.FragmentShader);

			GL.ShaderSource(vertexShaderId, vertex);
			GL.CompileShader(vertexShaderId);

			int shaderSuccess = 0;

			GL.GetShader(vertexShaderId, ShaderParameter.CompileStatus, out shaderSuccess);

			if (shaderSuccess == 0)
			{

				string vertexShaderCompilationError = GL.GetShaderInfoLog(vertexShaderId);
				if (vertexShaderCompilationError != null && vertexShaderCompilationError != "")
				{
					throw new Shader.CompilationException(vertexShaderCompilationError);
				}
			}

			GL.ShaderSource(fragmentShaderId, fragment);
			GL.CompileShader(fragmentShaderId);

			GL.GetShader(vertexShaderId, ShaderParameter.CompileStatus, out shaderSuccess);

			if (shaderSuccess == 0)
			{
				string fragmentShaderCompilationError = GL.GetShaderInfoLog(fragmentShaderId);
				if (fragmentShaderCompilationError != null && fragmentShaderCompilationError != "")
				{
					throw new Shader.CompilationException(fragmentShaderCompilationError);
				}
			}

			int programId = GL.CreateProgram();
			GL.AttachShader(programId, vertexShaderId);
			GL.AttachShader(programId, fragmentShaderId);

			if (Window.IsObsolete)
			{
				if (attribs != null)
				{
					for (int i = 0; i < attribs.Length; i++)
					{
						GL.BindAttribLocation(programId, i, attribs[i]);
					}
				}
			}

			GL.LinkProgram(programId);

			GL.DetachShader(programId, vertexShaderId);
			GL.DetachShader(programId, fragmentShaderId);

			GL.DeleteShader(vertexShaderId);
			GL.DeleteShader(fragmentShaderId);

			return programId;
		}

		public static void BindShader(int shaderId)
		{
			GL.UseProgram(shaderId);
		}

		public static int GetShaderUniformId(int shaderId, string name)
		{
			return GL.GetUniformLocation(shaderId, name);
		}

		public static void SetShaderUniform(int uid, int value)
		{
			GL.Uniform1(uid, value);
		}

		public static void SetShaderUniform(int uid, float value)
		{
			GL.Uniform1(uid, value);
		}

		public static void SetShaderUniform(int uid, Vector4 value)
		{
			GL.Uniform4(uid, value);
		}

		public static void SetShaderUniform(int uid, Vector3 value)
		{
			GL.Uniform3(uid, value);
		}

		public static void SetShaderUniform(int uid, Matrix4 value)
		{
			GL.UniformMatrix4(uid, false, ref value);
		}
	}
}
