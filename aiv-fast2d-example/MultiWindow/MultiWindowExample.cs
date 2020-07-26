using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Aiv.Fast2D.Example.MW
{
    static class MultiWindowExample
    {
        public static void Run()
        {
            Window.SetObsoleteMode();
            Window windowFake = new Window(800, 600, "Fake");

			Texture texture = new Texture("MultiWindow/Assets/circle.png");

			Sprite circle = new Sprite(texture.Width, texture.Height);

            
            Console.WriteLine(windowFake.Version);
            Console.WriteLine(windowFake.Vendor);
            Console.WriteLine(windowFake.SLVersion);
            Console.WriteLine(windowFake.Renderer);
            Console.WriteLine(windowFake.Extensions);
            

            foreach(string displayName in Window.Displays)
            {
                Console.WriteLine(displayName);
            }

            windowFake.SetDefaultViewportOrthographicSize(7);
            windowFake.SetClearColor(1f, 0, 0);
            Mesh triangleFake = new Mesh();
            triangleFake.v = new float[]
            {
                2, 0,
                1, 2,
                3, 2
            };
            triangleFake.UpdateVertex();

            Mesh wireframeTriangle = new Mesh();

            wireframeTriangle.v = new float[]
            {
                4, 0,
                3, 1,
                5, 1
            };
            wireframeTriangle.UpdateVertex();

            Window window = new Window(1024, 576, "Units based example");
            window.SetClearColor(0f, 1f, 0f);
            window.SetDefaultViewportOrthographicSize(10);

            Mesh triangle = new Mesh();
            triangle.v = new float[]
            {
                2, 0,
                1, 1,
                3, 1
            };
            triangle.UpdateVertex();
            triangle.pivot = new Vector2(2, 0.5f);

            Camera camera1 = new Camera();
            Camera camera2 = new Camera();
            Camera camera3 = new Camera();
            Camera movingCamera = new Camera();

            Sprite pointer = new Sprite(1, 1);

            

            while (window.opened && windowFake.opened)
            {

                window.SetCurrent();

                window.SetViewport(0, 0, 1024/2, 576 / 2, 5);
                window.SetScissorTest(window.CurrentViewportPosition.X, window.CurrentViewportPosition.Y, window.CurrentViewportSize.X, window.CurrentViewportSize.Y);
                window.SetClearColor(0.5f, 0.5f, 0.5f);
                window.ClearColor();
                window.SetCamera(movingCamera);
                triangle.scale = new Vector2(1f, 1f);
                triangle.position = window.MousePosition;
                triangle.DrawColor(0f, 1f, 0f, 1f);

                triangle.position = new Vector2(2, 2);
                triangle.DrawColor(1f, 0f, 0f, 1f);

				circle.SetAdditiveTint(-1f, -1f, 1f, -0.5f);
				circle.scale = new Vector2(0.005f, 0.005f);
				circle.DrawTexture(texture);

                window.SetViewport(0, 576/2, 1024/2, 576 / 2, 5);
                window.SetScissorTest(window.CurrentViewportPosition.X, window.CurrentViewportPosition.Y, window.CurrentViewportSize.X, window.CurrentViewportSize.Y);
                window.SetClearColor(0.5f, 0.5f, 1f);
                window.ClearColor();
                window.SetCamera(camera1);
                triangle.scale = new Vector2(1f, 1f);
                triangle.position = window.MousePosition;
                triangle.DrawColor(1f, 1f, 0f, 1f);

                window.SetViewport(1024/2, 0, 1024 / 2, 576 / 2, 5);
                window.SetScissorTest(window.CurrentViewportPosition.X, window.CurrentViewportPosition.Y, window.CurrentViewportSize.X, window.CurrentViewportSize.Y);
                window.SetClearColor(0.5f, 1f, 0.5f);
                window.ClearColor();
                window.SetCamera(camera3);
                triangle.scale = new Vector2(1f, 1f);
                triangle.position = window.MousePosition;
                triangle.DrawColor(1f, 0f, 0f, 1f);

                triangle.position = new Vector2(2, 2);
                triangle.DrawColor(1f, 1f, 0f, 1f);

                window.SetViewport(1024/2, 576 / 2, 1024 / 2, 576 / 2, 5);
                window.SetScissorTest(window.CurrentViewportPosition.X, window.CurrentViewportPosition.Y, window.CurrentViewportSize.X, window.CurrentViewportSize.Y);
                window.SetClearColor(1f, 0.5f, 0.5f);
                window.ClearColor();
                window.SetCamera(camera2);
                triangle.scale = new Vector2(1f, 1f);
                triangle.position = window.MousePosition;
                triangle.DrawColor(1f, 0f, 1f, 1f);

                if (window.GetKey(KeyCode.Esc))
                    break;

                if (window.GetKey(KeyCode.Right))
                    movingCamera.position.X += window.DeltaTime;

                if (window.GetKey(KeyCode.Left))
                    movingCamera.position.X -= window.DeltaTime;

                pointer.position = window.MousePosition;
                pointer.DrawSolidColor(1f, 0, 0, 1f);

                window.Update();

                windowFake.SetCurrent();

                triangleFake.position = windowFake.MousePosition;
                triangleFake.DrawColor(1f, 0f, 1f, 1f);

                wireframeTriangle.DrawWireframe(0f, 1f, 0f);

                windowFake.Update();
            }
        }
    }
}
