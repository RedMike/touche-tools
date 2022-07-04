namespace ToucheTools.Models.Instructions;

public class InitCharScriptInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.InitCharScript;
    public override int Width => 10;
    
    public ushort Character { get; set; }
    public ushort Color { get; set; }
    public ushort SpriteIndex { get; set; }
    public ushort SequenceIndex { get; set; }
    public ushort SequenceCharacterId { get; set; }

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
        Color = reader.ReadUInt16();
        SpriteIndex = reader.ReadUInt16();
        SequenceIndex = reader.ReadUInt16();
        SequenceCharacterId = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Character);
        writer.Write((ushort)Color);
        writer.Write((ushort)SpriteIndex);
        writer.Write((ushort)SequenceIndex);
        writer.Write((ushort)SequenceCharacterId);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character} {Color} {SpriteIndex} {SequenceIndex} {SequenceCharacterId}";
    }
}