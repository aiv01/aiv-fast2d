using System;
using Aiv.Fast2D;
using OpenTK;

namespace Aiv.Fast2D.Example.RenderTextureSample
{
    class GrayscaleEffect : PostProcessingEffect
    {
        private static string fragmentShader = @"
#version 330 core

precision highp float;

in vec2 uv;

uniform sampler2D tex;

out vec4 color;

void main() {
    color = texture(tex, uv);
    float gray = (color.r + color.g + color.b) / 3;
    color = vec4(gray, gray, gray, color.a);
}
";

        public GrayscaleEffect() : base(fragmentShader)
        {

        }
    }

    class YellowizerEffect : PostProcessingEffect
    {
        private static string fragmentShader = @"
#version 330 core

precision highp float;

in vec2 uv;

uniform sampler2D tex;

out vec4 color;

void main() {
    color = texture(tex, uv);
    color *= vec4(1, 1, 0, 1);
}
";

        public YellowizerEffect() : base(fragmentShader)
        {

        }
    }

    class Masker : PostProcessingEffect
    {
        private static string fragmentShader = @"
#version 330 core

precision highp float;

in vec2 uv;

uniform sampler2D tex;
uniform sampler2D mask;

out vec4 color;

void main() {
    color = texture(tex, uv) * texture(mask, uv).a;
}
";

        public Masker() : base(fragmentShader)
        {

        }
    }

    static class RenderTextureExample
    {
        public static void Run()
        {
            Window window = new Window(1024, 768, "GetPixels");

            RenderTexture screen = new RenderTexture(800, 600);

            Sprite sprite = new Sprite(100, 100);
            // half the render texture (to retain aspect ratio)
            Sprite lowAngle = new Sprite(400, 300);

            Sprite final = new Sprite(800, 600);

            GrayscaleEffect effect = new GrayscaleEffect();
            YellowizerEffect yellowizer = new YellowizerEffect();

            sprite.position = new Vector2(200, 200);
            lowAngle.position = new Vector2(400, 400);

            Random random = new Random();

            Texture dumbTexture = new Texture(800, 600);
            // in opengl, textures are flipped on the y axis
            dumbTexture.flipped = true;

            Masker masker = new Masker();
            masker.AddTexture("mask", new Texture("RenderTexture/Assets/star.png"));

            window.AddPostProcessingEffect(masker);

            window.SetClearColor(0, 0, 255, 255);

            while (window.IsOpened)
            {
                sprite.EulerRotation += 30 * window.deltaTime;

                window.RenderTo(screen);
                sprite.DrawSolidColor(255, 0, 0);
                if (random.Next() % 2 == 0)
                    screen.ApplyPostProcessingEffect(effect);
                if (random.Next() % 2 == 0)
                    screen.ApplyPostProcessingEffect(yellowizer);
                window.RenderTo(null);

                final.DrawTexture(screen);

                byte[] data = screen.Download();
                dumbTexture.Update(data);
                lowAngle.DrawTexture(dumbTexture);

                GC.Collect();

                window.Update();
            }
        }
    }
}
