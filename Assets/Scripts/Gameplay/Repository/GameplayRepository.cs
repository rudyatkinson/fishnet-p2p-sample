namespace RudyAtkinson.Gameplay.Repository
{
    public class GameplayRepository
    {
        private char _markTurn = 'X';

        public char GetMarkTurn()
        {
            return _markTurn;
        }

        public void SetMarkTurn(char mark)
        {
            _markTurn = mark;
        }
    }
}