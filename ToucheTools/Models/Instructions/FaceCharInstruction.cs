namespace ToucheTools.Models.Instructions;

public class FaceCharInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.FaceChar;
    public override int Width => 4;
    
    public short Character1 { get; set; }
    public short Character2 { get; set; }

    public bool CurrentCharacter => Character1 == 256;

    public override void Load(BinaryReader reader)
    {
        Character1 = reader.ReadInt16();
        Character2 = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character1);
        writer.Write((short)Character2);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(CurrentCharacter ? "current" : Character1)} towards {Character2}";
    }
}