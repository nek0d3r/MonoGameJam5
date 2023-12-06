using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

public class Action 
{
    public enum ActionType
    {
        Move = 0,
        Interact = 1,
        Pause = 2,
        Wander = 3
    }

    public int GameObjectIdentifier { get; set; }
    public ActionType ThisAction { get; set; }

    // I would have used a union for these. IF C# HAD UNIONS.

    // Move uses destination
    public List<Vector2> Destinations { get; set; }
    // Wander and pause use a duration.
    public float Duration { get; set; }
    // NaN is our sentinel to denote unstarted timer.
    private float _durationLeft = float.NaN;
    public IShapeF WanderArea { get; set; }
    // TODO: Figure out interacting with an item.

    // Actor is the entity whom this action is for.
    // Returns true when an action is completed.
    // Otherwise, return false.
    public bool PerformAction(Entity actor, GameTime tm)
    {
        switch (ThisAction) {
            case ActionType.Move:
                return doMoveAction(actor);
            case ActionType.Interact:
                // TODO: Implement
                break;
            case ActionType.Pause:
                return doWait(tm);
            case ActionType.Wander:
                return doWander(actor, tm);
        }
        return false;
    }

    private bool doMoveAction(Entity actor)
    {
        // TODO: Implement
        return false;
    }

    private bool doWait(GameTime tm)
    {
        if (_durationLeft is float.NaN)
        {
            _durationLeft = Duration;
        }
        _durationLeft -= tm.GetElapsedSeconds();
        return _durationLeft <= 0f;
    }

    private bool doWander(Entity actor, GameTime tm) {
        // TODO: Handle wandering
        return doWait(tm);
    }

    public void Reset()
    {
        // Make sure our durations properly reset.
        _durationLeft = float.NaN;
    }
}