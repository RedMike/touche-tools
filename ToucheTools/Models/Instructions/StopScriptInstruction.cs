namespace ToucheTools.Models.Instructions;

public class StopScriptInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.StopScript;
    public override string ToString()
    {
        return Opcode.ToString("G");
    }
}