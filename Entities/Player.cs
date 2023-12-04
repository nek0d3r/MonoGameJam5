using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using MonoGameJam5;

public class Player : Entity
{
    protected Vector2 _actualPosition;
    protected Vector2 _position;
    public override AnimatedSprite Sprite { get; set; }
    public override Vector2 Position
    {
        get => _position;
        set
        {
            _position = value;
            ActualPosition = value;
        }
    }
    protected Vector2 ActualPosition
    {
        get => _actualPosition;
        set
        {
            _actualPosition = value;
            Bounds = new CircleF(_actualPosition.ToPoint(), TileRender.TILE_SIZE / 2);
        }
    }
    public int Speed { get; set; } = 70;
    public override Facing Direction { get; protected set; } = Facing.Down;
    public override string Animation
    {
        get => _animation;
        set
        {
            _animation = value;
            Sprite.Play(_animation);
        }
    }
    public override IShapeF Bounds { get; protected set; }

    // Determines input for movement
    private Vector2 GetMovementDirection()
    {
        // Get inputs and add to direction vector
        Vector2 movementDirection = Vector2.Zero;
        KeyboardState key = Keyboard.GetState();
        Facing prevDirection = Direction;
        if (key.IsKeyDown(Keybindings.Left))
        {
            movementDirection -= Vector2.UnitX;
            Direction = Facing.Left;
        }
        if (key.IsKeyDown(Keybindings.Right))
        {
            movementDirection += Vector2.UnitX;
            Direction = Facing.Right;
        }
        if (key.IsKeyDown(Keybindings.Up))
        {
            movementDirection -= Vector2.UnitY;
            Direction = Facing.Up;
        }
        if (key.IsKeyDown(Keybindings.Down))
        {
            movementDirection += Vector2.UnitY;
            Direction = Facing.Down;
        }

        if (prevDirection != Direction)
        {
            Sprite.Effect = SpriteEffects.None;
            switch (Direction)
            {
                case Facing.Up:
                    Sprite.Play("playerUp");
                    break;
                case Facing.Right:
                    Sprite.Effect = SpriteEffects.FlipHorizontally;
                    Sprite.Play("playerSide");
                    break;
                case Facing.Left:
                    Sprite.Play("playerSide");
                    break;
                case Facing.Down:
                default:
                    Sprite.Play("playerDown");
                    break;
            }
        }

        // Can't normalize the zero vector so test for it before normalizing
        if (movementDirection != Vector2.Zero)
        {
            // Normalize the vector. This prevents issues like diagonal movement having more input than a single direction
            movementDirection.Normalize(); 
        }

        return movementDirection;
    }

    public override void Update(GameTime gameTime)
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
        ActualPosition += Speed * runMult * movementDirection * seconds;

        // Prevent moving beyond the map limits
        float X = ActualPosition.X, Y = ActualPosition.Y;
        if (X < TileRender.TILE_SIZE / 2)
        {
            X = TileRender.TILE_SIZE / 2;
        }
        if (Y < TileRender.TILE_SIZE / 2)
        {
            Y = TileRender.TILE_SIZE / 2;
        }
        if (X > map.WidthInPixels - TileRender.TILE_SIZE / 2)
        {
            X = map.WidthInPixels - TileRender.TILE_SIZE / 2;
        }
        if (Y > map.HeightInPixels - TileRender.TILE_SIZE / 2)
        {
            Y = map.HeightInPixels - TileRender.TILE_SIZE / 2;
        }
        ActualPosition = new Vector2(X, Y);
        _position = new Vector2((int)Math.Round(X), (int)Math.Round(Y));

        // Update sprite animation
        if (movementDirection != Vector2.Zero)
        {
            Sprite.Update(seconds*runMult);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, bool drawCollider = false)
    {
        spriteBatch.Draw(Sprite, Position);
        if (drawCollider)
        {
            spriteBatch.DrawCircle((CircleF)Bounds, 8, Color.Red, 3);
        }
    }

    public override void OnCollision(CollisionEventArgs collisionInfo)
    {
        throw new NotImplementedException();
    }
}