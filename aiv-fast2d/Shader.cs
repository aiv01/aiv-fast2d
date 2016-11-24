using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections.Generic;


namespace Aiv.Fast2D
{
	public class Shader : IDisposable
	{

		private int programId;

		private Dictionary<string, int> uniformCache;

		private bool disposed;

		public Shader (string vertex, string fragment)
		{
			int vertexShaderId = GL.CreateShader (ShaderType.VertexShader);
			int fragmentShaderId = GL.CreateShader (ShaderType.FragmentShader);

			GL.ShaderSource (vertexShaderId, vertex);
			GL.CompileShader (vertexShaderId);

			GL.ShaderSource (fragmentShaderId, fragment);
			GL.CompileShader (fragmentShaderId);

			this.programId = GL.CreateProgram ();
			GL.AttachShader (programId, vertexShaderId);
			GL.AttachShader (programId, fragmentShaderId);

			GL.LinkProgram (programId);

			GL.DetachShader (programId, vertexShaderId);
			GL.DetachShader (programId, fragmentShaderId);

			GL.DeleteShader (vertexShaderId);
			GL.DeleteShader (fragmentShaderId);

			this.uniformCache = new Dictionary<string, int> ();

		}

		public void Use ()
		{
			GL.UseProgram (this.programId);
		}

		private int GetUniform(string name) {
			int uid = -1;
			if (this.uniformCache.ContainsKey (name)) {
				uid = this.uniformCache [name];
			} else {
				uid = GL.GetUniformLocation (this.programId, name);
				this.uniformCache [name] = uid;
			}
			return uid;
		}

		public void SetUniform (string name, Matrix4 m)
		{
			this.Use ();
			int uid = this.GetUniform (name);
			GL.UniformMatrix4 (uid, false, ref m);
		}

		public void SetUniform (string name, int n)
		{
			this.Use ();
			int uid = this.GetUniform (name);
			GL.Uniform1 (uid, n);
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

        public void Dispose ()
		{
			if (disposed)
				return;
			GL.DeleteProgram (this.programId);
			disposed = true;
		}

		~Shader() {
			if (disposed)
				return;
			Context.shaderGC.Add (this.programId);
			disposed = true;
		}
	}
}

