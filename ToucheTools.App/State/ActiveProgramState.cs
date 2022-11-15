using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;
using ToucheTools.Models;
using ToucheTools.Models.Instructions;

namespace ToucheTools.App.State;

public class ActiveProgramState
{
    public class ProgramState
    {
        public int CurrentProgram { get; set; } = 0;
        public int CurrentOffset { get; set; } = 0;
        public int? JumpOffset { get; set; } = null;
        
        public ushort StackPointer { get; set; } = 0;
        public ushort[] Stack { get; set; } = new ushort[500];
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