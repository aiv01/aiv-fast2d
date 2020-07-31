using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;
using OpenTK;

namespace Aiv.Fast2D.Example.RTE
{
    class WobbleFX : PostProcessingEffect
    {
        private float elapsedTime;
        private float speed;

        private static string fragmentShader = @"
        #version 330 core
        precision highp float;
        in vec2 uv;
        uniform sampler2D tex;
        out vec4 color;
        uniform float time;

        void main(){
            vec2 uv2 = uv;
            
            uv2.x += (sin(uv2.y * 20 + time) / 100);
            
            color = texture(tex, uv2);

        }";

        public WobbleFX(float speed=1.0f) : base(fragmentShader)
        {
            this.speed = speed;
        }

        public override void Update(Window window)
        {
            this.elapsedTime += window.DeltaTime * this.speed;
            screenMesh.shader.SetUniform("time", this.elapsedTime);
        }
    }
}
