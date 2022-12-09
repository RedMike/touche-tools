namespace ToucheTools.Models.Instructions;

public class SetHitboxTextInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetHitboxText;
    public override int Width => 2;
    
    public short Num { get; set; }

    public bool IsCharacterHitbox => (Num & 0x4000) > 0;
    public short Hitbox => (short)(IsCharacterHitbox ? (Num & 0xFF) : (Num)); 

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Num);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Num}";
    }

    public override void DeserialiseRemainder(string remainder)
    {
        Num = short.Parse(remainder);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(IsCharacterHitbox ? "character" : "hitbox")} {Num}";
    }
}