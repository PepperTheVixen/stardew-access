using System.Collections;
using System.Reflection;
using StardewValley;
using StardewValley.Menus;

namespace stardew_access.Utils;

internal class ClickableComponentUtils
{
    internal static bool NarrateHoveredComponentUsingReflectionInMenu(IClickableMenu menu, bool skipAllClickableComponents = true)
    {
        if (menu is null) return false;

        var fields = menu.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        HashSet<string> skipFieldNames = new()
        {
            "currentlySnappedComponent"
        };
        if (skipAllClickableComponents) skipFieldNames.Add("allClickableComponents");

        var ccFieldInfos = fields.Where(x => IsAcceptableField(x, skipFieldNames));
        if (ccFieldInfos.Count() == 0)
        {
            return false;
        }

#if DEBUG
        string fieldNamesList = string.Join('\n', ccFieldInfos.Select(x => $"{x.FieldType}\t{x.Name}"));
        Log.Debug($"[ClickableComponentUtils] Fields found to check and narrate, menu in question: {menu.GetType().FullName}\n{fieldNamesList}", once: true);
#endif

        int x = Game1.getMouseX(true), y = Game1.getMouseY(true);

        foreach (var fieldInfo in ccFieldInfos)
        {
            if (fieldInfo.FieldType.IsGenericType)
            {
                // Cannot directly convert to List<T> as we don't exactly know what type is (although we do know that it'll either be `ClickableComponent` or one of it's derived class)
                // Ref: https://stackoverflow.com/a/14129466
                IList? ccList = (IList?)fieldInfo.GetValue(menu);
                if (ccList is null) continue;
                for (int j = 0; j < ccList.Count; j++)
                {
                    object? cco = ccList[j];
                    if (cco is not ClickableComponent cc) continue;
                    if (!IsHovered(cc, x, y)) continue;

                    NarrateComponent(cc!);
                    return true;
                }
            }
            else
            {
                ClickableComponent? cc = (ClickableComponent?)fieldInfo.GetValue(menu);
                if (!IsHovered(cc, x, y)) continue;

                NarrateComponent(cc!);
                return true;
            }
        }

        return false;
    }

    private static bool IsAcceptableField(FieldInfo x, HashSet<string> skipFieldNames) => !skipFieldNames.Contains(x.Name)
        && x.FieldType.IsGenericType
                ? x.FieldType.GetGenericTypeDefinition() == typeof(List<>) && IsInstanceOfCC(x.FieldType.GetGenericArguments()[0])
                : IsInstanceOfCC(x.FieldType);

    private static bool IsInstanceOfCC(Type fieldType) => fieldType == typeof(ClickableComponent) || fieldType.IsSubclassOf(typeof(ClickableComponent));

    internal static bool NarrateHoveredComponentFromList(List<ClickableComponent> clickableComponents)
    {
        if (clickableComponents == null || clickableComponents.Count == 0) return false;

        int x = Game1.getMouseX(true), y = Game1.getMouseY(true);
        for (int i = 0; i < clickableComponents.Count; i++)
        {
            if (!IsHovered(clickableComponents[i], x, y)) continue;

            NarrateComponent(clickableComponents[i]);
            return true;
        }

        return false;
    }

    private static bool IsHovered(ClickableComponent? cc, int x, int y) => cc is not null && cc.visible && cc.bounds.Contains(x, y);

    internal static void NarrateComponent(ClickableComponent clickableComponent, bool screenReaderInterrupt = true)
    {
        if (clickableComponent.ScreenReaderIgnore) return;

        MainClass.ScreenReader.SayWithMenuChecker(string.IsNullOrWhiteSpace(clickableComponent.ScreenReaderText)
                ? $"{clickableComponent.name} {clickableComponent.label}".Trim() : clickableComponent.ScreenReaderText, interrupt: screenReaderInterrupt);
    }
}
