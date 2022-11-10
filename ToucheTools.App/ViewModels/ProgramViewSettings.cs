using ToucheTools.Models;
using ToucheTools.Models.Instructions;

namespace ToucheTools.App.ViewModels;

public class ProgramViewSettings
{
    private readonly DatabaseModel _databaseModel;
    
    public int ActiveProgram { get; private set; }
    public List<int> Programs { get; private set; }
    
    
    public List<string> InstructionsView { get; private set; } = null!;
    public int EvaluateUntil { get; private set; }

    public List<int> ReferencedRoomsView { get; private set; } = null!;
    //for each sprite, (sequence, char, animation)
    public Dictionary<int, List<(int, int, int)>> ReferencedSpritesView { get; private set; } = null!; 

    public ProgramViewSettings(DatabaseModel model)
    {
        _databaseModel = model;
        Programs = _databaseModel.Programs.Keys.ToList();
        ActiveProgram = Programs.Contains(90) ? 90 : Programs.First();
        EvaluateUntil = -1;
        
        GenerateView();
    }

    public void SetActiveProgram(int program)
    {
        if (!Programs.Contains(program))
        {
            throw new Exception("Unknown program: " + program);
        }

        ActiveProgram = program;
        EvaluateUntil = -1;
        GenerateView();
    }

    public void SetEvaluateUntil(int index)
    {
        EvaluateUntil = index;
    }

    private void GenerateView()
    {
        var program = _databaseModel.Programs[ActiveProgram];
        InstructionsView = program.Instructions.OrderBy(pair => pair.Key).Select(pair => pair.Value.ToString()).ToList();
        ReferencedRoomsView = program.Instructions
            .Where(pair => pair.Value is LoadRoomInstruction)
            .Select(pair => (int)(((LoadRoomInstruction)pair.Value).Num))
            .Distinct()
            .OrderBy(k => k)
            .ToList();
        //load by index because that's what we need
        var loadSpriteInstructions = program.Instructions
            .Where(pair => pair.Value is LoadSpriteInstruction)
            .Select(pair => (LoadSpriteInstruction)pair.Value)
            .GroupBy(pair => pair.Index)
            .ToDictionary(g => (int)g.Key, 
                g => g.Select(s => (int)s.Num).Distinct().ToList());
        var loadSequenceInstructions = program.Instructions
            .Where(pair => pair.Value is LoadSequenceInstruction)
            .Select(pair => (LoadSequenceInstruction)pair.Value)
            .GroupBy(pair => pair.Index)
            .ToDictionary(g => (int)g.Key, 
                g => g.Select(s => (int)s.Num).Distinct().ToList());
        var initCharScriptInstructions = program.Instructions
            .Where(pair => pair.Value is InitCharScriptInstruction)
            .Select(pair => (InitCharScriptInstruction)pair.Value)
            .GroupBy(pair => pair.SpriteIndex)
            .ToDictionary(g => (int)g.Key, 
                g => g.Select(s => ((int)s.SequenceIndex, (int)s.SequenceCharacterId)).Distinct().ToList());
        
        var spriteView = new Dictionary<int, List<(int, int, int)>>();
        foreach (var pair in loadSpriteInstructions)
        {
            var spriteIndex = pair.Key;
            if (!initCharScriptInstructions.ContainsKey(spriteIndex))
            {
                throw new Exception("Missing char init script");
            }

            foreach (var spriteNum in pair.Value)
            {
                var list = new List<(int, int, int)>();
                foreach (var (seqIndex, charId) in initCharScriptInstructions[spriteIndex])
                {
                    if (!loadSequenceInstructions.ContainsKey(seqIndex))
                    {
                        throw new Exception("Missing sequence load");
                    }

                    foreach (var seqId in loadSequenceInstructions[seqIndex])
                    {
                        list.Add((seqId, charId, 0));
                    }
                }
                spriteView.Add(spriteNum, list);
            }
        }

        ReferencedSpritesView = spriteView;
    }
}