using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using MonoGameJam5;

public class Player
{
    public AnimatedSprite Sprite { get; set; }
    public Vector2 Position { get; set; } = Vector2.Zero;
    public int Speed { get; set; } = 70;

    public Player()
    {
    }

    // Determines input for movement
    private static Vector2 GetMovementDirection()
    {
        // Get inputs and add to direction vector
        Vector2 movementDirection = Vector2.Zero;
        KeyboardState key = Keyboard.GetState();
        if (key.IsKeyDown(Keybindings.Down))
        {
            movementDirection += Vector2.UnitY;
        }
        if (key.IsKeyDown(Keybindings.Up))
        {
            movementDirection -= Vector2.UnitY;
        }
        if (key.IsKeyDown(Keybindings.Left))
        {
            movementDirection -= Vector2.UnitX;
        }
        if (key.IsKeyDown(Keybindings.Right))
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

    public void Update(GameTime gameTime)
    {
        float seconds = gameTime.GetElapsedSeconds();
        Vector2 movementDirection = GetMovementDirection();
        TiledMap map = John._tiledMap;
        float runMult = 1;
        KeyboardState key = Keyboard.GetState();

        if (key.IsKeyDown(Keybindings.Run)) 
        {
            runMult = 3;
        }

        // Change camera position based on a provided speed, direction, and delta.
        // Time delta prevents tying a logical change to framerate.
        // See why Fallout 4 or Okami HD have locked framerates.
        Position += Speed * runMult * movementDirection * seconds;

        // Prevent moving beyond the map limits
        float X = Position.X, Y = Position.Y;
        if (Position.X < TileRender.TILE_SIZE / 2)
        {
            X = TileRender.TILE_SIZE / 2;
        }
        if (Position.Y < TileRender.TILE_SIZE / 2)
        {
            Y = TileRender.TILE_SIZE / 2;
        }
        if (Position.X > map.WidthInPixels - TileRender.TILE_SIZE / 2)
        {
            X = map.WidthInPixels - TileRender.TILE_SIZE / 2;
        }
        if (Position.Y > map.HeightInPixels - TileRender.TILE_SIZE / 2)
        {
            Y = map.HeightInPixels - TileRender.TILE_SIZE / 2;
        }
        Position = new Vector2(X, Y);

        // Update sprite animation
        if (movementDirection != Vector2.Zero)
        {
            Sprite.Update(seconds*runMult);
        }
    }
}