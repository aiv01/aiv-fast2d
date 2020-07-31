using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;

namespace Aiv.Fast2D.Example.RTE
{
    class GrayScaleFX : PostProcessingEffect
    {
        private static string fragmentShader = @"
        #version 330 core
        precision highp float;
        in vec2 uv;
        uniform sampler2D tex;
        out vec4 fragColor;

        void main(){
            vec4 color = texture(tex, uv);
            float gray = (color.r + color.g + color.b) / 3;
            color = vec4(gray,gray,gray,color.a);            

            //if (color.r == 1 && color.g == 0 && color.b == 0) color = vec4(1,1,1,1);
            //else color = vec4(gray,gray,gray,color.a);

            fragColor = color;
        }";

        public GrayScaleFX() : base(fragmentShader)
        {
        }
    }
}
