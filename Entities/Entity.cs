using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;

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
    public abstract int Identifier { get; set; }
    public abstract Vector2 Position { get; set; }
    protected abstract Vector2 ColliderPosition { get; }
    public abstract AnimatedSprite Sprite { get; set; }
    public abstract Facing Direction { get; set; }
    public abstract string Animation { get; set; }
    public abstract IShapeF Bounds { get; protected set; }
    public abstract int DrawPriority { get; set; }

    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch, bool drawCollider = false);
    public abstract void OnCollision(CollisionEventArgs collisionInfo);

    public static TiledMapObject GetGameObjectById(TiledMap tiledMap, int id)
    {
        IEnumerable<TiledMapObject> gameObjects = tiledMap.ObjectLayers.SelectMany(layer => layer.Objects);
        return gameObjects.First(gameObject => gameObject.Identifier == id);
    }
}