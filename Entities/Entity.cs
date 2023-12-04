using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using MonoGameJam5;

public abstract class Entity
{
    protected string _animation;
    public abstract Vector2 Position { get; set; }
    public abstract AnimatedSprite Sprite { get; set; }
    public abstract Facing Direction { get; protected set; }
    public abstract string Animation { get; set; }
    public abstract void Update(GameTime gameTime);
}