namespace ToucheTools.Models.Instructions;

public class NegInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Neg;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G") + $" get STK value, move STK position forwards by one, set STK value to bitwise NEG old value";
    }
}