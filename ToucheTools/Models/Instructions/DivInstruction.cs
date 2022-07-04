namespace ToucheTools.Models.Instructions;

public class DivInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Div;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G") + $" take STK value, move STK position forwards 1, then if 0 set to 0 else divide by STK value";
    }
}