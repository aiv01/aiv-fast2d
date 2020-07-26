using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;
using OpenTK;

namespace Aiv.Fast2D.Example
{
    public class WASDEffect : PostProcessingEffect
    {

        private Vector2 center;

        private static string fragmentShader = @"
#version 330 core

precision highp float;

in vec2 uv;

uniform sampler2D tex;

out vec4 color;

void main() {
        color = texture(tex, uv);
}
";

        public WASDEffect() : base(fragmentShader)
        {

        }

        public override void Update(Window window)
        {
            
            if (window.GetKey(KeyCode.D))
            {
                center.X += window.DeltaTime;
            }

            if (window.GetKey(KeyCode.A))
            {
                center.X -= window.DeltaTime;
            }

            if (window.GetKey(KeyCode.W))
            {
                center.Y += window.DeltaTime;
            }

            if (window.GetKey(KeyCode.S))
            {
                center.Y -= window.DeltaTime;
            }

            screenMesh.v = new float[]
            {
                    -1 + center.X, 1 + center.Y,
                    1+ center.X, 1 + center.Y,
                    1+ center.X, -1 + center.Y,

                    1+ center.X,-1 + center.Y,
                    -1+ center.X, -1 + center.Y,
                    -1+ center.X, 1 + center.Y
            };

            screenMesh.UpdateVertex();

        }

    }
}
