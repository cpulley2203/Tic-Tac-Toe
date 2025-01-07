using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    public class GameState
    {
        public Player[,] GameGrid {  get; private set; }
        public Player CurrentPlayer {  get; private set; }
        public int TurnsPassed { get; private set; }
        public bool GameOver { get; private set; }

        public event Action<int, int> MoveMade; //Raise this event when a player has successfully made a move 
        public event Action<GameResult> GameEnded; //Will supply receivers with a game result object when the game has ended
        public event Action GameRestarted; // Raised when the game is started over 

        //Constructor which intializes the game grid and sets the current player 
        public GameState()
        {
            GameGrid = new Player[3, 3];
            CurrentPlayer = Player.X;
            TurnsPassed = 0;
            GameOver = false;
        }

        //Helper Methods

        //Returns whether or not the current player can mark a given square
        private bool CanMakeMove(int r, int c)
        {
            return !GameOver && GameGrid[r, c] == Player.None;
        }

        //Checks if the grid is full
        private bool IsGridFull()
        {
            return TurnsPassed == 9;
        }

        //Switch current player after a move has been made
        private void SwitchPlayer()
        {
            if(CurrentPlayer == Player.X)
            {
                CurrentPlayer = Player.O;
            }
            else
            {
                CurrentPlayer = Player.X;
            }
        }

        //Check if player has won the game
        private bool AreSquaresMarked((int, int)[] squares, Player player)
        {
            foreach((int r, int c) in squares)
            {
                if (GameGrid[r,c] != player)
                {
                    return false;
                }
            }

            return true;
        }

        private bool DidMoveWin(int r, int c, out WinInfo winInfo)
        {
            (int, int)[] row = new[] { (r, 0), (r, 1), (r, 2) };
            (int, int)[] col = new[] { (0, c), (1, c), (2, c) };
            (int, int)[] mainDiag = new[] { (0, 0), (1, 1), (2, 2) };
            (int, int)[] antiDiag = new[] { (0, 2), (1, 1), (2, 0) };

            if (AreSquaresMarked(row, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.Row, Number = r };
                return true;
            }

            if (AreSquaresMarked(col, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.Column, Number = c };
                return true;
            }

            if (AreSquaresMarked(mainDiag, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.MainDiagonal };
                return true;
            }

            if (AreSquaresMarked(antiDiag, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.AntiDiagonal };
                return true;
            }

            winInfo = null;
            return false;

        }

        //Check if move ended the game
        private bool DidMoveEndGame(int r, int c, out GameResult gameResult)
        {
            if (DidMoveWin(r, c, out WinInfo winInfo))
            {
                gameResult = new GameResult { Winner = CurrentPlayer, WinInfo = winInfo };
                return true;
            }

            if (IsGridFull())
            {
                gameResult = new GameResult { Winner = Player.None };
                return true;
            }

            gameResult = null;
            return false;

        }

        //Chain all helper functions together
        public void MakeMove(int r, int c)
        {
            if(!CanMakeMove(r, c))
            {
                return;
            }

            GameGrid[r, c] = CurrentPlayer;
            TurnsPassed++;

            if (DidMoveEndGame(r, c, out GameResult gameResult))
            {
                GameOver = true;
                MoveMade?.Invoke(r, c);
                /*
                Same as line above
                 
                if(MoveMade != null)
                {
                    MoveMade(r, c);
                }

                */
                
                GameEnded?.Invoke(gameResult);
            }
            else
            {
                SwitchPlayer();
                MoveMade?.Invoke(r, c);
            }
        }

        //Reset the game state
        public void Reset()
        {
            GameGrid = new Player[3,3];
            CurrentPlayer = Player.X;
            TurnsPassed = 0;
            GameOver = false;
            GameRestarted?.Invoke();
        }




    }
}
