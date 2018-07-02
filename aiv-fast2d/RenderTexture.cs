using System;

namespace Aiv.Fast2D
{
	public class RenderTexture : Texture
	{
		protected int frameBuffer;
		protected int depthTexture;

		public int FrameBuffer
		{
			get
			{
				return frameBuffer;
			}
		}

		public int DepthId
		{
			get
			{
				return depthTexture;
			}
		}

		public RenderTexture(int width, int height, bool withDepth = false, int depthSize = 16, bool depthOnly = false) : base(width, height)
		{
			frameBuffer = Graphics.NewFrameBuffer();
			Graphics.BindFrameBuffer(frameBuffer);
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
			depthTexture = -1;
			if (withDepth)
			{
				// attach a depth texture
				depthTexture = Graphics.NewTexture();
				Graphics.BindTextureToUnit(depthTexture, 0);
				Graphics.TextureSetNearest();
				Graphics.DepthTexture(width, height, depthSize);
				Graphics.FrameBufferDepthTexture(depthTexture);
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
