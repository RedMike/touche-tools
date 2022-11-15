using Microsoft.Extensions.Logging;
using ToucheTools.App.ViewModels.Observables;
using ToucheTools.Models;
using ToucheTools.Models.Instructions;

namespace ToucheTools.App.ViewModels;

public class ProgramViewSettings
{
    private readonly DatabaseModel _databaseModel;
    private readonly ActiveProgram _program;
    private readonly LogData _log;
    
    public List<(int, string)> InstructionsView { get; private set; } = null!;
    public int EvaluateUntil { get; private set; }

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

        public int StackPointerLocation { get; set; }
        
        public Dictionary<int, int> KnownStackValues = new Dictionary<int, int>();

        public Dictionary<int, Dictionary<int, int>> InventoryValuesByKeyChar =
            new Dictionary<int, Dictionary<int, int>>();

        public int? StackPointerValue => KnownStackValues.ContainsKey(StackPointerLocation)
            ? KnownStackValues[StackPointerLocation]
            : null;
        
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
                StackPointerLocation = StackPointerLocation,
                KnownStackValues = KnownStackValues.ToDictionary(p => p.Key, p => p.Value),
                InventoryValuesByKeyChar = InventoryValuesByKeyChar.ToDictionary(p => p.Key, p => p.Value
                    .ToDictionary(p2 => p2.Key, p2 => p2.Value)),
            };
        }
    }
    //for each instruction, the apparent state after running it; StopScript resets the state (so jumps don't work)
    //TODO: make jumps correctly pass state
    public Dictionary<int, ProgramState> StateByInstruction { get; set; } = null!;
    public ProgramData Data { get; set; } = null!;
    

    public ProgramViewSettings(DatabaseModel model, ActiveProgram program, LogData log)
    {
        _databaseModel = model;
        
        _program = program;
        _log = log;
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
        var stateByInstruction = new Dictionary<int, ProgramState>();
        var prevState = new ProgramState();
        //TODO: recursing debug stuff
        //start recursing from the first instruction
        //  when hitting a jump instruction recurse through the jump and continue too
        //  also recurse from CSOs/ASOs
        //    if something runs from multiple branches, store an identifier and store as a list?
        foreach (var pair in program.Instructions.OrderBy(pair => pair.Key))
        {
            if (stateByInstruction.ContainsKey(pair.Key))
            {
                throw new Exception("Processed same instruction twice");
            }
            var state = prevState.Clone();
            
            if (pair.Key == 0)
            {
                //start of an episode
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
            }
            
            if (pair.Value is StopScriptInstruction)
            {
                //forcibly clear the state
                //TODO: jumps
                break;
            } else if (pair.Value is FetchScriptWordInstruction fetchScriptWord)
            {
                state.KnownStackValues[state.StackPointerLocation] = fetchScriptWord.Val;
            } else if (pair.Value is GetInventoryItemInstruction getInventoryItem)
            {
                var val = 0;
                if (state.InventoryValuesByKeyChar.ContainsKey(getInventoryItem.Character) && state
                        .InventoryValuesByKeyChar[getInventoryItem.Character].ContainsKey(getInventoryItem.Item))
                {
                    val = state.InventoryValuesByKeyChar[getInventoryItem.Character][getInventoryItem.Item];
                }
                state.KnownStackValues[state.StackPointerLocation] = val;
            } else if (pair.Value is SetInventoryItemInstruction setInventoryItem)
            {
                if (state.StackPointerValue == null)
                {
                    _log.Error("Unknown STK value when setting inventory");
                }
                else
                {
                    if (!state.InventoryValuesByKeyChar.ContainsKey(setInventoryItem.Character))
                    {
                        state.InventoryValuesByKeyChar[setInventoryItem.Character] = new Dictionary<int, int>();
                    }

                    state.InventoryValuesByKeyChar[setInventoryItem.Character][setInventoryItem.Item] =
                        state.StackPointerValue.Value;
                }
            } else if (pair.Value is AddItemToInventoryAndRedrawInstruction addInventoryItem)
            {
                if (state.StackPointerValue == null)
                {
                    _log.Error("Unknown STK value when adding to inventory");
                }
                else
                {
                    if (!state.InventoryValuesByKeyChar.ContainsKey(addInventoryItem.Character))
                    {
                        state.InventoryValuesByKeyChar[addInventoryItem.Character] = new Dictionary<int, int>();
                    }

                    state.InventoryValuesByKeyChar[addInventoryItem.Character][state.StackPointerValue.Value] = 1;
                }
            }
            else if (pair.Value is PushInstruction push)
            {
                state.StackPointerLocation--;
                state.KnownStackValues[state.StackPointerLocation] = 0;
            } else if (pair.Value is AddInstruction add)
            {
                var val = state.StackPointerValue ?? 0;
                state.StackPointerLocation++;
                var otherVal = state.StackPointerValue ?? 0;
                state.KnownStackValues[state.StackPointerLocation] = val + otherVal;
            } else if (pair.Value is LoadRoomInstruction loadRoom)
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
                if (state.LoadedSprites.ContainsKey(initCharScript.SpriteIndex) &&
                    state.LoadedSequences.ContainsKey(initCharScript.SequenceIndex))
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
                }
            } else if (pair.Value is StartEpisodeInstruction startEpisode)
            {
                if (!programData.OtherPrograms.Contains(startEpisode.Num))
                {
                    programData.OtherPrograms.Add(startEpisode.Num);
                }
                state.Flags[0] = startEpisode.Flag;
            } else if (pair.Value is SetFlagInstruction setFlag)
            {
                if (state.StackPointerValue == null)
                {
                    _log.Error($"Unknown value in STK when loading flag {setFlag.Flag}");
                }
                else
                {
                    var val = state.StackPointerValue;
                    state.Flags[setFlag.Flag] = val.Value;

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
                }
            } else if (pair.Value is AddRoomAreaInstruction addRoomArea)
            {
                int flag = addRoomArea.Flag;
                if (!state.Flags.ContainsKey(addRoomArea.Flag))
                {
                    flag = -1;
                    _log.Error($"Tried to find flag {addRoomArea.Flag} but not set");
                }

                if (!state.Flags.ContainsKey(addRoomArea.Flag + 1))
                {
                    flag = -1;
                    _log.Error($"Tried to find flag {(addRoomArea.Flag + 1)} but not set");
                }

                if (flag >= 0)
                {
                    state.BackgroundOffset[addRoomArea.Num] = (state.Flags[addRoomArea.Flag], state.Flags[addRoomArea.Flag + 1]);
                }
            }

            stateByInstruction.Add(pair.Key, state);
            prevState = state;
        }
        StateByInstruction = stateByInstruction;
        Data = programData;

        //TODO: this can collide
        CharacterScriptOffsetView = program.CharScriptOffsets.ToDictionary(c => c.Character, c => c.Offs);
        //TODO: this can collide
        ActionScriptOffsetView = program.ActionScriptOffsets.ToDictionary(a => (a.Action, a.Object1, a.Object2), a => a.Offset);
    }
}