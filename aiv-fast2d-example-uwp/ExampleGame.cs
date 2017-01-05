using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D.UWP;
using Windows.UI.Xaml.Controls;

namespace Aiv.Fast2D.Example.UWP
{
    class ExampleGame : Game
    {

        private Random random;
        Mesh mesh;

        public ExampleGame(SwapChainPanel panel) : base(panel)
        {
            this.random = new Random();
        }

        public override void GameSetup(Window window)
        {
            mesh = new Mesh();
            mesh.v = new float[]
            {
                0f, 1f,
                -1, -1,
                1, -1
            };
            mesh.UpdateVertex();

        }

        public override void GameUpdate(Window window)
        {
            window.SetClearColor(0f, 1f, 1f);

            mesh.DrawColor(1f, 0f, 0f);
        }
    }
}
