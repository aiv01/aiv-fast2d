using Aiv.Fast2D;
using OpenTK;
using System;

namespace Aiv.Fast2D.Example.RTE
{
    static class RenderTvExample
    {
        public static void Run()
        {
            //PostFxWithRenderTexture();

            FullExample();
        }

        private static void PostFxOnWindow()
        {
            Window win = new Window(800, 600, "RenderTV");

            Sprite bkgSprite = new Sprite(win.Width, win.Height);

            GrayScaleFX grayFx = new GrayScaleFX();

            win.AddPostProcessingEffect(grayFx);

            while (win.IsOpened)
            {
                bkgSprite.DrawColor(255, 0, 0);
                win.Update();
            }
        }

        private static void PostFxWithRenderTexture()
        {
            Window win = new Window(800, 600, "RenderTV");

            Sprite bkgSprite = new Sprite(win.Width, win.Height);

            GrayScaleFX grayFx = new GrayScaleFX();

            RenderTexture renderTxt = new RenderTexture(win.Width, win.Height);
            Sprite renderSpr = new Sprite(win.Width, win.Height);

            Texture prova = new Texture(win.Width, win.Height);


            while (win.IsOpened)
            {
                win.RenderTo(renderTxt);
                    bkgSprite.DrawColor(255, 0, 0);
                    grayFx.Apply(renderTxt);

                win.RenderTo(null);
                    renderSpr.DrawTexture(renderTxt);

                /*
                byte[] data = renderTxt.Download();
                prova.Update(data);
                renderSpr.DrawTexture(prova);
                */


                win.Update();
            }
        }

        private static void FullExample()
        {
            Window Win = new Window(1280, 720, "RenderTV");

            Texture bgTexture = new Texture("RenderTV/Assets/vaporBg.jpg");
            Sprite bg = new Sprite(bgTexture.Width, bgTexture.Height);

            Texture shipTexture = new Texture("RenderTV/Assets/futurama_ship.png");
            Sprite ship = new Sprite(shipTexture.Width, shipTexture.Height);
            ship.position = new Vector2(300, 300);

            RenderTexture renderT = new RenderTexture(Win.Width, Win.Height);
            Sprite renderSprite = new Sprite(Win.Width*0.4f, Win.Height * 0.4f);
            renderSprite.pivot = new Vector2(renderSprite.Width * 0.5f, renderSprite.Height * 0.5f);
            renderSprite.position = new Vector2(Win.Width * 0.5f, Win.Height * 0.5f);

            Texture tvTexture = new Texture("RenderTV/Assets/tv.png");
            Sprite tv = new Sprite(tvTexture.Width, tvTexture.Height);
            tv.pivot = new Vector2(tv.Width * 0.5f, tv.Height * 0.5f);
            tv.position = renderSprite.position;
            tv.scale = new Vector2(0.4f, 0.4f);


            float accumulator = 0;

            WobbleFX wobble = new WobbleFX(2);
            GrayScaleFX grayScale = new GrayScaleFX();

         
            while (Win.IsOpened)
            {
                //Update
                accumulator += Win.DeltaTime*2;
                ship.position.Y -= (float)Math.Sin(accumulator) *50 * Win.DeltaTime;
                wobble.Update(Win);
                
                //Draw
                Win.RenderTo(renderT);
                    bg.DrawTexture(bgTexture);
                    ship.DrawTexture(shipTexture);

                    renderT.ApplyPostProcessingEffect(wobble);
                    renderT.ApplyPostProcessingEffect(grayScale);
                

                Win.RenderTo(null);
                    renderSprite.DrawTexture(renderT);
                    tv.DrawTexture(tvTexture);

                Win.Update();
            }
        }
    }
}
