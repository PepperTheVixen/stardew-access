using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using stardew_access.Features;
using stardew_access.Utils;
using StardewValley;
using StardewValley.Menus;

namespace stardew_access.Patches;

// These patches are global, i.e. work on every menus
internal class IClickableMenuPatch : IPatch
{
    private static readonly HashSet<Type> SkipMenuTypes =
    [
        typeof(AnimalQueryMenu),
        typeof(Billboard),
        typeof(CarpenterMenu),
        typeof(ChooseFromIconsMenu),
        typeof(ConfirmationDialog),
        typeof(DialogueBox),
        typeof(FieldOfficeMenu),
        typeof(ForgeMenu),
        typeof(GeodeMenu),
        typeof(ItemGrabMenu),
        typeof(ItemListMenu),
        typeof(JojaCDMenu),
        typeof(JunimoNoteMenu),
        typeof(LetterViewerMenu),
        typeof(MuseumMenu),
        typeof(NumberSelectionMenu),
        typeof(PondQueryMenu),
        typeof(PrizeTicketMenu),
        typeof(PurchaseAnimalsMenu),
        typeof(QuestContainerMenu),
        typeof(QuestLog),
        typeof(ReadyCheckDialog),
        typeof(ShopMenu),
        typeof(SpecialOrdersBoard),
        typeof(StorageContainer),
        typeof(TailoringMenu),

        /*********
        ** Title Menus
        *********/
        typeof(TitleMenu),
        // typeof(AdvancedGameOptions),
        typeof(CharacterCustomization),
        typeof(CoopMenu),
        typeof(FarmhandMenu),
        typeof(LoadGameMenu),

        /*********
        ** Game Menu Pages
        *********/
        typeof(AnimalPage),
        typeof(CollectionsPage),
        typeof(CraftingPage),
        typeof(ExitPage),
        typeof(InventoryPage),
        typeof(PowersTab),
        typeof(SkillsPage),
        typeof(SocialPage),


        /*********
        ** Custom Menus
        *********/
        typeof(TileDataEntryMenu),
    ];

    private static bool justOpened = true;

    internal static HashSet<string> ManuallyPatchedCustomMenus = [];

    internal static string? CurrentMenu;
    internal static bool ManuallyCallingDrawPatch = false;

