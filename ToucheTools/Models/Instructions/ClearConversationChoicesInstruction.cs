namespace ToucheTools.Models.Instructions;

public class ClearConversationChoicesInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.ClearConversationChoices;
    public override int Width => 0;
    
    public override string ToString()
    {
        return Opcode.ToString("G");
    }
}