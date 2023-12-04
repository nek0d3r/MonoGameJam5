using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGameJam5;

public class Player
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 RelativePosition { get
    {
        Console.WriteLine($"Camera X: {Camera.Position.X}\tPlayer X: {Position.X}");
        return Position;
        // return new Vector2(
        //     Camera.Position.X - (Camera.Position.X - Position.X),
        //     Position.Y
        // );
    }}
    public int Speed { get; set; } = 200;
    public Texture2D Texture { get; set; }

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

        // Change camera position based on a provided speed, direction, and delta.
        // Time delta prevents tying a logical change to framerate.
        // See why Fallout 4 or Okami HD have locked framerates.
        Position += Speed * movementDirection * seconds;

        // Prevent moving beyond the map limits
        float X = Position.X, Y = Position.Y;
        if (Position.X < 0)
        {
            X = 0;
        }
        if (Position.Y < 0)
        {
            Y = 0;
        }
        if (Position.X > map.WidthInPixels - Texture.Width)
        {
            X = map.WidthInPixels - Texture.Width;
        }
        if (Position.Y > map.HeightInPixels - Texture.Height)
        {
            Y = map.HeightInPixels - Texture.Height;
        }
        Position = new Vector2(X, Y);
    }
}