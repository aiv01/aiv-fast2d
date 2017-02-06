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
        Mesh mesh2;
        Mesh mesh3;

        public ExampleGame(SwapChainPanel panel) : base(panel)
        {
            this.random = new Random();
        }

        public override void GameSetup(Window window)
        {
            mesh = new Mesh();
            mesh.v = new float[]
            {
                300f, 100f,
                600, 300,
                0, 300,

            };
            mesh.UpdateVertex();

            mesh.vc = new float[] {
                1, 0, 0, 1,
                0, 1, 0, 1,
                0, 0, 1, 1
            };
            mesh.UpdateVertexColor();

            mesh.pivot = new SharpDX.Vector2(300, 200);

            mesh.position = new SharpDX.Vector2(400, 400);

            mesh2 = new Mesh();

            mesh2.v = new float[]
            {
                0, 0,
                100, 0,
                100, 100,
                0, 0,
                100, 100,
                0, 100
            };

            mesh2.UpdateVertex();


            mesh3 = new Mesh();
            mesh3.v = new float[]
            {
                0, 0,
                100, 0,
                100, 100,
                0, 0,
                100, 100,
                0, 100
            };

            mesh3.UpdateVertex();

            mesh3.scale = new SharpDX.Vector2(2, 2);

        }

        public override void GameUpdate(Window window)
        {
            window.SetClearColor(0f, 0f, 0f);

            mesh3.position += window.JoystickAxisLeft(0) * window.deltaTime * 50f;
            mesh3.DrawColor(0f, 1f, 0f);

            mesh.position.X += 100f * window.deltaTime;
            mesh.EulerRotation += 30f * window.deltaTime;

            mesh.DrawColor(0f, 0.5f, 1f);

            mesh2.DrawColor(0f, 0f, 1f);
        }
    }
}
