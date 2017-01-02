using System;

namespace Aiv.Fast2D
{
    public class RenderTexture : Texture
    {
        private int frameBuffer;

        public int FrameBuffer
        {
            get
            {
                return frameBuffer;
            }
        }

        public RenderTexture(int width, int height) : base(width, height)
        {
			frameBuffer = Graphics.NewFrameBuffer();
			Graphics.BindFrameBuffer(frameBuffer);
			Graphics.FrameBufferTexture(this.Id);
			Graphics.BindFrameBuffer(Graphics.GetDefaultFrameBuffer());
            this.flipped = true;
        }

       
    }
}
