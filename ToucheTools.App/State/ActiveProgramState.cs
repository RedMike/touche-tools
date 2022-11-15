using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;
using ToucheTools.Models;
using ToucheTools.Models.Instructions;

namespace ToucheTools.App.State;

public class ActiveProgramState
{
    public class ProgramState
    {
        public class KeyChar
        {
            public int? SpriteIndex { get; set; }
            public int? SequenceIndex { get; set; }
            public int? Character { get; set; } //within sequence
            public int? Animation { get; set; } //within sequence
            public int? PositionX { get; set; }
            public int? PositionY { get; set; }
        }
        
        public int CurrentProgram { get; set; } = 0;
        public int CurrentOffset { get; set; } = 0;
        public int? JumpOffset { get; set; } = null;
        
        public ushort StackPointer { get; set; } = 0;
        public ushort[] Stack { get; set; } = new ushort[500];

        public int? LoadedRoom { get; set; } = null;
        public Dictionary<int, int> SpriteIndexToNum { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, int> SequenceIndexToNum { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, KeyChar> KeyChars { get; set; } = new Dictionary<int, KeyChar>();
    }

    private readonly DatabaseModel _model;
    private readonly ActiveProgram _program;
    private readonly LogData _log;

    public ActiveProgramState(DatabaseModel model, ActiveProgram program, LogData log)
    {
        _model = model;
        _program = program;
        _log = log;
        _program.ObserveActive(Update);
        Update();
    }

    public ProgramState CurrentState { get; set; } = new ProgramState();

    private void Update()
    {
        if (CurrentState.CurrentProgram != _program.Active)
        {
            CurrentState = new ProgramState()
            {
                CurrentProgram = _program.Active
            };
        }
    }
    
    public void Step()
    {
        var program = _model.Programs[_program.Active];

        var curOffset = CurrentState.CurrentOffset;
        
        if (!program.Instructions.ContainsKey(curOffset))
        {
            throw new Exception("Unknown program offset: " + curOffset);
        }

        var instruction = program.Instructions[curOffset];
        if (instruction is StopScriptInstruction)
        {
            if (CurrentState.JumpOffset == null)
            {
                throw new Exception("Reached end of script");
            }
            
            CurrentState.CurrentOffset = CurrentState.JumpOffset.Value;
            CurrentState.JumpOffset = null;
        } else if (instruction is NoopInstruction)
        {
            
        } else if (instruction is FetchScriptWordInstruction fetchScriptWord)
        {
            CurrentState.Stack[CurrentState.StackPointer] = fetchScriptWord.Val;
        } else if (instruction is LoadSpriteInstruction loadSprite)
        {
            if (CurrentState.SpriteIndexToNum.ContainsKey(loadSprite.Index))
            {
                throw new Exception("Reloaded same sprite: " + loadSprite.Index);
            }
            CurrentState.SpriteIndexToNum[loadSprite.Index] = loadSprite.Num;
        } else if (instruction is LoadSequenceInstruction loadSequence)
        {
            if (CurrentState.SequenceIndexToNum.ContainsKey(loadSequence.Index))
            {
                throw new Exception("Reloaded same sequence: " + loadSequence.Index);
            }
            CurrentState.SequenceIndexToNum[loadSequence.Index] = loadSequence.Num;
        } else if (instruction is InitCharScriptInstruction initCharScript)
        {
            if (!CurrentState.SpriteIndexToNum.ContainsKey(initCharScript.SpriteIndex))
            {
                throw new Exception("Sprite not loaded yet: " + initCharScript.SpriteIndex);
            }
            if (!CurrentState.SequenceIndexToNum.ContainsKey(initCharScript.SequenceIndex))
            {
                throw new Exception("Sequence not loaded yet: " + initCharScript.SequenceIndex);
            }

            if (CurrentState.KeyChars.ContainsKey(initCharScript.Character))
            {
                throw new Exception("Reinitialised same keychar: " + initCharScript.Character);
            }

            CurrentState.KeyChars[initCharScript.Character] = new ProgramState.KeyChar()
            {
                SpriteIndex = initCharScript.SpriteIndex,
                SequenceIndex = initCharScript.SequenceIndex,
                Character = initCharScript.SequenceCharacterId
            };
        } else if (instruction is LoadRoomInstruction loadRoom)
        {
            CurrentState.LoadedRoom = loadRoom.Num;
        }

        else
        {
            _log.Error($"Unhandled instruction type: {instruction.Opcode:G}");
        }

        var instructionOffsets = program.Instructions.Keys.OrderBy(k => k).ToList();
        var idx = instructionOffsets.FindIndex(k => k == curOffset);
        if (idx == instructionOffsets.Count - 1)
        {
            throw new Exception("Reached end of script");
        }

        idx += 1;
        CurrentState.CurrentOffset = instructionOffsets[idx];
    }
}