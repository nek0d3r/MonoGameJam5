using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.ViewportAdapters;

public static class Camera
{
    // Camera object and position, calculated property of camera view
    private static OrthographicCamera _camera;
    private static Vector2 _cameraPosition;
    public static Matrix ViewMatrix { get => _camera.GetViewMatrix(); }

    // Creates a new camera based on provided graphics
    public static void Initialize(BoxingViewportAdapter boxingViewportAdapter)
    {
        _camera = new OrthographicCamera(boxingViewportAdapter);
        _cameraPosition = new Vector2(
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
    public static void MoveCamera(GameTime gameTime, TiledMap map)
    {
        int speed = 200;
        float seconds = gameTime.GetElapsedSeconds();
        Vector2 movementDirection = GetMovementDirection();

        // Change camera position based on a provided speed, direction, and delta.
        // Time delta prevents tying a logical change to framerate.
        // See why Fallout 4 or Okami HD have locked framerates.
        _cameraPosition += speed * movementDirection * seconds;

        // Prevent scrolling beyond the map limits
        if (_cameraPosition.X < TileRender.BUFFER_SIZE.X / 2)
        {
            _cameraPosition.X = TileRender.BUFFER_SIZE.X / 2;
        }
        if (_cameraPosition.Y < TileRender.BUFFER_SIZE.Y / 2)
        {
            _cameraPosition.Y = TileRender.BUFFER_SIZE.Y / 2;
        }
        if (_cameraPosition.X > map.WidthInPixels - TileRender.BUFFER_SIZE.X / 2)
        {
            _cameraPosition.X = map.WidthInPixels - TileRender.BUFFER_SIZE.X / 2;
        }
        if (_cameraPosition.Y > map.HeightInPixels - TileRender.BUFFER_SIZE.Y / 2)
        {
            _cameraPosition.Y = map.HeightInPixels - TileRender.BUFFER_SIZE.Y / 2;
        }

        // Set camera to new position
        _camera.LookAt(_cameraPosition);
    }
}