using System;
using OpenTK;
#if !__MOBILE__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES30;
#endif
using System.Collections.Generic;

namespace Aiv.Fast2D
{
    public class Mesh : IDisposable
    {

        private int vertexArrayId;

        public float[] v;
        public float[] uv;
        public float[] vc;

        private int vBufferId;
        private int uvBufferId;
        private int vcBufferId;

        private struct VertexAttrib
        {
            public int bufferId;
            public int elementSize;

            public VertexAttrib(int bufferId, int elementSize)
            {
                this.bufferId = bufferId;
                this.elementSize = elementSize;
            }
        }

        private Dictionary<int, VertexAttrib> customBuffers;

        private bool disposed;

        public bool hasVertexColors;

        public Shader shader;

        public Vector2 position = Vector2.Zero;
        public Vector2 scale = Vector2.One;

        public Vector2 pivot = Vector2.Zero;

        public Camera Camera;

        private float rotation;

        public float Rotation
        {
            get
            {
                return this.rotation;
            }
            set
            {
                this.rotation = value;
            }
        }

        public float EulerRotation
        {
            get
            {
                return this.rotation * 180f / (float)Math.PI;
            }
            set
            {
                this.rotation = value * (float)Math.PI / 180f;
            }
        }

        // this is called to set uniform in shaders
        public delegate void ShaderSetupHook(Mesh mesh);
        protected ShaderSetupHook shaderSetupHook;

        public bool noMatrix;

        protected bool requireUseTexture;

        protected int instances;
        public int Instances
        {
            get
            {
                return instances;
            }
        }

        private static string simpleVertexShader = @"
#version 330 core

layout(location = 0) in vec2 vertex;
layout(location = 1) in vec2 uv;
layout(location = 2) in vec4 vc;

uniform mat4 mvp;
out vec2 uvout;
out vec4 vertex_color;

void main(){
        gl_Position = mvp * vec4(vertex.xy, 0.0, 1.0);
        uvout = uv;
        vertex_color = vc;
}";
        private static string simpleFragmentShader = @"
#version 330 core

precision highp float;

uniform vec4 color;
uniform float use_texture;
uniform float use_wireframe;
uniform sampler2D tex;

in vec2 uvout;
in vec4 vertex_color;

out vec4 out_color;

void main(){
    if (use_texture > 0.0) {
        out_color = texture(tex, uvout);
        out_color += vec4(vertex_color.xyz * out_color.a, vertex_color.a);
    }
    else if (use_wireframe > 0.0) {
        if(any(lessThan(vertex_color.xyz, vec3(use_wireframe)))) {
            out_color = color;
        }
        else {
            out_color = vec4(0, 0, 0, 0);    
        }
        return;
    }
    else {
        out_color = vertex_color;
    }
    out_color += color;
}";

        private static string simpleVertexShaderObsolete = @"
attribute vec2 vertex;
attribute vec2 uv;
attribute vec4 vc;

uniform mat4 mvp;
varying vec2 uvout;
varying vec4 vertex_color;

void main(){
        gl_Position = mvp * vec4(vertex.xy, 0.0, 1.0);
        uvout = uv;
        vertex_color = vc;
}";
        private static string simpleFragmentShaderObsolete = @"
precision mediump float;

uniform vec4 color;
uniform float use_texture;
uniform float use_wireframe;
uniform sampler2D tex;

varying vec2 uvout;
varying vec4 vertex_color;


void main(){
    if (use_texture > 0.0) {
        gl_FragColor = texture2D(tex, uvout);
        gl_FragColor += vec4(vertex_color.xyz * gl_FragColor.a, vertex_color.a);
    }
    else if (use_wireframe > 0.0) {
        if(any(lessThan(vertex_color.xyz, vec3(use_wireframe)))) {
            gl_FragColor = color;
        }
        else {
            gl_FragColor = vec4(0, 0, 0, 0);    
        }
        return;
    }
    else {
        gl_FragColor = vertex_color;
    }
    gl_FragColor += color;
}";

        private static Shader simpleShader = new Shader(simpleVertexShader, simpleFragmentShader, simpleVertexShaderObsolete, simpleFragmentShaderObsolete, new string[] { "vertex", "uv", "vc" });

        public int NewVBO()
        {
#if __MOBILE__
            int[] tmpStore = new int[1];
            GL.GenBuffers(1, tmpStore);
            return tmpStore[0];
#else
            return GL.GenBuffer();
#endif
        }

