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

        public class Example : Game
        {

            private Texture logoAiv;
            private Texture alien;
            private Texture alien2;

            private Sprite logo;
            private Tilemap tileMap;
            private Sprite ship;
            private Sprite ship2;
            private Sprite square;

            private InstancedSprite tiles;
            private InstancedSprite tiles2;

            private RenderTexture screen;
            private RenderTexture fake;
            private RenderTexture maskedAlien;

            private Sprite monitor;

            private Rope rope;

            private Mesh triangle;
            private Mesh farTriangles;
            private Mesh colouredTriangle;

            private int index;
            private float t;
            private int counter;
            private int height;

            private ParticleSystem particleSystem;
            private ParticleSystem particleSystem2;

            private PostProcessingEffect mainEffect;

            private Sprite spriteMask;
            private Texture circleMask;
            private Texture circleMask2;

            private Sprite maskedObject;
            private Sprite maskedBackground;

			private float deltaTimeAccumulator;

            public Example(int width, int height, string title) : base(width, height, title)
            {

            }

            protected override void GameSetup(Window window)
            {
                window.SetLogger(new ExampleLogger());
                window.SetIcon("Aiv.Fast2D.Example.Assets.2.ico");

                window.Position = new Vector2(0, 0);

                //window.SetCursor(false);

                logoAiv = new Texture("Aiv.Fast2D.Example.Assets.LogoAIV.png");


                alien = new Texture("Aiv.Fast2D.Example.Assets.owl.png");


                logo = new Sprite(logoAiv.Width, logoAiv.Height);

                height = 150;

                ship = new Sprite(alien.Width / 10, height);

                ship2 = new Sprite(alien.Width / 10, height);

                square = new Sprite(100, 100);

                tiles = new InstancedSprite(100, 100, 3);
                tiles.SetPosition(0, new Vector2(150, 100));
                tiles.SetPosition(1, new Vector2(200, 200));
                tiles.SetPosition(2, new Vector2(500, 500));

                tiles.SetScale(0, new Vector2(0.5f, 0.5f));
                tiles.SetScale(1, new Vector2(1.5f, 1.5f));

                tiles2 = new InstancedSprite(20, 20, 30);

                screen = new RenderTexture(800, 600);

                fake = new RenderTexture(1, 1);
                fake.Dispose();

                monitor = new Sprite(100, 100);
                monitor.position = new Vector2(400, 200);

                index = 0;
                t = 0;

                window.SetClearColor(100, 100, 100);

                counter = 0;

                particleSystem = new ParticleSystem(2, 2, 100);
                particleSystem.position = new Vector2(400, 200);

                rope = new Rope(400, 3);
                rope.position = new Vector2(400, 200);
                rope.SetDestination(new Vector2(400, 400));

                ship2.pivot = new Vector2(alien.Width / 20, height / 2);

                particleSystem2 = new ParticleSystem(1, 2, 50);

                triangle = new Mesh();
                triangle.v = new float[] { 100, 100, 50, 200, 150, 200 };
                triangle.UpdateVertex();
                triangle.uv = new float[] { 0.5f, 0.5f, 0, 0, 1, 0 };
                triangle.UpdateUV();

                farTriangles = new Mesh();
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

                colouredTriangle = new Mesh();
                colouredTriangle.v = new float[]
                {
                500, 200,
                400, 300,
                600, 300
                };
                colouredTriangle.UpdateVertex();

                colouredTriangle.vc = new float[]
                {
                1, 0, 0, 0.5f,
                0, 1, 0, 0.5f,
                0, 0, 1, 0.5f
                };
                colouredTriangle.UpdateVertexColor();

                alien2 = new Texture("Aiv.Fast2D.Example.Assets.2.png");
                maskedAlien = new RenderTexture(alien2.Width, alien2.Height);
                spriteMask = new Sprite(50, 50);

                circleMask = new Texture("Aiv.Fast2D.Example.Assets.mask_circle.png");
                circleMask2 = new Texture("Aiv.Fast2D.Example.Assets.mask_circle2.png");

                maskedObject = new Sprite(alien2.Width, alien2.Height);
                maskedObject.position = new Vector2(200, 200);
                maskedBackground = new Sprite(alien2.Width, alien2.Height);

                mainEffect = window.AddPostProcessingEffect(new GrayscaleEffect());
                mainEffect.enabled = false;

                window.AddPostProcessingEffect(new MaskEffect("Aiv.Fast2D.Example.Assets.mask_circle.png"));

                window.AddPostProcessingEffect(new BlackBands());

                window.AddPostProcessingEffect(new RedBands());

                // insert a postprocessing effect at the specific position
                window.SetPostProcessingEffect(1, new WASDEffect());

                window.SetPostProcessingEffect(1, new WobbleEffect(5));

                tileMap = new Tilemap("Assets/map001.csv", "Assets/tiles_spritesheet.png");
            }

            protected override void GameUpdate(Window window)
            {

                if (window.GetKey(KeyCode.Right))
                {
                    tileMap.position += new Vector2(1, 0) * window.deltaTime * 300;
					ship2.FlipX = false;
                }
                if (window.GetKey(KeyCode.Left))
                {
                    tileMap.position += new Vector2(-1, 0) * window.deltaTime * 300;
					ship2.FlipX = true;
                }
                if (window.GetKey(KeyCode.Up))
                {
                    tileMap.position += new Vector2(0, -1) * window.deltaTime * 300;
                }
                if (window.GetKey(KeyCode.Down))
                {
                    tileMap.position += new Vector2(0, 1) * window.deltaTime * 300;
                }

				if (window.GetKey(KeyCode.CtrlLeft))
				{
					deltaTimeAccumulator += window.deltaTime;
					window.SetSize(window.Width, (int)(300 * (1 + Math.Abs(Math.Sin(deltaTimeAccumulator)))));
				}
 
                tileMap.position += window.JoystickAxisRight(0) * window.deltaTime * 300;

                tileMap.Draw();

                for (int i = 0; i < tiles2.Instances; i++)
                {
                    tiles2.SetPosition(i, new Vector2(20 * i, 20 * i), true);
                    if (i % 2 == 0)
                    {
                        tiles2.SetAdditiveColor(i, new Vector4(1, -1, -1, 1), true);
                    }
                }
                tiles2.UpdatePositions();
                tiles2.UpdateAdditiveColors();



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
                    this.Exit();

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



				farTriangles.position.Y = window.MouseWheel;
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
                    window.SetScissorTest(window.Width / 2 - 200, window.Height / 2 - 200, 400, 400);
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

                if (window.GetKey(KeyCode.Num1))
                {
                    window.SetDefaultOrthographicSize(window.CurrentOrthoGraphicSize + window.deltaTime * 100);
                }

                colouredTriangle.position = window.mousePosition;
                colouredTriangle.pivot = new Vector2(500, 250);
                float triggerRight = window.JoystickTriggerRight(0);
                float triggerLeft = window.JoystickTriggerLeft(0);
                colouredTriangle.scale = new Vector2(1 + triggerLeft, 1 + triggerRight);
                colouredTriangle.Draw();
            }

        }

        static void Main(string[] args)
        {
            Example example = new Example(800, 600, "Aiv.Fast2D.Example");
            example.Run();
        }
    }
}
