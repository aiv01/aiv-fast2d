using System;
using OpenTK;

namespace Aiv.Fast2D.Example
{
    class Program
    {

        static void Main(string[] args)
        {

            Window window = new Window(800, 600, "Aiv.Fast2D.Example");
            window.SetIcon("aiv_fast2d_example.Assets.2.ico");

            window.SetCursor(false);

            Texture logoAiv = new Texture("aiv_fast2d_example.Assets.LogoAIV.png");


            Texture alien = new Texture("aiv_fast2d_example.Assets.owl.png");


            Sprite logo = new Sprite(logoAiv.Width, logoAiv.Height);

            int height = 150;

            Sprite ship = new Sprite(alien.Width / 10, height);

            Sprite ship2 = new Sprite(alien.Width / 10, height);

            Sprite square = new Sprite(100, 100);

            InstancedSprite tiles = new InstancedSprite(100, 100, 3);
            tiles.SetPosition(0, new Vector2(150, 100));
            tiles.SetPosition(1, new Vector2(200, 200));
            tiles.SetPosition(2, new Vector2(500, 500));

            tiles.SetScale(0, new Vector2(0.5f, 0.5f));
            tiles.SetScale(1, new Vector2(1.5f, 1.5f));

            InstancedSprite tiles2 = new InstancedSprite(20, 20, 30);

            RenderTexture screen = new RenderTexture(800, 600);

            Sprite monitor = new Sprite(100, 100);
            monitor.position = new Vector2(400, 200);

            int index = 0;
            float t = 0;

            window.SetClearColor(100, 100, 100);

            int counter = 0;

            ParticleSystem particleSystem = new ParticleSystem(2, 2, 100);
            particleSystem.position = new Vector2(400, 200);

            while (window.opened)
            {

                for(int i=0;i<tiles2.Instances;i++)
                {
                    tiles2.SetPosition(i, new Vector2(20 * i, 20 * i), true);
                }
                tiles2.UpdatePositions();

                

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


                square.DrawSolidColor(1f, 0, 0, 0.5f);

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

                if (window.GetKey(KeyCode.T))
                {
                    window.Title = string.Format("Counter = {0}", counter++);
                }

                if (window.GetKey(KeyCode.R))
                {
                    ship.SetAdditiveTint(1f, -1f, -1f, 0);
                    //ship.SetMultiplyTint(2f, 0, 0, 1);
                }

                if (window.GetKey(KeyCode.N))
                {
                    ship.SetAdditiveTint(0, 0, 0, 0);
                    //ship.SetMultiplyTint(2f, 0, 0, 1);
                }

                window.SetClearColor(100, 100, 100);
                RenderTexture.To(null);

              
                monitor.DrawTexture(screen);

                ship2.position = new Vector2(300, 300);
                ship2.SetAdditiveTint(-1f, 1f, -1f, 0);
                ship2.DrawTexture(alien, x, y, alien.Width / 10, height);

                Vector2 newPosition = tiles.GetPosition(2) - Vector2.One * 20f * window.deltaTime;
                tiles.SetPosition(2, newPosition);

                tiles.DrawSolidColor(0, 1, 1, 1);

                tiles2.position.X += 30 * window.deltaTime;
                tiles2.DrawSolidColor(1, 1, 0, 1);

                particleSystem.Update(window);


                window.Update();
            }
        }
    }
}
