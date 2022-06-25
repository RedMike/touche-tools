namespace ToucheTools.Models.Instructions;

public class FaceCharInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.FaceChar;
    
    public ushort Character1 { get; set; }
    public ushort Character2 { get; set; }

    public bool CurrentCharacter => Character1 == 256;

    public override void Load(BinaryReader reader)
    {
        Character1 = reader.ReadUInt16();
        Character2 = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Character1);
        writer.Write((ushort)Character2);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(CurrentCharacter ? "current" : Character1)} towards {Character2}";
    }
}