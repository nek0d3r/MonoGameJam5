using System;
using Microsoft.Xna.Framework;

public static class TileRender
{
    // Default size of window
    public static readonly Point DEFAULT_WINDOW_SIZE = new Point(624, 312);

    // Current size of window
    public static Point currentWindowSize = DEFAULT_WINDOW_SIZE;

    // Previous size of window
    public static Point previousWindowSize = currentWindowSize;

    // Tile size
    public static readonly int TILE_SIZE = 24;

    // Buffer tile dimensions
    public static readonly Point BUFFER_TILE_DIMS = new Point(26, 13);

    // Buffer size
    public static readonly Point BUFFER_SIZE = new Point(BUFFER_TILE_DIMS.X * TILE_SIZE, BUFFER_TILE_DIMS.Y * TILE_SIZE);

    // How to apply buffer to render target
    public static Rectangle renderDims;

    /***
    * Thrown on resizing the window.
    * Attempts to fill the buffer while maintaining aspect ratio.
    */
    public static void WindowChanged(object sender, EventArgs e)
    {
        // Update buffer bounds
        currentWindowSize.X = ((GameWindow)sender).ClientBounds.Width;
        currentWindowSize.Y = ((GameWindow)sender).ClientBounds.Height;
        
        var windowAspectRatio = (float)currentWindowSize.X / currentWindowSize.Y;
        var bufferAspectRatio = (float)BUFFER_SIZE.X / BUFFER_SIZE.Y;

        Point origin, dimensions;

        // If buffer aspect ratio is higher than window aspect ratio, fill to width
        if (bufferAspectRatio > windowAspectRatio)
        {
            dimensions.X = currentWindowSize.X;
            dimensions.Y = (int)(currentWindowSize.Y / ((float)bufferAspectRatio / windowAspectRatio));
            origin.X = 0;
            origin.Y = currentWindowSize.Y / 2 - dimensions.Y / 2;
        }
        // If window aspect ratio is higher than buffer aspect ratio, fill to height
        else if (windowAspectRatio > bufferAspectRatio)
        {
            dimensions.X = (int)(currentWindowSize.X / ((float)windowAspectRatio / bufferAspectRatio));
            dimensions.Y = currentWindowSize.Y;
            origin.X = currentWindowSize.X / 2 - dimensions.X / 2;
            origin.Y = 0;
        }
        // Window aspect ratio matches buffer, fill to bounds
        else
        {
            dimensions.X = currentWindowSize.X;
            dimensions.Y = currentWindowSize.Y;
            origin.X = 0;
            origin.Y = 0;
        }

        renderDims = new Rectangle(origin, dimensions);
    }
}