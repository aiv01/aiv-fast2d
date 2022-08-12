using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aiv.Fast2D.Collision
{
    public class CircleCollider
    {
        private float radius;

        // todo: this should be batched
        private Circle debugCircle;

        public CircleCollider(float _radius)
        {
            radius = _radius;
            debugCircle = new Circle(radius);
            debugCircle.Thickness = 0.03f;
        }

        public CircleCollider(Circle inCircle) : this(inCircle?.Radius ?? 0)
        {
            if(inCircle == null)
            {
                Console.WriteLine("Can't initialize a CircleCollider with a null reference.");
                return;
            }
        }

        // Unless is for a specific case try to keep this aligned with the visual sprite for accurate collisions.
        public Vector2 Position;

        /// <summary>
        /// Draws a circle edge representing the debug of this circle
        /// </summary>
        public void DrawDebug()
        {
            debugCircle.Position = Position;
            debugCircle.DrawColor(new Vector4(1,1,0,1));
        }

        public bool OverlapsWith(CircleCollider other)
        {
            if(other == null)
            {
                return false;
            }

            // todo this needs to be the center of the circle, it's pivot by default is the center
            float squaredDist = (Position - other.Position).Length;
            float minDistToCollide = radius + other.radius;
            return squaredDist <= minDistToCollide;
        }

        /// <summary>
        /// Returns a vector that applied to c1's position will resolve the collision (if there is one) with c2.
        /// C1 should be the moving object or the resulting vector will be flipped.
        /// </summary>
        /// <returns></returns>
        public static Vector2 ResolveOverlap(CircleCollider c1, CircleCollider c2)
        {
            if(!c1.OverlapsWith(c2))
            {
                return Vector2.Zero;
            }

            var dir = c1.Position - c2.Position;
            float dist = dir.Length;
            float nonOverlappingDistance = c1.radius + c2.radius;
            return dir.Normalized() * (nonOverlappingDistance - dist);
        }
    }
}
