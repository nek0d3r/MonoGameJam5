using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class Enemy : IObject
{
    public bool detectedPlayer { get; }
    private Point lastSpotted { get; }
    private float sightRange { get; }
    private float sightAngle { get; }

    private float hearingRange { get; }
    private float hearingSensitivity { get; }

    // Managers are intentionally faster than you,
    // so that you cannot cheese their speed easily.
    private float moveSpeed = 75f;

    private List<Point> soundsToParse { get; }

    // TODO: Make an actual class
    protected List<object> idleActions { get; }

    public void Update(GameTime tm)
    {
        // TODO: Check for player detection, and move along any predefined routes they have
        // TODO: Figure out if how to get map contents in here.

        // For each sound within range, discern if it is audible enough for them to pay heed.
        foreach (Point snd in soundsToParse) {

        }
        soundsToParse.Clear();
    }
}