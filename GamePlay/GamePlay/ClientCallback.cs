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
        #endregion
        public ClientCallback()
        {
        }
       
        public void StartGameUser(string p1)
        {
            if(startGame != null)
                startGame(p1);
        }

        public void OtherPlayerSignIn(string user)
        {
            if (updateUserList != null)
                updateUserList(user, "Add");
        }

        public void OtherPlayerDisconnected(string user)
        {
            if (updateUserList != null)
                updateUserList(user, "Del");
        }

        public void OtherPlayerStartedGame(string user1, string user2)
        {
            if (updateUserList != null)
            {
                updateUserList(user1, "Del");
                updateUserList(user2, "Del");
            }   
        }

        public void OtherPlayerMoved(MoveResult moveResult, int row, int col, Point p)
        {
            playerMove?.Invoke(moveResult, row, col, p);
        }

        public bool ConfirmGame(string userToGame)
        {
            return confirmGame != null ?confirmGame(userToGame) :false;
        }
    }
}