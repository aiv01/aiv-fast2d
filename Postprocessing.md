Applying PostProcessing Effects
-------------------------------

You can attach a chain of postprocessing effects (in the form of screen GLSL fragment shaders) to your window.

First step is creating a subclass of PostProcessingEffect and a fragment shader:

```cs
using Aiv.Fast2D;

namespace Aiv.Fast2D.Example
{
    public class DoNothingEffect : PostProcessingEffect
    {
        private static string fragmentShader = @"
#version 330 core

// required for mobile
precision highp float;

// screen uv
in vec2 uv;
// the screen texture
uniform sampler2D tex;
// the output color
out vec4 color;

void main() {
        // simply read and write the texture value
        color = texture(tex, uv);
}";
        public DoNothingEffect() : base(fragmentShader) {}

    }
}

```

Then you can simply add it to the postprocessing chain (effects inserted first will be applied first):

```cs
PostProcessingEffect yourEffect = window.AddPostProcessingEffect(new DoNothingEffect());
```

Effects can be enabled/disabled at runtime:

```cs
if (window.GetKey(KeyCode.Space))
    yourEffect.enabled = false;
```

Or inserted at a specific location of the chain:

```cs
// add the effect at position 2 of the chain
PostProcessingEffect yourEffect = window.SetPostProcessingEffect(2, new DoNothingEffect());
```

Examples:
---------

Simple black bands effect:

```cs
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

```

Masking with a texture (Update() is called at every game cycle):

```cs
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
```

Change screen geometry (use WASD to move the screen):

```cs
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
                center.X += window.deltaTime;
            }

            if (window.GetKey(KeyCode.A))
            {
                center.X -= window.deltaTime;
            }

            if (window.GetKey(KeyCode.W))
            {
                center.Y += window.deltaTime;
            }

            if (window.GetKey(KeyCode.S))
            {
                center.Y -= window.deltaTime;
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

```
