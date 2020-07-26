using System;
using System.Collections.Generic;
using System.Runtime;
using OpenTK;

namespace Aiv.Fast2D
{

	public partial class Window
	{
		private Matrix4 projectionMatrix;
		private float _aspectRatio;

		public Matrix4 ProjectionMatrix
		{
			get
			{
				return this.projectionMatrix;
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

		public int ScaledWidth
		{
			get
			{
				return (int)(this.width * this.scaleX);
			}
		}

		public int ScaledHeight
		{
			get
			{
				return (int)(this.height * this.scaleY);
			}
		}

		public float OrthoWidth
		{
			get
			{
				if (this.currentOrthographicSize > 0)
					return this.currentOrthographicSize * this._aspectRatio;
				return this.width;
			}
		}

		public float OrthoHeight
		{
			get
			{
				if (this.currentOrthographicSize > 0)
					return this.currentOrthographicSize;
				return this.height;
			}
		}

		public string Version
		{
			get
			{
				return Graphics.Version;
			}
		}

		public string Vendor
		{
			get
			{
				return Graphics.Vendor;
			}
		}

		public string SLVersion
		{
			get
			{
				return Graphics.SLVersion;
			}
		}

		public string Renderer
		{
			get
			{
				return Graphics.Renderer;
			}
		}

		public string Extensions
		{
			get
			{
				return Graphics.Extensions;
			}
		}

		private int defaultFramebuffer;
		private bool collectedDefaultFrameBuffer;

		protected float zNear;
		protected float zFar;


		public void SetZNearZFar(float near, float far)
		{
			zNear = near;
			zFar = far;
			this.SetViewport(0, 0, this.width, this.height);
		}

		public float ZNear
		{
			get
			{
				return zNear;
			}
		}

		public float ZFar
		{
			get
			{
				return zFar;
			}
		}

		public void EnableDepthTest()
		{
			Graphics.EnableDepthTest();
		}

		public void DisableDepthTest()
		{
			Graphics.DisableDepthTest();
		}

		public void CullFrontFaces()
		{
			Graphics.CullFrontFaces();
		}

		public void CullBackFaces()
		{
			Graphics.CullBackFaces();
		}

		public void DisableCullFaces()
		{
			Graphics.CullFacesDisable();
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
			Graphics.SetContext(current);
		}

		public void SetCurrent()
		{
			Window.SetCurrent(this);
		}

		public bool opened = true;

		public bool IsOpened
		{
			get
			{
				return opened;
			}
		}

		public void Exit(int code = 0)
		{
			System.Environment.Exit(code);
		}

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


		#region log
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
		#endregion


		#region camera
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
		#endregion

		#region obsoleteMode
		private static bool obsoleteMode;

		public static void SetObsoleteMode()
		{
			obsoleteMode = true;
		}

		public static bool IsObsolete
		{
			get
			{
				return obsoleteMode;
			}
		}
		#endregion


		#region postprocessing

		private List<PostProcessingEffect> postProcessingEffects;

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

		private PostProcessingEffect GetFirstPostProcessingEffect()
		{
			foreach (PostProcessingEffect effect in postProcessingEffects)
			{
				if (effect != null && effect.enabled)
					return effect;
			}
			return null;
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
					Graphics.BindFrameBuffer(GetNextPostProcessingEffectFramebuffer(i));
					this.ClearColor();
					// custom update cycle
					postProcessingEffects[i].Update(this);
					// blit to the next render destination
					postProcessingEffects[i].Apply();

				}
			}

		}

		#endregion

		public void SetClearColor(float r, float g, float b, float a = 1)
		{
			Graphics.SetClearColor(r, g, b, a);
		}

		public void SetClearColor(int r, int g, int b, int a = 255)
		{
			SetClearColor(r / 255f, g / 255f, b / 255f, a / 255f);
		}

		public void SetClearColor(Vector4 color)
		{
			SetClearColor(color.X, color.Y, color.Z, color.W);
		}

		public void ClearColor()
		{
			Graphics.ClearColor();
		}

		#region scissor
		public void SetScissorTest(bool enabled)
		{
			if (enabled)
			{
				Graphics.EnableScissorTest();
			}
			else
			{
				Graphics.DisableScissorTest();
			}
		}

