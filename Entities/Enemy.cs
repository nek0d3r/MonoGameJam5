using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;
using MonoGameJam5;
using MonoGame.Extended.Collections;

public struct Line
{
    public Point2 a, b;
}

public class Enemy : Entity
{
    public SingleLinkedList<Action> IdleActions { get; set; }
    private SingleLinkedListNode<Action> _actionCursor;
    public bool detectedPlayer { get; private set; }
    private Vector2 lastSpotted { get; set; }
    private List<Vector2> _sightState { get; set; } = new List<Vector2>();
    private float sightRange { get; } = 105.4f;
    private float sightAngle { get; } = Convert.ToSingle(Math.PI / 2);
    private int _sightRays = 10;

    private float hearingRange { get; }
    private float hearingSensitivity { get; }

    // Managers are intentionally faster than you,
    // so that you cannot cheese their speed easily.
    private float _defaultSpeed = 75f;
    public override float Speed { get; set; }
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
    private float _lastStepSound = 0f;
    public SoundEffect[] Footsteps { get; set; } = new SoundEffect[2];
    private Facing _lastDir;
    public override Facing Direction { get; set; }
    public Vector2 ActualDirection { get; set; } = Vector2.Zero;
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
    public Vector2 influence = Vector2.Zero;
    protected float maxInfluence = 0;
    public override IShapeF Bounds { get; protected set; }

    public List<Point> SoundsToParse { get; set; }
    public override int DrawPriority { get; set; } = 0;
    public override int Identifier { get; set; }

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
        ActualDirection = PosDiff;
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
            if (PosDiff.Y < 0)
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

    private bool LineIntersects(Line line1, Line line2, out Point2 point)
    {
        point = new Point2();

        float a1 = line1.b.Y - line1.a.Y;
        float b1 = line1.b.X - line1.a.X;
        float c1 = a1 * line1.a.X + b1 * line1.a.Y;

        float a2 = line2.b.Y - line2.a.Y;
        float b2 = line2.b.X - line2.a.X;
        float c2 = a2 * line2.a.X + b2 * line2.a.Y;

        float det = a1 * b2 - a2 * b1;

        if (Math.Abs(det) < 0.001) return false;

        float x = (b2 * c1 - b1 * c2) / det;
        float y = (a1 * c2 - a2 * c1) / det;
        // Male sure our intersection is within the segments.
        if ((x < line1.a.X && x < line1.b.X)||
            (x < line2.a.X && x < line2.b.X)||
            (x > line1.a.X && x > line1.b.X)||
            (x > line2.a.X && x > line2.b.X)||
            (y < line1.a.Y && y < line1.b.Y)||
            (y < line2.a.Y && y < line2.b.Y)||
            (y > line1.a.Y && y > line1.b.Y)||
            (y > line2.a.Y && y > line2.b.Y))
        {
            return false;
        }
        point = new Point2(x, y);
        return true;
    }

