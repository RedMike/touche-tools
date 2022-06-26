namespace ToucheTools.Models.Instructions;

public class EndConversationInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.EndConversation;
    public override string ToString()
    {
        return Opcode.ToString("G");
    }
}