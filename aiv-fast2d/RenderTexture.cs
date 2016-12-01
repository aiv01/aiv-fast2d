using System;
#if !__MOBILE__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES30;
#endif


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
#if !__MOBILE__
            frameBuffer = GL.GenFramebuffer();
#else
            int[] tmp_values = new int[1];
            GL.GenFramebuffers(1, tmp_values);
            frameBuffer = tmp_values[0];
#endif
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
#if !__MOBILE__
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, this.Id, 0);
#else
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, this.Id, 0);
#endif
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            this.flipped = true;
        }

       
    }
}
