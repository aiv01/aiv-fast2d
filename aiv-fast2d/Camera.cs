using System;
#if __SHARPDX__
using SharpDX;
using Matrix4 = SharpDX.Matrix;
#else
using OpenTK;
#endif

namespace Aiv.Fast2D
{
	public class Camera
	{
		public Vector2 position;
		public Vector2 pivot = Vector2.Zero;

		public virtual bool HasProjection
		{
			get
			{
				return false;
			}
		}


		public Camera(float x, float y)
		{
			this.position = new Vector2(x, y);

			if (Window.Current.CurrentCamera == null)
				Window.Current.SetCamera(this);
		}

		public Camera() : this(0, 0) { }

		public virtual Matrix4 Matrix()
		{
#if __SHARPDX__
            return Matrix4.Translation(-this.position.X + this.pivot.X, -this.position.Y + this.pivot.Y, 0);
#else
			return Matrix4.CreateTranslation(-this.position.X + this.pivot.X, -this.position.Y + this.pivot.Y, 0);
#endif
		}

		public virtual Matrix4 ProjectionMatrix()
		{
			return Matrix4.Identity;
		}
	}
}

