using ToucheTools.Constants;

namespace ToucheTools.App.Models;

public class GameModel
{
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
        {Palettes.ActionMenuTextColor, (50, 100, 205)}
    };
}