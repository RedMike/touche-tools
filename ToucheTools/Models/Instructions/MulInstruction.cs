namespace ToucheTools.Models.Instructions;

public class MulInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Mul;
    public override string ToString()
    {
        return Opcode.ToString("G") + $" take STK value, move STK position forwards 1, then multiply with STK value";
    }
}