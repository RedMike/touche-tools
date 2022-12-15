namespace ToucheTools.App.Utils;

public static class ColourHelper
{
    public static string ColourName(int i)
    {
        if (ToucheTools.Constants.Palettes.TransparencyColor == i)
        {
            return "Transparent";
        }
        if (ToucheTools.Constants.Palettes.TransparentSpriteMarkerColor == i)
        {
            return "Sprite Marker (Transparent)";
        }
        if (ToucheTools.Constants.Palettes.TransparentRoomMarkerColor == i)
        {
            return "Room Marker/UI text";
        }
        if (ToucheTools.Constants.Palettes.InventoryBackgroundColor == i)
        {
            return "Inventory/Conversation Background";
        }
        if (ToucheTools.Constants.Palettes.ConversationTextColor == i)
        {
            return "Conversation Text";
        }
        if (ToucheTools.Constants.Palettes.InventoryMoneyTextColor == i)
        {
            return "Inventory Money";
        }
        if (ToucheTools.Constants.Palettes.ActionMenuBackgroundColor == i)
        {
            return "Action Menu Background";
        }
        if (ToucheTools.Constants.Palettes.ActionMenuTextColor == i)
        {
            return "Action Menu Text";
        }
        var type = "Room";
        if (i >= ToucheTools.Constants.Palettes.StartOfSpriteColors)
        {
            type = "Sprite/Icon/UI";
        }
        var id = $"{type} {i}";
        return id;
    }
}