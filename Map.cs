using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class Map
{
    public class TilePos {
        public Tile tile { get; }
        public Point loc { get; }
        public TilePos (Tile t, int x, int y)
        {
            tile = t;
            loc = new Point(x, y);
        }
    }
    public List<TilePos>[] map { get; }

    private Point Size;

    public Map(Point size) {
        // Inititalize the map by layers
        map = new List<TilePos>[(int)Tile.TileLayer.NumLayers];
        for (int i = 0; i < (int)Tile.TileLayer.NumLayers; ++i)
        {
            map[i] = new List<TilePos>();
        }
        Size = size;
    }

    public void AddToMap(Tile tile, int x, int y, Tile.TileLayer layer)
    {
        List<TilePos> curLayer = map[(int)layer];
        curLayer.Add(new TilePos(tile, x, y));
    }
}