using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class RoomImagePreviewWindow : BaseWindow
{
    private readonly OpenedPackage _package;
    private readonly MainWindowState _state;
    private readonly PreviewInfoState _previewState;
    private readonly RenderWindow _render;
    private readonly PackageImages _images;

    private readonly Dictionary<string, (IntPtr, int, int)> _textures = new Dictionary<string, (IntPtr, int, int)>();
    
    public RoomImagePreviewWindow(OpenedPackage package, MainWindowState state, PreviewInfoState previewState, RenderWindow render, PackageImages images)
    {
        _package = package;
        _state = state;
        _previewState = previewState;
        _render = render;
        _images = images;
    }


    public override void Render()
    {
        if (_state.State != MainWindowState.States.ImageManagement)
        {
            return;
        }

        if (!_previewState.RoomImagePreviewOpen)
        {
            return;
        }

        if (_previewState.SelectedRoomImage == null)
        {
            return;
        }

        if (!_package.LoadedManifest.Images.ContainsKey(_previewState.SelectedRoomImage))
        {
            //error?
            return;
        }

        var image = _package.LoadedManifest.Images[_previewState.SelectedRoomImage];
        if (image.Type != OpenedPackage.ImageType.Room)
        {
            throw new Exception("Wrong image type");
        }
        var (texture, width, height) = GetTexture(_previewState.SelectedRoomImage);
        
        ImGui.Begin("Room Image Preview", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysHorizontalScrollbar);
        
        //background
        var imgOffset = ImGui.GetWindowContentRegionMin();
        var blankRect = _render.RenderCheckerboardRectangle(20, width, height,
            (70, 70, 70, 255), (170, 170, 170, 255)
        );
        ImGui.SetCursorPos(imgOffset);
        ImGui.Image(blankRect, new Vector2(width, height));
        
        //foreground
        ImGui.SetCursorPos(imgOffset);
        ImGui.Image(texture, new Vector2(width, height));

        if (ImGui.Button("Close preview"))
        {
            _previewState.RoomImagePreviewOpen = false;
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