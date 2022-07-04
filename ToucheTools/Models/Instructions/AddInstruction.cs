namespace ToucheTools.Models.Instructions;

public class AddInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Add;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G") + $" take STK value, move STK position forwards 1, then add to STK value";
    }
}