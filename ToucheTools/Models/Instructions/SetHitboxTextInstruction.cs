namespace ToucheTools.Models.Instructions;

public class SetHitboxTextInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetHitboxText;
    public override int Width => 2;
    
    public ushort Num { get; set; }

    public bool IsCharacterHitbox => (Num & 0x4000) > 0;

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Num);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(IsCharacterHitbox ? "character" : "hitbox")} {Num}";
    }
}