    public override void Update(GameTime tm)
    {
        _lastPos = Position;
        _lastDir = Direction;
        if (!detectedPlayer && IdleActions.First != null)
        {
            // Handle idleActions
            if (_actionCursor == null)
            {
                _actionCursor = IdleActions.First;
            }

            bool isDone = _actionCursor.Value.PerformAction(this, tm);
            if (isDone)
            {
                if (_actionCursor.Next == null)
                {
                    IdleActions = new SingleLinkedList<Action>();
                }
                else
                {
                    _actionCursor = _actionCursor.Next;
                }
            }
        }
        else if (detectedPlayer)
        {
            Vector2 toPlayer = lastSpotted - Position;
            toPlayer.Normalize();
            toPlayer *= this.Speed*tm.GetElapsedSeconds();
            Position += toPlayer;
        }

        // For each sound within range, discern if it is audible enough for them to pay heed.
        foreach (Point snd in SoundsToParse)
        {
            // TODO: Implement
        }
        SoundsToParse.Clear();

        // TODO: Do a full determination of movement.
        bool didMove = DetermineMovementDirection();

        // Check line of sight
        _sightState = new List<Vector2>();
        float leftRay = ActualDirection.ToAngle() - sightAngle / 2;
        float rayIncrement = sightAngle / _sightRays;
        for (int i = 0; i < _sightRays; i++)
        {
            _sightState.Add(Vector2.Transform(-Vector2.UnitX * sightRange, Matrix.CreateRotationZ(leftRay + i * rayIncrement)));
        }

        foreach (Entity entity in John.Entities)
        {
#if true
            RectangleF bounds;
            if (entity.Bounds is CircleF)
            {
                CircleF circle = (CircleF)entity.Bounds;
                bounds = new RectangleF(
                    new Point2(
                        circle.Center.X - circle.Radius,
                        circle.Center.Y - circle.Radius
                    ),
                    new Size2(circle.Diameter, circle.Diameter)
                );
            }
            else
            {
                bounds = (RectangleF)entity.Bounds;
            }
            Line[] lines = {
                new Line() { a = bounds.TopLeft, b = bounds.TopRight },
                new Line() { a = bounds.TopRight, b = bounds.BottomRight },
                new Line() { a = bounds.BottomRight, b = bounds.BottomLeft },
                new Line() { a = bounds.BottomLeft, b = bounds.TopLeft }
            };

            bool intersectedPlayer = false;

            for (int ray = 0; ray < _sightState.Count; ray++)
            {
                Line rayLine = new Line() { a = Position, b = Position + _sightState[ray] };
                foreach (Line line in lines)
                {
                    Point2 intersection;
                    if (LineIntersects(rayLine, line, out intersection))
                    {
                        if (entity is Player)
                        {
                            // Detect player
                            detectedPlayer = true;
                            intersectedPlayer = true;
                            lastSpotted = entity.Position;
                        }
                        else if (entity is Box || entity is Wall)
                        {
                            _sightState[ray] = Vector2.Normalize(_sightState[ray]);
                            _sightState[ray] *= Vector2.Distance(Position, intersection);
                        }
                    }
                }
            }

            if (!intersectedPlayer)
            {
                detectedPlayer = false;
            }
#else
            if (entity is Player)
            {
                Vector2 diffPos = entity.Position - Position;
                float len = diffPos.Length();
                if (len < 75f)
                {
                    detectedPlayer = true;
                    lastSpotted = entity.Position;
                }
                else
                {
                    detectedPlayer = false;
                }
            }
#endif
        }


        // Update sprite animation
        if (didMove)
        {
            // Employees lack a run.
            Sprite.Update(tm.GetElapsedSeconds()*Speed/_defaultSpeed);
        
            // Handle sound processing
            _lastStepSound += tm.GetElapsedSeconds()*Speed/_defaultSpeed;
            // I couldn't find a way to handle this programmatically,
            // to get the animation duration from the AnimatedSprite.
            // So I'll hardcode it.
            if (_lastStepSound > 0.57f)
            {
                Footsteps[Random.Shared.Next() % 2].Play(0.13f, Random.Shared.NextSingle() - 0.5f, 0f);
                _lastStepSound = 0f;
            }
        }

        // Can't normalize the zero vector so test for it before normalizing
        if (influence != Vector2.Zero)
        {
            influence.Normalize();
        }

        // Now add outside influences if they exist
        Position += maxInfluence * influence * tm.GetElapsedSeconds();

        influence = Vector2.Zero;
        maxInfluence = 0;
    }
    public override void Draw(SpriteBatch spriteBatch, bool drawCollider = false)
    {
        Vector2 drawPos = new Vector2((int)Math.Round(Position.X), (int)Math.Round(Position.Y));
        spriteBatch.Draw(Sprite, drawPos);
        if (drawCollider)
        {
            spriteBatch.DrawCircle((CircleF)Bounds, 8, Color.Red, 3);
            foreach (Vector2 line in _sightState)
            {
                spriteBatch.DrawLine(Position, line.Length(), line.ToAngle(), Color.Red, 2);
            }
        }
    }
    public override void OnCollision(CollisionEventArgs collisionInfo)
    {
        if (collisionInfo.Other is Conveyor)
        {
            Conveyor conveyor = (Conveyor)collisionInfo.Other;
            Vector2 force;
            switch (conveyor.Direction)
            {
                case Facing.North:
                    force = -Vector2.UnitY;
                    break;
                case Facing.South:
                    force = Vector2.UnitY;
                    break;
                case Facing.West:
                    force = -Vector2.UnitX;
                    break;
                default:
                    force = Vector2.UnitX;
                    break;
            }
            influence += force;

            if (conveyor.Speed > maxInfluence)
            {
                maxInfluence = conveyor.Speed;
            }
        }
    }
}