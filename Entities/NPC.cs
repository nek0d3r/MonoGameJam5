using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Collisions;

public class NPC : Entity
{
    protected LinkedList<Action> idleActions { get; private set; }
    // Regular NPCs are slower than you
    protected float moveSpeed = 60f;

    public override AnimatedSprite Sprite { get; set; }
    protected override Vector2 ColliderPosition { get; }
    public override Facing Direction { get; set; }
    public override string Animation { get; set; }
    public override Vector2 Position { get; set; }
    public override IShapeF Bounds { get; protected set; }
    public override int DrawPriority { get; set; } = 0;

    public override void Update(GameTime tm)
    {
        if (idleActions.First != null)
        {
            // Handle idleActions
            Action current = idleActions.First.Value;
            bool isDone = current.PerformAction(this, tm);
            if (isDone) {
                // Rotate the action to the end of the list.
                idleActions.RemoveFirst();
                current.Reset();
                idleActions.AddLast(current);
            }
        }
    }
    public override void Draw(SpriteBatch spriteBatch, bool drawCollider = false)
    {
        Vector2 drawPos = new Vector2((int)Math.Round(Position.X), (int)Math.Round(Position.X));
        spriteBatch.Draw(Sprite, drawPos);
        if (drawCollider)
        {
            spriteBatch.DrawCircle((CircleF)Bounds, 8, Color.Red, 3);
        }
    }
    public override void OnCollision(CollisionEventArgs collisionInfo)
    {

    }
}