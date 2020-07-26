using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;

namespace Aiv.Fast2D.Example
{
    public class MaskEffect : PostProcessingEffect
    {

        private Texture maskTexture;

        private static string fragmentShader = @"
#version 330 core

precision highp float;

in vec2 uv;

uniform sampler2D tex;
uniform sampler2D mask_tex;

out vec4 color;

void main() {
        vec4 base = texture(tex, uv);
        vec4 mask = texture(mask_tex, uv);

        color = vec4(base.rgb * mask.a, base.a);
}
";

        public MaskEffect(string fileName) : base(fragmentShader)
        {
            this.maskTexture = new Texture(fileName);
        }

        public MaskEffect(Texture mask) : base(fragmentShader)
        {
            this.maskTexture = mask;
        }

        public override void Update(Window window)
        {
            window.BindTextureToUnit(this.maskTexture, 1);
            screenMesh.shader.SetUniform("mask_tex", 1);
        }

    }
}
