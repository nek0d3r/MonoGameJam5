using Microsoft.Xna.Framework.Input;

public static class Keybindings
{
    // We can add or remove entries as needed when the theme solidifies, but
    // this should give us the ability to rebind keys.
    // As a left-handed dev, I will almost assuredly require this feature unless the controls are very basic.
    // -- Neila

    public static Keys Right;
    public static Keys Left;
    public static Keys Up;
    public static Keys Down;
    public static Keys Run;
    public static Keys Jump;
    public static Keys Interact;
    public static Keys Exit;

    /**
     * Default Keybindings
     */
    static Keybindings()
    {
        Right = Keys.D;
        Up = Keys.W;
        Down = Keys.S;
        Left = Keys.A;
        Run = Keys.LeftShift;
        Jump = Keys.Space;
        Interact = Keys.E;
        Exit = Keys.Escape;
    }
}