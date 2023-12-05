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
        get => new Vector2(
            Position.X - ColliderSize.Width / 2,
            Position.Y - ColliderSize.Height / 2
        );
    }
    protected Size2 ColliderSize
    {
        get => new Size2(
            TileRender.TILE_SIZE,
            TileRender.TILE_SIZE
        );
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
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 2);
        }
    }

    public override void OnCollision(CollisionEventArgs collisionInfo)
    {
    }
}