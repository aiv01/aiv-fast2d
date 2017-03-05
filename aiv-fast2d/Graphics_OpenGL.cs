using System;
using OpenTK;
#if !__MOBILE__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES30;
#endif

namespace Aiv.Fast2D
{

	using Vector2 = OpenTK.Vector2;

	public static class Graphics
	{
		public static void SetContext(Window window)
		{
			// on mobile devices, multiple contexts are not available
#if !__MOBILE__
			window.context.MakeCurrent();
#endif
		}

		public static void BindFrameBuffer(int frameBufferId)
		{
#if !__MOBILE__
			if (Window.IsObsolete)
			{
				GL.Ext.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferId);
				return;
			}
#endif
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferId);
		}

		public static string GetError()
		{
#if !__MOBILE__
			return GL.GetError().ToString();
#else
			return GL.GetErrorCode().ToString();
#endif
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

#if !__MOBILE__
			GL.Disable(EnableCap.Multisample);
#endif

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
#if !__MOBILE__
			GL.DeleteBuffer(id);
#else
			GL.DeleteBuffers(1, new int[] { id });
#endif
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
#if !__MOBILE__
			GL.DeleteVertexArray(id);
#else
			GL.DeleteVertexArrays(1, new int[] { id });
#endif
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
#if __MOBILE__
            int[] tmpStore = new int[1];
            GL.GenBuffers(1, tmpStore);
            return tmpStore[0];
#else
			return GL.GenBuffer();
#endif
		}

		public static int NewArray()
		{
			int vao = -1;
			// use VAO on modern OpenGL
			if (!Window.IsObsolete)
			{
#if !__MOBILE__
				vao = GL.GenVertexArray();
#else
                int[] tmpStore = new int[1];
                GL.GenVertexArrays(1, tmpStore);
                vao = tmpStore[0];
#endif
			}
			return vao;
		}


		public static void DrawArrays(int amount)
		{
#if !__MOBILE__
			GL.DrawArrays(PrimitiveType.Triangles, 0, amount);
#else
      		GL.DrawArrays(BeginMode.Triangles, 0, amount);
#endif
		}

		public static void MapBufferToArray(int bufferId, int index, int elementSize)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
			GL.EnableVertexAttribArray(index);
			GL.VertexAttribPointer(index, elementSize, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
		}

		public static void BufferData(float[] data)
		{
#if !__MOBILE__
			GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * sizeof(float)), data, BufferUsageHint.DynamicDraw);
#else
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * sizeof(float)), data, BufferUsage.DynamicDraw);
#endif
		}

		public static void BufferData(int bufferId, float[] data)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
			BufferData(data);
		}


		public static void BufferSubData(float[] data, int offset = 0)
		{
#if !__MOBILE__
			GL.BufferSubData<float>(BufferTarget.ArrayBuffer, (IntPtr)(offset * sizeof(float)), data.Length * sizeof(float), data);
#else
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(offset * sizeof(float)), (IntPtr)(data.Length * sizeof(float)), data);
#endif
		}

		public static void BufferSubData(int bufferId, float[] data, int offset = 0)
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
#if !__MOBILE__
			return GL.GenFramebuffer();
#else
            int[] tmp_values = new int[1];
            GL.GenFramebuffers(1, tmp_values);
            return tmp_values[0];
#endif
		}


		public static void FrameBufferTexture(int id)
		{
#if !__MOBILE__
			if (Window.IsObsolete)
			{
				GL.Ext.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, id, 0);
			}
			else
			{
				GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, id, 0);
			}
#else
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, id, 0);
#endif
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
#if !__MOBILE__
			GL.TexImage2D<byte>(TextureTarget.Texture2D, mipMap, PixelInternalFormat.Rgba8, width / (int)Math.Pow(2, mipMap), height / (int)Math.Pow(2, mipMap), 0, PixelFormat.Rgba, PixelType.UnsignedByte, bitmap);
#else
            GL.TexImage2D(TextureTarget.Texture2D, mipMap, PixelInternalFormat.Rgba, width / (int)Math.Pow(2, mipMap), height / (int)Math.Pow(2, mipMap), 0, OpenTK.Graphics.ES30.PixelFormat.Rgba, PixelType.UnsignedByte, bitmap);
#endif
		}

		public static void TextureSetRepeatX(bool repeat = true)
		{
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, repeat ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
		}

		public static void TextureSetRepeatY(bool repeat = true)
		{
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, repeat ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
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
#if !__MOBILE__
				// obsolete Desktop OpenGL does not have medium precision
				fragment = fragment.Replace("precision mediump float;", "");
#endif
			}
#if __MOBILE__
            vertex = vertex.Trim(new char[] { '\r', '\n' }).Replace("330 core", "300 es");
            fragment = fragment.Trim(new char[] { '\r', '\n' }).Replace("330 core", "300 es");
#endif
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
