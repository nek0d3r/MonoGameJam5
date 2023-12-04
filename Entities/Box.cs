using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using MonoGameJam5;

public class Box : Entity
{
    public override AnimatedSprite Sprite { get; set; }
    public override Vector2 Position { get; set; }
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

    public override void Update(GameTime gameTime)
    {
    }
}