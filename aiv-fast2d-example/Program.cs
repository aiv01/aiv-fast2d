using System;
using OpenTK;

namespace Aiv.Fast2D.Example
{
    class Program
    {

        static void Main(string[] args)
        {

            Window window = new Window(800, 600, "Aiv.Fast2D.Example");

            window.SetCursor(false);

            Texture logoAiv = new Texture("aiv_fast2d_example.Assets.LogoAIV.png");


            Texture alien = new Texture("aiv_fast2d_example.Assets.owl.png");


            Sprite logo = new Sprite(logoAiv.Width, logoAiv.Height);

            int height = 150;

            Sprite ship = new Sprite(alien.Width / 10, height);

            Sprite ship2 = new Sprite(alien.Width / 10, height);

            RenderTexture screen = new RenderTexture(800, 600);

            Sprite monitor = new Sprite(100, 100);
            monitor.position = new Vector2(400, 200);

            int index = 0;
            float t = 0;

            window.SetClearColor(100, 100, 100);

            while (window.opened)
            {

                ship.position.Y = 10;
                ship.position += new Vector2(5f, 0) * window.deltaTime;

                ship.scale = new Vector2(1f, 1f);

                t += window.deltaTime;
                if (t > 1f / 24f)
                {
                    index++;
                    if (index >= 51)
                        index = 0;
                    t = 0;
                }
                int x = (index % 10) * (alien.Width / 10);
                int y = (index / 10) * height;
                

                ship.DrawTexture(alien, x, y, alien.Width / 10, height);

                window.SetClearColor(255, 0, 0);
                RenderTexture.To(screen);

                logo.position.Y = 100;
                logo.position += new Vector2(50f, 0) * window.deltaTime;
                logo.scale = new Vector2(1f, 1f);
                logo.DrawTexture(logoAiv);

                if (window.GetKey(KeyCode.Esc))
                    break;

                if (window.GetKey(KeyCode.F))
                {
                    window.SetFullScreen(true);
                    window.SetResolution(1920, 1080);
                }

                window.SetClearColor(100, 100, 100);
                RenderTexture.To(null);

              
                monitor.DrawTexture(screen);

                ship2.position = new Vector2(300, 300);
                ship2.DrawTexture(alien, x, y, alien.Width / 10, height);



                window.Update();
            }
        }
    }
}
