﻿namespace ToucheTools.Models.Instructions;

public class UpdateRoomInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.UpdateRoom;
    public override int Width => 2;
    
    public short Area { get; set; }

    public override void Load(BinaryReader reader)
    {
        Area = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Area);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Area}";
    }

    public override void DeserialiseRemainder(string remainder, Dictionary<string, uint> labels)
    {
        Area = short.Parse(remainder);
    }

    public override string ToString()
    {
        return $"{Opcode:G} area {Area} without redrawing";
    }
}