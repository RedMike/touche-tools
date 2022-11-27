using System.Numerics;
using ImGuiNET;
using SkiaSharp;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class PackageWindow : BaseWindow
{
    private readonly PackageViewModel _viewModel;
    private readonly RenderWindow _render;

    private readonly Dictionary<string, SKBitmap> _bitmaps = new Dictionary<string, SKBitmap>();
    private readonly Dictionary<string, (IntPtr, int, int)> _textures = new Dictionary<string, (IntPtr, int, int)>();

    public PackageWindow(PackageViewModel viewModel, RenderWindow render)
    {
        _viewModel = viewModel;
        _render = render;
        
        
    }

    public override void Render()
    {
        ImGui.Begin("Loaded Folder");
        var paths = _viewModel.GetImages();
        var files = paths.Select(Path.GetFileName).ToArray();
        for (var i = 0; i < files.Length; i++)
        {
            var origInclude = _viewModel.IsFileIncluded(i);
            var include = origInclude;
            
            if (ImGui.Button(files[i]))
            {
                _viewModel.SetSelectedIndex(i);
            }

            ImGui.SameLine();
            
            ImGui.PushID($"Include {i}");
            ImGui.Checkbox("", ref include);
            ImGui.PopID();
            if (include != origInclude)
            {
                _viewModel.SetFileIncluded(i, include);
            }

            ImGui.SameLine();
            var types = Enum.GetValues<PackageViewModel.FileType>().ToList();
            var origFileTypeId = types.FindIndex(s => s == _viewModel.GetFileType(i));
            var fileTypeId = origFileTypeId;
            ImGui.PushID($"Type {i}");
            ImGui.Combo("", ref fileTypeId, types
                .OrderBy(t => t)
                .Select(t => t.ToString("G"))
                .ToArray(), types.Count);
            ImGui.PopID();
            if (fileTypeId != origFileTypeId)
            {
                _viewModel.SetFileType(i, types[fileTypeId]);
            }
        }
        ImGui.End();

        ImGui.Begin("Image Preview");
        var (texture, width, height) = GetImage(paths[_viewModel.GetSelectedIndex()]);

        var background = _render.RenderCheckerboardRectangle(10, width, height,
            (75, 75, 75, 255), (50, 50, 50, 255)
        );
        ImGui.SetCursorPos(ImGui.GetWindowContentRegionMin());
        ImGui.Image(background, new Vector2(width, height));
        ImGui.SetCursorPos(ImGui.GetWindowContentRegionMin());
        ImGui.Image(texture, new Vector2(width, height));
        ImGui.End();
    }

    private (IntPtr, int, int) GetImage(string file)
    {
        if (_textures.ContainsKey(file))
        {
            return _textures[file];
        }
        var stream = File.Open(file, FileMode.Open);
        var bitmap = SKBitmap.Decode(stream);
        _bitmaps[file] = bitmap;
        var img = _render.RenderImage(RenderWindow.RenderType.Primitive, file, bitmap.Width, bitmap.Height, bitmap.Pixels
            .SelectMany(p => new [] {p.Red, p.Green, p.Blue, p.Alpha})
            .ToArray()
        );
        _textures[file] = (img, bitmap.Width, bitmap.Height);
        return (img, bitmap.Width, bitmap.Height);
    }
}