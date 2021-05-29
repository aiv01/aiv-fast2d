using System;
using System.Collections.Generic;

namespace Aiv.Fast2D
{
    public class PostProcessingEffect
    {
        public bool enabled;
        protected Mesh screenMesh;
        protected bool useDepth;
        protected int depthSize;

        private static string vertexShader = @"
#version 330 core

layout(location = 0) in vec2 screen_vertex;
layout(location = 1) in vec2 screen_uv;

out vec2 uv;

void main(){
        gl_Position = vec4(screen_vertex.xy, 0.0, 1.0);
        uv = screen_uv;
}";

        private static string vertexShaderObsolete = @"
attribute vec2 screen_vertex;
attribute vec2 screen_uv;

varying vec2 uv;

void main(){
        gl_Position = vec4(screen_vertex.xy, 0.0, 1.0);
        uv = screen_uv;
}";

        protected RenderTexture renderTexture;

        protected Dictionary<string, Texture> textures;

        public RenderTexture RenderTexture
        {
            get
            {
                return renderTexture;
            }
        }

        public void AddTexture(string name, Texture texture)
        {
            if (textures == null)
                textures = new Dictionary<string, Texture>();
            textures.Add(name, texture);
        }

        public PostProcessingEffect(string fragmentShader, string fragmentShaderObsolete = null, bool useDepth = false, int depthSize = 16)
        {
            string[] attribs = null;
            if (fragmentShaderObsolete != null)
            {
                attribs = new string[] { "screen_vertex", "screen_uv" };
            }
            screenMesh = new Mesh(new Shader(vertexShader, fragmentShader, vertexShaderObsolete, fragmentShaderObsolete, attribs));
            screenMesh.hasVertexColors = false;

            this.useDepth = useDepth;
            this.depthSize = depthSize;

            screenMesh.v = new float[]
            {
                    -1, 1,
                    1, 1,
                    1, -1,

                    1,-1,
                    -1, -1,
                    -1, 1
            };

            screenMesh.uv = new float[]
            {
                    0, 1,
                    1, 1,
                    1, 0,

                    1, 0,
                    0, 0,
                    0, 1
            };

            // upload both vertices and uvs
            screenMesh.Update();
            screenMesh.noMatrix = true;

            // enabled by default
            this.enabled = true;

        }

        public void Setup(Window window)
        {
            if (renderTexture == null || 
                renderTexture.Height != window.ScaledWidth ||
                renderTexture.Width != window.ScaledWidth
                )
                renderTexture = new RenderTexture(window.ScaledWidth, window.ScaledHeight, this.useDepth, this.depthSize);
        }

        public void Apply(RenderTexture inRenderTexture = null)
        {
            if (inRenderTexture == null)
                inRenderTexture = renderTexture;

            if (textures != null)
            {
                int texture_unit = 2;
                foreach (string uniform in textures.Keys)
                {
                    Graphics.BindTextureToUnit(textures[uniform].Id, texture_unit);
                    screenMesh.shader.SetUniform(uniform, texture_unit);
                    texture_unit++;
                }
            }

            if (!this.useDepth)
            {
                screenMesh.DrawRenderTexture(inRenderTexture);
            }
            else
            {
                screenMesh.Draw((m) =>
                {
                    Graphics.BindTextureToUnit(inRenderTexture.TextureId, 0);
                    Graphics.BindTextureToUnit(inRenderTexture.DepthTextureId, 1);
                    m.shader.SetUniform("tex", 0);
                    m.shader.SetUniform("depth_tex", 1);
                });
            }
        }

        public virtual void Update(Window window)
        {

        }
    }
}
