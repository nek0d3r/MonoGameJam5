using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class Player
{
    private Vector2 position;

    public Player()
    {
    }

    // Determines input for movement
    private static Vector2 GetMovementDirection(KeyboardState key)
    {
        // Get inputs and add to direction vector
        var movementDirection = Vector2.Zero;
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

    public void Update()
    {
    }
}