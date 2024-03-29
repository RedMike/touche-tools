﻿using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.Utils;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class ProgramManagementWindow : BaseWindow
{
    private readonly OpenedManifest _manifest;
    private readonly MainWindowState _state;
    private readonly ProgramManagementState _programManagementState;
    private readonly PackagePrograms _programs;
    private readonly PackageRooms _rooms;

    public ProgramManagementWindow(MainWindowState state, ProgramManagementState programManagementState, PackagePrograms programs, PackageRooms rooms, OpenedManifest manifest)
    {
        _state = state;
        _programManagementState = programManagementState;
        _programs = programs;
        _rooms = rooms;
        _manifest = manifest;
    }

    public override void Render()
    {
        if (!_manifest.IsLoaded())
        {
            return;
        }
        if (_state.State != MainWindowState.States.ProgramManagement)
        {
            return;
        }
        var pos = Vector2.Zero + new Vector2(0.0f, ImGui.GetFrameHeight());
        ImGui.SetNextWindowPos(pos, ImGuiCond.Once);
        ImGui.Begin("Programs", ImGuiWindowFlags.NoCollapse);
        var game = _manifest.GetGame();
        var allPrograms = _manifest.GetAllPrograms().ToList();
        var includedPrograms = _manifest.GetIncludedPrograms();
        
        //figure out which actions that can be selected
        var actions = game.ActionDefinitions.Select(a => (a.Key, a.Value)).ToList();
        var actionList = actions.Select(a => a.Value).ToArray();
        
        //figure out which hitboxes are tied to which programs
        var hitboxes = new Dictionary<int, Dictionary<(int, int), string>>();
        var rooms = _manifest.GetIncludedRooms()
            .ToDictionary(p => p.Value.Index, p => p.Key);
        
        //figure out which keychars are tied to which programs
        var keyChars = new Dictionary<int, Dictionary<int, string>>();
        var sprites = _manifest.GetIncludedImages().Where(i => i.Value.Type == OpenedManifest.ImageType.Sprite)
            .ToDictionary(p => p.Value.Index, p => p.Key.ShortenPath());
        foreach (var programId in includedPrograms.Select(p => p.Value.Index).Distinct())
        {
            var programKeyChars = new Dictionary<int, string>();
            var keyCharMappings = _programs.GetKeyCharMappingsForProgram(programId);
            foreach (var (keyCharId, spriteId) in keyCharMappings)
            {
                var spriteName = "UNKNOWN";
                if (sprites.ContainsKey(spriteId))
                {
                    spriteName = sprites[spriteId];
                }

                programKeyChars[keyCharId] = $"{keyCharId} ({spriteName})";
            }

            var programHitboxes = new Dictionary<(int, int), string>();
            var roomMappings = _programs.GetRoomMappingsForProgram(programId);
            foreach (var roomId in roomMappings)
            {
                if (!rooms.ContainsKey(roomId))
                {
                    //TODO: warning
                    continue;
                }
                var room = _rooms.GetRoom(rooms[roomId]);
                foreach (var (hitboxId, hitbox) in room.Hitboxes)
                {
                    programHitboxes[(roomId, hitboxId)] = $"Room {roomId} hitbox {hitboxId} ({hitbox.Type:G} {hitbox.Item})";
                }
            }
            
            keyChars[programId] = programKeyChars;
            hitboxes[programId] = programHitboxes;
        }

        foreach (var path in allPrograms)
        {
            //included checkbox
            var origIsIncluded = includedPrograms.ContainsKey(path);
            var isIncluded = origIsIncluded;
            ImGui.PushID($"{path}_include");
            ImGui.Checkbox("", ref isIncluded);
            ImGui.PopID();
            if (isIncluded != origIsIncluded)
            {
                if (isIncluded)
                {
                    _manifest.IncludeFile(path);
                }
                else
                {
                    _manifest.ExcludeFile(path);
                }
            }
            ImGui.SameLine();

            //button to select for preview
            var isSelected = _programManagementState.SelectedProgram == path;
            if (isSelected)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.4f, 0.7f, 1.0f));
            }

            if (ImGui.Button(path.ShortenPath()))
            {
                _programManagementState.SelectedProgram = path;
                _programManagementState.PreviewOpen = true;
                _programManagementState.EditorOpen = true;
            }
            if (isSelected)
            {
                ImGui.PopStyleColor();
            }
            
            if (isIncluded)
            {
                ImGui.SameLine();
                //program type
                var program = includedPrograms[path];
                var types = OpenedManifest.ProgramTypeAsList();
                var origSelectedType = types.FindIndex(i => i == program.Type.ToString("G"));
                var selectedType = origSelectedType;
                ImGui.PushID($"{path}_type");
                ImGui.SetNextItemWidth(120.0f);
                ImGui.Combo("", ref selectedType, types.ToArray(), types.Count);
                ImGui.PopID();
                if (selectedType != origSelectedType)
                {
                    _manifest.LoadedManifest.Programs[path].Type = Enum.Parse<OpenedManifest.ProgramType>(types[selectedType]);
                    _manifest.ForceUpdate();
                }
                ImGui.SameLine();
                
                //program index
                var indexes = Enumerable.Range(1, 200).ToList();
                var origIndex = program.Index - 1;
                var index = origIndex;
                ImGui.PushID($"{path}_index");
                ImGui.SetNextItemWidth(60.0f);
                ImGui.Combo("", ref index, indexes.Select(i => i.ToString()).ToArray(), indexes.Count);
                ImGui.PopID();
                if (index != origIndex)
                {
                    _manifest.LoadedManifest.Programs[path].Index = index + 1;
                    _manifest.ForceUpdate();
                }
                
                if (program.Type == OpenedManifest.ProgramType.Action)
                {
                    //target = an action
                    //data = (roomId, hitboxId)
                    var origAction = -1;
                    if (program.Target != -1)
                    {
                        origAction = actions.FindIndex(a => a.Key == program.Target);
                    }
                    var action = origAction;
                    ImGui.SameLine();
                    ImGui.PushID($"{path}_action");
                    ImGui.SetNextItemWidth(100.0f);
                    ImGui.Combo("", ref action, actionList, actionList.Length);
                    ImGui.PopID();
                    if (action != origAction)
                    {
                        _manifest.LoadedManifest.Programs[path].Target = actions[action].Key;
                        _manifest.ForceUpdate();
                    }
                    
                    ImGui.SameLine();
                    var gameHitboxes = game.InventoryItems
                        .Select(p => ((-1, p.Key | 0x1000), p.Value.DefaultLabel))
                        .ToList();
                    var programHitboxes = hitboxes[program.Index]
                        .Select(p => (p.Key, p.Value))
                        .ToList();
                    //TODO: allow overwriting of hitboxes?
                    var allHitboxes = gameHitboxes.Concat(programHitboxes).ToList();
                    var hitboxList = allHitboxes.Select(p => p.Item2).ToArray();
                    var currentData = new [] {-1, -1};
                    if (program.Data.Length == 2)
                    {
                        currentData = program.Data;
                    }

                    var origHitbox = allHitboxes.FindIndex(i =>
                        i.Item1.Item1 == currentData[0] && i.Item1.Item2 == currentData[1]);
                    var hitbox = origHitbox;
                    
                    ImGui.PushID($"{path}_data");
                    ImGui.SetNextItemWidth(240.0f);
                    ImGui.Combo("", ref hitbox, hitboxList, hitboxList.Length);
                    ImGui.PopID();
                    if (hitbox != origHitbox)
                    {
                        var newData = new[] { allHitboxes[hitbox].Item1.Item1, allHitboxes[hitbox].Item1.Item2 };
                        _manifest.LoadedManifest.Programs[path].Data = newData;
                        _manifest.ForceUpdate();
                    }
                } else if (_manifest.LoadedManifest.Programs[path].Type == OpenedManifest.ProgramType.KeyChar)
                {
                    //target = a keychar
                    //data = empty
                    ImGui.SameLine();
                    var allProgramKeyChars = new Dictionary<int, string>();
                    if (keyChars.ContainsKey(program.Index))
                    {
                        allProgramKeyChars = keyChars[program.Index];
                    }
                    var programKeyChars = allProgramKeyChars
                        .Where(p => p.Key != 0) //you can't have a script on keychar 0
                        .Select(p => (p.Key, p.Value))
                        .ToList();
                    var programKeyCharList = programKeyChars.Select(p => p.Value).ToArray();
                    var origTarget = programKeyChars.FindIndex(i => i.Key == program.Target);
                    var target = origTarget;
                    ImGui.PushID($"{path}_target");
                    ImGui.SetNextItemWidth(120.0f);
                    ImGui.Combo("", ref target, programKeyCharList, programKeyCharList.Length);
                    ImGui.PopID();
                    if (target != origTarget)
                    {
                        _manifest.LoadedManifest.Programs[path].Target = programKeyChars[target].Key;
                        _manifest.ForceUpdate();
                    }
                } else if (_manifest.LoadedManifest.Programs[path].Type == OpenedManifest.ProgramType.Conversation)
                {
                    //target = a conversation ID (X = start convo, X+1 = potential choice 1, X+2 = potential choice 2, ...)
                    //data = (textId)
                    var target = program.Target;
                    ImGui.SameLine();
                    ImGui.PushID($"{path}_target");
                    ImGui.SetNextItemWidth(100.0f);
                    ImGui.InputInt("", ref target, 1);
                    ImGui.PopID();
                    if (target != program.Target)
                    {
                        _manifest.LoadedManifest.Programs[path].Target = target;
                        _manifest.ForceUpdate();
                    }

                    var origData = 0;
                    if (program.Data.Length == 1)
                    {
                        origData = program.Data[0];
                    }
                    var data = origData;
                    ImGui.SameLine();
                    ImGui.PushID($"{path}_data");
                    ImGui.SetNextItemWidth(100.0f);
                    ImGui.InputInt("", ref data, 1);
                    ImGui.PopID();
                    if (data != origData)
                    {
                        _manifest.LoadedManifest.Programs[path].Data = new[] { data };
                        _manifest.ForceUpdate();
                    }
                }
            }
        }

        ImGui.Separator();
        if (ImGui.Button("Refresh Programs"))
        {
            //TODO: add any new programs to list
        }
        ImGui.End();
    }
}