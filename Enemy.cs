using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using MonoGameJam5;

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
    public override Facing Direction { get; protected set; }
    public override string Animation { get; set; }
    public override Vector2 Position { get; set; }

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
}