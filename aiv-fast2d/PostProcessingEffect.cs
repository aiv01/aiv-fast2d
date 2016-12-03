using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aiv.Fast2D
{
    public class PostProcessingEffect
    {

        public bool enabled;

        protected Mesh screenMesh = new Mesh();

        private static string vertexShader = @"
#version 330 core

layout(location = 0) in vec2 screen_vertex;
layout(location = 1) in vec2 screen_uv;

out vec2 uv;

void main(){
        gl_Position = vec4(screen_vertex.xy, 0.0, 1.0);
        uv = screen_uv;
}";

        protected RenderTexture renderTexture;

        public RenderTexture RenderTexture
        {
            get
            {
                return renderTexture;
            }
        }

        public PostProcessingEffect(string fragmentShader)
        {
            screenMesh = new Mesh(new Shader(vertexShader, fragmentShader));

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
            renderTexture = new RenderTexture(window.Width, window.Height);
        }

        public void Apply(Window window)
        {
            screenMesh.DrawTexture(renderTexture);
        }

        public virtual void Update(Window window)
        {

        }
    }
}
