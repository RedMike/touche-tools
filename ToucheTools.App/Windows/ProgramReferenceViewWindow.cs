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
            _activeProgramState.BreakpointHit = false;
            _activeProgramState.AutoPlay = !_activeProgramState.AutoPlay;
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Press ESC"))
        {
            _activeProgramState.PressEscape();
        }

        ImGui.Separator();
        
        #region Scripts
        if (ImGui.CollapsingHeader("Scripts", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.BeginTable("scripts", 5);
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Offset");
            ImGui.TableSetupColumn("State");
            ImGui.TableSetupColumn("Delay");
            ImGui.TableSetupColumn("Tick");
            ImGui.TableHeadersRow();
            foreach (var script in state.Scripts)
            {
                var isRunning = script.Status == ActiveProgramState.ProgramState.ScriptStatus.Running;
                var isPaused = script.Status == ActiveProgramState.ProgramState.ScriptStatus.Paused;
                var isStopped = script.Status == ActiveProgramState.ProgramState.ScriptStatus.Stopped ||
                                script.Status == ActiveProgramState.ProgramState.ScriptStatus.NotInit;
                var isInCurrentTick = !isStopped && state.ScriptsForCurrentTick.Contains(script);
                
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
                
                ImGui.TableNextColumn();
                if (isRunning)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(new Vector4(0.3f, 0.6f, 0.8f, 1.0f)));
                }
                ImGui.Text($"{(isInCurrentTick ? "Y" : "")}");
                
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
        ImGui.Text("Tick counter: " + _activeProgramState.TickCounter);
        var currentScript = state.GetRunningScript();
        if (currentScript == null)
        {
            ImGui.Text("");
            ImGui.Text("Paused waiting for scripts.");
        }
        else
        {
            ImGui.Text($"Current script: {currentScript.Id}");
            LabelAndButton($"Current offset: ", currentScript.Offset.ToString("D5"));
        }
        
        LabelAndButton($"Current program: ", state.CurrentProgram.ToString());

        var currentRoom = state.LoadedRoom;
        if (currentRoom == null)
        {
            ImGui.Text("Not in a room.");
        }
        else
        {
            LabelAndButton($"Current room: ", currentRoom.Value.ToString());
        }
        #endregion
        
        ImGui.Separator();
        
        #region Palette
        if (ImGui.CollapsingHeader("Palette"))
        {
            ImGui.BeginTable("palette", 4);
            ImGui.TableSetupColumn("Index");
            ImGui.TableSetupColumn("R");
            ImGui.TableSetupColumn("G");
            ImGui.TableSetupColumn("B");
            ImGui.TableHeadersRow();
            for (var i = 0; i < 256; i++)
            {
                if (!_activeProgramState.LoadedPalette.ContainsKey(i))
                {
                    continue;
                }
                var col = _activeProgramState.LoadedPalette[i];
                var colScale = (0, 0, 0);
                if (_activeProgramState.LoadedPaletteScale.ContainsKey(i))
                {
                    colScale = _activeProgramState.LoadedPaletteScale[i];
                }

                ImGui.TableNextColumn();
                ImGui.Text($"{i}");
                ImGui.TableNextColumn();
                ImGui.Text($"{col.R} ({colScale.Item1}");
                ImGui.TableNextColumn();
                ImGui.Text($"{col.G} ({colScale.Item2}");
                ImGui.TableNextColumn();
                ImGui.Text($"{col.B} ({colScale.Item3}");
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
            ImGui.TableSetupColumn("Flag", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthFixed, 50.0f);
            ImGui.TableHeadersRow();
            foreach (var (flagId, flagValue) in _activeProgramState.Flags.OrderBy(p => p.Key))
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{Flags.GetFlagText(flagId)}");
                ImGui.TableNextColumn();
                ImGui.Text($"{flagValue}");
                ImGui.TableNextRow();
            }
            
            ImGui.EndTable();
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
        #region Key Char Positions
        if (ImGui.CollapsingHeader($"Key Char Positions"))
        {
            ImGui.BeginTable("key_char_position", 7);
            ImGui.TableSetupColumn("Index");
            ImGui.TableSetupColumn("X");
            ImGui.TableSetupColumn("Y");
            ImGui.TableSetupColumn("Z");
            ImGui.TableSetupColumn("LP");
            ImGui.TableSetupColumn("TP");
            ImGui.TableSetupColumn("LW");
            ImGui.TableHeadersRow();
            foreach (var (keyCharId, keyChar) in _activeProgramState.KeyChars)
            {
                if (keyChar.Initialised && !keyChar.OffScreen)
                {
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyCharId}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyChar.PositionX}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyChar.PositionY}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyChar.PositionZ}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyChar.LastPoint}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyChar.TargetPoint}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyChar.LastWalk}");
                    ImGui.TableNextRow();
                }
            }
            ImGui.EndTable();
        }
        #endregion
        #region Key Char Animations
        if (ImGui.CollapsingHeader($"Key Char Animations"))
        {
            ImGui.BeginTable("key_char_animation", 5);
            ImGui.TableSetupColumn("Index");
            ImGui.TableSetupColumn("Anim");
            ImGui.TableSetupColumn("Dir");
            ImGui.TableSetupColumn("Frame");
            ImGui.TableSetupColumn("Delay");
            ImGui.TableHeadersRow();
            foreach (var (keyCharId, keyChar) in _activeProgramState.KeyChars)
            {
                if (keyChar.Initialised && !keyChar.OffScreen)
                {
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyCharId}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyChar.CurrentAnim}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyChar.CurrentDirection}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyChar.CurrentAnimCounter}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyChar.CurrentAnimSpeed}");
                    ImGui.TableNextRow();
                }
            }
            ImGui.EndTable();
        }
        #endregion
        #region Key Char Waiting
        if (ImGui.CollapsingHeader($"Key Char Waiting"))
        {
            ImGui.BeginTable("key_char_wait", 5);
            ImGui.TableSetupColumn("Index");
            ImGui.TableSetupColumn("Other");
            ImGui.TableSetupColumn("Anim");
            ImGui.TableSetupColumn("Point");
            ImGui.TableSetupColumn("Walk");
            ImGui.TableHeadersRow();
            foreach (var (keyCharId, keyChar) in _activeProgramState.KeyChars)
            {
                if (keyChar.WaitForKeyChar != null)
                {
                    var otherKeyChar = _activeProgramState.KeyChars[keyChar.WaitForKeyChar.Value];
                    
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyCharId}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyChar.WaitForKeyChar}");
                    ImGui.TableNextColumn();
                    if (keyChar.WaitForAnimationId != null)
                    {
                        ImGui.Text($"{keyChar.WaitForAnimationId} ({otherKeyChar.CurrentAnim})");
                    }
                    else
                    {
                        ImGui.Text("");
                    }
                    ImGui.TableNextColumn();
                    if (keyChar.WaitForPoint != null)
                    {
                        ImGui.Text($"{keyChar.WaitForPoint} ({otherKeyChar.LastPoint})");
                    }
                    else
                    {
                        ImGui.Text("");
                    }
                    ImGui.TableNextColumn();
                    if (keyChar.WaitForWalk != null)
                    {
                        ImGui.Text($"{keyChar.WaitForWalk} (?)");
                    }
                    else
                    {
                        ImGui.Text("");
                    }
                    ImGui.TableNextRow();
                }
            }
            ImGui.EndTable();
        }
        #endregion
        ImGui.Separator();
        
        #region Areas
        if (ImGui.CollapsingHeader($"Areas"))
        {
            ImGui.BeginTable("areas", 8);
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("DX");
            ImGui.TableSetupColumn("DY");
            ImGui.TableSetupColumn("W");
            ImGui.TableSetupColumn("H");
            ImGui.TableSetupColumn("SX");
            ImGui.TableSetupColumn("SY");
            ImGui.TableSetupColumn("S");
            ImGui.TableHeadersRow();
            foreach (var (areaId, areaState) in state.ActiveRoomAreas)
            {
                foreach (var area in program.Areas.Where(a => a.Id == areaId))
                {
                    var destX = area.Rect.X;
                    var destY = area.Rect.Y;

                    ImGui.TableNextColumn();
                    ImGui.Text($"{area.Id}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{destX}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{destY}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{area.Rect.W}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{area.Rect.H}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{area.SrcX}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{area.SrcY}");
                    ImGui.TableNextColumn();
                    ImGui.Text($"{areaState:G}");
                    ImGui.TableNextRow();
                }
            }
        
            ImGui.EndTable();
        }
        if (ImGui.CollapsingHeader($"Background Areas"))
        {
            ImGui.BeginTable("background_areas", 7);
            ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthFixed, 15.0f);
            ImGui.TableSetupColumn("DX");
            ImGui.TableSetupColumn("DY");
            ImGui.TableSetupColumn("W", ImGuiTableColumnFlags.WidthFixed, 25.0f);
            ImGui.TableSetupColumn("H", ImGuiTableColumnFlags.WidthFixed, 25.0f);
            ImGui.TableSetupColumn("SX", ImGuiTableColumnFlags.WidthFixed, 25.0f);
            ImGui.TableSetupColumn("SY", ImGuiTableColumnFlags.WidthFixed, 25.0f);
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
                ImGui.Text($"{destX} ({background.Rect.X})");
                ImGui.TableNextColumn();
                ImGui.Text($"{destY} ({background.Rect.Y})");
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
        if (ImGui.CollapsingHeader($"Points"))
        {
            ImGui.BeginTable("points", 5);
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("X");
            ImGui.TableSetupColumn("Y");
            ImGui.TableSetupColumn("Z");
            ImGui.TableSetupColumn("Order");
            ImGui.TableHeadersRow();
            ushort idx = 0;
            foreach (var point in program.Points)
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{idx}");
                ImGui.TableNextColumn();
                ImGui.Text($"{point.X}");
                ImGui.TableNextColumn();
                ImGui.Text($"{point.Y}");
                ImGui.TableNextColumn();
                ImGui.Text($"{point.Z}");
                ImGui.TableNextColumn();
                ImGui.Text($"{point.Order}");
                ImGui.TableNextRow();
                idx++;
            }
        
            ImGui.EndTable();
        }
        if (ImGui.CollapsingHeader($"Rects"))
        {
            ImGui.BeginTable("rects", 5);
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("X");
            ImGui.TableSetupColumn("Y");
            ImGui.TableSetupColumn("W");
            ImGui.TableSetupColumn("H");
            ImGui.TableHeadersRow();
            ushort idx = 0;
            foreach (var rect in program.Rects)
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{idx}");
                ImGui.TableNextColumn();
                ImGui.Text($"{rect.X}");
                ImGui.TableNextColumn();
                ImGui.Text($"{rect.Y}");
                ImGui.TableNextColumn();
                ImGui.Text($"{rect.W}");
                ImGui.TableNextColumn();
                ImGui.Text($"{rect.H}");
                ImGui.TableNextRow();
                idx++;
            }
        
            ImGui.EndTable();
        }
        if (ImGui.CollapsingHeader($"Walks"))
        {
            ImGui.BeginTable("walks", 6);
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("P1");
            ImGui.TableSetupColumn("P2");
            ImGui.TableSetupColumn("R");
            ImGui.TableSetupColumn("A1");
            ImGui.TableSetupColumn("A2");
            ImGui.TableHeadersRow();
            ushort idx = 0;
            foreach (var walk in program.Walks)
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{idx}");
                ImGui.TableNextColumn();
                ImGui.Text($"{walk.Point1}");
                ImGui.TableNextColumn();
                ImGui.Text($"{walk.Point2}");
                ImGui.TableNextColumn();
                ImGui.Text($"{walk.ClipRect}");
                ImGui.TableNextColumn();
                ImGui.Text($"{walk.Area1}");
                ImGui.TableNextColumn();
                ImGui.Text($"{walk.Area2}");
                ImGui.TableNextRow();
                idx++;
            }
        
            ImGui.EndTable();
        }
        
        if (ImGui.CollapsingHeader($"Room Sprites"))
        {
            ImGui.BeginTable("room_sprites", 3);
            ImGui.TableSetupColumn("Sprite");
            ImGui.TableSetupColumn("X");
            ImGui.TableSetupColumn("Y");
            ImGui.TableHeadersRow();
            foreach (var (sprId, x, y) in _activeProgramState.CurrentState.ActiveRoomSprites)
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{sprId} ({_activeProgramState.LoadedSprites[sprId].SpriteNum})");
                ImGui.TableNextColumn();
                ImGui.Text($"{x}");
                ImGui.TableNextColumn();
                ImGui.Text($"{y}");
                ImGui.TableNextRow();
            }
        
            ImGui.EndTable();
        }
        #endregion
        
        ImGui.Separator();
        
        #region Talk
        if (ImGui.CollapsingHeader($"Talk Entries"))
        {
            ImGui.BeginTable("talk_entries", 5);
            ImGui.TableSetupColumn("KC1", ImGuiTableColumnFlags.WidthFixed, 25.0f);
            ImGui.TableSetupColumn("KC2", ImGuiTableColumnFlags.WidthFixed, 25.0f);
            ImGui.TableSetupColumn("Num", ImGuiTableColumnFlags.WidthFixed, 25.0f);
            ImGui.TableSetupColumn("Counter", ImGuiTableColumnFlags.WidthFixed, 25.0f);
            ImGui.TableSetupColumn("Text");
            ImGui.TableHeadersRow();
            foreach (var talkEntry in _activeProgramState.TalkEntries
                         .OrderBy(t => t.TalkingKeyChar)
                         .ThenBy(t => t.OtherKeyChar))
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{talkEntry.TalkingKeyChar}");
                ImGui.TableNextColumn();
                ImGui.Text($"{talkEntry.OtherKeyChar}");
                ImGui.TableNextColumn();
                ImGui.Text($"{talkEntry.Num}");
                ImGui.TableNextColumn();
                ImGui.Text($"{talkEntry.Counter}");
                ImGui.TableNextColumn();
                ImGui.Text($"{talkEntry.Text}");
                ImGui.TableNextRow();
            }
        
            ImGui.EndTable();
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