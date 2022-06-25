namespace ToucheTools.Models.Instructions;

public class EnableInputInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.EnableInput;
    public override string ToString()
    {
        return Opcode.ToString("G");
    }
}