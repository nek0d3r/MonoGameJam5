using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public class Conveyor : Tile
{

    public bool Active { get; set; }
    public enum Direction {
        North,
        South,
        East,
        West
    };
    public Direction SlideDir { get; set; }

    // Speed in pixels per unit of movement.
    // TODO: Figure out the time of a unit of movement.
    public int SlideSpeed { get; set; }

    public override int OnBump()
    {
        return 0;
    }

    public override int OnTake()
    {
        return 0;
    }

    public override int OnDrop()
    {
        return 0;
    }

    public override int OnInteract()
    {
        // Try to get a box from the conveyor if there's one connected to this tile.
        if (ConnectedTo.CheckTileOpt(TileOptions.Grabbable))
        {
            // TODO: Grab the item and disconnect it from the conveyor
            return 1;
        }
        // Otherwise do nothing.
        return 0;
    }

    public override int IdleAction()
    {
        // If the conveyor is on, it should move the connected box a little
        // in the direction the conveyor is running.
        if (Active) {
            if (ConnectedTo != null)
            {
                // TODO: Move the box in the conveyor's direction.
                // TODO: Handle the box moving to the next conveyor tile.
                return 1;
            }
            // Animate the conveyor
            int framesForDir = Image.Count;
            animFrame = (animFrame + 1) % framesForDir + framesForDir * (int)SlideDir;
        }
        return 0;
    }

    public override int X { get => 0; }
    public override int Y { get => 0; }

    public Conveyor(List<Texture2D> imgs, bool activ, Direction dir, int spd){
        Active = activ;
        SlideDir = Direction.North;
        SlideSpeed = spd;
        Image = imgs;
        animFrame = Image.Count / 4 * (int)SlideDir;
        List<TileOptions> lst = new List<TileOptions>();
        lst.Add(TileOptions.Interactable);
        SetTileOpts(lst);
    }
}