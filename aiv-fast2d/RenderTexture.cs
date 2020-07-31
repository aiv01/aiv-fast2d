using System;

namespace Aiv.Fast2D
{
	public class RenderTexture : IDisposable //: Texture
	{
		/// <summary>
		/// The Width of this render texture
		/// </summary>
        public int Width { get { return texture.Width; } }

		/// <summary>
		/// The Height of this render texture
		/// </summary>
		public int Height { get { return texture.Height; } }

        /// <summary>
        /// Frame Buffer Id associated to this render texture
        /// </summary>
        public int FrameBufferId { get; }

		/// <summary>
		/// Depth Buffer Id associated to this render texture
		/// </summary>
        public int DepthBufferId { get; }

		/// <summary>
		/// Texture Id associated to this render texture
		/// </summary>
        public int TextureId { get { return texture.Id; } }

		private Texture texture;

		public RenderTexture(int width, int height, bool withDepth = false, int depthSize = 16, bool depthOnly = false)
			//: base(width, height)
		{
			texture = new Texture(width, height);		

			FrameBufferId = Graphics.NewFrameBuffer();
			Graphics.BindFrameBuffer(FrameBufferId);
			if (depthOnly)
			{
				Graphics.DepthTexture(width, height, depthSize);
				Graphics.TextureSetNearest();
				Graphics.FrameBufferDepthTexture(this.texture.Id);
				Graphics.FrameBufferDisableDraw();
			}
			else
			{
				Graphics.FrameBufferTexture(this.texture.Id);
			}
			DepthBufferId = -1;
			if (withDepth)
			{
				// attach a depth texture
				DepthBufferId = Graphics.NewTexture();
				Graphics.BindTextureToUnit(DepthBufferId, 0);
				Graphics.TextureSetNearest();
				Graphics.DepthTexture(width, height, depthSize);
				Graphics.FrameBufferDepthTexture(DepthBufferId);
			}
			Graphics.BindFrameBuffer(Graphics.GetDefaultFrameBuffer());
			
			this.texture.flipped = true;
		}

		/// <summary>
		/// Apply a Post Processing FX
		/// </summary>
		/// <param name="effect">the effect to be applyied</param>
        public void ApplyPostProcessingEffect(PostProcessingEffect effect)
        {
            effect.Apply(this);
        }


		/// <summary>
		/// Download texture data from the GPU
		/// </summary>
		/// <returns></returns>
        public byte[] Download()
        {
			return this.texture.Download();
        }

		/// <summary>
		/// Destroy the render texture and resources associated
		/// </summary>
		public void Dispose()
		{
			texture.Dispose();
		}

	}
}
