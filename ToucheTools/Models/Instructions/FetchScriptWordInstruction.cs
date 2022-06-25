namespace ToucheTools.Models.Instructions;

public class FetchScriptWordInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.FetchScriptWord;
    
    public ushort Val { get; set; }

    public override void Load(BinaryReader reader)
    {
        Val = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Val);
    }

    public override string ToString()
    {
        return $"{Opcode:G} set STK value to {Val}";
    }
}