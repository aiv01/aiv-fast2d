using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D.Collision;
using OpenTK;

namespace Aiv.Fast2D.Example.DCE
{
    public static class DrawCircleExample
    {
        public static void Run()
        {
            Window window = new Window(400, 300, "Draw Circle Example");

            Circle filledCircle = new Circle(50);
            filledCircle.Thickness = 1;
            filledCircle.Position = new OpenTK.Vector2(0,0);

            Circle nonFilledCircle = new Circle(100);
            nonFilledCircle.Thickness = 0.2f;
            nonFilledCircle.Position = new OpenTK.Vector2(100, 100);

            Sprite s = new Sprite(100, 100);
            s.position = new OpenTK.Vector2(0, 0);

            // Create colliders shapes associated with those 2 circles
            CircleCollider c1 = new CircleCollider(filledCircle);
            CircleCollider c2 = new CircleCollider(nonFilledCircle);

            while (window.IsOpened)
            {
                // Try to move the circle
                if (window.GetKey(KeyCode.A))
                    filledCircle.Position -= new Vector2(100) * window.DeltaTime;
                if (window.GetKey(KeyCode.D))
                    filledCircle.Position += new Vector2(100) * window.DeltaTime;

                // Move the collider to stick with the circle graphics
                // You might want to create your player (or anything else) class and deal with this internally.
                c1.Position = filledCircle.Position;
                c2.Position = nonFilledCircle.Position;

                // resolve a possible collision;
                var resolveVector = CircleCollider.ResolveOverlap(c1, c2);
                c1.Position += resolveVector;
                filledCircle.Position += resolveVector;

                // Check if those objects collide
                bool overlap = c1.OverlapsWith(c2);

                // Finally render stuff
                s.DrawColor(new OpenTK.Vector4(1, 0, 1, 1));
                filledCircle.DrawColor( overlap ? new OpenTK.Vector4(1, 0, 0, 1) : new OpenTK.Vector4(0, 0, 1, 1));
                nonFilledCircle.DrawColor(new OpenTK.Vector4(0, 1, 0, 1));

                // After normal rendering draw the colliders debug
                c1.DrawDebug();
                c2.DrawDebug();

                window.Update();
            }
        }
    }
}