        public int NewVAO()
        {
            int vao = -1;
            // use VAO on modern OpenGL
            if (!Window.IsObsolete)
            {
#if !__MOBILE__
                vao = GL.GenVertexArray();
#else
                int[] tmpStore = new int[1];
                GL.GenVertexArrays(1, tmpStore);
                vao = tmpStore[0];
#endif    
            }
            return vao;
        }

        public Mesh(Shader shader = null)
        {

            this.customBuffers = new Dictionary<int, VertexAttrib>();

            // use VAO if possible
            this.vertexArrayId = NewVAO();
            if (this.vertexArrayId > -1)
            {
                this.Bind();
            }

            // vertex
            this.vBufferId = NewVBO();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vBufferId);
            int vertexAttribId = 0;
            GL.EnableVertexAttribArray(vertexAttribId);
            GL.VertexAttribPointer(vertexAttribId, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);


            // uv
            this.uvBufferId = NewVBO();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.uvBufferId);
            int uvAttribId = 1;
            GL.EnableVertexAttribArray(uvAttribId);
            GL.VertexAttribPointer(uvAttribId, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);


            // vc
            this.vcBufferId = NewVBO();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vcBufferId);
            int vcAttribId = 2;
            GL.EnableVertexAttribArray(vcAttribId);
            GL.VertexAttribPointer(vcAttribId, 4, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);


            if (shader == null)
            {
                shader = simpleShader;
                shader.SetUniform("tex", 0);
            }

            this.shader = shader;

            this.noMatrix = false;
            this.hasVertexColors = true;
            this.requireUseTexture = true;
        }

        protected int NewFloatBuffer(int attribArrayId, int elementSize, float[] data, int divisor = 0)
        {
            this.Bind();

            int bufferId = NewVBO();
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);

            GL.EnableVertexAttribArray(attribArrayId);
            GL.VertexAttribPointer(attribArrayId, elementSize, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            if (divisor > 0)
            {
                GL.VertexAttribDivisor(attribArrayId, divisor);
            }
#if !__MOBILE__
            GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * sizeof(float)), data, BufferUsageHint.DynamicDraw);
#else
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * sizeof(float)), data, BufferUsage.DynamicDraw);
#endif
            this.customBuffers[attribArrayId] = new VertexAttrib(bufferId, elementSize);
            return bufferId;
        }

        protected void UpdateFloatBuffer(int bufferId, float[] data, int offset = 0)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
#if !__MOBILE__
            GL.BufferSubData<float>(BufferTarget.ArrayBuffer, (IntPtr)(offset * sizeof(float)), data.Length * sizeof(float), data);
#else
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(offset * sizeof(float)), (IntPtr)(data.Length * sizeof(float)), data);
#endif
        }

        public void Update()
        {
            this.UpdateVertex();
            this.UpdateUV();
            this.UpdateVertexColor();
        }

        public void UpdateVertex()
        {
            if (this.v == null)
                return;
            this.Bind();
            // we use dynamic drawing, could be inefficient for simpler cases, but improves performance in case of complex animations
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vBufferId);
#if !__MOBILE__
            GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(this.v.Length * sizeof(float)), this.v, BufferUsageHint.DynamicDraw);
#else
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(this.v.Length * sizeof(float)), this.v, BufferUsage.DynamicDraw);
#endif
        }

        public void UpdateUV()
        {
            if (this.uv == null)
                return;
            this.Bind();
            // we use dynamic drawing, could be inefficient for simpler cases, but improves performance in case of complex animations
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.uvBufferId);
#if !__MOBILE__
            GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(this.uv.Length * sizeof(float)), this.uv, BufferUsageHint.DynamicDraw);
#else
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(this.uv.Length * sizeof(float)), this.uv, BufferUsage.DynamicDraw);
#endif
        }

        public void UpdateVertexColor()
        {
            if (this.vc == null)
                return;
            this.Bind();
            // we use dynamic drawing, could be inefficient for simpler cases, but improves performance in case of complex animations
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vcBufferId);
#if !__MOBILE__
            GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(this.vc.Length * sizeof(float)), this.vc, BufferUsageHint.DynamicDraw);
