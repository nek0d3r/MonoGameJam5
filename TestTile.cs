public class TestTile : Tile
{
    public override int IdleAction() { return 0; }
    public override int OnTake() { return 0; }
    public override int OnDrop() { return 0; }
    public override int OnInteract() { return 0; }
    public override int OnBump() { return 0; }

    public override int X { get => 0; }
    public override int Y { get => 0; }
}