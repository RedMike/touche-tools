using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.Windows;

public class ProgramReferenceViewWindow : IWindow
{
    private readonly WindowSettings _windowSettings;
    private readonly SpriteViewSettings _spriteViewSettings;
    private readonly ProgramViewSettings _programViewSettings;
    private readonly ProgramViewState _programViewState;
    private readonly ActivePalette _palette;
    private readonly ActiveRoom _room;
    private readonly ActiveSprite _sprite;

    public ProgramReferenceViewWindow(WindowSettings windowSettings, SpriteViewSettings spriteViewSettings, ProgramViewSettings programViewSettings, ProgramViewState programViewState, ActivePalette palette, ActiveRoom room, ActiveSprite sprite)
    {
        _windowSettings = windowSettings;
        _spriteViewSettings = spriteViewSettings;
        _programViewSettings = programViewSettings;
        _programViewState = programViewState;
        _palette = palette;
        _room = room;
        _sprite = sprite;
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

        ImGui.Text("Rooms:");
        foreach (var room in _programViewSettings.ReferencedRoomsView)
        {
            ImGui.SameLine();
            if (ImGui.Button($"{room}"))
            {
                _room.SetActive(room);
                _palette.SetActive(room);
                _windowSettings.OpenRoomView();
            }
        }
        ImGui.Separator();
        ImGui.Text("Sprites (Sequence, Character, Animation):");
        foreach (var pair in _programViewSettings.ReferencedSpritesView.OrderBy(p => p.Key))
        {
            if (ImGui.TreeNodeEx(pair.Key.ToString(), ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var (seqId, charId, animId) in pair.Value)
                {
                    if (ImGui.Button($"({seqId}, {charId}, {animId})"))
                    {
                        _sprite.SetActive(pair.Key);
                        _spriteViewSettings.SetActiveSequence(seqId);
                        _spriteViewSettings.SelectCharacter(charId);
                        _spriteViewSettings.SelectAnimation(animId);
                        _windowSettings.OpenSpriteView();
                    }
                }
                ImGui.TreePop();
            }
        }
        
        ImGui.Separator();
        ImGui.Text("Character Script Offsets:");
        foreach (var pair in _programViewSettings.CharacterScriptOffsetView.OrderBy(p => p.Key))
        {
            if (ImGui.Button($"{pair.Key} - {pair.Value:D5}"))
            {
                _programViewState.QueueScrollToOffset(pair.Value);
                _programViewSettings.SetEvaluateUntil(_programViewState.OffsetToIndex[pair.Value]);
            }
        }
            
        ImGui.Separator();
        ImGui.Text("Action Script Offsets (Action, Obj1, Obj2):");
        foreach (var pair in _programViewSettings.ActionScriptOffsetView
                     .OrderBy(p => p.Key.Item1)
                     .ThenBy(p => p.Key.Item2)
                     .ThenBy(p => p.Key.Item3)
                 )
        {
            if (ImGui.Button($"({pair.Key.Item1}, {pair.Key.Item2}, {pair.Key.Item3}) - {pair.Value:D5}"))
            {
                _programViewState.QueueScrollToOffset(pair.Value);
                _programViewSettings.SetEvaluateUntil(_programViewState.OffsetToIndex[pair.Value]);
            }
        }
        
        ImGui.End();
    }
}