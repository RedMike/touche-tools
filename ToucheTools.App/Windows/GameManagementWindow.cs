using System.Numerics;
using ImGuiNET;
using ToucheTools.App.Models;
using ToucheTools.App.State;
using ToucheTools.App.Utils;
using ToucheTools.App.ViewModels;
using ToucheTools.Constants;

namespace ToucheTools.App.Windows;

public class GameManagementWindow : BaseWindow
{
    private readonly OpenedManifest _manifest;
    private readonly MainWindowState _state;
    private readonly GameManagementState _gameManagementState;

    public GameManagementWindow(OpenedManifest manifest, MainWindowState state, GameManagementState gameManagementState)
    {
        _manifest = manifest;
        _state = state;
        _gameManagementState = gameManagementState;
    }

    public override void Render()
    {
        if (_state.State != MainWindowState.States.GameManagement)
        {
            return;
        }

        var game = _manifest.GetGame();
        
        ImGui.Begin("Game Editor", ImGuiWindowFlags.NoCollapse);

        if (ImGui.TreeNodeEx("Colours"))
        {
            foreach (var (colId, (origR, origG, origB)) in game.CustomColors)
            {
                if (Palettes.TransparencyColor == colId)
                {
                    continue; //does not need to be edited because it's not representable
                }
                if (Palettes.TransparentSpriteMarkerColor == colId)
                {
                    continue; //does not need to be edited because it's not representable
                }

                var isRemovable = !Palettes.SpecialColors.Contains(colId);
                var id = ColourHelper.ColourName(colId);

                ImGui.Text($"{id} -");

                int r = origR;
                int g = origG;
                int b = origB;
                
                ImGui.SameLine();
                ImGui.PushID($"Col{colId}R");
                ImGui.SetNextItemWidth(80.0f);
                ImGui.InputInt("", ref r, 1, 1);
                ImGui.PopID();
                if (r < 0)
                {
                    r = 0;
                }
                if (r > 255)
                {
                    r = 255;
                }
                
                ImGui.SameLine();
                ImGui.PushID($"Col{colId}G");
                ImGui.SetNextItemWidth(80.0f);
                ImGui.InputInt("", ref g, 1, 1);
                ImGui.PopID();
                if (g < 0)
                {
                    g = 0;
                }
                if (g > 255)
                {
                    g = 255;
                }
                
                ImGui.SameLine();
                ImGui.PushID($"Col{colId}B");
                ImGui.SetNextItemWidth(80.0f);
                ImGui.InputInt("", ref b, 1, 1);
                ImGui.PopID();
                if (b < 0)
                {
                    b = 0;
                }
                if (b > 255)
                {
                    b = 255;
                }
                
                if (r != origR || g != origG || b != origB)
                {
                    game.CustomColors[colId] = ((byte)r, (byte)g, (byte)b);
                    _manifest.ForceUpdate();
                }

                if (isRemovable)
                {
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(80.0f);
                    ImGui.PushID($"Col{colId}Remove");
                    if (ImGui.Button("Remove"))
                    {
                        game.CustomColors.Remove(colId);
                        _manifest.ForceUpdate();
                    }
                    ImGui.PopID();
                }
                
                ImGui.SameLine();
                ImGui.SetNextItemWidth(40.0f);
                ImGui.PushID($"Col{colId}Remove");
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(r/255.0f, g/255.0f, b/255.0f, 1.0f));
                ImGui.Button("Example");
                ImGui.PopStyleColor();
                ImGui.PopID();
                
            }

            var remainingCols = Enumerable.Range(0, 255)
                .Where(c => 
                    c >= Palettes.StartOfSpriteColors && //TODO: should the room colours also be settable?
                    !Palettes.SpecialColors.Contains(c) &&
                    !game.CustomColors.ContainsKey(c)
                ).ToList();
            var origCurrentCol = remainingCols.FindIndex(c => c == _gameManagementState.CurrentColour);
            var currentCol = origCurrentCol;
            
            ImGui.PushID($"ColAdd");
            ImGui.Combo("", ref currentCol, remainingCols.Select(c => $"{c}").ToArray(), remainingCols.Count);
            if (currentCol != origCurrentCol)
            {
                _gameManagementState.CurrentColour = remainingCols[currentCol];
            }
            ImGui.PopID();

            ImGui.SameLine();
            if (ImGui.Button("Add"))
            {
                var curCol = _gameManagementState.CurrentColour;
                game.CustomColors[curCol] = (255, 0, 255);
                _manifest.ForceUpdate();
            }

            ImGui.TreePop();
        }
        
