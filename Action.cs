using Microsoft.Xna.Framework;

public class Action 
{
    public enum ActionType
    {
        Move,
        Interact,
        Pause,
        Wander
    }

    public ActionType ThisAction { get; protected set; }

    // I would have used a union for these. IF C# HAD UNIONS.

    // Move uses destination
    public Point Destination { get; protected set; }
    // Wander and pause use a duration.
    public float Duration { get; protected set; }
    // NaN is our sentinel to 
    private float _durationLeft = float.NaN;
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
        _durationLeft -= tm.ElapsedGameTime.Milliseconds / 1000f;
        return _durationLeft <= 0f;
    }

    private bool doWander(Entity actor, GameTime tm) {
        // TODO: Handle wandering
        return doWait(tm);
    }
}