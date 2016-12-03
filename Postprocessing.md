Applying PostProcessing Effects
-------------------------------

You can attach a chain of postprocessing effects (in the form of screen GLSL fragment shaders) to your window.

First step is creating a subclass of PostProcessingEffect and fragment shader:

```cs
using Aiv.Fast2D;

namespace Aiv.Fast2D.Example
{
    public class DoNothingEffect : PostProcessingEffect
    {
        private static string fragmentShader = @"
#version 330 core

precision highp float;

// screen uv
in vec2 uv;
// the screen texture
uniform sampler2D tex;
// the output color
out vec4 color;

void main() {
        color = texture(tex, uv);
}
";
        public DoNothingEffect() : base(fragmentShader)
        {

        }

    }
}

```
