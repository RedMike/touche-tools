namespace ToucheTools.Models.Instructions;

public class AndInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.And;
    public override string ToString()
    {
        return Opcode.ToString("G") + $" get STK value, move STK position forwards by one, set STK value to old value bitwise AND new one";
    }
}