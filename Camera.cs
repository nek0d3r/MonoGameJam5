using System.IO.Compression;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.ViewportAdapters;

public static class Camera
{
    private static OrthographicCamera _camera;
    private static Vector2 _cameraPosition;
    public static Matrix ViewMatrix { get => _camera.GetViewMatrix(); }

    public static void Initialize(BoxingViewportAdapter boxingViewportAdapter)
    {
        _camera = new OrthographicCamera(boxingViewportAdapter);
        _cameraPosition = new Vector2(
            TileRender.DEFAULT_WINDOW_SIZE.X / 2,
            TileRender.DEFAULT_WINDOW_SIZE.Y / 2
        );
    }
    public static Vector2 GetMovementDirection()
    {
        var movementDirection = Vector2.Zero;
        var state = Keyboard.GetState();
        if (state.IsKeyDown(Keys.Down))
        {
            movementDirection += Vector2.UnitY;
        }
        if (state.IsKeyDown(Keys.Up))
        {
            movementDirection -= Vector2.UnitY;
        }
        if (state.IsKeyDown(Keys.Left))
        {
            movementDirection -= Vector2.UnitX;
        }
        if (state.IsKeyDown(Keys.Right))
        {
            movementDirection += Vector2.UnitX;
        }
        
        // Can't normalize the zero vector so test for it before normalizing
        if (movementDirection != Vector2.Zero)
        {
            movementDirection.Normalize(); 
        }
        
        return movementDirection;
    }

    public static void MoveCamera(GameTime gameTime, TiledMap map)
    {
        var speed = 200;
        var seconds = gameTime.GetElapsedSeconds();
        var movementDirection = GetMovementDirection();
        _cameraPosition += speed * movementDirection * seconds;

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

        _camera.LookAt(_cameraPosition);
    }
}