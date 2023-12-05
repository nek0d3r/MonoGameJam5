using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;

public class Enemy : Entity
{
    public bool detectedPlayer { get; }
    private Point lastSpotted { get; }
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

    public override void Update(GameTime tm)
    {
        // TODO: Check for player detection, and move along any predefined routes they have
        // TODO: Figure out if how to get map contents in here.

        // For each sound within range, discern if it is audible enough for them to pay heed.
        foreach (Point snd in soundsToParse) {

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