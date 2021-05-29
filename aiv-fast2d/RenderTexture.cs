using System;

namespace Aiv.Fast2D
{
	public class RenderTexture : Texture //IDisposable 
	{
/*
		/// <summary>
		/// The Width of this render texture
		/// </summary>
        public int Width { get { return texture.Width; } }

		/// <summary>
		/// The Height of this render texture
		/// </summary>
		public int Height { get { return texture.Height; } }
*/
        /// <summary>
        /// Frame Buffer Id associated to this render texture
        /// </summary>
        public int FrameBufferId { get; }

		/// <summary>
		/// Depth Buffer Id associated to this render texture
		/// </summary>
        public int DepthTextureId { get; }

		/// <summary>
		/// Texture Id associated to this render texture
		/// </summary>
        public int TextureId { get { return texture.Id; } }

		private Texture texture;

        public RenderTexture(int width, int height, bool withDepth = false, int depthSize = 16, bool depthOnly = false)
			: base(width, height)
		{
			//texture = new Texture(width, height);		
			texture = this;

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
			DepthTextureId = -1;
			if (withDepth)
			{
				// attach a depth texture
				DepthTextureId = Graphics.NewTexture();
				Graphics.BindTextureToUnit(DepthTextureId, 0);
				Graphics.TextureSetNearest();
				Graphics.DepthTexture(width, height, depthSize);
				Graphics.FrameBufferDepthTexture(DepthTextureId);
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

/*     NOTE: RenderTexture need a Dispose because of DepthTextureId
             Understand how to manage this thing in case of Inheritance (from Texture)
		public new void Dispose()
		{
			texture.Dispose();
			if (DepthTextureId != -1)
            {
				Graphics.DeleteTexture(DepthTextureId);
            }
		}
*/		
	}
}
