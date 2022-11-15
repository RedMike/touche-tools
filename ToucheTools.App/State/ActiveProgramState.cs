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
            public bool Initialised { get; set; } = false;
            
            #region Graphics
            public int? SpriteIndex { get; set; }
            public int? SequenceIndex { get; set; }
            public int? Character { get; set; } //within sequence
            #endregion
            
            #region Animation
            public int Anim1Start { get; set; }
            public int Anim1Count { get; set; }
            public int Anim2Start { get; set; }
            public int Anim2Count { get; set; }
            public int Anim3Start { get; set; }
            public int Anim3Count { get; set; }
            #endregion
            
            #region Position
            public ushort? LastProgramPoint { get; set; }
            public int? PositionX { get; set; }
            public int? PositionY { get; set; }
            public int? PositionZ { get; set; }
            public int Direction { get; set; }
            #endregion
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

        public Dictionary<ushort, ushort> Flags { get; set; } = new Dictionary<ushort, ushort>()
        {
            //values set from game code
            { 291, 240 },
            { 292, 16 },
            { 293, 0 },
            { 294, 1 },
        };
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
            CurrentState.Flags[0] = (ushort)_program.Active;
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

            if (!CurrentState.KeyChars.ContainsKey(initCharScript.Character))
            {
                CurrentState.KeyChars[initCharScript.Character] = new ProgramState.KeyChar();
            }
            var keyChar = CurrentState.KeyChars[initCharScript.Character];
            keyChar.SpriteIndex = initCharScript.SpriteIndex;
            keyChar.SequenceIndex = initCharScript.SequenceIndex;
            keyChar.Character = initCharScript.SequenceCharacterId;
        } else if (instruction is LoadRoomInstruction loadRoom)
        {
            CurrentState.LoadedRoom = loadRoom.Num;
        } else if (instruction is SetCharFrameInstruction setCharFrame)
        {
            if (!CurrentState.KeyChars.ContainsKey(setCharFrame.Character))
            {
                CurrentState.KeyChars[setCharFrame.Character] = new ProgramState.KeyChar();
            }
            var keyChar = CurrentState.KeyChars[setCharFrame.Character];
            
            if (setCharFrame.TransitionType == SetCharFrameInstruction.Type.Loop) // 0
            {
                keyChar.Anim2Start = setCharFrame.Val2;
                keyChar.Anim2Count = setCharFrame.Val3;
                keyChar.Anim3Start = setCharFrame.Val2;
                keyChar.Anim3Count = setCharFrame.Val3;
            } else if (setCharFrame.TransitionType == SetCharFrameInstruction.Type.RandomCountThenStop) //1
            {
                _log.Error("Unknown transition type " + setCharFrame.TransitionType.ToString("G"));
            }
            else if (setCharFrame.TransitionType == SetCharFrameInstruction.Type.TalkFrames) //2
            {
                keyChar.Anim1Start = setCharFrame.Val2;
                keyChar.Anim1Count = setCharFrame.Val3;
            } 
            else if (setCharFrame.TransitionType == SetCharFrameInstruction.Type.StartPaused) //3
            {
                _log.Error("Unknown transition type " + setCharFrame.TransitionType.ToString("G"));
            }
            else if (setCharFrame.TransitionType == SetCharFrameInstruction.Type.Todo4) //4
            {
                keyChar.Anim3Start = setCharFrame.Val2;
                keyChar.Anim3Count = setCharFrame.Val3;
            }
            else
            {
                throw new Exception("Unknown transition type: " + setCharFrame.TransitionType);
            }
        } else if (instruction is EnableInputInstruction)
        {
            
        } else if (instruction is SetFlagInstruction setFlag)
        {
            var val = CurrentState.Stack[CurrentState.StackPointer];
            CurrentState.Flags[setFlag.Flag] = val;
            
            if (setFlag.Flag == 104)
            {
                //TODO: selects current keychar
            } else if (setFlag.Flag == 611 && val != 0)
            {
                //TODO: quits game
            } else if (setFlag.Flag == 612)
            {
                var newVal = 999; //TODO: randomly generate
                CurrentState.Flags[613] = (ushort)newVal;
            } //TODO: more
        } else if (instruction is SetCharBoxInstruction setCharBox)
        {
            if (!CurrentState.KeyChars.ContainsKey(setCharBox.Character))
            {
                CurrentState.KeyChars[setCharBox.Character] = new ProgramState.KeyChar();
            }
            var keyChar = CurrentState.KeyChars[setCharBox.Character];
            var point = program.Points[setCharBox.Num];
            keyChar.PositionX = point.X;
            keyChar.PositionY = point.Y;
            keyChar.PositionZ = point.Z;
            keyChar.LastProgramPoint = setCharBox.Num;
        } else if (instruction is InitCharInstruction initChar)
        {
            if (!CurrentState.KeyChars.ContainsKey(initChar.Character))
            {
                CurrentState.KeyChars[initChar.Character] = new ProgramState.KeyChar();
            }
            var keyChar = CurrentState.KeyChars[initChar.Character];
            keyChar.Anim1Start = 0;
            keyChar.Anim1Count = 1;
            keyChar.Anim2Start = 0;
            keyChar.Anim2Count = 1;
            keyChar.Anim3Start = 0;
            keyChar.Anim3Count = 1;
            keyChar.Direction = 0;
            keyChar.Initialised = true;
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