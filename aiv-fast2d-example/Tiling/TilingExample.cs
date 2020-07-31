using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;

namespace Aiv.Fast2D.Example.TLE
{
    static class TilingExample
    {
        public static void Run()
        {
            /*
             *  Basically there are 2 implmentation of tiling:
             *  - Sprite based: that generate a Sprite for each tile to be rendered. This solution have some weaknesses. Read comments on the SpriteBasedTiling method
             *  - Mesh based: that generate a mesh containing vertices and uvs for all the tiles to be drawn.
             *  
             *  Summary: 
             *      Mesh based seems better because just one Draw Call is made to draw all the tiles,
             *      and seems not affected by wrong pixel selection (or lines artifact between tiles)
             */

            //SpriteBasedTiling();            
            //MeshBasedTilingSimple();
            MeshBasedTilingComplex();
        }

        private static void MeshBasedTilingSimple()
        {
            Window window = new Window(800, 600, "Tiling Example");

            SmartTilemap tmap = new SmartTilemap("Tiling/Assets/simple-scene.csv", "Tiling/Assets/simple-sheet-32x32.png", 32);

            while (window.IsOpened)
            {
                window.SetClearColor(255, 255, 255);
                tmap.Draw();
                window.Update();
            }
        }

        private static void MeshBasedTilingComplex()
        {
            Window window = new Window(800, 600, "Tiling Example");

            SmartTilemap tmap = new SmartTilemap("Tiling/Assets/complex-01-scene.csv", "Tiling/Assets/complex-01-sheet-71x71.png", 70, true);
            //window.SetAlphaBlending();

            while (window.IsOpened)
            {
                window.SetClearColor(0, 0, 0);
                tmap.Draw();
                window.Update();
            }
        }

        private static void SmallTiling()
        {
            Window window = new Window(400, 300, "Tiling Example");

            Texture tilesheet = new Texture("Tiling/Assets/tilesheet-32x32.png", true);

            Sprite tile00 = new Sprite(32, 32);
            tile00.position.X = 0;
            tile00.position.Y = 0;

            Sprite tile01 = new Sprite(32, 32);
            tile01.position.X = 32;
            tile01.position.Y = 0;

            Sprite tile02 = new Sprite(32, 32);
            tile02.position.X = 64;
            tile02.position.Y = 0;

            Sprite tile03 = new Sprite(32, 32);
            tile03.position.X = 96;
            tile03.position.Y = 0;

            Sprite tile10 = new Sprite(32, 32);
            tile10.position.X = 0;
            tile10.position.Y = 32;

            Sprite tile11 = new Sprite(32, 32);
            tile11.position.X = 32;
            tile11.position.Y = 32;

            Sprite tile12 = new Sprite(32, 32);
            tile12.position.X = 64;
            tile12.position.Y = 32;


            int size = 32;
            int offset = 32;
            while (window.IsOpened)
            {
                window.SetClearColor(255, 255, 255);

                Console.WriteLine("first");
                tile00.DrawTexture(tilesheet, 0, 0, size, size);
                Console.WriteLine("second");
                tile01.DrawTexture(tilesheet, offset, 0, size, size);
                Console.WriteLine("third");
                tile02.DrawTexture(tilesheet, offset, 0, size, size);
                Console.WriteLine("fourth");
                tile03.DrawTexture(tilesheet, offset, 0, size, size);

                tile10.DrawTexture(tilesheet, offset, 0, size, size);
                tile11.DrawTexture(tilesheet, offset, 0, size, size);
                tile12.DrawTexture(tilesheet, 0, 0, size, size);

                window.Update();
            }
        }

        /// <summary>
        /// Sprite Based tiling is always affected by wrong pixel texture selection, 
        /// even using GL_NEAREST.
        /// - With GL_NEAREST, the last pixel column of the first Sprite (tile00) is filled with the first pixel column of the second Sprite (tile01)
        ///   So where should be a black column of pixel, there is a yellow one.
        /// = Without GL_NEAREST (default seems to be GL_LINEAR), the first pixel column of tile01 is interpolated with the last pixel column of tile00
        ///   So where should be a yellow column of pixel, there is a very dark yellow one.
        /// 
        /// Possible Solution to the issue is:
        /// - wrap each tile in the tilesheet with 1px border who repeat the color of each tile edge.
        /// - This way using both GL_NEAREST or GL_LINEAR should select the right color when on the edge of the tile
        /// 
        /// NOTE: Anyway Sprite based tiling is not good for perfomrance because for each sprite/tile we want to draw on window we need to make a Draw Call
        ///       to upload Vertices and UV.
        /// </summary>
        private static void SpriteBasedTiling()
        {
            Window window = new Window(400, 300, "Tiling Example");

            Texture tilesheet = new Texture("Tiling/Assets/simple-sheet-32x32.png");
            tilesheet.SetNearest();

            Sprite tile00 = new Sprite(32, 32);
            tile00.position.X = 0;
            tile00.position.Y = 0;

            Sprite tile01 = new Sprite(32, 32);
            tile01.position.X = 32;
            tile01.position.Y = 0;

            int size = 32;
            int offset = 32;
            while (window.IsOpened)
            {
                window.SetClearColor(255, 255, 255);
                
                tile00.DrawTexture(tilesheet, 0, 0, size, size);
                tile01.DrawTexture(tilesheet, offset, 0, size, size);
                
                window.Update();
            }
        }
    }
}
