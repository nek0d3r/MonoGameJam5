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
using System.Linq;
using System.Dynamic;
using MonoGame.Extended.Timers;

public struct Line
{
    public Point2 p1, p2;
    public float A { get => p1.Y - p2.Y; }
    public float B { get => p2.X - p1.X; }
    public float C { get => -(p1.X * p2.Y - p2.X * p1.Y); }
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
    private int _sightRays = 20;
    private List<Point2> rayCollisions = new List<Point2>();

    private float hearingRange { get; }
    private float hearingSensitivity { get; }
    private float _playerSpotDelayTime = 5f;
    private float _elapsedSpotDelay = 0;

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

    // Determine intersection of two lines using Cramer's rule
    private static bool LineIntersects(Line line1, Line line2, out Point2 point)
    {
        point = new Point2();

        /*********************************
        * Starting with linear system:
        * | A1 * x + B1 * y = C1
        * | A2 * x + B2 * y = C2
        * 
        * Use D as the main determinant of the system:
        * | A1 | B1 |
        * | A2 | B2 |
        *********************************/
        float D = line1.A * line2.B - line1.B * line2.A;

        if (Math.Abs(D) < 0.001)
        {
            return false;
        }

        /*********************************
        * X determinant from the matrix:
        * | C1 | B1 |
        * | C2 | B2 |
        *********************************/
        float Dx = line1.C * line2.B - line1.B * line2.C;

        /*********************************
        * Y determinant from the matrix:
        * | A1 | C1 |
        * | A2 | C2 |
        *********************************/
        float Dy = line1.A * line2.C - line1.C * line2.A;

        // Now we can apply Cramer's rule
        float x = Dx / D;
        float y = Dy / D;

        // Male sure our intersection is within the segments.
        if ((x < line1.p1.X && x < line1.p2.X)||
            (x < line2.p1.X && x < line2.p2.X)||
            (x > line1.p1.X && x > line1.p2.X)||
            (x > line2.p1.X && x > line2.p2.X)||
            (y < line1.p1.Y && y < line1.p2.Y)||
            (y < line2.p1.Y && y < line2.p2.Y)||
            (y > line1.p1.Y && y > line1.p2.Y)||
            (y > line2.p1.Y && y > line2.p2.Y))
        {
            return false;
        }

        point = new Point2(x, y);
        return true;
    }

    private static Line[] GetLinesFromCollisionBounds(IShapeF shape)
    {
        RectangleF bounds;
        if (shape is CircleF)
        {
            CircleF circle = (CircleF)shape;
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
            bounds = (RectangleF)shape;
        }
        return new Line[] {
            new Line() { p1 = bounds.TopLeft, p2 = bounds.TopRight },
            new Line() { p1 = bounds.TopRight, p2 = bounds.BottomRight },
            new Line() { p1 = bounds.BottomRight, p2 = bounds.BottomLeft },
            new Line() { p1 = bounds.BottomLeft, p2 = bounds.TopLeft }
        };
    }

    public Line GetSegmentedLineFromRay(Vector2 ray)
    {
        return new Line()
        {
            p1 = Position,
            p2 = Position + Vector2.Transform(
                ray,
                Matrix.CreateRotationZ(Convert.ToSingle(Math.PI / 2))
            )
        };
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
            if (toPlayer.Length() < ((CircleF)Bounds).Radius)
            {
                _elapsedSpotDelay += tm.GetElapsedSeconds();
                if (_elapsedSpotDelay > _playerSpotDelayTime)
                {
                    _elapsedSpotDelay = 0;
                    detectedPlayer = false;
                }
            }
            else
            {
                ActualDirection = toPlayer;
                toPlayer.Normalize();
                toPlayer *= this.Speed*tm.GetElapsedSeconds();
                Position += toPlayer;
            }
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

        rayCollisions = new List<Point2>();

        foreach (Entity entity in John.Entities)
        {
            // Wait until rays are calculated before testing players
            // Don't intersect with ourselves
            if (entity is Enemy || entity is Player)
            {
                continue;
            }
            
            Line[] lines = GetLinesFromCollisionBounds(entity.Bounds);

            for (int ray = 0; ray < _sightState.Count; ray++)
            {
                Line rayLine = GetSegmentedLineFromRay(_sightState[ray]);

                foreach (Line line in lines)
                {
                    if (LineIntersects(rayLine, line, out Point2 intersection))
                    {
                        rayCollisions.Add(intersection);
                        if (entity is Box || entity is Wall)
                        {
                            float lastDist = _sightState[ray].Length();
                            // Only change if collision is closer than the last one.
                            if (lastDist > Vector2.Distance(Position, intersection))
                            {
                                _sightState[ray] = Vector2.Normalize(_sightState[ray]);
                                _sightState[ray] *= Vector2.Distance(Position, intersection);
                            }
                        }
                    }
                }
            }
        }

        John.Entities.Where(entity => entity is Player)
                     .ToList()
                     .ForEach(entity =>
                     {
                        IShapeF playerBounds = new RectangleF(
                            entity.Position.X - TileRender.TILE_SIZE / 4,
                            entity.Position.Y - TileRender.TILE_SIZE / 2,
                            TileRender.TILE_SIZE / 2,
                            TileRender.TILE_SIZE
                        );
                        Line[] lines = GetLinesFromCollisionBounds(playerBounds);
                        for (int ray = 0; ray < _sightState.Count; ray++)
                        {
                            Line rayLine = GetSegmentedLineFromRay(_sightState[ray]);

                            foreach (Line line in lines)
                            {
                                if (LineIntersects(rayLine, line, out Point2 intersection))
                                {
                                    detectedPlayer = true;
                                    lastSpotted = entity.Position;
                                }
                            }
                        }
                     });


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
                Line rayLine = new Line()
                {
                    p1 = Position,
                    p2 = Position + Vector2.Transform(
                        line,
                        Matrix.CreateRotationZ(Convert.ToSingle(Math.PI / 2))
                    )
                };
                spriteBatch.DrawPoint(rayLine.p1, Color.Blue, 5);
                spriteBatch.DrawPoint(rayLine.p2, Color.Blue, 5);
            }
            foreach (Point2 point in rayCollisions)
            {
                spriteBatch.DrawPoint(point, Color.DeepPink, 5);
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