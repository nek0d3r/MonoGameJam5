using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;

public class Enemy : Entity
{
    public LinkedList<Action> IdleActions { get; set; }
    public bool detectedPlayer { get; private set; }
    private Vector2 lastSpotted { get; set; }
    private float sightRange { get; }
    private float sightAngle { get; }

    private float hearingRange { get; }
    private float hearingSensitivity { get; }

    // Managers are intentionally faster than you,
    // so that you cannot cheese their speed easily.
    private float _defaultSpeed = 75f;
    public float Speed { get; set; }
    public override AnimatedSprite Sprite { get; set; }
    protected override Vector2 ColliderPosition
    {
        get => new Vector2(
            Position.X,
            Position.Y + TileRender.TILE_SIZE / 2 - ColliderRadius
        );
    }
    protected float ColliderRadius
    {
        get => TileRender.TILE_SIZE * 0.25f;
    }
    private Facing _lastDir;
    public override Facing Direction { get; set; }
    public override string Animation
    {
        get => _animation;
        set
        {
            _animation = value;
            Sprite.Play(_animation);
        }
    }
    private Vector2 _lastPos;
    private Vector2 _thisPos;
    public override Vector2 Position
    {
        get => _thisPos;
        set
        {
            _thisPos = value;
            Bounds = new CircleF(ColliderPosition, ColliderRadius);
        }
    }
    public override IShapeF Bounds { get; protected set; }

    public List<Point> SoundsToParse { get; set; }
    public override int DrawPriority { get; set; } = 0;

    private void doAnim(bool always) {
        if (always || _lastDir != Direction)
        {
            Sprite.Effect = SpriteEffects.None;
            switch (Direction)
            {
                case Facing.North:
                    Sprite.Play("managerUp");
                    break;
                case Facing.East:
                    Sprite.Effect = SpriteEffects.FlipHorizontally;
                    Sprite.Play("managerSide");
                    break;
                case Facing.West:
                    Sprite.Play("managerSide");
                    break;
                case Facing.South:
                default:
                    Sprite.Play("managerDown");
                    break;
            }
        }
    }

    // Returns true if movement occurred, regardless of whether it was
    // a new facing or not.
    // Returns false if stationary.
    private bool DetermineMovementDirection()
    {
        // Based on movement direction, determine a direction to face the sprite.
        Vector2 PosDiff = Position - _lastPos;
        // If no movement has been had, then hold Direction.
        if (PosDiff.Length() == 0)
        {
            Direction = _lastDir;
            return false;
        }
        PosDiff.Normalize();
        // Determine the largest magnitude of direction.
        // If the x component is larger than the Y component, we're going E/W
        if (Math.Abs(PosDiff.X) > Math.Abs(PosDiff.Y))
        {
            if (PosDiff.X > 0)
            {
                Direction = Facing.East;
            }
            else
            {
                Direction = Facing.West;
            }
        }
        // Otherwise, we're goin N/S
        else
        {
            if (PosDiff.Y > 0)
            {
                Direction = Facing.North;
            }
            else
            {
                Direction = Facing.South;
            }
        }
        // Now we update the animation if we've changed directions.
        doAnim(false);
        return true;
    }
    public override void Update(GameTime tm)
    {
        _lastPos = Position;
        _lastDir = Direction;
        if (!detectedPlayer && IdleActions.First != null)
        {
            // Handle idleActions
            Action current = IdleActions.First.Value;
            bool isDone = current.PerformAction(this, tm);
            if (isDone) {
                // Rotate the action to the end of the list.
                IdleActions.RemoveFirst();
                current.Reset();
                IdleActions.AddLast(current);
            }
        }
        // TODO: Figure out if how to get map contents in here.

        // TODO: Check line of sight

        // For each sound within range, discern if it is audible enough for them to pay heed.
        foreach (Point snd in SoundsToParse)
        {
            // TODO: Implement
        }
        SoundsToParse.Clear();

        // TODO: Do a full determination of movement.
        bool didMove = DetermineMovementDirection();

        // Update sprite animation
        if (didMove)
        {
            // Employees lack a run.
            Sprite.Update(tm.GetElapsedSeconds()*Speed/_defaultSpeed);
        }
    }
    public override void Draw(SpriteBatch spriteBatch, bool drawCollider = false)
    {
        Vector2 drawPos = new Vector2((int)Math.Round(Position.X), (int)Math.Round(Position.Y));
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