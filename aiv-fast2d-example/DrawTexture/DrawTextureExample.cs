using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;

namespace Aiv.Fast2D.Example
{
    static class DrawTextureExample
    {
        public static void Run()
        {
            Window window = new Window(800, 600, "texture");

            Texture aj = new Texture("DrawTexture/Assets/aj.jpg");
            Sprite sprite = new Sprite(300, 300);

            while (window.IsOpened)
            {
                sprite.DrawTexture(aj);
                window.Update();
            }
        }
    }
}
