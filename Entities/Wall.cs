using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;
using MonoGameJam5;

public class Wall : Entity
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
            Position.X,
            Position.Y - TileRender.TILE_SIZE
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
        get => throw new NotSupportedException();
        protected set => throw new NotSupportedException();
    }
    public override string Animation
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }
    public override IShapeF Bounds { get; protected set; }


    public override void Update(GameTime gameTime) { }

    public override void Draw(SpriteBatch spriteBatch, bool drawCollider = false)
    {
        if (drawCollider)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 2);
        }
    }

    public override void OnCollision(CollisionEventArgs collisionInfo) { }
}