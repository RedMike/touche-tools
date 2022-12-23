using ToucheTools.Constants;

namespace ToucheTools.App.Models;

public class GameModel
{
    public class InventoryItem
    {
        /// <summary>
        /// Name used by default, but a specific program could technically overwrite it
        /// </summary>
        public string DefaultLabel { get; set; } = "";

        public int[] DefaultActions { get; set; } = new int[8];
    }
    
    public Dictionary<int, string> ActionDefinitions { get; set; } = new Dictionary<int, string>()
    {
        { -Actions.DoNothing, "DoNothing" },
        { -Actions.LeftClick, "LeftClick" },
        { -Actions.LeftClickWithItem, "LeftClickWithItem" },
    };
    
    public Dictionary<int, (byte, byte, byte)> CustomColors { get; set; } = new Dictionary<int, (byte, byte, byte)>()
    {
        {Palettes.TransparencyColor, (0, 0, 0)},
        {Palettes.TransparentSpriteMarkerColor, (255, 0, 255)},
        {Palettes.TransparentRoomMarkerColor, (255, 255, 255)},
        {Palettes.ConversationTextColor, (200, 200, 255)},
        {Palettes.InventoryMoneyTextColor, (200, 200, 255)},
        {Palettes.ActionMenuTextColor, (50, 60, 105)}
    };

    public Dictionary<int, InventoryItem> InventoryItems { get; set; } = new Dictionary<int, InventoryItem>()
    {
    };
}