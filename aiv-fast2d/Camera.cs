using System;
using OpenTK;

namespace Aiv.Fast2D
{
	public class Camera
	{
		public Vector2 position;
		public Vector2 pivot = Vector2.Zero;

		public Camera (float x, float y)
		{
			this.position = new Vector2 (x, y);

			if (Context.mainCamera == null)
				Context.mainCamera = this;
		}

		public Matrix4 Matrix ()
		{
			return Matrix4.CreateTranslation (-this.position.X + this.pivot.X, -this.position.Y + this.pivot.Y, 0);
		}
	}
}

