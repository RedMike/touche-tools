namespace ToucheTools.Models.Instructions;

public class EnableInputInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.EnableInput;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G");
    }
}