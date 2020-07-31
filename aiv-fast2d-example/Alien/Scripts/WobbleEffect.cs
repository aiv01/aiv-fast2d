using Aiv.Fast2D;

namespace Aiv.Fast2D.Example
{
    public class WobbleEffect : PostProcessingEffect
    {

        // here we accumulate the time
        private float wave;
        private float speed;

        private static string fragmentShader = @"
#version 330 core

precision highp float;

in vec2 uv;

uniform sampler2D tex;
uniform float wave;

out vec4 color;

void main() {
        vec2 uv2 = uv;
        // r * r * pi
        uv2.x += sin(uv2.y * 3 * 3 * 3.14159 +  wave) / 100;

        color = texture(tex, uv2);
}";


        public WobbleEffect(float speed) : base(fragmentShader)
        {
            this.speed = speed;
        }

        public override void Update(Window window)
        {
            this.wave += window.DeltaTime * this.speed;
            screenMesh.shader.SetUniform("wave", this.wave);
        }

    }
}
