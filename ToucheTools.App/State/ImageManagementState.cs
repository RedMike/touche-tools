namespace ToucheTools.App.State;

public class ImageManagementState
{
    public string? SelectedImage { get; set; } = null;
    public bool PreviewOpen { get; set; } = false;
    public bool EditorOpen { get; set; } = false;
}