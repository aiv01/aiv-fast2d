using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Aiv.Fast2D.Example
{
    public class Rope : Segment
    {
        private float maxLength;
        private float currentLength;
        private float angle;
        private float angleVelocity;

        private bool angleSet;

        public Rope(float maxLength, float lineWidth) : base(0, 0, 0, 0, lineWidth)
        {
            this.maxLength = maxLength;
        }

        public void SetDestination(float x, float y)
        {
            SetDestination(new Vector2(x, y));
        }

        public void SetDestination(Vector2 destination)
        {
            destination -= this.position;
            Vector2 direction = destination.Normalized();
            currentLength = Math.Min(maxLength, destination.Length);
            Point2 = direction * currentLength;

            // update the angle between the rope and the up vector (if required)
            if (!angleSet)
            {
                angle = (float)Math.Acos(Vector2.Dot(new Vector2(0, 1), Point2.Normalized()));
                
                angleSet = true;
            }
        }

        public void UpdatePhysics(Window window)
        {
            if (window.GetKey(KeyCode.Right))
            {
                angle += 0.5f * window.deltaTime;
            }

            if (window.GetKey(KeyCode.Left))
            {
                angle -= 0.5f * window.deltaTime;
            }


            float angleAccel = -60.0f / currentLength * (float)Math.Sin(angle);
            angleVelocity += angleAccel * window.deltaTime;

            angle += angleVelocity * window.deltaTime;

            SetDestination(this.position.X + (float)Math.Sin(angle) * currentLength, this.position.Y + (float)Math.Cos(angle) * currentLength);
        }
    }
}
