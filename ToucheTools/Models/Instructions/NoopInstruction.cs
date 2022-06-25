namespace ToucheTools.Models.Instructions;

public class NoopInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Noop;
    public override string ToString()
    {
        return Opcode.ToString("G");
    }
}