using System;
using Aiv.Fast2D;

namespace Aiv.Fast2D.Example
{
    static class DrawTextureExample
    {
        public static void Run()
        {
            Window window = new Window(800, 600, "texture");

            Texture aj = new Texture("DrawTexture/Assets/aj.jpg");

            Sprite sprite1 = new Sprite(300, 300);

            Sprite sprite2 = new Sprite(300, 300);
            sprite2.position.X = 300;
            sprite2.SetMultiplyTint(0.2f, 0.2f, 0f, 0.5f);
            sprite2.SetAdditiveTint(0.1f, 0.5f, 0.3f, 0.5f);

            Sprite sprite3 = new Sprite(32, 32);
            sprite3.position.X = 600;

            //Enable texture repeat on X and Y
            Texture grass = new Texture("DrawTexture/Assets/earthGrass.png", false, true, true);
            Sprite sprite4 = new Sprite(300, 300);
            sprite4.position.Y = 300;

            while (window.IsOpened)
            {
                sprite1.DrawTexture(aj);

                sprite2.DrawTexture(aj);

                sprite3.DrawTexture(aj, 0, 0, 32, 32);
                
                //Draw a 2x2 texture
                sprite4.DrawTexture(grass, 0, 0, grass.Width * 2, grass.Height * 2);

                window.Update();
            }
        }
    }
}
