using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePlay
{
    public sealed class GameWindowManger
    {
        public WaitingForGame WaitingForGameWindow { get; set; }
        public GameWindow GameWindow { get; set; }
        private static readonly Lazy<GameWindowManger>
            lazy =
            new Lazy<GameWindowManger>
                (() => new GameWindowManger());

        public static GameWindowManger Instance { get { return lazy.Value; } }

        private GameWindowManger()
        {
        }
        
    }
}
