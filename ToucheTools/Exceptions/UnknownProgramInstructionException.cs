using ToucheTools.Models;

namespace ToucheTools.Exceptions;

public class UnknownProgramInstructionException : Exception
{
    public UnknownProgramInstructionException(ProgramDataModel.Opcode opcode) : base($"Unknown opcode {opcode:G}")
    {
        
    }
}