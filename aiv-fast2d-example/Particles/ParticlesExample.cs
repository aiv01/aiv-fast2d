using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Aiv.Fast2D.Example.PRE
{
    static class ParticlesExample
    {
        public static void Run()
        {
            
            Window window = new Window(800, 600, "Units based example");
            
            Particles solid = new Particles(3, 3, 100);
            solid.position = new Vector2(400, 200);

            Particles textured = new Particles(20, 20, 20);
            textured.position = new Vector2(400, 400);

            Texture txt = new Texture("Particles/Assets/smile.png");

            while (window.IsOpened)
            {
                solid.DrawColor(0, 255, 0, 200, window.DeltaTime);

                textured.DrawTexture(txt, window.DeltaTime);
                
                window.Update();
            }
        }
    }
}
