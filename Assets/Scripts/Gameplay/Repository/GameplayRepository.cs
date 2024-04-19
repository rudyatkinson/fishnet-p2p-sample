using System.Collections.Generic;

namespace RudyAtkinson.Gameplay.Repository
{
    public class GameplayRepository
    {
        private Dictionary<char, int> _winDict = new() { { 'X', 0 }, { 'O', 0 } };
        private char _markTurn = 'X';

        public char GetMarkTurn()
        {
            return _markTurn;
        }

        public void SetMarkTurn(char mark)
        {
            _markTurn = mark;
        }

        public Dictionary<char, int> GetWinDict()
        {
            return _winDict;
        }
    }
}