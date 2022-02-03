using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace stardew_access.Patches
{
    internal class BuildingNAnimalMenuPatches
    {
        internal static Vector2[] marked = new Vector2[10];
        internal static Building?[] availableBuildings = new Building[100];
        internal static CarpenterMenu? carpenterMenu = null;
        internal static string carpenterMenuQuery = "";
        internal static bool isSayingBlueprintInfo = false;
        internal static string prevBlueprintInfo = "";
        internal static bool isOnFarm = false, isUpgrading = false, isDemolishing = false, isPainting = false, isConstructing = false, isMoving = false, isMagicalConstruction = false;

        internal static void CarpenterMenuPatch(
            CarpenterMenu __instance, bool ___onFarm, List<Item> ___ingredients, int ___price,
            List<BluePrint> ___blueprints, int ___currentBlueprintIndex, bool ___upgrading, bool ___demolishing, bool ___moving,
            bool ___painting, bool ___magicalConstruction)
        {
            try
            {
                isOnFarm = ___onFarm;
                carpenterMenu = __instance;
                isMagicalConstruction = ___magicalConstruction;
                if (!___onFarm)
                {
                    isUpgrading = false;
                    isDemolishing = false;
                    isPainting = false;
                    isMoving = false;
                    isConstructing = false;

                    #region The blueprint menu
                    BluePrint currentBluprint = __instance.CurrentBlueprint;
                    if (currentBluprint == null)
                        return;

                    int x = Game1.getMouseX(), y = Game1.getMouseY(); // Mouse x and y position
                    bool isBPressed = Game1.input.GetKeyboardState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B);
                    string ingredients = "";
                    string name = currentBluprint.displayName;
                    string upgradeName = currentBluprint.nameOfBuildingToUpgrade;
                    string description = currentBluprint.description;
                    string price = $"{___price}g";
                    string blueprintInfo;

                    #region Get ingredients
                    for (int i = 0; i < ___ingredients.Count; i++)
                    {
                        string itemName = ___ingredients[i].DisplayName;
                        int itemStack = ___ingredients[i].Stack;
                        string itemQuality = "";

                        int qualityValue = ((StardewValley.Object)___ingredients[i]).quality;
                        if (qualityValue == 1)
                        {
                            itemQuality = "Silver quality";
                        }
                        else if (qualityValue == 2 || qualityValue == 3)
                        {
                            itemQuality = "Gold quality";
                        }
                        else if (qualityValue >= 4)
                        {
                            itemQuality = "Iridium quality";
                        }

                        ingredients = $"{ingredients}, {itemStack} {itemName} {itemQuality}";
                    }
                    #endregion

                    blueprintInfo = $"{name}, Price: {price}, Ingredients: {ingredients}, Description: {description}";

                    if (isBPressed && !isSayingBlueprintInfo)
                    {
                        SayBlueprintInfo(blueprintInfo);
                    }
                    else if (prevBlueprintInfo != blueprintInfo)
                    {
                        prevBlueprintInfo = blueprintInfo;
                        SayBlueprintInfo(blueprintInfo);
                    }
                    else
                    {
                        if (__instance.backButton != null && __instance.backButton.containsPoint(x, y))
                        {
                            string toSpeak = "Previous Blueprint";
                            if (carpenterMenuQuery != toSpeak)
                            {
                                carpenterMenuQuery = toSpeak;
                                MainClass.screenReader.Say(toSpeak, true);
                            }
                            return;
                        }

                        if (__instance.forwardButton != null && __instance.forwardButton.containsPoint(x, y))
                        {
                            string toSpeak = "Next Blueprint";
                            if (carpenterMenuQuery != toSpeak)
                            {
                                carpenterMenuQuery = toSpeak;
                                MainClass.screenReader.Say(toSpeak, true);
                            }
                            return;
                        }

                        if (__instance.demolishButton != null && __instance.demolishButton.containsPoint(x, y))
                        {
                            string toSpeak = $"Demolish Building" + (__instance.CanDemolishThis(___blueprints[___currentBlueprintIndex]) ? "" : ", cannot demolish building");
                            if (carpenterMenuQuery != toSpeak)
                            {
                                carpenterMenuQuery = toSpeak;
                                MainClass.screenReader.Say(toSpeak, true);
                            }
                            return;
                        }

                        if (__instance.okButton != null && __instance.okButton.containsPoint(x, y))
                        {
                            string toSpeak = "Cunstruct Building" + (___blueprints[___currentBlueprintIndex].doesFarmerHaveEnoughResourcesToBuild() ? "" : ", cannot cunstrut building, not enough resources to build.");
                            if (carpenterMenuQuery != toSpeak)
                            {
                                carpenterMenuQuery = toSpeak;
                                MainClass.screenReader.Say(toSpeak, true);
                            }
                            return;
                        }

                        if (__instance.moveButton != null && __instance.moveButton.containsPoint(x, y))
                        {
                            string toSpeak = "Move Building";
                            if (carpenterMenuQuery != toSpeak)
                            {
                                carpenterMenuQuery = toSpeak;
                                MainClass.screenReader.Say(toSpeak, true);
                            }
                            return;
                        }

                        if (__instance.paintButton != null && __instance.paintButton.containsPoint(x, y))
                        {
                            string toSpeak = "Paint Building";
                            if (carpenterMenuQuery != toSpeak)
                            {
                                carpenterMenuQuery = toSpeak;
                                MainClass.screenReader.Say(toSpeak, true);
                            }
                            return;
                        }

                        if (__instance.cancelButton != null && __instance.cancelButton.containsPoint(x, y))
                        {
                            string toSpeak = "Cancel Button";
                            if (carpenterMenuQuery != toSpeak)
                            {
                                carpenterMenuQuery = toSpeak;
                                MainClass.screenReader.Say(toSpeak, true);
                            }
                            return;
                        }
                    }
                    #endregion
                }
                else
                {
                    if (___demolishing)
                        isDemolishing = true;
                    else if (___upgrading)
                        isUpgrading = true;
                    else if (___painting)
                        isPainting = true;
                    else if (___moving)
                        isMoving = true;
                    else
                        isConstructing = true;
                }
            }
            catch (Exception e)
            {
                MainClass.monitor.Log($"Unable to narrate Text:\n{e.Message}\n{e.StackTrace}", LogLevel.Error);
            }
        }
        private static async void SayBlueprintInfo(string info)
        {
            isSayingBlueprintInfo = true;
            MainClass.screenReader.Say(info, true);
            await Task.Delay(300);
            isSayingBlueprintInfo = false;
        }

        public static string? Demolish(Building? toDemolish)
        {
            if (toDemolish == null)
                return null;

            string? response = null;
            // This code is taken from the game's code (CarpenterMenu.cs::654)
            Farm farm = Game1.getLocationFromName("Farm") as Farm;
            Action buildingLockFailed = delegate
            {
                if (isDemolishing)
                {
                    response = Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed");
                }
            };
            Action continueDemolish = delegate
            {
                if (isDemolishing && toDemolish != null && farm.buildings.Contains(toDemolish))
                {
                    if ((int)toDemolish.daysOfConstructionLeft > 0 || (int)toDemolish.daysUntilUpgrade > 0)
                    {
                        response = Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_DuringConstruction");
                    }
                    else if (toDemolish.indoors.Value != null && toDemolish.indoors.Value is AnimalHouse && (toDemolish.indoors.Value as AnimalHouse).animalsThatLiveHere.Count > 0)
                    {
                        response = Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_AnimalsHere");
                    }
                    else if (toDemolish.indoors.Value != null && toDemolish.indoors.Value.farmers.Any())
                    {
                        response = Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere");
                    }
                    else
                    {
                        if (toDemolish.indoors.Value != null && toDemolish.indoors.Value is Cabin)
                        {
                            foreach (Farmer current in Game1.getAllFarmers())
                            {
                                if (current.currentLocation != null && current.currentLocation.Name == (toDemolish.indoors.Value as Cabin).GetCellarName())
                                {
                                    response = Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere");
                                    return;
                                }
                            }
                        }
                        if (toDemolish.indoors.Value is Cabin && (toDemolish.indoors.Value as Cabin).farmhand.Value.isActive())
                        {
                            response = Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_FarmhandOnline");
                        }
                        else
                        {
                            toDemolish.BeforeDemolish();
                            Chest chest = null;
                            if (toDemolish.indoors.Value is Cabin)
                            {
                                List<Item> list = (toDemolish.indoors.Value as Cabin).demolish();
                                if (list.Count > 0)
                                {
                                    chest = new Chest(playerChest: true);
                                    chest.fixLidFrame();
                                    chest.items.Set(list);
                                }
                            }
                            if (farm.destroyStructure(toDemolish))
                            {
                                _ = (int)toDemolish.tileY;
                                _ = (int)toDemolish.tilesHigh;
                                Game1.flashAlpha = 1f;
                                toDemolish.showDestroyedAnimation(Game1.getFarm());
                                Game1.playSound("explosion");
                                Utility.spreadAnimalsAround(toDemolish, farm);
                                DelayedAction.functionAfterDelay(carpenterMenu.returnToCarpentryMenu, 1500);
                                // freeze = true;
                                if (chest != null)
                                {
                                    farm.objects[new Vector2((int)toDemolish.tileX + (int)toDemolish.tilesWide / 2, (int)toDemolish.tileY + (int)toDemolish.tilesHigh / 2)] = chest;
                                }
                            }
                        }
                    }
                }
            };
            if (toDemolish != null)
            {
                if (toDemolish.indoors.Value != null && toDemolish.indoors.Value is Cabin && !Game1.IsMasterGame)
                {
                    response = Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed");
                    toDemolish = null;
                    return response;
                }
                if (!carpenterMenu.CanDemolishThis(toDemolish))
                {
                    toDemolish = null;
                    return response;
                }
                if (!Game1.IsMasterGame && !carpenterMenu.hasPermissionsToDemolish(toDemolish))
                {
                    toDemolish = null;
                    return response;
                }
            }
            if (toDemolish != null && toDemolish.indoors.Value is Cabin)
            {
                Cabin cabin = toDemolish.indoors.Value as Cabin;
                if (cabin.farmhand.Value != null && (bool)cabin.farmhand.Value.isCustomized)
                {
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\UI:Carpenter_DemolishCabinConfirm", cabin.farmhand.Value.Name), Game1.currentLocation.createYesNoResponses(), delegate (Farmer f, string answer)
                    {
                        if (answer == "Yes")
                        {
                            Game1.activeClickableMenu = carpenterMenu;
                            Game1.player.team.demolishLock.RequestLock(continueDemolish, buildingLockFailed);
                        }
                        else
                        {
                            DelayedAction.functionAfterDelay(carpenterMenu.returnToCarpentryMenu, 1000);
                        }
                    });
                    return response;
                }
            }
            if (toDemolish != null)
            {
                Game1.player.team.demolishLock.RequestLock(continueDemolish, buildingLockFailed);
            }

            return response;
        }

        public static string? Contstruct(Building? toCunstruct, Vector2 position)
        {
            string? response = null;
            Game1.player.team.buildLock.RequestLock(delegate
            {
                if (isOnFarm && Game1.locationRequest == null)
                {
                    if (tryToBuild(position))
                    {
                        carpenterMenu.CurrentBlueprint.consumeResources();
                        DelayedAction.functionAfterDelay(carpenterMenu.returnToCarpentryMenuAfterSuccessfulBuild, 2000);
                        // freeze = true;
                    }
                    else
                    {
                        response = Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild");
                    }
                }
                Game1.player.team.buildLock.ReleaseLock();
            });

            return response;
        }

        public static bool tryToBuild(Vector2 position)
        {
            return ((Farm)Game1.getLocationFromName("Farm")).buildStructure(carpenterMenu.CurrentBlueprint, position, Game1.player, isMagicalConstruction);
        }

        public static string? Upgrade(Building? toUpgrade)
        {
            string? response = null;
            // This code is taken from the game's code (CarpenterMenu.cs::775)
            if (toUpgrade != null && carpenterMenu.CurrentBlueprint.name != null && toUpgrade.buildingType.Equals(carpenterMenu.CurrentBlueprint.nameOfBuildingToUpgrade))
            {
                carpenterMenu.CurrentBlueprint.consumeResources();
                toUpgrade.daysUntilUpgrade.Value = 2;
                toUpgrade.showUpgradeAnimation(Game1.getFarm());
                Game1.playSound("axe");
                DelayedAction.functionAfterDelay(carpenterMenu.returnToCarpentryMenuAfterSuccessfulBuild, 1500);
                // freeze = true;
                // Game1.multiplayer.globalChatInfoMessage("BuildingBuild", Game1.player.Name, Utility.AOrAn(carpenterMenu.CurrentBlueprint.displayName), carpenterMenu.CurrentBlueprint.displayName, Game1.player.farmName);
            }
            else if (toUpgrade != null)
            {
                response = Game1.content.LoadString("Strings\\UI:Carpenter_CantUpgrade_BuildingType");
            }
            return response;
        }

        public static string? Paint(Building? toPaint)
        {
            string? response = null;
            Farm farm_location = Game1.getFarm();
            if (toPaint != null)
            {
                if (!toPaint.CanBePainted())
                {
                    response = Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint");
                    return response;
                }
                if (!carpenterMenu.HasPermissionsToPaint(toPaint))
                {
                    response = Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint_Permission");
                    return response;
                }
                toPaint.color.Value = Color.White;
                carpenterMenu.SetChildMenu(new BuildingPaintMenu(toPaint));
            }
            /* TODO Add painting of farm house
            else if (farm_location.GetHouseRect().Contains(Utility.Vector2ToPoint(new Vector2(toPaint.tileX, toPaint.tileY))))
            {
                if (!carpenterMenu.CanPaintHouse())
                {
                    response = Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint");
                }
                else if (!carpenterMenu.HasPermissionsToPaint(null))
                {
                    response = Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint_Permission");
                }
                else
                {
                    carpenterMenu.SetChildMenu(new BuildingPaintMenu("House", () => (farm_location.paintedHouseTexture != null) ? farm_location.paintedHouseTexture : Farm.houseTextures, farm_location.houseSource.Value, farm_location.housePaintColor.Value));
                }
            }*/
            return response;
        }

        public static string? Move(Building? toMove, Vector2 position)
        {
            string? response = null;

            return response;
        }
    }

}