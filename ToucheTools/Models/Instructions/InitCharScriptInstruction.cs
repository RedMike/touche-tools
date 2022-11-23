namespace ToucheTools.Models.Instructions;

public class InitCharScriptInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.InitCharScript;
    public override int Width => 10;
    
    public short Character { get; set; }
    public short Color { get; set; }
    public short SpriteIndex { get; set; }
    public short SequenceIndex { get; set; }
    public short SequenceCharacterId { get; set; }

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadInt16();
        Color = reader.ReadInt16();
        SpriteIndex = reader.ReadInt16();
        SequenceIndex = reader.ReadInt16();
        SequenceCharacterId = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
        writer.Write((short)Color);
        writer.Write((short)SpriteIndex);
        writer.Write((short)SequenceIndex);
        writer.Write((short)SequenceCharacterId);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character} color {Color} sprite {SpriteIndex} seq {SequenceIndex} char {SequenceCharacterId}";
    }
}