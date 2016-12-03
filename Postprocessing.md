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

