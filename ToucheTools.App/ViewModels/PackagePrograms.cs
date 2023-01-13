using ToucheTools.Helpers;
using ToucheTools.Models;
using ToucheTools.Models.Instructions;

namespace ToucheTools.App.ViewModels;

public class PackagePrograms
{
    private readonly OpenedManifest _manifest;

    private Dictionary<int, Dictionary<uint, BaseInstruction>> _programs = null!;
    private Dictionary<int, Dictionary<string, uint>> _actionOffsets = null!;
    private Dictionary<int, Dictionary<string, uint>> _charOffsets = null!;
    private Dictionary<int, Dictionary<string, uint>> _convoOffsets = null!;
    private Dictionary<int, Dictionary<string, uint>> _labels = null!;
    private Dictionary<int, Dictionary<int, int>> _keyChars = null!;
    private Dictionary<int, HashSet<int>> _rooms = null!;
    
    public PackagePrograms(OpenedManifest manifest)
    {
        _manifest = manifest;

        _manifest.Observe(Update);
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

    public Dictionary<string, uint> GetCharOffsetsForProgram(int programId)
    {
        return _charOffsets[programId];
    }

    public Dictionary<string, uint> GetConvoOffsetsForProgram(int programId)
    {
        return _convoOffsets[programId];
    }

    public Dictionary<string, uint> GetLabelOffsetsForProgram(int programId)
    {
        return _labels[programId];
    }

    public Dictionary<int, int> GetKeyCharMappingsForProgram(int programId)
    {
        return _keyChars[programId];
    }

    public HashSet<int> GetRoomMappingsForProgram(int programId)
    {
        return _rooms[programId];
    }

    public Dictionary<int, Dictionary<uint, BaseInstruction>> GetIncludedPrograms()
    {
        return _programs;
    }

    private void Update()
    {
        _programs = new Dictionary<int, Dictionary<uint, BaseInstruction>>();
        _actionOffsets = new Dictionary<int, Dictionary<string, uint>>();
        _charOffsets = new Dictionary<int, Dictionary<string, uint>>();
        _convoOffsets = new Dictionary<int, Dictionary<string, uint>>();
        _labels = new Dictionary<int, Dictionary<string, uint>>();
        _keyChars = new Dictionary<int, Dictionary<int, int>>();
        _rooms = new Dictionary<int, HashSet<int>>();
        if (!_manifest.IsLoaded())
        {
            return;
        }

        foreach (var group in _manifest.GetIncludedPrograms().GroupBy(p => p.Value.Index))
        {
            var programId = group.Key;

            var mainProgram = "";
            var spriteMappings = new Dictionary<int, int>();
            var charPrograms = new List<string>();
            var actionPrograms = new List<string>();
            var convoPrograms = new List<string>();
            _keyChars[programId] = new Dictionary<int, int>();
            _rooms[programId] = new HashSet<int>();
            var foundMain = false;
            foreach (var (programPath, programData) in group)
            {
                if (programData.Type == OpenedManifest.ProgramType.Unknown)
                {
                    //TODO: log warning
                    continue;
                }
                
                if (programData.Type == OpenedManifest.ProgramType.Main)
                {
                    if (foundMain)
                    {
                        //TODO: log error
                        continue;
                    }

                    foundMain = true;
                    mainProgram = programPath;   
                } else if (programData.Type == OpenedManifest.ProgramType.KeyChar)
                {
                    charPrograms.Add(programPath);
                } else if (programData.Type == OpenedManifest.ProgramType.Action)
                {
                    actionPrograms.Add(programPath);
                } else if (programData.Type == OpenedManifest.ProgramType.Conversation)
                {
                    convoPrograms.Add(programPath);
                }
                else
                {
                    throw new Exception("Unknown program type");
                }
            }

            //first load the main program
            var labels = new Dictionary<string, uint>();
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
                var lines = _manifest.LoadFileLines(mainProgram)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                var secondPassList = new Dictionary<uint, string>();
                foreach (var line in lines)
                {
                    if (line.StartsWith(";"))
                    {
                        //it's a comment
                        continue;
                    }

                    if (line.EndsWith(":"))
                    {
                        //it's a label
                        var labelName = line.Substring(0, line.LastIndexOf(":", StringComparison.Ordinal));
                        labels[labelName] = trackedOffset;
                        continue;
                    }
                    var (opcode, remainder) = BaseInstruction.DeserialiseOpcode(line);
                    var instruction = ProgramInstructionHelper.Get(opcode);
                    instructions[trackedOffset] = instruction;
                    secondPassList[trackedOffset] = remainder;
                    trackedOffset = (uint)(trackedOffset + instruction.Width + 1);
                }

                //second pass deserialisation to allow labels to work correctly
                foreach (var (offset, remainder) in secondPassList)
                {
                    var instruction = instructions[offset];
                    instruction.DeserialiseRemainder(remainder, labels);

                    if (instruction is LoadSpriteInstruction loadSprite)
                    {
                        spriteMappings[loadSprite.Index] = loadSprite.Num;
                    }

                    if (instruction is InitCharScriptInstruction initCharScript)
                    {
                        _keyChars[programId][initCharScript.Character] = spriteMappings[initCharScript.SpriteIndex];
                    }

                    if (instruction is LoadRoomInstruction loadRoom)
                    {
                        _rooms[programId].Add(loadRoom.Num);
                    }
                }

                var endInstruction = ProgramInstructionHelper.Get(ProgramDataModel.Opcode.StopScript);
                instructions[trackedOffset] = endInstruction;
                trackedOffset = (uint)(trackedOffset + endInstruction.Width + 1);
            }
            //then load the char programs
            _charOffsets[programId] = new Dictionary<string, uint>();
            foreach (var charProgram in charPrograms)
            {
                _charOffsets[programId][charProgram] = trackedOffset;
                var lines = _manifest.LoadFileLines(charProgram)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                var secondPassList = new Dictionary<uint, string>();
                foreach (var line in lines)
                {
                    if (line.StartsWith(";"))
                    {
                        //it's a comment
                        continue;
                    }
                    
                    if (line.EndsWith(":"))
                    {
                        //it's a label
                        var labelName = line.Substring(0, line.LastIndexOf(":", StringComparison.Ordinal));
                        labels[labelName] = trackedOffset;
                        continue;
                    }
                    var (opcode, remainder) = BaseInstruction.DeserialiseOpcode(line);
                    var instruction = ProgramInstructionHelper.Get(opcode);
                    instructions[trackedOffset] = instruction;
                    secondPassList[trackedOffset] = remainder;
                    trackedOffset = (uint)(trackedOffset + instruction.Width + 1);
                }
                
                //second pass deserialisation to allow labels to work correctly
                foreach (var (offset, remainder) in secondPassList)
                {
                    var instruction = instructions[offset];
                    instruction.DeserialiseRemainder(remainder, labels);
                }

                var endInstruction = ProgramInstructionHelper.Get(ProgramDataModel.Opcode.StopScript);
                instructions[trackedOffset] = endInstruction;
                trackedOffset = (uint)(trackedOffset + endInstruction.Width + 1);
            }

            _labels[programId] = labels;
            
            //load the action programs
            _actionOffsets[programId] = new Dictionary<string, uint>();
            foreach (var actionProgram in actionPrograms)
            {
                _actionOffsets[programId][actionProgram] = trackedOffset;
                var lines = _manifest.LoadFileLines(actionProgram)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                var secondPassList = new Dictionary<uint, string>();
                foreach (var line in lines)
                {
                    if (line.StartsWith(";"))
                    {
                        //it's a comment
                        continue;
                    }
                    
                    if (line.EndsWith(":"))
                    {
                        //it's a label
                        var labelName = line.Substring(0, line.LastIndexOf(":", StringComparison.Ordinal));
                        labels[labelName] = trackedOffset;
                        continue;
                    }
                    var (opcode, remainder) = BaseInstruction.DeserialiseOpcode(line);
                    var instruction = ProgramInstructionHelper.Get(opcode);
                    instructions[trackedOffset] = instruction;
                    secondPassList[trackedOffset] = remainder;
                    trackedOffset = (uint)(trackedOffset + instruction.Width + 1);
                }
                
                //second pass deserialisation to allow labels to work correctly
                foreach (var (offset, remainder) in secondPassList)
                {
                    var instruction = instructions[offset];
                    instruction.DeserialiseRemainder(remainder, labels);
                }

                var endInstruction = ProgramInstructionHelper.Get(ProgramDataModel.Opcode.StopScript);
                instructions[trackedOffset] = endInstruction;
                trackedOffset = (uint)(trackedOffset + endInstruction.Width + 1);
            }
            
            //load the convo programs
            _convoOffsets[programId] = new Dictionary<string, uint>();
            foreach (var convoProgram in convoPrograms)
            {
                _convoOffsets[programId][convoProgram] = trackedOffset;
                var lines = _manifest.LoadFileLines(convoProgram)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                var secondPassList = new Dictionary<uint, string>();
                foreach (var line in lines)
                {
                    if (line.StartsWith(";"))
                    {
                        //it's a comment
                        continue;
                    }
                    
                    if (line.EndsWith(":"))
                    {
                        //it's a label
                        var labelName = line.Substring(0, line.LastIndexOf(":", StringComparison.Ordinal));
                        labels[labelName] = trackedOffset;
                        continue;
                    }
                    var (opcode, remainder) = BaseInstruction.DeserialiseOpcode(line);
                    var instruction = ProgramInstructionHelper.Get(opcode);
                    instructions[trackedOffset] = instruction;
                    secondPassList[trackedOffset] = remainder;
                    trackedOffset = (uint)(trackedOffset + instruction.Width + 1);
                }
                
                //second pass deserialisation to allow labels to work correctly
                foreach (var (offset, remainder) in secondPassList)
                {
                    var instruction = instructions[offset];
                    instruction.DeserialiseRemainder(remainder, labels);
                }

                var endInstruction = ProgramInstructionHelper.Get(ProgramDataModel.Opcode.StopScript);
                instructions[trackedOffset] = endInstruction;
                trackedOffset = (uint)(trackedOffset + endInstruction.Width + 1);
            }

            _programs[programId] = instructions;
        }
    }
}