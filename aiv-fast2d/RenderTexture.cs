using System;
using OpenTK.Graphics.OpenGL;


namespace Aiv.Fast2D
{
    public class RenderTexture : Texture
    {
        private int frameBuffer;

        public RenderTexture(int width, int height) : base(width, height)
        {
            frameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, this.Id, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            this.flipped = true;
        }

        public static void To(RenderTexture renderTexture, bool clear = true)
        {
            if (renderTexture == null)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                return;
            }
            else {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, renderTexture.frameBuffer);
            }

            if (clear)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }
        }
    }
}
