using ToucheTools.App.ViewModels.Observables;
using ToucheTools.Models;
using ToucheTools.Models.Instructions;

namespace ToucheTools.App.ViewModels;

public class ProgramViewSettings
{
    private readonly DatabaseModel _databaseModel;
    private readonly ActiveProgram _program;
    
    public List<(int, string)> InstructionsView { get; private set; } = null!;
    public int EvaluateUntil { get; private set; }

    public List<int> ReferencedRoomsView { get; private set; } = null!;
    //for each sprite, (sequence, char, animation)
    public Dictionary<int, List<(int, int, int)>> ReferencedSpritesView { get; private set; } = null!;

    //for each sprite, the offset
    public Dictionary<int, int> CharacterScriptOffsetView { get; private set; } = null!;
    //for each (action, obj1, obj2), offset
    public Dictionary<(int, int, int), int> ActionScriptOffsetView { get; private set; } = null!;

    public class ProgramData
    {
        public List<int> LoadedRooms { get; set; } = new List<int>();
        public List<int> LoadedSprites { get; set; } = new List<int>();
        public List<int> LoadedSequences { get; set; } = new List<int>();
        public List<(int, int, int)> SpriteSequenceCharacterCombinations { get; set; } = new List<(int, int, int)>();
        public List<int> OtherPrograms { get; set; } = new List<int>();
    }

    public class ProgramState
    {
        public int? LoadedRoom { get; set; } = null;
        public Dictionary<int, int> LoadedSprites { get; set; } = new Dictionary<int, int>(); //index to sprite num
        public Dictionary<int, int> LoadedSequences { get; set; } = new Dictionary<int, int>(); //index to seq num
        public Dictionary<int, int> SpriteToSequence { get; set; } = new Dictionary<int, int>(); //sprite num to seq num
        public Dictionary<int, (int, int, int)> LoadedKeyChars { get; set; } = new Dictionary<int, (int, int, int)>(); //keychar to (sprite, seq, char) nums

        public Dictionary<int, (int, int)> BackgroundOffset { get; set; } = new Dictionary<int, (int, int)>(); //background num to area offset
        
        public Dictionary<int, int> Flags { get; set; } = new Dictionary<int, int>();
        public ProgramState Clone()
        {
            //deep copy
            return new ProgramState()
            {
                LoadedRoom = LoadedRoom,
                LoadedSprites = LoadedSprites.ToDictionary(p => p.Key, p => p.Value),
                LoadedSequences = LoadedSequences.ToDictionary(p => p.Key, p => p.Value),
                SpriteToSequence = SpriteToSequence.ToDictionary(p => p.Key, p => p.Value),
                LoadedKeyChars =
                    LoadedKeyChars.ToDictionary(p => p.Key, p => (p.Value.Item1, p.Value.Item2, p.Value.Item3)),
                Flags = Flags.ToDictionary(p => p.Key, p => p.Value),
            };
        }
    }
    //for each instruction, the apparent state after running it; StopScript resets the state (so jumps don't work)
    //TODO: make jumps correctly pass state
    public List<ProgramState> StateByInstruction { get; set; } = null!;
    public ProgramData Data { get; set; } = null!;
    

    public ProgramViewSettings(DatabaseModel model, ActiveProgram program)
    {
        _databaseModel = model;
        
        _program = program;
        _program.ObserveActive(GenerateView);
        EvaluateUntil = -1;
        
        GenerateView();
    }

    public void SetEvaluateUntil(int index)
    {
        EvaluateUntil = index;
    }

