using System;
using OpenTK;
using System.Drawing;
using System.Collections.Generic;

namespace Aiv.Fast2D
{
    public class InstancedSprite : Sprite
    {

        private static string instancedSpriteVertexShader = @"
#version 330 core

layout(location = 0) in vec2 vertex;
layout(location = 1) in vec2 uv;
layout(location = 2) in vec2 position;
layout(location = 3) in vec2 scale;

uniform mat4 mvp;

out vec2 uvout;

void main(){
        gl_Position = mvp * vec4((vertex.xy * scale) + position, 0.0, 1.0);
        uvout = uv;
}";
        private static string instancedSpriteFragmentShader = @"
#version 330 core

in vec2 uvout;

uniform vec4 mul_tint;
uniform vec4 add_tint;
uniform sampler2D tex;
uniform float use_texture;

out vec4 color;

void main(){
        if (use_texture > 0) {
            color = texture(tex, uvout) * mul_tint;
            color += vec4(add_tint.xyz * color.a, add_tint.a);
        }
        else {
            color = add_tint;
        }
}";

        private static Shader instancedSpriteShader = new Shader(instancedSpriteVertexShader, instancedSpriteFragmentShader);

        private int positionsBuffer;
        private int scalesBuffer;

        private float[] positionsData;
        private float[] scalesData;

        

        public void SetPosition(int instanceId, Vector2 position)
        {
            positionsData[instanceId * 2] = position.X;
            positionsData[instanceId * 2 + 1] = position.Y;
            UpdateFloatBuffer(positionsBuffer, new float []{ position.X, position.Y }, instanceId * 2);
        }

        public Vector2 GetPosition(int instanceId)
        {
            float x = positionsData[instanceId * 2];
            float y = positionsData[instanceId * 2 + 1];
            return new Vector2(x, y);
        }

        public void SetScale(int instanceId, Vector2 scale)
        {
            scalesData[instanceId * 2] = scale.X;
            scalesData[instanceId * 2 + 1] = scale.Y;
            UpdateFloatBuffer(scalesBuffer, new float[] { scale.X, scale.Y }, instanceId * 2);
        }

        public Vector2 GetScale(int instanceId)
        {
            float x = scalesData[instanceId * 2];
            float y = scalesData[instanceId * 2 + 1];
            return new Vector2(x, y);
        }

        public InstancedSprite(float width, float height) : base(width, height)
        {
            this.instances = 1;
            SetupInstances();
        }

        public InstancedSprite(float width, float height, int instances) : base(width, height)
        {
            this.instances = instances;
            SetupInstances();
        }

        private void SetupInstances()
        {
            this.shader = instancedSpriteShader;
            positionsData = new float[2 * this.instances];
            scalesData = new float[2 * this.instances];
            // fill scalesData with 1's
            for(int i=0;i<scalesData.Length;i++)
            {
                scalesData[i] = 1f;
            }
            positionsBuffer = NewFloatBuffer(2, 2, positionsData, 1);
            scalesBuffer = NewFloatBuffer(3, 2, scalesData, 1);
        }
    }
}
