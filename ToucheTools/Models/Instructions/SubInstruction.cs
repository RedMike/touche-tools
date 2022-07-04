namespace ToucheTools.Models.Instructions;

public class SubInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Sub;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G") + $" take STK value, move STK position forwards 1, then subtract from STK value";
    }
}