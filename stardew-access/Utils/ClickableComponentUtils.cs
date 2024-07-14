
using StardewValley;
using StardewValley.Menus;

namespace stardew_access.Utils;

internal class ClickableComponentUtils
{
    internal static bool NarrateHoveredComponentFromList(List<ClickableComponent> clickableComponents)
    {
        if (clickableComponents == null || clickableComponents.Count == 0) return false;

        int x = Game1.getMouseX(true), y = Game1.getMouseY(true);
        for (int i = 0; i < clickableComponents.Count; i++)
        {
            if (!clickableComponents[i].bounds.Contains(x, y)) continue;

            NarrateComponent(clickableComponents[i]);
            return true;
        }

        return false;
    }

    internal static void NarrateComponent(ClickableComponent clickableComponent, bool screenReaderInterrupt = true)
    {
        if (clickableComponent.ScreenReaderIgnore) return;

        MainClass.ScreenReader.SayWithMenuChecker(clickableComponent.ScreenReaderText, interrupt: screenReaderInterrupt);
    }

}
