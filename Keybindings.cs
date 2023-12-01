using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

public static class Keybindings {
    // We can add or remove entries as needed when the theme solidifies, but
    // this should give us the ability to rebind keys.
    // As a left-handed dev, I will almost assuredly require this feature unless the controls are very basic.
    // -- Neila

    static Keys Right;
    static Keys Left;
    static Keys Up;
    static Keys Down;
    static Keys Jump;
    static Keys Interact;

    /**
     * Default Keybindings
     */
    public Keybindings(){
        Right = Keys.D;
        Up = Keys.W;
        Down = Keys.S;
        Left = Keys.A;
        Jump = Keys.Space;
        Interact = Keys.E;
    }
}