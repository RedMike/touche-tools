namespace ToucheTools.Models.Instructions;

public class TestGreaterInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.TestGreater;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G") + $" get STK value, move STK position forwards one, check prev value greater; " +
               $"set STK value to -1 if yes, 0 if no";
    }
}