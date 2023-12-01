using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public abstract class Tile {
    /**
     * Define what each tile looks like and how it interacts.
     */
    public enum TileOptions {
        Interactable = 1,
        Grabbable = 2,
        Walkable = 4,

    };

    public enum TileLayer {
        Floor = 0,
        Underfoot,
        Active,
        Looming
    };

    public int TileOpts { get; private set; }

    public TileLayer Layer { get; set; }

    private Texture2D Image;

    // Useful for connecting box tiles together,
    // such as when they're on a conveyor belt.
    private Tile ConnectedTo;

    // Friendly reminder that this does not stop you from
    // providing the same flag multiple times. Repeats
    // are not double-counted.
    public void SetTileOpts(List<TileOptions> lst) {
        int total = 0;
        foreach (TileOptions item in lst) {
            total |= (int)item;
        }
        TileOpts = total;
    }
    public bool CheckTileOpt(TileOptions opt) {
        return (TileOpts & (int)opt) != 0;
    }

    public abstract int IdleAction();
    public abstract int OnTake();
    public abstract int OnDrop();
    public abstract int OnInteract();
    public abstract int OnBump();
}