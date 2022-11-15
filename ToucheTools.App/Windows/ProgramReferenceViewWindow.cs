﻿using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.Windows;

public class ProgramReferenceViewWindow : IWindow
{
    private readonly WindowSettings _windowSettings;
    private readonly ActiveProgramState _activeProgramState;

    public ProgramReferenceViewWindow(WindowSettings windowSettings, ActiveProgramState activeProgramState)
    {
        _windowSettings = windowSettings;
        _activeProgramState = activeProgramState;
    }

    public void Render()
    {
        if (!_windowSettings.ProgramViewOpen)
        {
            return;
        }
        
        var viewW = 350.0f;
        var viewH = 600.0f;
        ImGui.SetNextWindowPos(new Vector2(400.0f, 200.0f));
        ImGui.SetNextWindowSize(new Vector2(viewW, viewH));
        ImGui.Begin("Program References", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar);

        if (ImGui.Button("Step"))
        {
            _activeProgramState.Step();
        }
        
        ImGui.Separator();
        
        var state = _activeProgramState.CurrentState;

        #region Offsets
        LabelAndButton("Current offset: ", $"{state.CurrentOffset:D5}");
        if (state.JumpOffset != null)
        {
            LabelAndButton("In a branch, will return to offset: ", $"{state.JumpOffset.Value:D5}");
        }
        ImGui.Text($"STK at: {state.StackPointer:D4} value: {state.Stack[state.StackPointer]}");
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
        
        #region Characters
        if (state.KeyChars.Count > 0)
        {
            if (ImGui.CollapsingHeader("Key Character Graphics", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.BeginTable("key_char_graphics", 6);
                ImGui.TableSetupColumn("ID");
                ImGui.TableSetupColumn("Sprite");
                ImGui.TableSetupColumn("Seq");
                ImGui.TableSetupColumn("Char");
                ImGui.TableSetupColumn("Anim");
                ImGui.TableSetupColumn("Link");
                ImGui.TableHeadersRow();
                foreach (var (keyCharId, keyChar) in state.KeyChars)
                {
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyCharId}");
                    ImGui.TableNextColumn();
                    if (keyChar.SpriteIndex != null)
                    {
                        ImGui.Text($"{state.SpriteIndexToNum[keyChar.SpriteIndex.Value]}");
                    }
                    ImGui.TableNextColumn();
                    if (keyChar.SequenceIndex != null)
                    {
                        ImGui.Text($"{state.SequenceIndexToNum[keyChar.SequenceIndex.Value]}");
                    }
                    ImGui.TableNextColumn();
                    if (keyChar.Character != null)
                    {
                        ImGui.Text($"{keyChar.Character.Value}");
                    }
                    ImGui.TableNextColumn();
                    if (keyChar.Animation != null)
                    {
                        ImGui.Text($"{keyChar.Animation.Value}");
                    }
                    ImGui.TableNextColumn();
                    if (ImGui.Button("Go to"))
                    {
                        //TODO: link to sprite view
                    }
                    ImGui.TableNextRow();
                }
            
                ImGui.EndTable();
            }
            
            if (ImGui.CollapsingHeader("Key Character Position", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.BeginTable("key_char_position", 3);
                ImGui.TableSetupColumn("ID");
                ImGui.TableSetupColumn("X");
                ImGui.TableSetupColumn("Y");
                ImGui.TableHeadersRow();
                foreach (var (keyCharId, keyChar) in state.KeyChars)
                {
                    ImGui.TableNextColumn();
                    ImGui.Text($"{keyCharId}");
                    ImGui.TableNextColumn();
                    if (keyChar.PositionX != null)
                    {
                        ImGui.Text($"{keyChar.PositionX}");
                    }
                    ImGui.TableNextColumn();
                    if (keyChar.PositionY != null)
                    {
                        ImGui.Text($"{keyChar.PositionY}");
                    }
                    ImGui.TableNextRow();
                }
            
                ImGui.EndTable();
            }
        }
        #endregion

        // if (_programViewSettings.EvaluateUntil >= 0)
        // {
        //     ImGui.Text("State:");
        //     var stateAtEvaluate = _programViewSettings.StateByInstruction[_programViewSettings.InstructionsView[_programViewSettings.EvaluateUntil].Item1];
        //     
        //
        //     ImGui.Text($"STK pointer: {stateAtEvaluate.StackPointerLocation:D5} value '{stateAtEvaluate.StackPointerValue}'");
        //     
        //     if (stateAtEvaluate.LoadedRoom != null)
        //     {
        //         ImGui.Text($"Currently in room:");
        //         ImGui.SameLine();
        //         if (ImGui.Button($"{stateAtEvaluate.LoadedRoom}"))
        //         {
        //             _room.SetActive(stateAtEvaluate.LoadedRoom.Value);
        //             _palette.SetActive(stateAtEvaluate.LoadedRoom.Value);
        //             _windowSettings.OpenRoomView();
        //         }
        //     }
        //
        //     if (stateAtEvaluate.Flags.Count > 0)
        //     {
        //         if (ImGui.CollapsingHeader("Flags", ImGuiTreeNodeFlags.DefaultOpen))
        //         {
        //             ImGui.BeginTable("flags", 2);
        //             ImGui.TableSetupColumn("Flag");
        //             ImGui.TableSetupColumn("Value");
        //             ImGui.TableHeadersRow();
        //             foreach (var pair in stateAtEvaluate.Flags.OrderBy(p => p.Key))
        //             {
        //                 ImGui.TableNextColumn();
        //                 ImGui.Text(pair.Key.ToString());
        //                 ImGui.TableNextColumn();
        //                 ImGui.Text(pair.Value.ToString());
        //                 ImGui.TableNextRow();
        //             }
        //
        //             ImGui.EndTable();
        //         }
        //     }
        //
        //     if (stateAtEvaluate.InventoryValuesByKeyChar.Count > 0)
        //     {
        //         ImGui.Text("Inventory:");
        //         foreach (var pair in stateAtEvaluate.InventoryValuesByKeyChar.OrderBy(p => p.Key))
        //         {
        //             if (ImGui.CollapsingHeader($"KeyChar {pair.Key}"))
        //             {
        //                 ImGui.BeginTable($"items_{pair.Key}", 2);
        //                 ImGui.TableSetupColumn("Item");
        //                 ImGui.TableSetupColumn("Amount");
        //                 ImGui.TableHeadersRow();
        //                 foreach (var itemPair in pair.Value.OrderBy(p => p.Key))
        //                 {
        //                     ImGui.TableNextColumn();
        //                     ImGui.Text(itemPair.Key.ToString());
        //                     ImGui.TableNextColumn();
        //                     ImGui.Text(itemPair.Value.ToString());
        //                     ImGui.TableNextRow();
        //                 }
        //
        //                 ImGui.EndTable();
        //             }
        //         }
        //     }
        // }
        //
        // ImGui.Separator();
        //
        // ImGui.Text("Data:");
        // ImGui.Text("Other programs:");
        // foreach (var program in _programViewSettings.Data.OtherPrograms.OrderBy(x => x))
        // {
        //     ImGui.SameLine();
        //     if (ImGui.Button($"{program}"))
        //     {
        //         _program.SetActive(program);
        //     }
        // }
        //
        // ImGui.Text("Rooms:");
        // foreach (var room in _programViewSettings.Data.LoadedRooms.OrderBy(x => x))
        // {
        //     ImGui.SameLine();
        //     if (ImGui.Button($"{room}"))
        //     {
        //         _room.SetActive(room);
        //         _palette.SetActive(room);
        //         _windowSettings.OpenRoomView();
        //     }
        // }
        //
        // ImGui.Separator();
        // ImGui.Text("Sprites (Sequence, Character):");
        // foreach (var group in _programViewSettings.Data.SpriteSequenceCharacterCombinations
        //              .OrderBy(x => x.Item1)
        //              .GroupBy(x => x.Item1))
        // {
        //     if (ImGui.TreeNodeEx(group.Key.ToString(), ImGuiTreeNodeFlags.DefaultOpen))
        //     {
        //         foreach (var (_, seqId, charId) in group)
        //         {
        //             if (ImGui.Button($"({seqId}, {charId})"))
        //             {
        //                 _sprite.SetActive(group.Key);
        //                 _sequence.SetActive(seqId);
        //                 _character.SetActive(charId);
        //                 _windowSettings.OpenSpriteView();
        //             }
        //         }
        //         ImGui.TreePop();
        //     }
        // }
        //
        // ImGui.Separator();
        // ImGui.Text("Character Script Offsets:");
        // foreach (var pair in _programViewSettings.CharacterScriptOffsetView.OrderBy(p => p.Key))
        // {
        //     if (ImGui.Button($"{pair.Key} - {pair.Value:D5}"))
        //     {
        //         _programViewState.QueueScrollToOffset(pair.Value);
        //         _programViewSettings.SetEvaluateUntil(_programViewState.OffsetToIndex[pair.Value]);
        //     }
        // }
        //     
        // ImGui.Separator();
        // ImGui.Text("Action Script Offsets (Action, Obj1, Obj2):");
        // foreach (var pair in _programViewSettings.ActionScriptOffsetView
        //              .OrderBy(p => p.Key.Item1)
        //              .ThenBy(p => p.Key.Item2)
        //              .ThenBy(p => p.Key.Item3)
        //          )
        // {
        //     if (ImGui.Button($"({pair.Key.Item1}, {pair.Key.Item2}, {pair.Key.Item3}) - {pair.Value:D5}"))
        //     {
        //         _programViewState.QueueScrollToOffset(pair.Value);
        //         _programViewSettings.SetEvaluateUntil(_programViewState.OffsetToIndex[pair.Value]);
        //     }
        // }
        
        ImGui.End();
    }

    private bool LabelAndButton(string label, string button)
    {
        ImGui.Text(label);
        ImGui.SameLine();
        return ImGui.Button(button);
    }
}