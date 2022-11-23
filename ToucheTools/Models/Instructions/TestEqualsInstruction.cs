namespace ToucheTools.Models.Instructions;

public class TestEqualsInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.TestEquals;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G") + $" STK val against STK+1 val, set STK+1 val -1 yes, 0 no";
    }
}