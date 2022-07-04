namespace ToucheTools.Models.Instructions;

public class DisableInputInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.DisableInput;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G");
    }
}