    private void GenerateView()
    {
        var program = _databaseModel.Programs[_program.Active];
        InstructionsView = program.Instructions.OrderBy(pair => pair.Key).Select(pair => (pair.Key, pair.Value.ToString())).ToList();

        var programData = new ProgramData();
        var stateByInstruction = new List<ProgramState>();
        var prevState = new ProgramState();
        foreach (var pair in program.Instructions.OrderBy(pair => pair.Key))
        {
            var state = prevState.Clone();
            if (pair.Value is StopScriptInstruction)
            {
                //forcibly clear the state
                //TODO: jumps
                state = new ProgramState();
            } else
            if (pair.Value is LoadRoomInstruction loadRoom)
            {
                state.LoadedRoom = loadRoom.Num;
                if (!programData.LoadedRooms.Contains(loadRoom.Num))
                {
                    programData.LoadedRooms.Add(loadRoom.Num);
                }
            } else if (pair.Value is LoadSpriteInstruction loadSprite)
            {
                state.LoadedSprites[loadSprite.Index] = loadSprite.Num;
                if (!programData.LoadedSprites.Contains(loadSprite.Num))
                {
                    programData.LoadedSprites.Add(loadSprite.Num);
                }
            } else if (pair.Value is LoadSequenceInstruction loadSeq)
            {
                state.LoadedSequences[loadSeq.Index] = loadSeq.Num;
                if (!programData.LoadedSequences.Contains(loadSeq.Num))
                {
                    programData.LoadedSequences.Add(loadSeq.Num);
                }
            } else if (pair.Value is InitCharScriptInstruction initCharScript)
            {
                var spriteNum = state.LoadedSprites[initCharScript.SpriteIndex];
                var seqNum = state.LoadedSequences[initCharScript.SequenceIndex];
                state.SpriteToSequence[spriteNum] = seqNum;
                state.LoadedKeyChars[initCharScript.Character] =
                    (spriteNum, seqNum, initCharScript.SequenceCharacterId);
                if (!programData.SpriteSequenceCharacterCombinations.Contains((spriteNum, seqNum,
                        initCharScript.SequenceCharacterId)))
                {
                    programData.SpriteSequenceCharacterCombinations.Add((spriteNum, seqNum,
                        initCharScript.SequenceCharacterId));
                }
            } else if (pair.Value is StartEpisodeInstruction startEpisode)
            {
                state.Flags[0] = startEpisode.Flag;
                
                for (var i = 200; i < 300; i++)
                {
                    if (state.Flags.ContainsKey(i))
                    {
                        state.Flags[i] = 0;
                    }
                }

                state.Flags[291] = 240;
                state.Flags[292] = 16;
                state.Flags[293] = 0;
                state.Flags[294] = 1;
                if (!programData.OtherPrograms.Contains(startEpisode.Flag))
                {
                    programData.OtherPrograms.Add(startEpisode.Flag);
                }
            } else if (pair.Value is SetFlagInstruction setFlag)
            {
                var val = -1; //TODO: loaded from STK
                state.Flags[setFlag.Flag] = val;

                if (setFlag.Flag == 104)
                {
                    //TODO: selects current keychar
                } else if (setFlag.Flag == 611 && val != 0)
                {
                    //TODO: quits game
                } else if (setFlag.Flag == 612)
                {
                    var newVal = -2; //TODO: randomly generate
                    state.Flags[613] = newVal;
                } //TODO: more
            } else if (pair.Value is AddRoomAreaInstruction addRoomArea)
            {
                if (!state.Flags.ContainsKey(addRoomArea.Flag))
                {
                    throw new Exception($"Tried to find flag {addRoomArea.Flag} but not set");
                }

                if (!state.Flags.ContainsKey(addRoomArea.Flag + 1))
                {
                    throw new Exception($"Tried to find second flag {(addRoomArea.Flag + 1)} but not set");
                }

                state.BackgroundOffset[addRoomArea.Num] = (state.Flags[addRoomArea.Flag], state.Flags[addRoomArea.Flag + 1]);
            }

            stateByInstruction.Add(state);
            prevState = state;
        }
        StateByInstruction = stateByInstruction;
        Data = programData;
        
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

        //TODO: this can collide
        CharacterScriptOffsetView = program.CharScriptOffsets.ToDictionary(c => c.Character, c => c.Offs);
        //TODO: this can collide
        ActionScriptOffsetView = program.ActionScriptOffsets.ToDictionary(a => (a.Action, a.Object1, a.Object2), a => a.Offset);
    }
}