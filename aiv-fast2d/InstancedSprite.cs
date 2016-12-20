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
layout(location = 4) in vec4 additive_tint;
layout(location = 5) in vec4 multiply_tint;

uniform mat4 mvp;

out vec2 uvout;
out vec4 additive_tint_out;
out vec4 multiply_tint_out;

void main(){
        gl_Position = mvp * vec4((vertex.xy * scale) + position, 0.0, 1.0);
        uvout = uv;
        additive_tint_out = additive_tint;
        multiply_tint_out = multiply_tint;
}";
        private static string instancedSpriteFragmentShader = @"
#version 330 core

precision highp float;

in vec2 uvout;
in vec4 additive_tint_out;
in vec4 multiply_tint_out;

uniform vec4 mul_tint;
uniform vec4 add_tint;
uniform sampler2D tex;
uniform float use_texture;

out vec4 color;

void main(){
        if (use_texture > 0.0) {
            color = texture(tex, uvout) * mul_tint * multiply_tint_out;
            color += vec4((add_tint.xyz + additive_tint_out.xyz) * color.a, add_tint.a + additive_tint_out.a);
        }
        else {
            color = add_tint + additive_tint_out;
        }
}";

        private static Shader instancedSpriteShader = new Shader(instancedSpriteVertexShader,
            instancedSpriteFragmentShader);

        private int positionsBuffer;
        private int scalesBuffer;
        private int additiveColorBuffer;
        private int multiplyColorBuffer;

        private float[] positionsData;
        private float[] scalesData;
        private float[] additiveColorData;
        private float[] multiplyColorData;


        public void SetPosition(int instanceId, Vector2 position, bool noUpload = false)
        {
            positionsData[instanceId * 2] = position.X;
            positionsData[instanceId * 2 + 1] = position.Y;
            if (!noUpload)
                UpdateFloatBuffer(positionsBuffer, new float[] { position.X, position.Y }, instanceId * 2);
        }

        public Vector2 GetPosition(int instanceId)
        {
            float x = positionsData[instanceId * 2];
            float y = positionsData[instanceId * 2 + 1];
            return new Vector2(x, y);
        }

        public void UpdatePositions()
        {
            UpdateFloatBuffer(positionsBuffer, positionsData);
        }

        public void SetScale(int instanceId, Vector2 scale, bool noUpload = false)
        {
            scalesData[instanceId * 2] = scale.X;
            scalesData[instanceId * 2 + 1] = scale.Y;
            if (!noUpload)
                UpdateFloatBuffer(scalesBuffer, new float[] { scale.X, scale.Y }, instanceId * 2);
        }

        public Vector2 GetScale(int instanceId)
        {
            float x = scalesData[instanceId * 2];
            float y = scalesData[instanceId * 2 + 1];
            return new Vector2(x, y);
        }

        public void UpdateScales()
        {
            UpdateFloatBuffer(scalesBuffer, scalesData);
        }

        public void SetAdditiveColor(int instanceId, Vector4 color, bool noUpload = false)
        {
            additiveColorData[instanceId * 4] = color.X;
            additiveColorData[instanceId * 4 + 1] = color.Y;
            additiveColorData[instanceId * 4 + 2] = color.Z;
            additiveColorData[instanceId * 4 + 3] = color.W;
            if (!noUpload)
                UpdateFloatBuffer(additiveColorBuffer, new float[] { color.X, color.Y, color.Z, color.W }, instanceId * 4);
        }

        public Vector4 GetAdditiveColor(int instanceId)
        {
            float x = additiveColorData[instanceId * 4];
            float y = additiveColorData[instanceId * 4 + 1];
            float z = additiveColorData[instanceId * 4 + 2];
            float w = additiveColorData[instanceId * 4 + 3];
            return new Vector4(x, y, z, w);
        }

        public void UpdateAdditiveColors()
        {
            UpdateFloatBuffer(additiveColorBuffer, additiveColorData);
        }

        public void SetMultiplyColor(int instanceId, Vector4 color, bool noUpload = false)
        {
            multiplyColorData[instanceId * 4] = color.X;
            multiplyColorData[instanceId * 4 + 1] = color.Y;
            multiplyColorData[instanceId * 4 + 2] = color.Z;
            multiplyColorData[instanceId * 4 + 3] = color.W;
            if (!noUpload)
                UpdateFloatBuffer(multiplyColorBuffer, new float[] { color.X, color.Y, color.Z, color.W }, instanceId * 4);
        }

        public Vector4 GetMultiplyColor(int instanceId)
        {
            float x = multiplyColorData[instanceId * 4];
            float y = multiplyColorData[instanceId * 4 + 1];
            float z = multiplyColorData[instanceId * 4 + 2];
            float w = multiplyColorData[instanceId * 4 + 3];
            return new Vector4(x, y, z, w);
        }

        public void UpdateMultiplyColors()
        {
            UpdateFloatBuffer(multiplyColorBuffer, multiplyColorData);
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
            this.hasVertexColors = false;

            this.shader = instancedSpriteShader;

            positionsData = new float[2 * this.instances];

            scalesData = new float[2 * this.instances];
            // fill scalesData with 1's
            for (int i = 0; i < scalesData.Length; i++)
            {
                scalesData[i] = 1f;
            }

            additiveColorData = new float[4 * this.instances];

            multiplyColorData = new float[4 * this.instances];
            for (int i = 0; i < multiplyColorData.Length; i++)
            {
                multiplyColorData[i] = 1f;
            }

            positionsBuffer = NewFloatBuffer(2, 2, positionsData, 1);
            scalesBuffer = NewFloatBuffer(3, 2, scalesData, 1);
            additiveColorBuffer = NewFloatBuffer(4, 4, additiveColorData, 1);
            multiplyColorBuffer = NewFloatBuffer(5, 4, multiplyColorData, 1);
        }
    }
}
