using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;

public enum Facing
{
    North = 0,
    South = 1,
    West = 2,
    East = 3
}
public abstract class Entity : ICollisionActor
{
    protected string _animation;
    public abstract Vector2 Position { get; set; }
    protected abstract Vector2 ColliderPosition { get; }
    public abstract AnimatedSprite Sprite { get; set; }
    public abstract Facing Direction { get; set; }
    public abstract string Animation { get; set; }
    public abstract IShapeF Bounds { get; protected set; }

    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch, bool drawCollider = false);
    public abstract void OnCollision(CollisionEventArgs collisionInfo);
}