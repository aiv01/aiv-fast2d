using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;

namespace Aiv.Fast2D.Example
{

    /// <summary>
    /// like black bands, but with transparency
    /// </summary>
    public class RedBands : PostProcessingEffect
    {

        private static string fragmentShader = @"
#version 330 core

precision highp float;

in vec2 uv;

uniform sampler2D tex;

out vec4 color;

void main() {
    color = texture(tex, uv);
    if (uv.x < 0.1 || uv.x > 0.9) {
        color += vec4(1, 0, 0, -0.5);
    }
}
";

        public RedBands() : base(fragmentShader)
        {
        }

    }
}
