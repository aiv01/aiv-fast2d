using System;
using OpenTK;


namespace Aiv.Fast2D.Example
{
    class Program
    {

        class ExampleLogger : ILogger
        {
            public void Log(string message)
            {
                Console.WriteLine("[Aiv.Fast2D.Example - {0}] {1}", DateTime.Now, message);
            }
        }

        static void Main(string[] args)
        {

            Context.logger = new ExampleLogger();

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

            RenderTexture fake = new RenderTexture(1, 1);
            fake.Dispose();

            Sprite monitor = new Sprite(100, 100);
            monitor.position = new Vector2(400, 200);

            int index = 0;
            float t = 0;

            window.SetClearColor(100, 100, 100);

            int counter = 0;

            ParticleSystem particleSystem = new ParticleSystem(2, 2, 100);
            particleSystem.position = new Vector2(400, 200);

            Rope rope = new Rope(400, 3);
            rope.position = new Vector2(400, 200);
            rope.SetDestination(new Vector2(400, 400));

            ship2.pivot = new Vector2(alien.Width / 20, height / 2);

            ParticleSystem particleSystem2 = new ParticleSystem(1, 2, 50);

            Mesh triangle = new Mesh();
            triangle.v = new float[] { 100, 100, 50, 200, 150, 200 };
            triangle.UpdateVertex();
            triangle.uv = new float[] { 0.5f, 0.5f, 0, 0, 1, 0 };
            triangle.UpdateUV();

            Mesh farTriangles = new Mesh();
            farTriangles.v = new float[]
            {
                300, 100,
                200, 200,
                400, 200,

                400, 400,
                300, 500,
                500, 500
            };
            farTriangles.UpdateVertex();


            
            Texture alien2 = new Texture("aiv_fast2d_example.Assets.2.png");
            RenderTexture maskedAlien = new RenderTexture(alien2.Width, alien2.Height);
            Sprite spriteMask = new Sprite(50, 50);

            Texture circleMask = new Texture("aiv_fast2d_example.Assets.mask_circle.png");
            Texture circleMask2 = new Texture("aiv_fast2d_example.Assets.mask_circle2.png");

            Sprite maskedObject = new Sprite(alien2.Width, alien2.Height);
            maskedObject.position = new Vector2(200, 200);
            Sprite maskedBackground = new Sprite(alien2.Width, alien2.Height);

            PostProcessingEffect mainEffect = window.AddPostProcessingEffect(new GrayscaleEffect());
            mainEffect.enabled = false;

            window.AddPostProcessingEffect(new MaskEffect("aiv_fast2d_example.Assets.mask_circle.png"));

            window.AddPostProcessingEffect(new BlackBands());
            
            window.AddPostProcessingEffect(new RedBands());

            // insert a postprocessing effect at the specific position
            window.SetPostProcessingEffect(1, new WASDEffect());

            while (window.opened)
            {

                for (int i = 0; i < tiles2.Instances; i++)
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
                window.RenderTo(screen);

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
                window.RenderTo(null);


                monitor.DrawTexture(screen);



                Vector2 newPosition = tiles.GetPosition(2) - Vector2.One * 20f * window.deltaTime;
                tiles.SetPosition(2, newPosition);

                tiles.DrawSolidColor(0, 1, 1, 1);

                tiles2.position.X += 30 * window.deltaTime;
                tiles2.DrawSolidColor(1, 1, 0, 1);

                particleSystem.Update(window);

                //rope.SetDestination(window.mousePosition);
                rope.UpdatePhysics(window);
                rope.DrawSolidColor(1f, 0f, 1f, 1f);

                ship2.position = rope.position + rope.Point2;
                ship2.SetAdditiveTint(-1f, 1f, -1f, 0);
                ship2.DrawTexture(alien, x, y, alien.Width / 10, height);

                particleSystem2.position = ship2.position;
                particleSystem2.Update(window);


                

                farTriangles.DrawColor(0f, 0f, 1f, 1f);

                triangle.v[4] = window.mouseX;
                triangle.v[5] = window.mouseY;
                triangle.UpdateVertex();

                if (window.HasFocus)
                {
                    triangle.DrawTexture(alien);
                }
                else
                {
                    window.SetScissorTest(window.Width/2 - 200, window.Height/2 - 200, 400, 400);
                    triangle.DrawColor(1f, 0f, 1f, 1f);
                    window.SetScissorTest(false);
                }

                window.SetClearColor(0f, 0f, 0f, 0f);
                window.RenderTo(maskedAlien);
               
                maskedBackground.DrawTexture(alien2);
                window.SetMaskedBlending();
                spriteMask.scale = Vector2.One;
                spriteMask.position = new Vector2(150, 100);
                spriteMask.DrawTexture(circleMask);

                spriteMask.scale = new Vector2(2f, 2.7f);
                spriteMask.position = new Vector2(180, 280);
                spriteMask.DrawTexture(circleMask2);

                window.SetAlphaBlending();
                window.SetClearColor(0.5f, 0.5f, 0.5f);
                window.RenderTo(null);

                maskedObject.DrawTexture(maskedAlien);

                if (window.GetKey(KeyCode.Space))
                {
                    mainEffect.enabled = true;
                }

                if (window.GetKey(KeyCode.Return))
                {
                    mainEffect.enabled = false;
                }

                window.Update();
            }
        }
    }
}
