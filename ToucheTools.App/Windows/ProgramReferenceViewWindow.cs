using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;
using ToucheTools.Constants;
using ToucheTools.Models;

namespace ToucheTools.App.Windows;

public class ProgramReferenceViewWindow : IWindow
{
    public readonly DatabaseModel _model;
    private readonly WindowSettings _windowSettings;
    private readonly ActiveProgramState _activeProgramState;

    public ProgramReferenceViewWindow(WindowSettings windowSettings, ActiveProgramState activeProgramState, DatabaseModel model)
    {
        _windowSettings = windowSettings;
        _activeProgramState = activeProgramState;
        _model = model;
    }

    public void Render()
    {
        if (!_windowSettings.ProgramViewOpen)
        {
            return;
        }
        
        //TODO: move this
        _activeProgramState.Tick();
        
        var viewW = 350.0f;
        var viewH = 600.0f;
        ImGui.SetNextWindowPos(new Vector2(400.0f, 200.0f));
        ImGui.SetNextWindowSize(new Vector2(viewW, viewH));
        ImGui.Begin("Program References", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar);

        var state = _activeProgramState.CurrentState;
        var program = _model.Programs[state.CurrentProgram];

        if (ImGui.Button("Step"))
        {
            _activeProgramState.Step();
        }

        ImGui.SameLine();
        if (ImGui.Button("Step Until Paused"))
        {
            _activeProgramState.StepUntilPaused();
        }
        
        ImGui.SameLine();
        if (ImGui.Button(_activeProgramState.AutoPlay ? "Stop Auto" : "Step Auto"))
        {
            _activeProgramState.AutoPlay = !_activeProgramState.AutoPlay;
        }

        ImGui.Separator();
        
        #region Scripts
        if (ImGui.CollapsingHeader("Scripts", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.BeginTable("scripts", 5);
            ImGui.TableSetupColumn("Type");
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Offset");
            ImGui.TableSetupColumn("State");
            ImGui.TableSetupColumn("Delay");
            ImGui.TableHeadersRow();
            foreach (var script in state.Scripts)
            {
                var isRunning = script.Status == ActiveProgramState.ProgramState.ScriptStatus.Running;
                var isPaused = script.Status == ActiveProgramState.ProgramState.ScriptStatus.Paused;
                var isStopped = script.Status == ActiveProgramState.ProgramState.ScriptStatus.Stopped ||
                                script.Status == ActiveProgramState.ProgramState.ScriptStatus.NotInit;
                
                if (isStopped)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.15f, 0.2f, 0.15f, 1.0f));
                } else if (isPaused)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.25f, 0.3f, 0.35f, 1.0f));
                }
                ImGui.TableNextColumn();
                if (isRunning)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(new Vector4(0.3f, 0.6f, 0.8f, 1.0f)));
                }
                ImGui.Text($"{script.Type:G}");
                ImGui.TableNextColumn();
                if (isRunning)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(new Vector4(0.3f, 0.6f, 0.8f, 1.0f)));
                }
                ImGui.Text($"{script.Id}");
                ImGui.TableNextColumn();
                if (isRunning)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(new Vector4(0.3f, 0.6f, 0.8f, 1.0f)));
                }
                ImGui.Text($"{script.Offset}");
                
                ImGui.TableNextColumn();
                if (isRunning)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(new Vector4(0.3f, 0.6f, 0.8f, 1.0f)));
                }
                ImGui.Text($"{script.Status:G}");
                
                ImGui.TableNextColumn();
                if (isRunning)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(new Vector4(0.3f, 0.6f, 0.8f, 1.0f)));
                }
                ImGui.Text($"{script.Delay}");
                
                ImGui.TableNextRow();
                
                if (isStopped || isPaused)
                {
                    ImGui.PopStyleColor();
                }
            }
            
            ImGui.EndTable();
        }
        #endregion
        
        ImGui.Separator();

        #region Status
        var currentScript = state.GetRunningScript();
        if (currentScript == null)
        {
            ImGui.Text("Paused waiting for scripts.");
        }
        else
        {
            ImGui.Text($"Current script: {currentScript.Type:G} {currentScript.Id}");
            LabelAndButton($"Current offset: ", currentScript.Offset.ToString("D5"));
        }
        #endregion
        
        ImGui.Separator();
        
        #region Sprites
        if (ImGui.CollapsingHeader($"Sprites"))
        {
            ImGui.BeginTable("sprites", 3);
            ImGui.TableSetupColumn("Index");
            ImGui.TableSetupColumn("Spr Num");
            ImGui.TableSetupColumn("Seq Num");
            ImGui.TableHeadersRow();
            var idx = 0;
            foreach (var sprite in _activeProgramState.LoadedSprites)
            {
                if (sprite.SpriteNum != null || sprite.SequenceNum != null)
                {
                    ImGui.TableNextColumn();
                    ImGui.Text($"{idx}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{sprite.SpriteNum}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{sprite.SequenceNum}");
                    ImGui.TableNextRow();
                }

                idx++;
            }
            
            ImGui.EndTable();
        }
        #endregion
        
        ImGui.Separator();
        
        #region Key Char Graphics
        if (ImGui.CollapsingHeader($"Key Char Graphics"))
        {
            ImGui.BeginTable("key_char_graphics", 4);
            ImGui.TableSetupColumn("Index");
            ImGui.TableSetupColumn("Spr Num");
            ImGui.TableSetupColumn("Seq Num");
            ImGui.TableSetupColumn("Char");
            ImGui.TableHeadersRow();
            foreach (var (keyCharId, keyChar) in _activeProgramState.KeyChars)
            {
                if (keyChar.Initialised && !keyChar.OffScreen && keyChar.SpriteIndex != null && keyChar.SequenceIndex != null)
                {
                    var spriteIndex = keyChar.SpriteIndex.Value;
                    var seqIndex = keyChar.SequenceIndex.Value;
                    var sprite = _activeProgramState.LoadedSprites[spriteIndex];
                    var seq = _activeProgramState.LoadedSprites[seqIndex];
                    var character = keyChar.Character?.ToString() ?? "";
                    
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyCharId}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{spriteIndex} ({sprite.SpriteNum})");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{seqIndex} ({seq.SequenceNum})");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{character}");
                    ImGui.TableNextRow();
                }
            }
            ImGui.EndTable();
        }
        #endregion
        ImGui.Separator();
        
        ImGui.End();
        return;
        
        #region Stack
        ImGui.Text($"STK at: {state.StackPointer:D4} value: {state.StackValue}");
        if (ImGui.CollapsingHeader("Stack"))
        {
            ImGui.BeginTable("stack", 2);
            ImGui.TableSetupColumn("Cell");
            ImGui.TableSetupColumn("Value");
            ImGui.TableHeadersRow();
            foreach (var (idx, value) in state.GetFullStackValues().OrderBy(p => p.Key))
            {
                ImGui.TableNextColumn();
                if (idx == state.StackPointer)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(new Vector4(0.3f, 0.6f, 0.8f, 1.0f)));
                }
                ImGui.Text($"{idx:D3}");
                ImGui.TableNextColumn();
                if (idx == state.StackPointer)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(new Vector4(0.3f, 0.6f, 0.8f, 1.0f)));
                }
                ImGui.Text($"{value}");
                ImGui.TableNextRow();
            }
            
            ImGui.EndTable();
        }
        #endregion
        
        ImGui.Separator();
        
        #region Flags
        if (ImGui.CollapsingHeader("Flags"))
        {
            ImGui.BeginTable("flags", 2);
            ImGui.TableSetupColumn("Flag");
            ImGui.TableSetupColumn("Value");
            ImGui.TableHeadersRow();
            foreach (var (flagId, flagValue) in _activeProgramState.Flags.OrderBy(p => p.Key))
            {
                var flagName = Flags.GetFlagText(flagId);
                ImGui.TableNextColumn();
                ImGui.Text($"{flagName}");
                ImGui.TableNextColumn();
                ImGui.Text($"{flagValue}");
                ImGui.TableNextRow();
            }
            
            ImGui.EndTable();
        }
        #endregion
        
        ImGui.Separator();
        
        #region Location
        if (state.LoadedRoom == null)
        {
            ImGui.Text($"Not in a room yet");
        }
        else
        {
            LabelAndButton("In room: ", $"{state.LoadedRoom}");
        }
        #endregion
        
        ImGui.Separator();
        
        #region Inventory
        ImGui.Text("Global money: " + _activeProgramState.GlobalMoney);
        if (ImGui.CollapsingHeader("Item Inventory"))
        {
            ImGui.BeginTable("item_inventory", 6);
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Item");
            ImGui.TableHeadersRow();
            var inventoryId = 0;
            foreach (var inventoryList in _activeProgramState.InventoryLists)
            {
                foreach (var item in inventoryList.GetActualItems())
                {
                    ImGui.TableNextColumn();
                    ImGui.Text($"{inventoryId}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{item:D2}");
                    ImGui.TableNextRow();
                }
                inventoryId++;
            }
            
            ImGui.EndTable();
        }
        #endregion
        
        ImGui.Separator();
        
        #region Backgrounds
        if (program.Backgrounds.Count > 0)
        {
            if (ImGui.CollapsingHeader("Background Areas", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.BeginTable("background_areas", 8);
                ImGui.TableSetupColumn("ID");
                ImGui.TableSetupColumn("Draw");
                ImGui.TableSetupColumn("DX");
                ImGui.TableSetupColumn("DY");
                ImGui.TableSetupColumn("W");
                ImGui.TableSetupColumn("H");
                ImGui.TableSetupColumn("SX");
                ImGui.TableSetupColumn("SY");
                ImGui.TableHeadersRow();
                ushort idx = 0;
                foreach (var background in program.Backgrounds)
                {
                    var destX = background.Rect.X;
                    if (state.BackgroundOffsets.ContainsKey(idx))
                    {
                        destX = state.BackgroundOffsets[idx].Item1;
                    }
                    var destY = background.Rect.Y;
                    if (state.BackgroundOffsets.ContainsKey(idx))
                    {
                        destY = state.BackgroundOffsets[idx].Item2;
                    }

                    ImGui.TableNextColumn();
                    ImGui.Text($"{idx}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{(destY != 20000 ? "Y" : "N")}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{destX}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{destY}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{background.Rect.W}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{background.Rect.H}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{background.SrcX}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{background.SrcY}");
                    ImGui.TableNextRow();
                    idx++;
                }
            
                ImGui.EndTable();
            }
        }
        #endregion
        
        ImGui.End();
    }

    private bool LabelAndButton(string label, string button)
    {
        ImGui.Text(label);
        ImGui.SameLine();
        return ImGui.Button(button);
    }
}