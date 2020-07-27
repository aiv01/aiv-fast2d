using Aiv.Fast2D.Example.Alien;
using Aiv.Fast2D.Example.CWE;
using Aiv.Fast2D.Example.DSE;
using Aiv.Fast2D.Example.MW;
using Aiv.Fast2D.Example.RenderTextureSample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aiv.Fast2D.Example
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== AivFast2d Example ===");
            Console.WriteLine("Possible examples:");
            Console.WriteLine("[1] Drawing a Sprite");
            Console.WriteLine("[2] Drawing a Texture");
            Console.WriteLine("[3] PostEffect on RenderTexture");
            Console.WriteLine("[4] Alien");
            Console.WriteLine("[5] Multi Window");
            Console.WriteLine("[6] Close/Exit Window");
            Console.WriteLine();

            int minChoice = 0;
            int maxChoice = 6;
            int choice;
            do
            {
                Console.Write("Pick a number [type 0 to exit]: ");
                string input = Console.ReadLine();

                bool isValidNumber = int.TryParse(input, out choice);
                if (!isValidNumber ||
                    choice < minChoice || choice > maxChoice)
                {
                    Console.WriteLine("Invalid choice!!!");
                }
                else {
                    switch (choice)
                    {
                        case 0: break;
                        case 1: DrawSpriteExample.Run(); break;
                        case 2: DrawTextureExample.Run(); break;
                        case 3: RenderTextureExample.Run(); break;
                        case 4: AlienExample.Run(); break;
                        case 5: MultiWindowExample.Run(); break;
                        case 6: CloseWindowExample.Run(); break;
                    }

                    if (choice == 0) break;
                };
            } while (true);
        }
    }
}
