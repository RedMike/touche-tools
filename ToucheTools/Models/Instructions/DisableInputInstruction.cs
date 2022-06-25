namespace ToucheTools.Models.Instructions;

public class DisableInputInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.DisableInput;
    public override string ToString()
    {
        return Opcode.ToString("G");
    }
}