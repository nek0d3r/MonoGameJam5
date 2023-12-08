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
    // Current destination
    private int _destination = 0;
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
                return doMoveAction(actor, tm);
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

    private bool doMoveAction(Entity actor, GameTime tm)
    {
        bool isDone = false;

        // Total distance actor can move
        float totalDistance = Vector2.Distance(
            actor.Position,
            actor.Position + actor.Speed * Vector2.UnitX * tm.GetElapsedSeconds()
        );

        // New position for actor to eventually move to
        Vector2 newPosition = actor.Position;

        // As long as we have distance remaining to move
        while (totalDistance > 0)
        {
            Vector2 direction = Destinations[_destination] - actor.Position;

            // Direction shouldn't be a zero vector, but just in case
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }

            // The distance between the actor and this destination
            float displacement = Vector2.Distance(actor.Position, Destinations[_destination]);

            // Can't quite reach this destination
            if (displacement > totalDistance)
            {
                newPosition += direction * totalDistance;
                totalDistance = 0;
            }
            // This destination was reached
            else
            {
                // Set new position to destination and mark off distance
                newPosition = Destinations[_destination];
                totalDistance -= displacement;

                // If we have no more destinations, this action is complete
                if (_destination == Destinations.Count - 1)
                {
                    totalDistance = 0;
                    isDone = true;
                    _destination = 0;
                }
                // Otherwise, start moving to next destination
                else
                {
                    _destination++;
                }
            }

            // Update actor to new position
            actor.Position = newPosition;
        }

        return isDone;
    }

    private bool doWait(GameTime tm)
    {
        if (_durationLeft is float.NaN)
        {
            _durationLeft = Duration;
        }
        _durationLeft -= tm.GetElapsedSeconds();
        if (_durationLeft > 0f)
        {
            return false;
        }
        else
        {
            _durationLeft = float.NaN;
        }
        return true;
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