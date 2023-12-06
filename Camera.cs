using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.ViewportAdapters;
using MonoGameJam5;

public class Camera
{
    // Camera object and position, calculated property of camera view
    public OrthographicCamera _camera;
    public Vector2 Position { get; private set; }
    public Matrix ViewMatrix { get => _camera.GetViewMatrix(); }

    // Creates a new camera based on provided graphics
    public Camera(BoxingViewportAdapter boxingViewportAdapter)
    {
        _camera = new OrthographicCamera(boxingViewportAdapter);
        Position = new Vector2(
            TileRender.DEFAULT_WINDOW_SIZE.X / 2,
            TileRender.DEFAULT_WINDOW_SIZE.Y / 2
        );
    }

    // Handle camera movement based on user input
    public void MoveCamera(GameTime gameTime, Player player)
    {
        Position = player.Position;
        TiledMap map = John._tiledMap;

        // Prevent scrolling beyond the map limits
        float X = Position.X, Y = Position.Y;
        if (Position.X < TileRender.BUFFER_SIZE.X / 2)
        {
            X = TileRender.BUFFER_SIZE.X / 2;
        }
        if (Position.Y < TileRender.BUFFER_SIZE.Y / 2)
        {
            Y = TileRender.BUFFER_SIZE.Y / 2;
        }
        if (Position.X > map.WidthInPixels - TileRender.BUFFER_SIZE.X / 2)
        {
            X = map.WidthInPixels - TileRender.BUFFER_SIZE.X / 2;
        }
        if (Position.Y > map.HeightInPixels - TileRender.BUFFER_SIZE.Y / 2)
        {
            Y = map.HeightInPixels - TileRender.BUFFER_SIZE.Y / 2;
        }
        Position = new Vector2((int)X, (int)Y);

        // Set camera to new position
        _camera.LookAt(Position);
    }
}