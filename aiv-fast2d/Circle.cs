using OpenTK;

namespace Aiv.Fast2D
{
    /// <summary>
    /// Base class to render native circles.
    /// Uses a quad canvas using a sprite as a base.
    /// </summary>
    public class Circle
    {
        private static string circleVertexShader = @"
#version 330 core

layout(location = 0) in vec2 vertex;
layout(location = 1) in vec2 uv;

uniform mat4 mvp;

out vec2 uvout;

void main(){
        gl_Position = mvp * vec4(vertex.xy, 0.0, 1.0);
        uvout = uv;
}";

        private static string circleFragmentShader = @"
#version 330 core
precision highp float;

uniform float use_texture;
uniform sampler2D tex;
uniform vec4 color;
uniform vec4 mul_tint;
uniform vec4 add_tint;
uniform float thickness; // 1 is full filled, 0 invisible
uniform float fade; // the fade at the border of the circle for smoothing

in vec2 uvout;
out vec4 frag_color;

void main(){
        if (use_texture > 0.0) {
            frag_color = texture(tex, uvout);
        }
        else {
			frag_color = color;
        }

		vec2 uv = uvout * 2 - 1;
		float distance = 1 - length(uv);
		float stepDistance = smoothstep(0, fade, distance);
		
		// calculate the base color of the circle.
        if (use_texture > 0.0) {
            frag_color = texture(tex, uvout);
        }
        else {
		    frag_color = (vec4(stepDistance) * color) * color.a;
        }
        
		// we are adding fade to the thickness here because thickness 1 should give a full filled circle.
		// not doing this means that we'll end up with a little empty space in the middle of the circle.
		// on the other hand doing it means that one edge fading (the internal one) is always bigger by the amount of fade compared to the outer one.
		// I think this is an acceptable solution. To have this perfect we need to add a branch and skip this bit.
		// calculate if this pixel needs to be visible or not.
		float innerSmooth = (smoothstep(thickness + fade, thickness, distance));	
		frag_color.rgba *= innerSmooth;

        // having strange results with tints, not sure if is alpha blending or what else.
        frag_color *= mul_tint * stepDistance * innerSmooth;
        frag_color += vec4(add_tint.rgb * frag_color.a, add_tint.a) * stepDistance * innerSmooth;
}";

        private Sprite spriteInternal;

        private float cachedThickness;
        private float cachedFade;

        public Circle(float radius)
        {
            Radius = radius;

            // With radius 100 our sprite canvas needs to be 200 pixels on both axis. Otherwise we'll have 100 diameter.
            spriteInternal = new Sprite(radius * 2f, radius * 2f);
            spriteInternal.shader = new Shader(circleVertexShader, circleFragmentShader, null, null, new string[] { "vertex", "uv" });
            spriteInternal.pivot = new Vector2(radius, radius);

            // set a default thickness of 1 for a filled circle
            Thickness = 1f;

            // fade is good at approx 0.01f
            Fade = 0.01f;
        }

        // User-Exposed properties
        public float Radius { get; private set; }

        public Vector2 Position
        {
            get => spriteInternal.position;
            set => spriteInternal.position = value;
        }

        /// <summary>
        /// How tick the circle is. 0 is invisible, 1 is fully filled, everything in between will leave space in the middle render the border.
        /// </summary>
        public float Thickness
        {
            get => cachedThickness;
            set
            {
                cachedThickness = value;
                spriteInternal.shader.SetUniform("thickness", value);
            }
        }

        /// <summary>
        /// The smoothing factor of the circle edge. Often a very small value (default is 0.01).
        /// If 0 there is no smoothing.
        /// </summary>
        public float Fade
        {
            get => cachedFade;
            set
            {
                cachedFade = value;
                spriteInternal.shader.SetUniform("fade", value);
            }
        }

        public void SetMultiplyTint(Vector4 multiplyTint) => spriteInternal.SetMultiplyTint(multiplyTint);
        public void SetAdditiveTint(Vector4 additiveTint) => spriteInternal.SetAdditiveTint(additiveTint);

        public void DrawColor(Vector4 color)
        {
            spriteInternal.DrawColor(color);
        }

        /// <summary>
        /// Texture rendering is supported for circles but it doesn't make too much sense. The texture will be cut off out of the border of the circle.
        /// If you have a circular sprite to render just use a simple sprite.
        /// </summary>
        /// <param name="texture"></param>
        public void DrawTexture(Texture texture)
        {
            spriteInternal.DrawTexture(texture);
        }
    }
}
