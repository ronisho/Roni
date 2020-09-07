using GamePlay.GameServiceRef;
using System;

using System.Windows;
using System.Windows.Documents;

namespace GamePlay
{
    public class ClientCallback : IGameServiceCallback
    {
        #region Delegates
        internal Func<string,bool> confirmGame;
        internal Action<string> startGame;
        internal Action<string,string> updateUserList;
        internal Action<MoveResult,int,int,Point> playerMove;

        public ClientCallback()
        {
        }
        #endregion


        public void StartGameUser(string p1)
        {
            startGame(p1);
        }

        public void OtherPlayerSignIn(string user)
        {
            updateUserList(user, "Add");
        }

        public void OtherPlayerDisconnected(string user)
        {
            updateUserList(user, "Del");
        }

        public void OtherPlayerStartedGame(string user1, string user2)
        {
            updateUserList(user1, "Del");
            updateUserList(user2, "Del");
        }

        public void OtherPlayerMoved(MoveResult moveResult, int row, int col, Point p)
        {
            GameWindowManger.Instance.GameWindow.playerMove(moveResult, row, col,p);
        }

        public bool ConfirmGame(string userToGame)
        {
            return confirmGame(userToGame);
        }
    }
}