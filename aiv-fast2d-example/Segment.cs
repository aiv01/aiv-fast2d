using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Aiv.Fast2D.Example
{
    public class Segment : Sprite
    {
        private Vector2 point1;
        private Vector2 point2;

        private float lineWidth;

        public Vector2 Point1
        {
            get
            {
                return this.point1;
            }
            set
            {
                this.point1 = value;
                this.UpdatePoints();
            }
        }

        public Vector2 Point2
        {
            get
            {
                return this.point2;
            }
            set
            {
                this.point2 = value;
                this.UpdatePoints();
            }
        }

        public float LineWidth
        {
            get
            {
                return lineWidth;
            }
            set
            {
                lineWidth = value;
                this.UpdatePoints();
            }
        }

        public Segment(float x1, float y1, float x2, float y2, float width) : base(width, Math.Abs(y2 - y1))
        {
            point1 = new Vector2(x1, y1);
            point2 = new Vector2(x2, y2);
            this.lineWidth = width;
            this.UpdatePoints();
        }

        private void UpdatePoints()
        {
            // compute line points
            Vector2 lineVector = point2 - point1;
            // get the right vector of the line
            Vector2 right = new Vector2(-lineVector.Y, lineVector.X).Normalized();

            Vector2 leftStart = point1 + right * -lineWidth/2f;
            Vector2 rightStart = point1 + right * lineWidth/2f;

            Vector2 leftEnd = point2 + right * -lineWidth/2f;
            Vector2 rightEnd = point2 + right * lineWidth/2f;

            this.v = new float[]
            {
                leftStart.X, leftStart.Y,
                rightStart.X, rightStart.Y,
                leftEnd.X, leftEnd.Y,
                rightStart.X, rightStart.Y,
                rightEnd.X,rightEnd.Y,
                leftEnd.X, leftEnd.Y
            };
            this.UpdateVertex();
        }

        
    }
}
