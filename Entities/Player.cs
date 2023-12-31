using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using MonoGameJam5;

public class Player : Entity
{
    protected Vector2 _actualPosition;
    protected Vector2 _position;
    public Vector2 influence = Vector2.Zero;
    protected float maxInfluence = 0;
    public override AnimatedSprite Sprite { get; set; }

    private float _coyoteTimeThresh = 0.6f; // 0.6 seconds of collision before you're defeated.
    private float _thisUpdateTime = 0f; // Needed to have Coyote Time
    private bool _collidedWithEnemy = false;
    public float CoyoteTimeUsed { get; set; } = 0f;
    public bool LostGame { get; set; } = false;
    public bool WonGame { get; set; } = false;
    public override Vector2 Position
    {
        get => _position;
        set
        {
            ActualPosition = value;
        }
    }
    protected Vector2 ActualPosition
    {
        get => _actualPosition;
        set
        {
            _actualPosition = value;
            _position = new Vector2(
                (int)Math.Round(_actualPosition.X),
                (int)Math.Round(_actualPosition.Y)
            );
            Bounds = new CircleF(ColliderPosition, ColliderRadius);
        }
    }
    protected override Vector2 ColliderPosition
    {
        get => new Vector2(
            ActualPosition.X,
            ActualPosition.Y + TileRender.TILE_SIZE / 2 - ColliderRadius
        );
    }
    protected float ColliderRadius
    {
        get => TileRender.TILE_SIZE * 0.25f;
    }
    private float _lastStepSound = 0f;
    public SoundEffect[] Footsteps { get; set; } = new SoundEffect[2];
    public override float Speed { get; set; } = 70f;
    public override Facing Direction { get; set; } = Facing.South;
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

    // Determines input for movement
    private Vector2 GetMovementDirection()
    {
        // Get inputs and add to direction vector
        Vector2 movementDirection = Vector2.Zero;
        KeyboardState key = Keyboard.GetState();
        Facing prevDirection = Direction;
        if (key.IsKeyDown(Keybindings.Left))
        {
            movementDirection -= Vector2.UnitX;
            Direction = Facing.West;
        }
        if (key.IsKeyDown(Keybindings.Right))
        {
            movementDirection += Vector2.UnitX;
            Direction = Facing.East;
        }
        if (key.IsKeyDown(Keybindings.Up))
        {
            movementDirection -= Vector2.UnitY;
            Direction = Facing.North;
        }
        if (key.IsKeyDown(Keybindings.Down))
        {
            movementDirection += Vector2.UnitY;
            Direction = Facing.South;
        }

        if (prevDirection != Direction)
        {
            Sprite.Effect = SpriteEffects.None;
            switch (Direction)
            {
                case Facing.North:
                    Sprite.Play("playerUp");
                    break;
                case Facing.East:
                    Sprite.Effect = SpriteEffects.FlipHorizontally;
                    Sprite.Play("playerSide");
                    break;
                case Facing.West:
                    Sprite.Play("playerSide");
                    break;
                case Facing.South:
                default:
                    Sprite.Play("playerDown");
                    break;
            }
        }

        // Can't normalize the zero vector so test for it before normalizing
        if (movementDirection != Vector2.Zero)
        {
            // Normalize the vector. This prevents issues like diagonal movement having more input than a single direction
            movementDirection.Normalize(); 
        }

        return movementDirection;
    }

    public override void Update(GameTime gameTime)
    {
        float seconds = gameTime.GetElapsedSeconds();
        Vector2 movementDirection = GetMovementDirection();
        TiledMap map = John._tiledMap;
        float runMult = 1;
        KeyboardState key = Keyboard.GetState();

        // Reset Coyote time if you escape.
        if (_collidedWithEnemy == false)
        {
            CoyoteTimeUsed = 0f;
        }
        _collidedWithEnemy = false;
        if (key.IsKeyDown(Keybindings.Run)) 
        {
            runMult = 3;
        }

        // Can't normalize the zero vector so test for it before normalizing
        if (influence != Vector2.Zero)
        {
            influence.Normalize();
        }

        // Change camera position based on a provided speed, direction, and delta.
        // Time delta prevents tying a logical change to framerate.
        // See why Fallout 4 or Okami HD have locked framerates.
        ActualPosition += Speed * runMult * movementDirection * seconds;

        // Additionally add outside influences if they exist
        ActualPosition += maxInfluence * influence * seconds;

        influence = Vector2.Zero;
        maxInfluence = 0;

        // Update sprite animation
        if (movementDirection != Vector2.Zero)
        {
            Sprite.Update(seconds*runMult);
        }
        _thisUpdateTime = seconds;
        
        // Handle sound processing
        _lastStepSound += seconds * runMult;
        // I couldn't find a way to handle this programmatically,
        // to get the animation duration from the AnimatedSprite.
        // So I'll hardcode it.
        if (_lastStepSound > 0.6f)
        {
            Footsteps[Random.Shared.Next() % 2].Play(0.05f, Random.Shared.NextSingle() - 0.5f, 0f);
            _lastStepSound = 0f;
        }
    }

    public override void Draw(SpriteBatch spriteBatch, bool drawCollider = false)
    {
        spriteBatch.Draw(Sprite, Position);
        if (drawCollider)
        {
            IShapeF playerBounds = new RectangleF(
                Position.X - TileRender.TILE_SIZE / 4,
                Position.Y - TileRender.TILE_SIZE / 2,
                TileRender.TILE_SIZE / 2,
                TileRender.TILE_SIZE
            );
            spriteBatch.DrawRectangle((RectangleF)playerBounds, Color.Green, 2);
            spriteBatch.DrawCircle((CircleF)Bounds, 15, Color.Red, 2);
        }
    }

    public override void OnCollision(CollisionEventArgs collisionInfo)
    {
        if (collisionInfo.Other is Box ||
            collisionInfo.Other is Wall ||
            collisionInfo.Other is NPC)
        {
            ActualPosition -= collisionInfo.PenetrationVector;
        }
        else if (collisionInfo.Other is Conveyor)
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
        else if (collisionInfo.Other is Enemy) 
        {
            _collidedWithEnemy = true;
            // Coyote Time goes here.
            CoyoteTimeUsed += _thisUpdateTime;
            if (CoyoteTimeUsed >= _coyoteTimeThresh)
            {
                // Initiate lose sequence here.
                LostGame = true;
            }
        }
        else if (collisionInfo.Other is Toilet)
        {
            WonGame = true;
        }
    }
}