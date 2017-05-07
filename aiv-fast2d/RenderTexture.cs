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

		public RenderTexture(int width, int height, bool withDepth = false, int depthSize = 16) : base(width, height)
		{
			frameBuffer = Graphics.NewFrameBuffer();
			Graphics.BindFrameBuffer(frameBuffer);
			Graphics.FrameBufferTexture(this.Id);
			depthTexture = -1;
			if (withDepth)
			{
				depthTexture = Graphics.DepthTexture(width, height, depthSize);
				Graphics.FrameBufferDepthTexture(depthTexture);
			}
			Graphics.BindFrameBuffer(Graphics.GetDefaultFrameBuffer());
			this.flipped = true;
		}


	}
}
