using System.Linq;

namespace GameService
{
    internal class GameZone
    {
        #region proprety
        private const int ROW = 7;
        private const int COL = 7;
        private string p1;
        private string p2;
        private readonly char playerOneChar = 'A';
        private readonly char playerTwoChar = 'B';
        private ICallback callback1;
        private ICallback callback2;
        private char[,] board;
        private string currentPlayer;
        private int poin = 0;
        #endregion

        public GameZone(string p1 ,string p2, ICallback callback1, ICallback callback2)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.callback1 = callback1;
            this.callback2 = callback2;
            this.board = new char[ROW, COL];
            this.currentPlayer = p1;
            initBoard();
        }

        private void initBoard()
        {
            for (int i = 0; i < ROW; i++)
            {
                for (int j = 0; j < COL; j++)
                {
                    this.board[i, j] = '\0';
                }
            }
        }

        internal MoveResult VerifyMove(int selectedCol, string player, System.Windows.Point p)
        {
            selectedCol = selectedCol - 1;
            //check if its player turn
            if (!player.Equals(currentPlayer))
                return MoveResult.NotYourTurn;
            int i = ROW - 1;
            //find the row to put
            for (; i >= 0; i--)
            {
                if (board[i, selectedCol] == '\0')
                    break;
            }
            //check if is unlegal move
            if (colIsFull(selectedCol))
                return MoveResult.UnlegalMove;
            //fill the board with the current move
            this.board[i, selectedCol] = player.Equals(p1) ? playerOneChar : playerTwoChar;
            //check if the player win the game
            if (ItsAWin(player))
            {
                notifyAnotherPlayer(player, i, selectedCol, MoveResult.YouLose, p);
                updatePointWinner(player, player.Equals(p1) ? playerOneChar : playerTwoChar);
                updatePointLose(player.Equals(p1) ? p2 : p1, player.Equals(p1) ? playerTwoChar : playerOneChar);
                return MoveResult.YouWon;
            }
            this.changePlayerTurn();
            notifyAnotherPlayer(player, i, selectedCol, MoveResult.GameOn, p);
            return MoveResult.GameOn;
        }

        private void updatePointWinner(string player, char p)
        {
            using (var ctx = new fourinrowDB_RoniShoseov_EilonOsherContext())
            {
                var UserWin = (from u in ctx.Users
                                where u.UserName == player
                                select u).FirstOrDefault();
                var WinGamePoint = (from g in ctx.SingleGames
                                 where g.Player1_UserName == player & g.Status == true
                                 select g).FirstOrDefault();

                if (checkInCol(p))
                {
                    UserWin.Points += 1100;
                    poin = 1100;
                }
                else
                {
                    UserWin.Points += 1000;
                    poin = 1000;
                }
                UserWin.NumOfGames += 1;
                UserWin.NumOfWins += 1;
                WinGamePoint.Winner = player;
                WinGamePoint.GamePoint = poin;
                WinGamePoint.Status = false;
                ctx.SaveChanges();
            }
        }

        private void updatePointLose(string player, char p)
        {
            using (var ctx = new fourinrowDB_RoniShoseov_EilonOsherContext())
            {
                var UserLose = (from u in ctx.Users
                               where u.UserName == player
                               select u).FirstOrDefault();

                if (checkInCol(p))
                {
                    UserLose.Points += 100;
                }
                UserLose.Points += (10 * cntSquare(p));
                UserLose.NumOfGames += 1;
                UserLose.NumOfLosses += 1;
                ctx.SaveChanges();
            }
        }

        private int cntSquare(char player)
        {
            int cnt = 0;
            for (int i = 0; i < ROW; i++)
            {
                for (int j = 0; j < COL; j++)
                    if (board[i, j] == player)
                        cnt++;
            }
            return cnt;
        }

        private bool checkInCol(char player)
        {
            int cnt = 0;
            for (int i = 0; i < ROW; i++)
            {
                for (int j = 0; j < COL; j++)
                    if (board[i, j] == player)
                    {
                        cnt++;
                        if (cnt >= COL)
                            return true;
                    }
            }
            return false;
        }

        private void changePlayerTurn()
        {
            currentPlayer = currentPlayer.Equals(p1) ? p2 : p1;
        }

        private void notifyAnotherPlayer(string player, int i, int selectedCol, MoveResult result, System.Windows.Point p)
        {
            if (player.Equals(p1))
            {
                callback2.OtherPlayerMoved(result, i, selectedCol, p);
            }
            else
            {
                callback1.OtherPlayerMoved(result, i, selectedCol, p);
            }
        }

        private bool colIsFull(int selectedCol)
        {
            return board[0, selectedCol] != '\0';
        }

        private bool ItsAWin(string player)
        {
            char p = playerOneChar;
            if (player.Equals(p2))
                p = playerTwoChar;
            return areFourConnected(p);
        }

        public bool areFourConnected(char player)
        {
            // horizontalCheck 
            for (int j = 0; j < getHeight() - 3; j++)
            {
                for (int i = 0; i < getWidth(); i++)
                {
                    if (this.board[i, j] == player && this.board[i, j + 1] == player && this.board[i, j + 2] == player && this.board[i, j + 3] == player)
                    {
                        return true;
                    }
                }
            }
            // verticalCheck
            for (int i = 0; i < getWidth() - 3; i++)
            {
                for (int j = 0; j < this.getHeight(); j++)
                {
                    if (this.board[i, j] == player && this.board[i + 1, j] == player && this.board[i + 2, j] == player && this.board[i + 3, j] == player)
                    {
                        return true;
                    }
                }
            }
            // ascendingDiagonalCheck 
            for (int i = 3; i < getWidth(); i++)
            {
                for (int j = 0; j < getHeight() - 3; j++)
                {
                    if (this.board[i, j] == player && this.board[i - 1, j + 1] == player && this.board[i - 2, j + 2] == player && this.board[i - 3, j + 3] == player)
                        return true;
                }
            }
            // descendingDiagonalCheck
            for (int i = 3; i < getWidth(); i++)
            {
                for (int j = 3; j < getHeight(); j++)
                {
                    if (this.board[i, j] == player && this.board[i - 1, j - 1] == player && this.board[i - 2, j - 2] == player && this.board[i - 3, j - 3] == player)
                        return true;
                }
            }
            return false;
        }

        private int getWidth()
        {
            return ROW;
        }

        private int getHeight()
        {
            return COL;
        }
    }
}
