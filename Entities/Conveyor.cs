using System;
using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;
using MonoGameJam5;

public enum ConveyorType
{
    Start = 0,
    Connect = 1,
    End = 2,
    Path = 3
}

public class Conveyor : Entity
{
    protected Vector2 _position;
    protected Facing _direction;
    // If we don't want to have to twiddle with animation speeds for any speed of conveyor,
    // we'll want some infrastructure to always store a default speed.
    private const float _defaultSpeed = 30f;
    public float Speed { get; set; } = _defaultSpeed;
    public override AnimatedSprite Sprite { get; set; }

    public SoundEffect IdleSound { get; set; }
    private bool _isPlayingSound = false;
    public override Vector2 Position
    {
        get => _position;
        set
        {
            _position = value;
            Bounds = new RectangleF(ColliderPosition.ToPoint(), ColliderSize);
        }
    }
    protected override Vector2 ColliderPosition
    {
        get
        {
            // Default to center
            float x = _position.X - ColliderSize.Width / 2;
            float y = _position.Y - ColliderSize.Height / 2;

            // North flow ends and south flow starts and paths shift collider to
            // align with bottom of tile
            if ((_direction == Facing.North && ConveyorType == ConveyorType.End) ||
                (_direction == Facing.South &&
                    (ConveyorType == ConveyorType.Start ||
                     ConveyorType == ConveyorType.Path)))
            {
                y = _position.Y + TileRender.TILE_SIZE / 2 - ColliderSize.Height;
            }
            // South flow ends and north flow starts and paths shift collider to
            // align with top of tile
            else if ((_direction == Facing.South && ConveyorType == ConveyorType.End) ||
                      _direction == Facing.North &&
                        (ConveyorType == ConveyorType.Start ||
                         ConveyorType == ConveyorType.Path))
            {
                y = _position.Y - TileRender.TILE_SIZE / 2;
            }
            else if ((_direction == Facing.West && ConveyorType == ConveyorType.End) ||
                      _direction == Facing.East &&
                        (ConveyorType == ConveyorType.Start ||
                         ConveyorType == ConveyorType.Path))
            {
                x = _position.X + TileRender.TILE_SIZE / 2 - ColliderSize.Width;
            }
            else if ((_direction == Facing.East && ConveyorType == ConveyorType.End) ||
                      _direction == Facing.West &&
                        (ConveyorType == ConveyorType.Start ||
                         ConveyorType == ConveyorType.Path))
            {
                x = _position.X - TileRender.TILE_SIZE / 2;
            }

            return new Vector2(x, y);
        }
    }
    protected Size2 ColliderSize
    {
        // x values are centered, y values are not
        get
        {
            // Default to full tile size
            float x = TileRender.TILE_SIZE;
            float y = TileRender.TILE_SIZE;

            // North/south conveyors are always fixed width
            if (_direction == Facing.North ||
                _direction == Facing.South)
            {
                x *= 0.7f;
                // Start/end pieces are always a fixed height
                if (ConveyorType == ConveyorType.Start ||
                    ConveyorType == ConveyorType.End)
                {
                    y *= 0.25f;
                }
                // Path pieces are always a fixed height
                else if (ConveyorType == ConveyorType.Path)
                {
                    y *= 0.75f;
                }
            }
            // East/west conveyors are always fixed height
            else
            {
                y *= 0.4f;
                // Start/end pieces are always a fixed width
                if (ConveyorType == ConveyorType.Start ||
                    ConveyorType == ConveyorType.End)
                {
                    x *= 0.2f;
                }
                // Path pieces are always a fixed width
                else if (ConveyorType == ConveyorType.Path)
                {
                    x *= 0.8f;
                }
            }

            return new Size2(x, y);
        }
    }
    public override Facing Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            switch (_direction)
            {
                case Facing.North:
                    Sprite.Effect = SpriteEffects.FlipVertically;
                    break;
                case Facing.West:
                    Sprite.Effect = SpriteEffects.FlipHorizontally;
                    break;
                default:
                    Sprite.Effect = SpriteEffects.None;
                    break;
            }
        }
    }
    public ConveyorType ConveyorType { get; set; }
    public override string Animation
    {
        get => _animation;
        set
        {
            _animation = value;
            Sprite.Play(_animation);
        }
    }
    public override IShapeF Bounds { get; protected set; }
    public override int DrawPriority { get; set; } = 0;
    public override int Identifier { get; set; }

    public override void Update(GameTime gameTime)
    {
        // Make sure if we've set a conveyor to be faster, it automatically animates faster.
        Sprite.Update(gameTime.GetElapsedSeconds()*Speed/_defaultSpeed);
        if (!_isPlayingSound) {
            SoundEffectInstance inst = IdleSound.CreateInstance();
            inst.IsLooped = true;
            inst.Volume = 0.12f / John.NumConveyors;
            inst.Play();
            _isPlayingSound = true;
        }
    }

    public override void Draw(SpriteBatch spriteBatch, bool drawCollider = false)
    {
        // Make an integer-rounded rendering position to avoid graphical glitchiness from our renderer.
        Vector2 DrawPos = new Vector2((int)Math.Round(Position.X), (int)Math.Round(Position.Y));
        spriteBatch.Draw(Sprite, DrawPos);
        if (drawCollider)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 1);
        }
    }

    public override void OnCollision(CollisionEventArgs collisionInfo)
    {
    }
}