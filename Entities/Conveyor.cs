using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;
using MonoGameJam5;

public enum ConveyorType
{
    Start = 0,
    Connect = 1,
    End = 2,
    Path = 3
}

public class Conveyor : Entity
{
    protected Vector2 _position;
    protected Facing _direction;
    public override AnimatedSprite Sprite { get; set; }
    public override Vector2 Position
    {
        get => _position;
        set
        {
            _position = value;
            Bounds = new RectangleF(ColliderPosition.ToPoint(), ColliderSize);
        }
    }
    protected override Vector2 ColliderPosition
    {
        get
        {
            // Default to top left
            float x = _position.X - ColliderSize.Width / 2;
            float y = _position.Y - ColliderSize.Height / 2;

            // North/south only needs y adjustments
            if (_direction == Facing.North)
            {
                if (ConveyorType == ConveyorType.End)
                {
                }
            }
            else if (_direction == Facing.South)
            {
                if (ConveyorType == ConveyorType.End)
                {
                    y = _position.Y;
                }
            }
            // East/west only needs x adjustments
            else
            {
            }

            return new Vector2(x, y);
        }
    }
    protected Size2 ColliderSize
    {
        // x values are centered, y values are not
        get
        {
            // Default to full tile size
            float x = TileRender.TILE_SIZE;
            float y = TileRender.TILE_SIZE;

            // North/south conveyors are always fixed width
            if (_direction == Facing.North ||
                _direction == Facing.South)
            {
                x *= 0.7f;
                // Start/end pieces are always a fixed height
                if (ConveyorType == ConveyorType.Start ||
                    ConveyorType == ConveyorType.End)
                {
                    y *= 0.25f;
                }
            }
            // East/west conveyors are always fixed height
            else
            {
                y *= 0.4f;
            }

            return new Size2(x, y);
        }
    }
    public override Facing Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            switch (_direction)
            {
                case Facing.North:
                    Sprite.Effect = SpriteEffects.FlipVertically;
                    break;
                case Facing.West:
                    Sprite.Effect = SpriteEffects.FlipHorizontally;
                    break;
                default:
                    Sprite.Effect = SpriteEffects.None;
                    break;
            }
        }
    }
    public ConveyorType ConveyorType { get; set; }
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
    public override int DrawPriority { get; set; } = 0;

    public override void Update(GameTime gameTime)
    {
        Sprite.Update(gameTime.GetElapsedSeconds());
    }

    public override void Draw(SpriteBatch spriteBatch, bool drawCollider = false)
    {
        spriteBatch.Draw(Sprite, Position);
        if (drawCollider)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 1);
        }
    }

    public override void OnCollision(CollisionEventArgs collisionInfo)
    {
    }
}