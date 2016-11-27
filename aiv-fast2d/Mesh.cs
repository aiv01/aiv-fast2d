using System;
using OpenTK;
#if !__MOBILE__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES30;
#endif

namespace Aiv.Fast2D
{
    public class Mesh : IDisposable
    {

        private int vertexArrayId;

        public float[] v;
        public float[] uv;

        private int vBufferId;
        private int uvBufferId;

        private bool disposed;

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

        protected int instances;
        public int Instances
        {
            get
            {
                return instances;
            }
        }

        public Mesh(Shader shader)
        {
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
            this.shader = shader;
        }

        protected int NewFloatBuffer(int attribArrayId, int elementSize, float[] data, int divisor = 0)
        {
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

        public void Bind()
        {
            GL.BindVertexArray(this.vertexArrayId);
        }

        // here we update translations, scaling and rotations
        private void ApplyMatrix()
        {

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

        public void DrawTexture(Texture tex)
        {
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
        public void DrawTexture(int textureId)
        {
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


        // simple draw without textures (useful for subclasses)
        public void Draw(ShaderSetupHook hook = null)
        {
            this.Bind();
            // clear current texture
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            string status0 = Aiv.Fast2D.Context.GetError();
            this.shader.Use();
            if (hook != null)
            {
                hook(this);
            }
            else if (this.shaderSetupHook != null)
            {
                this.shaderSetupHook(this);
            }
            string status1 = Aiv.Fast2D.Context.GetError();
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
            GL.DeleteVertexArray(this.vertexArrayId);
#else
            GL.DeleteBuffers(2, new int[] { this.vBufferId, this.uvBufferId });
            GL.DeleteVertexArrays(1, new int[] { this.vertexArrayId });
#endif
            disposed = true;
        }

        ~Mesh()
        {
            if (disposed)
                return;
            Context.bufferGC.Add(this.vBufferId);
            Context.bufferGC.Add(this.uvBufferId);
            Context.vaoGC.Add(this.vertexArrayId);

            disposed = true;
        }

    }
}

