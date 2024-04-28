namespace RudyAtkinson.Tile.Model
{
    public struct TileModel
    {
        public int X;
        public int Y;
        public char Mark;

        public TileModel(int x, int y)
        {
            X = x;
            Y = y;
            Mark = ' ';
        }

        public override string ToString()
        {
            return "X: " + X + ", Y: " + Y;
        }
    }
}