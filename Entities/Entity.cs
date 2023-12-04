using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;

public enum Facing
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3
}
public abstract class Entity : ICollisionActor
{
    protected string _animation;
    public abstract Vector2 Position { get; set; }
    protected abstract Vector2 ColliderPosition { get; }
    public abstract AnimatedSprite Sprite { get; set; }
    public abstract Facing Direction { get; protected set; }
    public abstract string Animation { get; set; }
    public abstract IShapeF Bounds { get; protected set; }

    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch, bool drawCollider = false);
    public abstract void OnCollision(CollisionEventArgs collisionInfo);
}