#else
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(this.vc.Length * sizeof(float)), this.vc, BufferUsage.DynamicDraw);
#endif
        }

        public void Bind()
        {
            if (!Window.IsObsolete)
            {
                GL.BindVertexArray(this.vertexArrayId);
            }
            else
            {
                // vertex
                GL.BindBuffer(BufferTarget.ArrayBuffer, this.vBufferId);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
                // uv
                GL.BindBuffer(BufferTarget.ArrayBuffer, this.uvBufferId);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
                // vc
                GL.BindBuffer(BufferTarget.ArrayBuffer, this.vcBufferId);
                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
                // custom buffers
                foreach (int customAttribId in this.customBuffers.Keys)
                {
                    int customBufferId = this.customBuffers[customAttribId].bufferId;
                    int customBufferElementSize = this.customBuffers[customAttribId].elementSize;
                    GL.BindBuffer(BufferTarget.ArrayBuffer, customBufferId);
                    GL.EnableVertexAttribArray(customAttribId);
                    GL.VertexAttribPointer(customAttribId, customBufferElementSize, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
                }
            }
        }

        // here we update translations, scaling and rotations
        private void ApplyMatrix()
        {
            if (this.noMatrix)
                return;

            // WARNING !!! OpenTK uses row-major while OpenGL uses column-major
            Matrix4 m =
                Matrix4.CreateTranslation(-this.pivot.X, -this.pivot.Y, 0) *
#if !__MOBILE__
                Matrix4.CreateScale(this.scale.X, this.scale.Y, 1) *
#else
                Matrix4.Scale(this.scale.X, this.scale.Y, 1) *
#endif
                Matrix4.CreateRotationZ(this.rotation) *
                // here we do not re-add the pivot, so translation is pivot based too
                Matrix4.CreateTranslation(this.position.X, this.position.Y, 0);


            if (this.Camera != null)
            {
                m *= this.Camera.Matrix();
            }
            else if (Window.Current.CurrentCamera != null)
            {
                m *= Window.Current.CurrentCamera.Matrix();
            }

            Matrix4 mvp = m * Window.Current.OrthoMatrix;

            // pass the matrix to the shader
            this.shader.SetUniform("mvp", mvp);
        }

        public virtual void DrawTexture(Texture tex)
        {
            if (this.v == null || this.uv == null)
                return;
            // upload fake vcs (if required) to avoid crashes
            if (this.vc == null && this.hasVertexColors)
            {
                this.vc = new float[this.v.Length * 2];
                this.UpdateVertexColor();
            }
            this.Bind();
            tex.Bind();
            if (this.requireUseTexture)
                this.shader.SetUniform("use_texture", 1f);
            this.shader.Use();
            if (this.shaderSetupHook != null)
            {
                this.shaderSetupHook(this);
            }
            this.ApplyMatrix();
            if (instances <= 1)
            {
#if !__MOBILE__
                GL.DrawArrays(PrimitiveType.Triangles, 0, this.v.Length / 2);
#else
                GL.DrawArrays(BeginMode.Triangles, 0, this.v.Length / 2);
#endif
            }
            else
            {
                GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, this.v.Length / 2, instances);
            }
        }

        // fast version of drawtexture without UV re-upload
        public virtual void DrawTexture(int textureId)
        {
            if (this.v == null || this.uv == null)
                return;
            // upload fake vcs (if required) to avoid crashes
            if (this.vc == null && this.hasVertexColors)
            {
                this.vc = new float[this.v.Length * 2];
                this.UpdateVertexColor();
            }
            this.Bind();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            if (this.requireUseTexture)
                this.shader.SetUniform("use_texture", 1f);
            this.shader.Use();
            if (this.shaderSetupHook != null)
            {
                this.shaderSetupHook(this);
            }
            this.ApplyMatrix();
            if (instances <= 1)
            {
#if !__MOBILE__
                GL.DrawArrays(PrimitiveType.Triangles, 0, this.v.Length / 2);
#else
                GL.DrawArrays(BeginMode.Triangles, 0, this.v.Length / 2);
#endif
            }
            else
            {
                GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, this.v.Length / 2, instances);
            }
        }


        // simply set the 'color' uniform of the shader
        public virtual void DrawColor(float r, float g, float b, float a = 1)
        {
            this.shader.SetUniform("color", new Vector4(r, g, b, a));
            this.Draw();
            // always reset the color
            this.shader.SetUniform("color", Vector4.Zero);
        }

        public virtual void DrawColor(int r, int g, int b, int a = 255)
        {
            DrawColor(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        public virtual void DrawWireframe(float r, float g, float b, float a = 1, float tickness = 0.02f)
        {
            if (this.v == null)
                return;

            // check if vcs neet to be temporarily stored
            // and check if the shader supports them
            if (!this.hasVertexColors)
                return;

            // store original vcs
            float[] vcs_storage = this.vc;

            int numVcs = this.v.Length * 2;
            this.vc = new float[numVcs];
            for (int i = 0; i < numVcs; i += 12)
            {
                this.vc[i] = 1f;
                this.vc[i + 1] = 0f;
                this.vc[i + 2] = 0f;

                this.vc[i + 4] = 0f;
                this.vc[i + 5] = 1f;
                this.vc[i + 6] = 0f;

                this.vc[i + 8] = 0f;
                this.vc[i + 9] = 0f;
                this.vc[i + 10] = 1f;
            }
            this.UpdateVertexColor();

            this.shader.SetUniform("color", new Vector4(r, g, b, a));
            this.shader.SetUniform("use_wireframe", tickness);
            this.Draw();
            this.shader.SetUniform("use_wireframe", -1f);
            // always reset the color
            this.shader.SetUniform("color", Vector4.Zero);
            // reset old vcs (could be null)
            this.vc = vcs_storage;
        }

        public virtual void DrawWireframe(int r, int g, int b, int a = 255, float tickness = 0.02f)
        {
            DrawWireframe(r / 255f, g / 255f, b / 255f, a / 255f, tickness);
        }

        // simple draw without textures (useful for subclasses)
        public void Draw(ShaderSetupHook hook = null)
        {
            if (this.v == null)
                return;
            // upload fake uvs (if required) to avoid crashes
            if (this.uv == null)
            {
                this.uv = new float[this.v.Length];
                this.UpdateUV();
            }
            // upload fake vcs (if required) to avoid crashes
            if (this.vc == null && this.hasVertexColors)
            {
                this.vc = new float[this.v.Length * 2];
                this.UpdateVertexColor();
            }
            this.Bind();
            // clear current texture
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            if (this.requireUseTexture)
                this.shader.SetUniform("use_texture", -1f);

            this.shader.Use();
            if (hook != null)
            {
                hook(this);
            }
            else if (this.shaderSetupHook != null)
            {
                this.shaderSetupHook(this);
            }

            this.ApplyMatrix();
            if (instances <= 1)
            {
#if !__MOBILE__
                GL.DrawArrays(PrimitiveType.Triangles, 0, this.v.Length / 2);
#else
                GL.DrawArrays(BeginMode.Triangles, 0, this.v.Length / 2);
#endif
            }
            else
            {
                GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, this.v.Length / 2, instances);
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
#if !__MOBILE__
            GL.DeleteBuffer(this.vBufferId);
            GL.DeleteBuffer(this.uvBufferId);
            GL.DeleteBuffer(this.vcBufferId);
            GL.DeleteVertexArray(this.vertexArrayId);
#else
            GL.DeleteBuffers(3, new int[] { this.vBufferId, this.uvBufferId, this.vcBufferId });
            GL.DeleteVertexArrays(1, new int[] { this.vertexArrayId });
#endif
            Window.Current.Log(string.Format("buffer {0} deleted", this.vBufferId));
            Window.Current.Log(string.Format("buffer {0} deleted", this.uvBufferId));
            Window.Current.Log(string.Format("buffer {0} deleted", this.vcBufferId));

            foreach (VertexAttrib customAttrib in this.customBuffers.Values)
            {
#if !__MOBILE__
                GL.DeleteBuffer(customAttrib.bufferId);
#else
                GL.DeleteBuffers(1, new int[] { customAttrib.bufferId });
#endif
                Window.Current.Log(string.Format("buffer {0} deleted", customAttrib.bufferId));
            }

            Window.Current.Log(string.Format("vertexArray {0} deleted", this.vertexArrayId));
            disposed = true;
        }

        ~Mesh()
        {
            if (disposed)
                return;
            Window.Current.bufferGC.Add(this.vBufferId);
            Window.Current.bufferGC.Add(this.uvBufferId);
            Window.Current.bufferGC.Add(this.vcBufferId);
            foreach (VertexAttrib customAttrib in this.customBuffers.Values)
            {
                Window.Current.bufferGC.Add(customAttrib.bufferId);
            }
            Window.Current.vaoGC.Add(this.vertexArrayId);

            disposed = true;
        }

    }
}

