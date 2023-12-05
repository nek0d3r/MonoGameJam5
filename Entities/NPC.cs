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
    private Facing _lastDir;
    public override Facing Direction { get; set; }
    public override string Animation { get; set; }
    private Vector2 _lastPos;
    public override Vector2 Position { get; set; }
    public override IShapeF Bounds { get; protected set; }
    public override int DrawPriority { get; set; } = 0;
    private void DetermineMovementDirection()
    {
        // Based on movement direction, determine a direction to face the sprite.
        Vector2 PosDiff = Position - _lastPos;
        // If no movement has been had, then hold Direction.
        if (PosDiff.Length() == 0)
        {
            Direction = _lastDir;
            return;
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
        if (_lastDir != Direction)
        {
            Sprite.Effect = SpriteEffects.None;
            switch (Direction)
            {
                case Facing.North:
                    Sprite.Play("employeeUp");
                    break;
                case Facing.East:
                    Sprite.Effect = SpriteEffects.FlipHorizontally;
                    Sprite.Play("employeeSide");
                    break;
                case Facing.West:
                    Sprite.Play("employeeSide");
                    break;
                case Facing.South:
                default:
                    Sprite.Play("employeeDown");
                    break;
            }
        }

    }
    public override void Update(GameTime tm)
    {
        _lastPos = Position;
        _lastDir = Direction;
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
        // After this point, we should be able to determine any position change.
        DetermineMovementDirection();
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