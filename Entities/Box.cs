using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;
using MonoGameJam5;

public class Box : Entity
{
    protected Vector2 _position;
    public Vector2 influence = Vector2.Zero;
    protected float maxInfluence = 0;
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
        set => throw new System.NotSupportedException();
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
    public override int DrawPriority { get; set; } = 0;
    public override int Identifier { get; set; }

    public override void Update(GameTime gameTime)
    {
        // Can't normalize the zero vector so test for it before normalizing
        if (influence != Vector2.Zero)
        {
            influence.Normalize();
        }

        // Add outside influences if they exist
        Position += maxInfluence * influence * gameTime.GetElapsedSeconds();

        influence = Vector2.Zero;
        maxInfluence = 0;
    }

    public override void Draw(SpriteBatch spriteBatch, bool drawCollider = false)
    {
        // Make an integer-rounded rendering position to avoid graphical glitchiness from our renderer.
        Vector2 DrawPos = new Vector2((int)Math.Round(Position.X), (int)Math.Round(Position.Y));
        spriteBatch.Draw(Sprite, DrawPos);
        if (drawCollider)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 2);
        }
    }

    public override void OnCollision(CollisionEventArgs collisionInfo)
    {
        if (collisionInfo.Other is Conveyor)
        {
            Conveyor conveyor = (Conveyor)collisionInfo.Other;
            Vector2 force;
            switch (conveyor.Direction)
            {
                case Facing.North:
                    force = -Vector2.UnitY;
                    break;
                case Facing.South:
                    force = Vector2.UnitY;
                    break;
                case Facing.West:
                    force = -Vector2.UnitX;
                    break;
                default:
                    force = Vector2.UnitX;
                    break;
            }
            influence += force;

            if (conveyor.Speed > maxInfluence)
            {
                maxInfluence = conveyor.Speed;
            }
        }
    }
}