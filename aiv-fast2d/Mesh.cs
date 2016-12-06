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

        private List<int> customBuffers;

        private bool disposed;

        public bool hasVertexColors;

        public Shader shader;

        public Vector2 position = Vector2.Zero;
        public Vector2 scale = Vector2.One;

        public Vector2 pivot = Vector2.Zero;

        public Camera camera;

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
uniform sampler2D tex;

in vec2 uvout;
in vec4 vertex_color;

out vec4 out_color;

void main(){
       out_color = texture(tex, uvout) + color + vertex_color;
}";

        private static Shader simpleShader = new Shader(simpleVertexShader, simpleFragmentShader);

        public Mesh(Shader shader = null)
        {
            this.customBuffers = new List<int>();
#if !__MOBILE__
            this.vertexArrayId = GL.GenVertexArray();
#else
            int []tmpStore = new int[1];
            GL.GenVertexArrays(1, tmpStore);
            this.vertexArrayId = tmpStore[0];
#endif
            this.Bind();
#if !__MOBILE__
            this.vBufferId = GL.GenBuffer();
#else
            GL.GenBuffers(1, tmpStore);
            this.vBufferId = tmpStore[0];
#endif
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vBufferId);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

#if !__MOBILE__
            this.uvBufferId = GL.GenBuffer();
#else
            GL.GenBuffers(1, tmpStore);
            this.uvBufferId = tmpStore[0];
#endif
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.uvBufferId);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

#if !__MOBILE__
            this.vcBufferId = GL.GenBuffer();
#else
            GL.GenBuffers(1, tmpStore);
            this.vcBufferId = tmpStore[0];
#endif
            GL.EnableVertexAttribArray(2);
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vcBufferId);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            if (shader == null)
            {
                shader = simpleShader;
                shader.SetUniform("tex", 0);
            }
            this.shader = shader;
            this.noMatrix = false;
            this.hasVertexColors = true;
        }

        protected int NewFloatBuffer(int attribArrayId, int elementSize, float[] data, int divisor = 0)
        {
            this.Bind();
#if !__MOBILE__
            int bufferId = GL.GenBuffer();
#else
            int []tmpStore = new int[1];
            GL.GenBuffers(1, tmpStore);
            int bufferId = tmpStore[0];
#endif
            GL.EnableVertexAttribArray(attribArrayId);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
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
            this.customBuffers.Add(bufferId);
            return bufferId;
        }

        protected void UpdateFloatBuffer(int bufferId, float[] data, int offset = 0)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);
#if !__MOBILE__
            GL.BufferSubData<float>(BufferTarget.ArrayBuffer, (IntPtr)(offset * sizeof(float)), data.Length * sizeof(float), data);
#else
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(offset * sizeof(float)),(IntPtr)(data.Length * sizeof(float)), data);
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
            GL.BindVertexArray(this.vertexArrayId);
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


            if (this.camera != null)
            {
                m *= this.camera.Matrix();
            }
            else if (Context.mainCamera != null)
            {
                m *= Context.mainCamera.Matrix();
            }

            Matrix4 foo = Context.currentWindow.OrthoMatrix;

            Matrix4 mvp = m * Context.currentWindow.OrthoMatrix;

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
        public virtual void DrawColor(float r, float g, float b, float a)
        {
            this.shader.SetUniform("color", new Vector4(r, g, b, a));
            this.Draw();
            // always reset the color
            this.shader.SetUniform("color", Vector4.Zero);
        }

        public virtual void DrawColor(int r, int g, int b, int a)
        {
            DrawColor(r / 255f, g / 255f, b / 255f, a / 255f);
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
            Context.Log(string.Format("buffer {0} deleted", this.vBufferId));
            Context.Log(string.Format("buffer {0} deleted", this.uvBufferId));
            Context.Log(string.Format("buffer {0} deleted", this.vcBufferId));

            foreach (int customBufferId in this.customBuffers)
            {
#if !__MOBILE__
                GL.DeleteBuffer(customBufferId);
#else
                GL.DeleteBuffers(1, new int[] { customBufferId });
#endif
                Context.Log(string.Format("buffer {0} deleted", customBufferId));
            }

            Context.Log(string.Format("vertexArray {0} deleted", this.vertexArrayId));
            disposed = true;
        }

        ~Mesh()
        {
            if (disposed)
                return;
            Context.bufferGC.Add(this.vBufferId);
            Context.bufferGC.Add(this.uvBufferId);
            Context.bufferGC.Add(this.vcBufferId);
            foreach(int customBufferId in this.customBuffers)
            {
                Context.bufferGC.Add(customBufferId);
            }
            Context.vaoGC.Add(this.vertexArrayId);

            disposed = true;
        }

    }
}

