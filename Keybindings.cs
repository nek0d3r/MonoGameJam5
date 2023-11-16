using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

public class Keybindings {
    // We can add or remove entries as needed when the theme solidifies, but
    // this should give us the ability to rebind keys.
    // As a left-handed dev, I will almost assuredly require this feature unless the controls are very basic.
    // -- Neila

    protected Keys Right;
    protected Keys Left;
    protected Keys Up;
    protected Keys Down;
    protected Keys Jump;
    protected List<Keys> Actions;

    /**
     * Default Keybindings
     */
    public Keybindings(){
        Right = Keys.D;
        Up = Keys.W;
        Down = Keys.S;
        Left = Keys.A;
        Jump = Keys.Space;
        Actions = new List<Keys>();
    }
}