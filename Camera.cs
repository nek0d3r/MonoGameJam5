using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.ViewportAdapters;
using MonoGameJam5;

public static class Camera
{
    // Camera object and position, calculated property of camera view
    public static OrthographicCamera _camera;
    public static Vector2 Position { get; private set; }
    public static Matrix ViewMatrix { get => _camera.GetViewMatrix(); }

    // Creates a new camera based on provided graphics
    public static void Initialize(BoxingViewportAdapter boxingViewportAdapter)
    {
        _camera = new OrthographicCamera(boxingViewportAdapter);
        Position = new Vector2(
            TileRender.DEFAULT_WINDOW_SIZE.X / 2,
            TileRender.DEFAULT_WINDOW_SIZE.Y / 2
        );
    }

    // Determines input for movement
    private static Vector2 GetMovementDirection()
    {
        // Get inputs and add to direction vector
        Vector2 movementDirection = Vector2.Zero;
        KeyboardState state = Keyboard.GetState();
        if (state.IsKeyDown(Keybindings.Down))
        {
            movementDirection += Vector2.UnitY;
        }
        if (state.IsKeyDown(Keybindings.Up))
        {
            movementDirection -= Vector2.UnitY;
        }
        if (state.IsKeyDown(Keybindings.Left))
        {
            movementDirection -= Vector2.UnitX;
        }
        if (state.IsKeyDown(Keybindings.Right))
        {
            movementDirection += Vector2.UnitX;
        }
        
        // Can't normalize the zero vector so test for it before normalizing
        if (movementDirection != Vector2.Zero)
        {
            // Normalize the vector. This prevents issues like diagonal movement having more input than a single direction
            movementDirection.Normalize(); 
        }
        
        return movementDirection;
    }

    // Handle camera movement based on user input
    public static void MoveCamera(GameTime gameTime, Player player)
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