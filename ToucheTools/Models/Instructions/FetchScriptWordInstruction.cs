namespace ToucheTools.Models.Instructions;

public class FetchScriptWordInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.FetchScriptWord;
    public override int Width => 2;
    
    public short Val { get; set; }

    public override void Load(BinaryReader reader)
    {
        Val = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Val);
    }

    public override string ToString()
    {
        return $"{Opcode:G} set STK value to {Val}";
    }
}