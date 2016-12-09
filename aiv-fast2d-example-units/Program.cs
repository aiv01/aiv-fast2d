using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Aiv.Fast2D.Example.Units
{
    class Program
    {
        static void Main(string[] args)
        {
            Context.orthographicSize = 10;
            Window window = new Window(1024, 576, "Units based example");

            Mesh triangle = new Mesh();
            triangle.v = new float[]
            {
                2, 0,
                1, 1,
                3, 1
            };
            triangle.UpdateVertex();
            triangle.pivot = new Vector2(2, 0.5f);

            while (window.opened)
            {
                window.SetViewport(0, 576/2, 1024, 576 / 2);
                triangle.scale = new Vector2(1f, 1f);
                triangle.position = window.mousePosition;
                triangle.DrawColor(0f, 1f, 0f, 1f);

                window.SetViewport(0, 0, 1024, 576 / 2);
                triangle.scale = new Vector2(2f, 2f);
                triangle.position = window.mousePosition;
                triangle.DrawColor(1f, 1f, 0f, 1f);

                if (window.GetKey(KeyCode.Esc))
                    break;

                window.Update();
            }
        }
    }
}
