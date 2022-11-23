﻿namespace ToucheTools.Models.Instructions;

public class RemoveItemFromInventoryInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.RemoveItemFromInventory;
    public override int Width => 2;
    
    public short Character { get; set; }
    
    public bool CurrentCharacter => Character == 256;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadInt16();
    }

    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
    }

    public override string ToString()
    {
        return $"{Opcode:G} remove item of STK value from {(CurrentCharacter ? "current" : Character)}";
    }
}