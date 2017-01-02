using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml.Controls;

namespace Aiv.Fast2D.UWP
{
    public interface IGame
    {
        void GameSetup(Window window);
        void GameUpdate(Window window);
    }

    public class Game : IGame
    {
        private Window window;
        private bool requestedExit;

        public Game(SwapChainPanel panel)
        {
            window = new Window(panel, this);
            
        }

        public virtual void GameSetup(Window window)
        {

        }

        public virtual void GameUpdate(Window window)
        {

        }

    }
}
