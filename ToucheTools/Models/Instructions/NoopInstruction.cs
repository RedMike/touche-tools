namespace ToucheTools.Models.Instructions;

public class NoopInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Noop;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G");
    }
}