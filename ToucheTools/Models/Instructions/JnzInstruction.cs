using ToucheTools.Helpers;

namespace ToucheTools.Models.Instructions;

public class JnzInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Jnz;
    public override int Width => 2;
    
    public ushort NewOffset { get; set; }

    public override void Load(BinaryReader reader)
    {
        NewOffset = reader.ReadInt16().AsUshort();//game does it this way;
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write(NewOffset.AsShort());
    }

    public override string ToString()
    {
        return $"{Opcode:G} if STK val not 0, jump to {NewOffset}";
    }
}