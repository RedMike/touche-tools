using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class ImagePreviewWindow : BaseWindow
{
    private readonly OpenedPackage _package;
    private readonly MainWindowState _state;
    private readonly ImageManagementState _imageManagementState;
    private readonly RenderWindow _render;
    private readonly PackageImages _images;

    private readonly Dictionary<string, (IntPtr, int, int)> _textures = new Dictionary<string, (IntPtr, int, int)>();
    
    public ImagePreviewWindow(OpenedPackage package, MainWindowState state, ImageManagementState imageManagementState, RenderWindow render, PackageImages images)
    {
        _package = package;
        _state = state;
        _imageManagementState = imageManagementState;
        _render = render;
        _images = images;
    }


    public override void Render()
    {
        if (_state.State != MainWindowState.States.ImageManagement)
        {
            return;
        }

        if (!_imageManagementState.PreviewOpen)
        {
            return;
        }

        if (_imageManagementState.SelectedImage == null)
        {
            return;
        }

        if (!_package.LoadedManifest.Images.ContainsKey(_imageManagementState.SelectedImage))
        {
            //error?
            return;
        }

        var (texture, width, height) = GetTexture(_imageManagementState.SelectedImage);
        
        ImGui.Begin("Image Preview", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysHorizontalScrollbar);
        
        //background
        var imgOffset = ImGui.GetWindowContentRegionMin();
        var blankRect = _render.RenderCheckerboardRectangle(20, width, height,
            (40, 30, 40, 255), (50, 40, 50, 255)
        );
        ImGui.SetCursorPos(imgOffset);
        ImGui.Image(blankRect, new Vector2(width, height));
        
        //foreground
        ImGui.SetCursorPos(imgOffset);
        ImGui.Image(texture, new Vector2(width, height));

        if (ImGui.Button("Close preview"))
        {
            _imageManagementState.PreviewOpen = false;
        }
        
        ImGui.End();
    }

    private (IntPtr, int, int) GetTexture(string path)
    {
        if (_textures.ContainsKey(path))
        {
            //TODO: clear cache
            return _textures[path];
        }
        var (width, height, bytes) = _images.GetImage(path);
        var img = _render.RenderImage(RenderWindow.RenderType.Primitive, path, width, height, bytes);
        _textures[path] = (img, width, height);
        return (img, width, height);
    }
}