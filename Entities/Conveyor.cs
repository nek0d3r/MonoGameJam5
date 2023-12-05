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
// ID & 2147483648 for horizontal flip
public class Conveyor : Entity
{
    protected Vector2 _position;
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
            Position.Y
        );
    }
    protected Size2 ColliderSize
    {
        get => new Size2(
            TileRender.TILE_SIZE * 0.75f,
            TileRender.TILE_SIZE * 0.5f
        );
    }
    public override Facing Direction
    {
        get => throw new System.NotSupportedException();
        protected set => throw new System.NotSupportedException();
    }
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


    public override void Update(GameTime gameTime)
    {
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