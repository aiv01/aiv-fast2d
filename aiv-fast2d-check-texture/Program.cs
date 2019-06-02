using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;

namespace aiv_fast2d_check_texture
{
    class Program
    {
        static void Main(string[] args)
        {

            Window window = new Window(1024, 768, "texture");

            Texture aj = new Texture("Assets/aj.jpg");

            Sprite sprite = new Sprite(300, 300);

            while (window.IsOpened)
            {

                sprite.DrawTexture(aj);

                window.Update();
            }

        }
    }
}
