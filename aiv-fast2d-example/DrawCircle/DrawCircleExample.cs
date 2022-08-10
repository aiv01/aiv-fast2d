using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aiv.Fast2D.Example.DCE
{
    public static class DrawCircleExample
    {
        public static void Run()
        {
            Window window = new Window(400, 300, "Draw Circle Example");

            Circle filledCircle = new Circle(100);
            filledCircle.Thickness = 1;
            filledCircle.Position = new OpenTK.Vector2(200,200);

            Circle nonFilledCircle = new Circle(200);
            nonFilledCircle.Thickness = 0.2f;
            nonFilledCircle.Fade = 0.2f; // quite a bit of fade, keep it low
            nonFilledCircle.Position = new OpenTK.Vector2(100, 100);

            while (window.IsOpened)
            {

                filledCircle.DrawColor(new OpenTK.Vector4(1, 0, 0, 1));
                nonFilledCircle.DrawColor(new OpenTK.Vector4(0, 1, 0, 1));

                window.Update();
            }
        }
    }
}
