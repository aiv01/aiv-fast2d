using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

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

        public Mesh(Shader shader)
        {
            this.vertexArrayId = GL.GenVertexArray();
            this.Bind();
            this.vBufferId = GL.GenBuffer();
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vBufferId);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
            this.uvBufferId = GL.GenBuffer();
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.uvBufferId);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
            this.shader = shader;
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
            GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(this.v.Length * sizeof(float)), this.v, BufferUsageHint.DynamicDraw);
        }

        public void UpdateUV()
        {
            if (this.uv == null)
                return;
            this.Bind();
            // we use dynamic drawing, could be inefficient for simpler cases, but improves performance in case of complex animations
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.uvBufferId);
            GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(this.uv.Length * sizeof(float)), this.uv, BufferUsageHint.DynamicDraw);
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
                Matrix4.CreateScale(this.scale.X, this.scale.Y, 1) *
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
            GL.DrawArrays(PrimitiveType.Triangles, 0, this.v.Length / 2);
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
            GL.DrawArrays(PrimitiveType.Triangles, 0, this.v.Length / 2);
        }


        // simple draw without textures (useful for subclasses)
        public void Draw(ShaderSetupHook hook=null)
        {
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
            GL.DrawArrays(PrimitiveType.Triangles, 0, this.v.Length / 2);
        }

        public void Dispose()
        {
            if (disposed)
                return;
            GL.DeleteBuffer(this.vBufferId);
            GL.DeleteBuffer(this.uvBufferId);
            GL.DeleteVertexArray(this.vertexArrayId);
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

