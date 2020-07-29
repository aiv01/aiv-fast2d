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
            sprite2.position.X = 350;
            sprite2.SetMultiplyTint(0.2f, 0.2f, 0f, 0.5f);
            sprite2.SetAdditiveTint(0.1f, 0.5f, 0.3f, 0.5f);

            while (window.IsOpened)
            {
                sprite1.DrawTexture(aj);

                sprite2.DrawTexture(aj);
                window.Update();
            }
        }
    }
}
