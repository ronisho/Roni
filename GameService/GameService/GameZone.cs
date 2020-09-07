using System;

namespace GameService
{
    internal class GameZone
    {
        private const int ROW = 6;
        private const int COL = 7;
        private string p1;
        private string p2;
        private char playerOneChar = 'A';
        private char playerTwoChar = 'B';
        private ICallback callback1;
        private ICallback callback2;
        private char[,] board;
        private string currentPlayer;

        public GameZone(string p1, string p2, ICallback callback1, ICallback callback2)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.callback1 = callback1;
            this.callback2 = callback2;
            this.board = new char[ROW, COL];
            initBoard();
            this.currentPlayer = p1;

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

        internal MoveResult VerifyMove(int selectedCol, string player)
        {
            if (!player.Equals(currentPlayer))
                return MoveResult.NotYourTurn;
            int i = ROW - 1;
            for (; i >= 0; i--)             //find an empty spot to insert
            {
                if (board[i, selectedCol] == '\0')
                    break;
            }
            if (colIsFull(selectedCol))
                return MoveResult.UnlegalMove;
            this.board[i, selectedCol] = player.Equals(p1) ? playerOneChar : playerTwoChar;
            if (ItsAWin(player))
                return MoveResult.YouWon;
            this.changePlayerTurn();
            return MoveResult.GameOn;

        }

        private void changePlayerTurn()
        {
            currentPlayer = currentPlayer.Equals(p1) ? p2 : p1;
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



        private bool checkRowWin()
        {
            throw new NotImplementedException();
        }

        private bool checkColWin()
        {
            throw new NotImplementedException();
        }
    }
}
