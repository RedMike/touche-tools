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
}