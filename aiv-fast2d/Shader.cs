using System;
using System.Collections.Generic;
using OpenTK;

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

        public Shader(string vertexModern, string fragmentModern, string vertexObsolete = null, string fragmentObsolete = null, string[] attribs = null, int[] attibsSizes = null, string[] vertexUniforms = null, string[] fragmentUniforms = null)
        {

            this.programId = Graphics.CompileShader(vertexModern, fragmentModern, vertexObsolete, fragmentObsolete, attribs, attibsSizes, vertexUniforms, fragmentUniforms);
            this.uniformCache = new Dictionary<string, int>();
        }

        public void Use()
        {
            Graphics.BindShader(this.programId);
        }

        private int GetUniform(string name)
        {
            int uid = -1;
            if (this.uniformCache.ContainsKey(name))
            {
                uid = this.uniformCache[name];
            }
            else {
                uid = Graphics.GetShaderUniformId(this.programId, name);
                this.uniformCache[name] = uid;
            }
            return uid;
        }

        public void SetUniform(string name, Matrix4 m)
        {
            this.Use();
            int uid = this.GetUniform(name);
            Graphics.SetShaderUniform(uid, m);
        }

        public void SetUniform(string name, int n)
        {
            this.Use();
            int uid = this.GetUniform(name);
            Graphics.SetShaderUniform(uid, n);
        }

        public void SetUniform(string name, float n)
        {
            this.Use();
            int uid = this.GetUniform(name);
            Graphics.SetShaderUniform(uid, n);
        }

        public void SetUniform(string name, Vector4 value)
        {
            this.Use();
            int uid = this.GetUniform(name);
            Graphics.SetShaderUniform(uid, value);
        }

		public void SetUniform(string name, Vector3 value)
		{
			this.Use();
			int uid = this.GetUniform(name);
			Graphics.SetShaderUniform(uid, value);
		}

        public void Dispose()
        {
            if (disposed)
                return;
            Graphics.DeleteShader(this.programId);
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

