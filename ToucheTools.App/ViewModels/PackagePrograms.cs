using ToucheTools.Helpers;
using ToucheTools.Models;
using ToucheTools.Models.Instructions;

namespace ToucheTools.App.ViewModels;

public class PackagePrograms
{
    private readonly OpenedPackage _package;

    private Dictionary<int, Dictionary<uint, BaseInstruction>> _programs = null!;
    private Dictionary<int, Dictionary<string, uint>> _actionOffsets = null!;
    public PackagePrograms(OpenedPackage package)
    {
        _package = package;

        _package.Observe(Update);
        Update();
    }

    public Dictionary<uint, BaseInstruction> GetProgram(int programId)
    {
        return _programs[programId];
    }
    
    public Dictionary<string, uint> GetActionOffsetsForProgram(int programId)
    {
        return _actionOffsets[programId];
    }

    public Dictionary<int, Dictionary<uint, BaseInstruction>> GetIncludedPrograms()
    {
        return _programs;
    }

    private void Update()
    {
        _programs = new Dictionary<int, Dictionary<uint, BaseInstruction>>();
        _actionOffsets = new Dictionary<int, Dictionary<string, uint>>();
        if (!_package.IsLoaded())
        {
            return;
        }

        foreach (var group in _package.GetIncludedPrograms().GroupBy(p => p.Value.Index))
        {
            var programId = group.Key;

            var mainProgram = "";
            var charPrograms = new List<string>();
            var actionPrograms = new List<string>();
            var foundMain = false;
            foreach (var (programPath, programData) in group)
            {
                if (programData.Type == OpenedPackage.ProgramType.Unknown)
                {
                    //TODO: log warning
                    continue;
                }
                
                if (programData.Type == OpenedPackage.ProgramType.Main)
                {
                    if (foundMain)
                    {
                        throw new Exception("Found two mains for the same program");
                    }

                    foundMain = true;
                    mainProgram = programPath;   
                } else if (programData.Type == OpenedPackage.ProgramType.KeyChar)
                {
                    charPrograms.Add(programPath);
                } else if (programData.Type == OpenedPackage.ProgramType.Action)
                {
                    actionPrograms.Add(programPath);
                }
                else
                {
                    throw new Exception("Unknown program type");
                }
            }

            //first load the main program
            var instructions = new Dictionary<uint, BaseInstruction>();
            uint trackedOffset = 0;
            if (!foundMain)
            {
                //  if no main program, use a stub
                instructions.Add(0, new NoopInstruction());
                trackedOffset = (uint)(trackedOffset + instructions[0].Width);
                //TODO: warning
            }
            else
            {
                var lines = File.ReadAllLines(mainProgram)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                foreach (var line in lines)
                {
                    if (line.StartsWith(";"))
                    {
                        //it's a comment
                        continue;
                    }
                    //TODO: handle labels
                    var (opcode, remainder) = BaseInstruction.DeserialiseOpcode(line);
                    var instruction = ProgramInstructionHelper.Get(opcode);
                    instruction.DeserialiseRemainder(remainder);
                    instructions[trackedOffset] = instruction;
                    trackedOffset = (uint)(trackedOffset + instruction.Width + 1);
                }

                var endInstruction = ProgramInstructionHelper.Get(ProgramDataModel.Opcode.StopScript);
                instructions[trackedOffset] = endInstruction;
                trackedOffset = (uint)(trackedOffset + endInstruction.Width + 1);
            }
            //then load the char programs
            foreach (var charProgram in charPrograms)
            {
                var lines = File.ReadAllLines(charProgram)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                foreach (var line in lines)
                {
                    if (line.StartsWith(";"))
                    {
                        //it's a comment
                        continue;
                    }
                    //TODO: handle labels
                    var (opcode, remainder) = BaseInstruction.DeserialiseOpcode(line);
                    var instruction = ProgramInstructionHelper.Get(opcode);
                    instruction.DeserialiseRemainder(remainder);
                    instructions[trackedOffset] = instruction;
                    trackedOffset = (uint)(trackedOffset + instruction.Width + 1);
                }

                var endInstruction = ProgramInstructionHelper.Get(ProgramDataModel.Opcode.StopScript);
                instructions[trackedOffset] = endInstruction;
                trackedOffset = (uint)(trackedOffset + endInstruction.Width + 1);
            }
            
            //finally load the action programs
            _actionOffsets[programId] = new Dictionary<string, uint>();
            foreach (var actionProgram in actionPrograms)
            {
                _actionOffsets[programId][actionProgram] = trackedOffset;
                var lines = File.ReadAllLines(actionProgram)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                foreach (var line in lines)
                {
                    if (line.StartsWith(";"))
                    {
                        //it's a comment
                        continue;
                    }
                    //TODO: handle labels
                    var (opcode, remainder) = BaseInstruction.DeserialiseOpcode(line);
                    var instruction = ProgramInstructionHelper.Get(opcode);
                    instruction.DeserialiseRemainder(remainder);
                    instructions[trackedOffset] = instruction;
                    trackedOffset = (uint)(trackedOffset + instruction.Width + 1);
                }

                var endInstruction = ProgramInstructionHelper.Get(ProgramDataModel.Opcode.StopScript);
                instructions[trackedOffset] = endInstruction;
                trackedOffset = (uint)(trackedOffset + endInstruction.Width + 1);
            }

            _programs[programId] = instructions;
        }
    }
}