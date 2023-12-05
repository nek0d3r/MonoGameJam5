using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;

public class Enemy : Entity
{
    protected LinkedList<Action> idleActions { get; private set; }
    public bool detectedPlayer { get; private set; }
    private Vector2 lastSpotted { get; set; }
    private float sightRange { get; }
    private float sightAngle { get; }

    private float hearingRange { get; }
    private float hearingSensitivity { get; }

    // Managers are intentionally faster than you,
    // so that you cannot cheese their speed easily.
    protected float moveSpeed = 75f;

    public override AnimatedSprite Sprite { get; set; }
    protected override Vector2 ColliderPosition { get; }
    public override Facing Direction { get; set; }
    public override string Animation { get; set; }
    public override Vector2 Position { get; set; }
    public override IShapeF Bounds { get; protected set; }

    private List<Point> soundsToParse { get; }
    public override int DrawPriority { get; set; } = 0;

    public override void Update(GameTime tm)
    {
        if (!detectedPlayer && idleActions.First != null)
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
        // TODO: Figure out if how to get map contents in here.

        // TODO: Check line of sight

        // For each sound within range, discern if it is audible enough for them to pay heed.
        foreach (Point snd in soundsToParse)
        {
            // TODO: Implement
        }
        soundsToParse.Clear();
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
        if (collisionInfo.Other is Player) {
            // TODO: Coyote Time would go here.
            // TODO: Initiate lose sequence here.
        }
    }
}