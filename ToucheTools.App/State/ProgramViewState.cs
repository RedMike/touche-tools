namespace ToucheTools.App.State;

public class ProgramViewState
{
    public Dictionary<int, int> OffsetToIndex { get; set; } = null!;
    public Dictionary<int, float> OffsetYPos { get; set; } = null!;
    private float? _setOffsetY = null;

    public float? GetQueuedScroll()
    {
        if (_setOffsetY == null)
        {
            return null;
        }
        var value = _setOffsetY.Value;
        _setOffsetY = null;
        return value;
    }
    
    public void QueueScrollToOffset(int offset)
    {
        if (!OffsetYPos.ContainsKey(offset))
        {
            throw new Exception($"Missing offset: {offset}");
        }

        _setOffsetY = OffsetYPos[offset];
    }
}