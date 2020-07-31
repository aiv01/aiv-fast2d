using System;

namespace Aiv.Fast2D
{
	public class RenderTexture : Texture
	{
		/// <summary>
		/// Frame Buffer Id associated to this render texture
		/// </summary>
        public int FrameBufferId { get; }

		/// <summary>
		/// Depth Buffer Id associated to this render texture
		/// </summary>
        public int DepthBufferId { get; }

        public RenderTexture(int width, int height, bool withDepth = false, int depthSize = 16, bool depthOnly = false) : base(width, height)
		{
			FrameBufferId = Graphics.NewFrameBuffer();
			Graphics.BindFrameBuffer(FrameBufferId);
			if (depthOnly)
			{
				Graphics.DepthTexture(width, height, depthSize);
				Graphics.TextureSetNearest();
				Graphics.FrameBufferDepthTexture(this.Id);
				Graphics.FrameBufferDisableDraw();
			}
			else
			{
				Graphics.FrameBufferTexture(this.Id);
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
			this.flipped = true;
		}

        public void ApplyPostProcessingEffect(PostProcessingEffect effect)
        {
            effect.Apply(this);
        }

	}
}
