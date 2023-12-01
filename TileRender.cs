using Microsoft.Xna.Framework;

public static class TileRender
{
    // Default size of window
    public static readonly Point DEFAULT_WINDOW_SIZE = new Point(624, 312);

    // Current size of window
    public static Point currentWindowSize = DEFAULT_WINDOW_SIZE;

    // Previous size of window
    public static Point previousWindowSize = currentWindowSize;

    // Pixel depth
    public static readonly int PIXEL_DEPTH = 24;

    // Buffer tile dimensions
    public static readonly Point BUFFER_TILE_DIMS = new Point(26, 13);

    // Buffer size
    public static readonly Point BUFFER_SIZE = new Point(BUFFER_TILE_DIMS.X * PIXEL_DEPTH, BUFFER_TILE_DIMS.Y * PIXEL_DEPTH);

    // How to apply buffer to render target
    public static Rectangle renderDims;
}