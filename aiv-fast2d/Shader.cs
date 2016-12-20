using System;
#if !__MOBILE__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES30;
#endif
using OpenTK;
using System.Collections.Generic;


namespace Aiv.Fast2D
{
    public class Shader : IDisposable
    {

        public class CompilationException : Exception
        {
            public CompilationException(string message) : base(message) { }
        }

        public class UnsupportedVersionException : Exception
        {
            public UnsupportedVersionException(string message) : base(message) { }
        }

        private int programId;

        private Dictionary<string, int> uniformCache;

        private bool disposed;

        public Shader(string vertexModern, string fragmentModern, string vertexObsolete = null, string fragmentObsolete = null, string[] attribs = null)
        {
            string vertex = vertexModern;
            string fragment = fragmentModern;

            if (Window.IsObsolete)
            {
                if (vertexObsolete == null || fragmentObsolete == null || attribs == null)
                {
                    throw new UnsupportedVersionException("Unsupported OpenGL version for this shader");
                }
                vertex = vertexObsolete;
                fragment = fragmentObsolete;
#if !__MOBILE__
                // obsolete Desktop OpenGL does not have medium precision
                fragment = fragment.Replace("precision mediump float;", "");
#endif
            }
#if __MOBILE__
            vertex = vertex.Trim(new char[] { '\r', '\n' }).Replace("330 core", "300 es");
            fragment = fragment.Trim(new char[] { '\r', '\n' }).Replace("330 core", "300 es");
#endif
            int vertexShaderId = GL.CreateShader(ShaderType.VertexShader);
            int fragmentShaderId = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vertexShaderId, vertex);
            GL.CompileShader(vertexShaderId);

            string vertexShaderCompilationError = GL.GetShaderInfoLog(vertexShaderId);
            if (vertexShaderCompilationError != null && vertexShaderCompilationError != "")
            {
                throw new CompilationException(vertexShaderCompilationError);
            }

            GL.ShaderSource(fragmentShaderId, fragment);
            GL.CompileShader(fragmentShaderId);
            string fragmentShaderCompilationError = GL.GetShaderInfoLog(fragmentShaderId);
            if (fragmentShaderCompilationError != null && fragmentShaderCompilationError != "")
            {
                throw new CompilationException(fragmentShaderCompilationError);
            }

            this.programId = GL.CreateProgram();
            GL.AttachShader(programId, vertexShaderId);
            GL.AttachShader(programId, fragmentShaderId);

            if (Window.IsObsolete)
            {
                if (attribs != null)
                {
                    for (int i = 0; i < attribs.Length; i++)
                    {
                        GL.BindAttribLocation(programId, i, attribs[i]);
                    }
                }
            }

            GL.LinkProgram(programId);

            GL.DetachShader(programId, vertexShaderId);
            GL.DetachShader(programId, fragmentShaderId);

            GL.DeleteShader(vertexShaderId);
            GL.DeleteShader(fragmentShaderId);

            this.uniformCache = new Dictionary<string, int>();

        }

        public void Use()
        {
            GL.UseProgram(this.programId);
        }

        public int GetAttribLocation(string name)
        {
            return GL.GetAttribLocation(this.programId, name);
        }

        public void BindAttribLocation(int attribId, string name)
        {
            GL.BindAttribLocation(this.programId, attribId, name);
        }

        private int GetUniform(string name)
        {
            int uid = -1;
            if (this.uniformCache.ContainsKey(name))
            {
                uid = this.uniformCache[name];
            }
            else {
                uid = GL.GetUniformLocation(this.programId, name);
                this.uniformCache[name] = uid;
            }
            return uid;
        }

        public void SetUniform(string name, Matrix4 m)
        {
            this.Use();
            int uid = this.GetUniform(name);
            GL.UniformMatrix4(uid, false, ref m);
        }

        public void SetUniform(string name, int n)
        {
            this.Use();
            int uid = this.GetUniform(name);
            GL.Uniform1(uid, n);
        }

        public void SetUniform(string name, float n)
        {
            this.Use();
            int uid = this.GetUniform(name);
            GL.Uniform1(uid, n);
        }

        public void SetUniform(string name, Vector4 value)
        {
            this.Use();
            int uid = this.GetUniform(name);
            GL.Uniform4(uid, value);
        }

        public void Dispose()
        {
            if (disposed)
                return;
            GL.DeleteProgram(this.programId);
            Window.Current.Log(string.Format("shader {0} deleted", this.programId));
            disposed = true;
        }

        ~Shader()
        {
            if (disposed)
                return;
            Window.Current.shaderGC.Add(this.programId);
            disposed = true;
        }
    }
}

