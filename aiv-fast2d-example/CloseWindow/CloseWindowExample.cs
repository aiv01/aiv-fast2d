using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;

namespace Aiv.Fast2D.Example.CWE
{
    static class CloseWindowExample
    {
        public static void Run()
        {
            Console.WriteLine("> Type 'C' to invoke Window.Close()");
            Console.WriteLine("> Type 'E' to invoke Window.Exit()");

            Window window = new Window(400, 300, "Close Window Example");
            
            while (window.IsOpened)
            {
                if (window.GetKey(KeyCode.C)) window.Close();
                else if (window.GetKey(KeyCode.E)) window.Exit();
                else window.Update();
            }
        }
    }
}
