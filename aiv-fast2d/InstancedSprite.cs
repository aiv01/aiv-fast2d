using System;
using OpenTK;

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

uniform vec4 color;
uniform vec4 mul_tint;
uniform vec4 add_tint;
uniform sampler2D tex;
uniform float use_texture;

out vec4 fragColor;

void main() {
        if (use_texture > 0.0) {
            fragColor = texture(tex, uvout);
        }
        else {
            fragColor = color;
        }

        fragColor *= mul_tint * multiply_tint_out;
        fragColor += vec4((add_tint.xyz + additive_tint_out.xyz) * fragColor.a, add_tint.a + additive_tint_out.a);
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

        /// <summary>
        /// Sprite specialization which offer hardware accelerated instancing.
        /// Useful to render multiple mesh at time reducing draw call at minimum (ex. Particles, grasses, etc...)
        /// </summary>
        /// <param name="width">the width used for each instance</param>
        /// <param name="height">the height used for each instance</param>
        /// <param name="instances">number of instances to be loaded</param>
        public InstancedSprite(float width, float height, int instances) : base(width, height)
        {
            this.instances = instances;
            SetupInstances();
        }


        /// <summary>
        /// Set the position for the specified instance.
        /// </summary>
        /// <param name="instanceId">the instance id</param>
        /// <param name="position">the instance position</param>
        /// <param name="uploadImmediatly">if data has to be immediatly uploaded to the GPU. if <c>false</c> rember to call <seealso cref="UpdatePositionForAllInstances"/></param>
        public void SetPositionPerInstance(int instanceId, Vector2 position, bool uploadImmediatly = false)
        {
            positionsData[instanceId * 2] = position.X;
            positionsData[instanceId * 2 + 1] = position.Y;
            if (uploadImmediatly)
                UpdateFloatBuffer(positionsBuffer, new float[] { position.X, position.Y }, instanceId * 2);
        }

        /// <summary>
        /// Retrieve current position for the specified instance
        /// </summary>
        /// <param name="instanceId">the instance id</param>
        /// <returns></returns>
        public Vector2 GetPositionPerInstance(int instanceId)
        {
            float x = positionsData[instanceId * 2];
            float y = positionsData[instanceId * 2 + 1];
            return new Vector2(x, y);
        }

        /// <summary>
        /// Upload all the instance positions to the GPU.
        /// </summary>
        public void UpdatePositionForAllInstances()
        {
            UpdateFloatBuffer(positionsBuffer, positionsData);
        }

        /// <summary>
        /// Set the scaling for the specified instance.
        /// </summary>
        /// <param name="instanceId">the instance id</param>
        /// <param name="scale">the scaling vector</param>
        /// <param name="uploadImmediatly">if data has to be immediatly uploaded to the GPU. if <c>false</c> rember to call <seealso cref="UpdateScaleForAllInstances"/></param>
        public void SetScale(int instanceId, Vector2 scale, bool uploadImmediatly = false)
        {
            scalesData[instanceId * 2] = scale.X;
            scalesData[instanceId * 2 + 1] = scale.Y;
            if (uploadImmediatly)
                UpdateFloatBuffer(scalesBuffer, new float[] { scale.X, scale.Y }, instanceId * 2);
        }

        /// <summary>
        /// Retrieve the current scaling for the specified intance.
        /// </summary>
        /// <param name="instanceId">the instance id</param>
        /// <returns></returns>
        public Vector2 GetScale(int instanceId)
        {
            float x = scalesData[instanceId * 2];
            float y = scalesData[instanceId * 2 + 1];
            return new Vector2(x, y);
        }

        /// <summary>
        /// Upload all the instance scaling data to the GPU.
        /// </summary>
        public void UpdateScaleForAllInstances()
        {
            UpdateFloatBuffer(scalesBuffer, scalesData);
        }

        /// <summary>
        /// Set the additive tint for the specified instance.
        /// </summary>
        /// <param name="instanceId">the instance id</param>
        /// <param name="color">the color to be added</param>
        /// <param name="uploadImmediatly">if data has to be immediatly uploaded to the GPU. if <c>false</c> rember to call <seealso cref="UpdateAdditiveTintForAllInstances"/></param>
        public void SetAdditiveTintPerInstance(int instanceId, Vector4 color, bool uploadImmediatly = false)
        {
            additiveColorData[instanceId * 4] = color.X;
            additiveColorData[instanceId * 4 + 1] = color.Y;
            additiveColorData[instanceId * 4 + 2] = color.Z;
            additiveColorData[instanceId * 4 + 3] = color.W;
            if (uploadImmediatly)
                UpdateFloatBuffer(additiveColorBuffer, new float[] { color.X, color.Y, color.Z, color.W }, instanceId * 4);
        }


        /// <summary>
        /// Retrieve the current additive tint for the specified intance.
        /// </summary>
        /// <param name="instanceId">the instance id</param>
        /// <returns></returns>
        public Vector4 GetAdditiveTintPerInstance(int instanceId)
        {
            float x = additiveColorData[instanceId * 4];
            float y = additiveColorData[instanceId * 4 + 1];
            float z = additiveColorData[instanceId * 4 + 2];
            float w = additiveColorData[instanceId * 4 + 3];
            return new Vector4(x, y, z, w);
        }


        /// <summary>
        /// Upload all the instance additive tint data to the GPU.
        /// </summary>
        public void UpdateAdditiveTintForAllInstances()
        {
            UpdateFloatBuffer(additiveColorBuffer, additiveColorData);
        }

        /// <summary>
        /// Set the multiply tint for the specified instance.
        /// </summary>
        /// <param name="instanceId">the instance id</param>
        /// <param name="color">the color to be added</param>
        /// <param name="uploadImmediatly">if data has to be immediatly uploaded to the GPU. if <c>false</c> rember to call <seealso cref="UpdateMultiplyTintForAllInstance"/></param>
        public void SetMultiplyTintPerInstance(int instanceId, Vector4 color, bool uploadImmediatly = false)
        {
            multiplyColorData[instanceId * 4] = color.X;
            multiplyColorData[instanceId * 4 + 1] = color.Y;
            multiplyColorData[instanceId * 4 + 2] = color.Z;
            multiplyColorData[instanceId * 4 + 3] = color.W;
            if (uploadImmediatly)
                UpdateFloatBuffer(multiplyColorBuffer, new float[] { color.X, color.Y, color.Z, color.W }, instanceId * 4);
        }


        /// <summary>
        /// Retrieve the current multiply tint for the specified intance.
        /// </summary>
        /// <param name="instanceId">the instance id</param>
        /// <returns></returns>
        public Vector4 GetMultiplyTintPerInstance(int instanceId)
        {
            float x = multiplyColorData[instanceId * 4];
            float y = multiplyColorData[instanceId * 4 + 1];
            float z = multiplyColorData[instanceId * 4 + 2];
            float w = multiplyColorData[instanceId * 4 + 3];
            return new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Upload all the instance multiply tint data to the GPU.
        /// </summary>
        public void UpdateMultiplyTintForAllInstance()
        {
            UpdateFloatBuffer(multiplyColorBuffer, multiplyColorData);
        }

        override public void DrawWireframe(Vector4 color, float tickness = 0.02f)
        {
            throw new Exception("Wireframe mode is not supported by this class!");
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