        if (ImGui.TreeNodeEx("Actions"))
        {
            foreach (var (actionId, origLabel) in game.ActionDefinitions)
            {
                var id = $"{actionId}";
                if (actionId == -Actions.DoNothing)
                {
                    id = "DoNothing";
                } else if (actionId == -Actions.LeftClick)
                {
                    id = "LeftClick";
                } else if (actionId == -Actions.LeftClickWithItem)
                {
                    id = "LeftClickWithItem";
                }

                ImGui.Text($"{id} -");
                ImGui.SameLine();
                var actionLabel = origLabel;
                ImGui.PushID($"Action{actionId}");
                ImGui.InputText("", ref actionLabel, 32);
                if (actionLabel != origLabel)
                {
                    game.ActionDefinitions[actionId] = actionLabel;
                }
                ImGui.PopID();
            }

            if (ImGui.Button("Add Action"))
            {
                var newId = 1;
                if (game.ActionDefinitions.Any(a => a.Key >= 0 && !Actions.BuiltInActions.Contains(-a.Key)))
                {
                    newId = game.ActionDefinitions.Where(a => a.Key >= 0 && !Actions.BuiltInActions.Contains(-a.Key))
                        .Select(a => a.Key).Max() + 1;
                    while (Actions.BuiltInActions.Contains(-newId) || game.ActionDefinitions.ContainsKey(newId))
                    {
                        newId++;
                    }
                }

                game.ActionDefinitions[newId] = "~";
            }
            
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx("Inventory Items"))
        {
            //inventory items are tied to icons, so only loaded icons are valid
            var validItemIds = _manifest.GetIncludedImages().Where(p => p.Value.Type == OpenedManifest.ImageType.Icon)
                .Select(p => p.Value.Index)
                .Where(id => id != Icons.MoneyIcon && id != Icons.DefaultMouseCursor)
                .ToList();
            
            var actions = game.ActionDefinitions
                .Select(a => (a.Key, a.Value))
                .Where(a => !Actions.BuiltInActions.Contains(-a.Key))
                .ToList();
            actions.Insert(0, (-1, "-"));
            var actionList = actions.Select(a => a.Key < 0 ? $"-" : $"Action {a.Key} ({a.Value})").ToArray();

            foreach (var itemId in validItemIds)
            {
                var itemActions = new int[8];
                var origLabel = "";
                if (game.InventoryItems.ContainsKey(itemId))
                {
                    origLabel = game.InventoryItems[itemId].DefaultLabel;
                    itemActions = game.InventoryItems[itemId].DefaultActions;
                }

                if (ImGui.TreeNodeEx($"Inventory{itemId}", ImGuiTreeNodeFlags.None, $"Item {itemId} - {origLabel}"))
                {
                    var label = origLabel;
                    
                    ImGui.PushID($"Inventory{itemId}label");
                    ImGui.InputText("", ref label, 32);
                    ImGui.PopID();
                    if (label != origLabel)
                    {
                        if (!game.InventoryItems.ContainsKey(itemId))
                        {
                            game.InventoryItems[itemId] = new GameModel.InventoryItem();
                        }
                        game.InventoryItems[itemId].DefaultLabel = label;
                    }
                    
                    for (var i = 0; i < 8; i++)
                    {
                        var origSelectedAction = actions.FindIndex(a => a.Key == itemActions[i]);
                        if (origSelectedAction == -1)
                        {
                            origSelectedAction = 0;
                        }
                        var selectedAction = origSelectedAction;
                        ImGui.PushID($"Inventory{itemId}Action{i}");
                        ImGui.Combo("", ref selectedAction, actionList, actionList.Length);
                        if (selectedAction != origSelectedAction)
                        {
                            if (!game.InventoryItems.ContainsKey(itemId))
                            {
                                game.InventoryItems[itemId] = new GameModel.InventoryItem();
                            }
                            
                            var actionId = actions[selectedAction].Key;
                            game.InventoryItems[itemId].DefaultActions[i] = actionId;
                        }
                        ImGui.PopID();
                    }
                    
                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }
        
        ImGui.End();
    }
}