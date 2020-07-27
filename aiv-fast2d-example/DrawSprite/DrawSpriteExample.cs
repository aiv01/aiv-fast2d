﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;

namespace Aiv.Fast2D.Example.DSE
{
    static class DrawSpriteExample
    {
        public static void Run()
        {
            Window window = new Window(400, 300, "Draw Sprite Example");

            Sprite redLine = new Sprite(100, 10);
            redLine.position.X = 0;
            redLine.position.Y = 0;

            Sprite greenLine = new Sprite(100, 10);
            greenLine.position.X = 80;
            greenLine.position.Y = 51;

            while (window.IsOpened)
            {
                //redLine.DrawColor(255, 0, 0);
                //blueLine.DrawColor(0, 0, 255);
                redLine.DrawSolidColor(255, 0, 0);
                greenLine.DrawSolidColor(0, 255, 0);
                window.Update();
            }
        }
    }
}