using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;

namespace Aiv.Fast2D.Example
{
    public class BlackBands : PostProcessingEffect
    {

        private static string fragmentShader = @"
#version 330 core

precision highp float;

in vec2 uv;

uniform sampler2D tex;

out vec4 color;

void main() {
    if (uv.y < 0.1 || uv.y > 0.9) {
        color = vec4(0, 0, 0, 1);
    }
    else {
        color = texture(tex, uv);
    }
}
";

        public BlackBands() : base(fragmentShader)
        {
        }

    }
}