    public void Apply(Harmony harmony)
    {
        harmony.Patch(
                original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.exitThisMenu)),
                postfix: new HarmonyMethod(typeof(IClickableMenuPatch), nameof(IClickableMenuPatch.ExitThisMenuPatch))
        );

        harmony.Patch(
            original: AccessTools.Method(typeof(IClickableMenu), "draw", new Type[] { typeof(SpriteBatch) }),
            postfix: new HarmonyMethod(typeof(IClickableMenuPatch), nameof(IClickableMenuPatch.DrawPatch))
        );

        harmony.Patch(
            original: AccessTools.Method(typeof(IClickableMenu), "draw", new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) }),
            postfix: new HarmonyMethod(typeof(IClickableMenuPatch), nameof(IClickableMenuPatch.DrawPatch))
        );
    }

    public static void DrawPatch()
    {
        try
        {
            // The only case when the active menu is null (in vanilla stardew) is when a hud message with no icon is displayed.
            if (Game1.activeClickableMenu is null) return;

            var activeMenu = GetActiveMenu();
            if (activeMenu is null) return;

            // FIXME For some reason custom mod menus don't trigger this patch, even though they implement IClickableMenu,
            // the following method essentially detects if the active menu hasn't triggered this method
            // and then manually calls it from ModEntry::OnUpdateTicked
            if (!ManuallyCallingDrawPatch) CurrentMenu = activeMenu.GetType()?.FullName;
            else ManuallyCallingDrawPatch = false;

            var activeMenuType = activeMenu.GetType();
            if (activeMenuType is not null
                    && (SkipMenuTypes.Contains(activeMenuType) || ManuallyPatchedCustomMenus.Contains(activeMenuType.FullName ?? "")))
            {
                return;
            }

#if DEBUG
            if (justOpened)
            {
                justOpened = false;
                Log.Debug($"[IClickableMenuPatch.DrawPatch] Attempting to patch menu {{ManuallyCalled:{ManuallyCallingDrawPatch}}}: {activeMenuType?.FullName}");
            }
#endif

            if (activeMenu.currentlySnappedComponent == null || string.IsNullOrWhiteSpace(activeMenu!.currentlySnappedComponent.ScreenReaderText))
            {
                if (OptionsElementUtils.NarrateOptionSlotsInMenuUsingReflection(activeMenu))
                    return;
            }
            else
            {
                ClickableComponentUtils.NarrateComponent(activeMenu.currentlySnappedComponent);
                return;
            }

            ClickableComponentUtils.NarrateHoveredComponentFromList(activeMenu.allClickableComponents);

            /*********
            ** TODO:
            ** 1. Speak hovered text and/or item when all other methods fail
            ** 2. Use reflection on menu to speak common ui elements
            *********/

        }
        catch (Exception e)
        {
            Log.Error($"[IClickableMenuPatch.DrawPatch] An error occurred in draw patch:\n{e.StackTrace}\n{e.Message}");
        }
    }

    private static IClickableMenu? GetActiveMenu()
    {
        var activeMenu = Game1.activeClickableMenu;

        if (activeMenu.GetParentMenu() != null)
        {
            // To let the parent menu's draw() call set `activeMenu` to the child menu, in the next if condition.
            return null;
        }

        if (activeMenu.GetChildMenu() != null && activeMenu.GetChildMenu().IsActive())
        {
            activeMenu = activeMenu.GetChildMenu();
        }

        if (activeMenu is TitleMenu titleMenu && TitleMenu.subMenu != null)
        {
            return TitleMenu.subMenu;
        }

        if (activeMenu is GameMenu gameMenu)
        {
            return gameMenu.GetCurrentPage();
        }

        return activeMenu;
    }

    private static void ExitThisMenuPatch(IClickableMenu __instance)
    {
        try
        {
            Log.Verbose($"[IClickableMenuPatch.ExitThisMenuPatch] Closed {__instance.GetType().FullName} menu, performing cleanup...");
            Cleanup(__instance);
        }
        catch (Exception e)
        {
            Log.Error($"[IClickableMenuPatch.ExitThisMenuPatch] An error occurred in exit this menu patch:\n{e.Message}\n{e.StackTrace}");
        }
    }

    internal static void Cleanup(IClickableMenu menu)
    {
        switch (menu)
        {
            case LetterViewerMenu:
                LetterViewerMenuPatch.Cleanup();
                break;
            case GameMenu:
                CraftingPagePatch.Cleanup();
                break;
            case JunimoNoteMenu:
                JunimoNoteMenuPatch.Cleanup();
                break;
            case CarpenterMenu:
                CarpenterMenuPatch.Cleanup();
                break;
            case PurchaseAnimalsMenu:
                PurchaseAnimalsMenuPatch.Cleanup();
                break;
            case AnimalQueryMenu:
                AnimalQueryMenuPatch.Cleanup();
                break;
            case DialogueBox:
                DialogueBoxPatch.Cleanup();
                break;
            case QuestLog:
                QuestLogPatch.Cleanup();
                break;
            case PondQueryMenu:
                PondQueryMenuPatch.Cleanup();
                break;
            case NumberSelectionMenu:
                NumberSelectionMenuPatch.Cleanup();
                break;
            case NamingMenu:
                NamingMenuPatch.Cleanup();
                break;
            case RenovateMenu:
                RenovateMenuPatch.Cleanup();
                break;
        }

        MainClass.ScreenReader.Cleanup();
        InventoryUtils.Cleanup();
        TextBoxPatch.activeTextBoxes = "";
        CurrentMenu = null;
        ManuallyCallingDrawPatch = false;
        justOpened = true;
    }
}
