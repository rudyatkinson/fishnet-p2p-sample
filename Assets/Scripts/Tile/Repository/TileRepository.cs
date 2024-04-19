using System.Collections.Generic;
using System.Linq;
using RudyAtkinson.Tile.View;

namespace RudyAtkinson.Tile.Repository
{
    public class TileRepository
    {
        public List<TileView> TileViews { get; } = new ();
        public Dictionary<char, Queue<TileView>> TileMarkQueueDictionary { get; } = new ();

        public char[,] MarkMatrix()
        {
            var maxXValue = TileViews.Max(tile => tile.TileModel.Value.X + 1);
            var maxYValue = TileViews.Max(tile => tile.TileModel.Value.Y + 1);
            
            var matrix = new char[maxXValue, maxYValue];

            foreach (var tileView in TileViews)
            {
                var tileModel = tileView.TileModel.Value;
                var x = tileModel.X;
                var y = tileModel.Y;
                var mark = tileModel.Mark;
                
                matrix[x, y] = mark;
            }

            return matrix;
        }
    }
}