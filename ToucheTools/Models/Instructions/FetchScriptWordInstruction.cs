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
    
    protected override string SerialiseInternal()
    {
        return $"{Val}";
    }

    public override void DeserialiseRemainder(string remainder)
    {
        Val = short.Parse(remainder);
    }

    public override string ToString()
    {
        return $"{Opcode:G} STK val now {Val}";
    }
}