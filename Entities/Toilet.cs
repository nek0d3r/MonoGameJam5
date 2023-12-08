using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;

public class Toilet : Entity
{
    protected Vector2 _position;
    public override Vector2 Position
    {
        get => _position;
        set
        {
            _position = value;
            _position = new Vector2(
                (int)Math.Round(_position.X),
                (int)Math.Round(_position.Y)
            );
            Bounds = new CircleF(ColliderPosition, ColliderRadius);
        }
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
    protected override Vector2 ColliderPosition
    {
        get => new Vector2(
            Position.X,
            Position.Y + TileRender.TILE_SIZE / 2 - ColliderRadius
        );
    }
    protected float ColliderRadius
    {
        get => TileRender.TILE_SIZE * 0.25f;
    }
    public override AnimatedSprite Sprite { get; set; }
    public override Facing Direction { get; set; } = Facing.South;
    public override IShapeF Bounds { get; protected set; }
    public override int DrawPriority { get; set; } = 0;
    public override int Identifier { get; set; }
    public override float Speed { get; set; }

    public override void Update(GameTime gameTime)
    {
        // I don't think we have anything to do here right now.
    }

    public override void Draw(SpriteBatch spriteBatch, bool drawCollider = false)
    {
        spriteBatch.Draw(Sprite, Position);
        if (drawCollider)
        {
            spriteBatch.DrawCircle((CircleF)Bounds, 15, Color.Red, 2);
        }
    }

    public override void OnCollision(CollisionEventArgs collisionInfo)
    {
        // The only thing we care about is colliding withthe player, which the player handles.
    }
}