		public void SetScissorTest(int x, int y, int width, int height)
		{
			SetScissorTest(true);
			Graphics.Scissor((int)(x * this.scaleX),
					   (int)(((this.height - y) - height) * this.scaleY),
					   (int)(width * this.scaleX),
					   (int)(height * this.scaleY));
		}

		public void SetScissorTest(float x, float y, float width, float height)
		{
			SetScissorTest((int)x, (int)y, (int)width, (int)height);
		}
		#endregion


		private int GetDefaultFrameBuffer()
		{
			foreach (PostProcessingEffect effect in postProcessingEffects)
			{
				if (effect != null && effect.enabled)
					return effect.RenderTexture.FrameBuffer;
			}
			if (!collectedDefaultFrameBuffer)
			{
				defaultFramebuffer = Graphics.GetDefaultFrameBuffer();
				collectedDefaultFrameBuffer = true;
			}

			return defaultFramebuffer;
		}


		public void BindTextureToUnit(Texture texture, int unit)
		{
			Graphics.BindTextureToUnit(texture.Id, unit);
		}

		#region gc

		public List<int> textureGC = new List<int>();
		public List<int> bufferGC = new List<int>();
		public List<int> vaoGC = new List<int>();
		public List<int> shaderGC = new List<int>();

		private void RunGraphicsGC()
		{
			// destroy useless resources
			// use for for avoiding "changing while iterating
			for (int i = 0; i < this.bufferGC.Count; i++)
			{
				int _id = this.bufferGC[i];
				Graphics.DeleteBuffer(_id);
				this.Log(string.Format("buffer {0} deleted", _id));
			}
			this.bufferGC.Clear();

			for (int i = 0; i < this.vaoGC.Count; i++)
			{
				int _id = this.vaoGC[i];
				Graphics.DeleteArray(_id);
				this.Log(string.Format("array {0} deleted", _id));
			}
			this.vaoGC.Clear();

			for (int i = 0; i < this.textureGC.Count; i++)
			{
				int _id = this.textureGC[i];
				Graphics.DeleteTexture(_id);
				this.Log(string.Format("texture {0} deleted", _id));
			}
			this.textureGC.Clear();

			for (int i = 0; i < this.shaderGC.Count; i++)
			{
				int _id = this.shaderGC[i];
				Graphics.DeleteShader(_id);
				this.Log(string.Format("shader {0} deleted", _id));
			}
			this.shaderGC.Clear();
		}
		#endregion


		public void ResetFrameBuffer()
		{
			Graphics.BindFrameBuffer(GetDefaultFrameBuffer());
			ClearColor();
		}


		public void SetViewport(int x, int y, int width, int height, float orthoSize = 0, bool virtualScreen = false)
		{
			if (zNear == 0 && zFar == 0)
			{
				zNear = -1;
				zFar = 1;
			}
			// store values before changes
			this.viewportPosition = new Vector2(x, y);
			this.viewportSize = new Vector2(width, height);
			// fix y as it is downsided in OpenGL
			y = (this.height - y) - height;
			if (virtualScreen)
			{
				Graphics.Viewport(0,
						0,
						width,
						height);
			}
			else
			{
				Graphics.Viewport((int)(x * this.scaleX),
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
				this.projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, orthoSize * this._aspectRatio, orthoSize, 0, zNear, zFar);
			}
			else
			{
				this.projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, zNear, zFar);

			}
			
			this.currentOrthographicSize = orthoSize;
		}

		public void RenderTo(RenderTexture renderTexture, bool clear = true, float orthoSize = 0)
		{
			if (renderTexture == null)
			{
				Graphics.BindFrameBuffer(GetDefaultFrameBuffer());
				SetViewport(0, 0, this.width, this.height);
				return;
			}
			else
			{
				Graphics.BindFrameBuffer(renderTexture.FrameBuffer);
				// unscaled,virtual viewport
				SetViewport(0, 0, renderTexture.Width, renderTexture.Height, orthoSize, true);
			}

			if (clear)
			{
				this.ClearColor();
			}
		}

		private void FinalizeSetup()
		{
			// more gentle GC
			GCSettings.LatencyMode = GCLatencyMode.LowLatency;

			postProcessingEffects = new List<PostProcessingEffect>();

			Window.SetCurrent(this);
		}

		public void SetAlphaBlending()
		{
			Graphics.SetAlphaBlending();
		}

		public void SetMaskedBlending()
		{
			Graphics.SetMaskedBlending();
		}
	}
}

