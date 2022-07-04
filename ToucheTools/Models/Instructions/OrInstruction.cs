namespace ToucheTools.Models.Instructions;

public class OrInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Or;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G") + $" get STK value, move STK position forwards by one, set STK value to old value bitwise OR new one";
    }
}