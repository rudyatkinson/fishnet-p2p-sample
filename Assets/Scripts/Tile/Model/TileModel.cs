namespace RudyAtkinson.Tile.Model
{
    public struct TileModel
    {
        public int X;
        public int Y;
        public char Mark;

        public TileModel(int x, int y, char mark)
        {
            X = x;
            Y = y;
            Mark = mark;
        }

        public override string ToString()
        {
            return "X: " + X + ", Y: " + Y;
        }
    }
}