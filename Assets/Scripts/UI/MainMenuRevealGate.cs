using System;

/// <summary>
/// Simple pub/sub gate that lets the title screen's open/close animation
/// (<see cref="TitleScreenController"/>) control when the Menu scene's main content - the
/// Category Select screen and first-launch username prompt, built by
/// <see cref="CategorySelectUIController"/> - becomes visible.
///
/// Decoupling this way means neither script needs a direct reference to the other:
/// CategorySelectUIController just hides itself on enable and waits for <see cref="Reveal"/> to
/// be raised; TitleScreenController raises it once its "OpenMain" animation finishes playing.
/// Both objects are recreated fresh every time the Menu scene loads, so no persisted state is
/// needed here - it's a one-shot signal for the current scene's lifetime only.
/// </summary>
public static class MainMenuRevealGate
{
    public static event Action Revealed;

    public static void Reveal()
    {
        Revealed?.Invoke();
    }
}
