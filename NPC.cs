using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;

public class NPC : Entity
{
    // TODO: Make an actual class
    protected List<object> idleActions { get; }
    // Regular NPCs are slower than you
    protected float moveSpeed = 60f;

    public override AnimatedSprite Sprite { get; set; }
    public override Facing Direction { get; protected set; }
    public override string Animation { get; set; }
    public override Vector2 Position { get; set; }

    public override void Update(GameTime tm)
    {

    }
}