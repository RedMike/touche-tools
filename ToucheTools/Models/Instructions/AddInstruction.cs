namespace ToucheTools.Models.Instructions;

public class AddInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Add;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G") + $" STK val to STK+1 val";
    }
}