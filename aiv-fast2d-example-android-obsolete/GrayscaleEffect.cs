using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;

namespace Aiv.Fast2D.Android.Example.Obsolete
{
    public class GrayscaleEffect : PostProcessingEffect
    {
        private static string fragmentShader = @"
#version 330 core

precision highp float;

in vec2 uv;

uniform sampler2D tex;

out vec4 color;

void main() {
    color = texture(tex, uv);
    float gray = (color.r + color.g + color.b) / 3.0;
    color = vec4(gray, gray, gray, color.a);
}
";

        private static string fragmentShaderObsolete = @"
precision mediump float;

varying vec2 uv;

uniform sampler2D tex;

void main() {
    vec4 color = texture2D(tex, uv);
    float gray = (color.r + color.g + color.b) / 3.0;
    gl_FragColor = vec4(gray, gray, gray, color.a);
}
";

        public GrayscaleEffect() : base(fragmentShader, fragmentShaderObsolete)
        {

        }
    }
}
