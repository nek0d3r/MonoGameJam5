using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class NPC : IObject
{
    // TODO: Make an actual class
    protected List<object> idleActions { get; }
    // Regular NPCs are slower than you
    protected float moveSpeed = 60f;

    public void Update(GameTime tm)
    {

    }
}