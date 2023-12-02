using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;

public static class Camera
{
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

    public static void MoveCamera(GameTime gameTime, ref Vector2 cameraPosition, TiledMap map)
    {
        var speed = 200;
        var seconds = gameTime.GetElapsedSeconds();
        var movementDirection = GetMovementDirection();
        cameraPosition += speed * movementDirection * seconds;
        if (cameraPosition.X < TileRender.renderDims.Width / 2)
        {
            cameraPosition.X = TileRender.renderDims.Width / 2;
        }
        if (cameraPosition.Y < TileRender.renderDims.Height / 2)
        {
            cameraPosition.Y = TileRender.renderDims.Height / 2;
        }
        if (cameraPosition.X > map.WidthInPixels - TileRender.renderDims.Width / 2)
        {
            cameraPosition.X = map.WidthInPixels - TileRender.renderDims.Width / 2;
        }
        if (cameraPosition.Y > map.HeightInPixels - TileRender.renderDims.Height / 2)
        {
            cameraPosition.Y = map.HeightInPixels - TileRender.renderDims.Height / 2;
        }
    }
}