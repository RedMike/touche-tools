using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace ToucheTools.App;

public class RenderWindow : IDisposable
{
    public static readonly Vector3 ClearColor = new Vector3(0.3f, 0.45f, 0.7f);

    private readonly string _title;
    private readonly int _w;
    private readonly int _h;

    private bool _windowOpen = false;
    private Sdl2Window _window = null!;
    private GraphicsDevice _graphicsDevice = null!;
    private ImGuiController _controller = null!;
    private CommandList _commandList = null!;
    
    private DateTime _lastUpdate = DateTime.MinValue;
    
    public RenderWindow(string title, int w, int h)
    {
        _title = title;
        _w = w;
        _h = h;

        CreateWindow();
    }

    public bool IsOpen()
    {
        return _windowOpen;
    }

    public void ProcessInput()
    {
        InputSnapshot snapshot = _window.PumpEvents();
        if (!_window.Exists)
        {
            _windowOpen = false;
        }

        var delta = NextFrameDelta();
        _controller.Update((float)delta, snapshot);
    }

    public void Render()
    {
        _commandList.Begin();
        _commandList.SetFramebuffer(_graphicsDevice.MainSwapchain.Framebuffer);
        _commandList.ClearColorTarget(0, new RgbaFloat(ClearColor.X, ClearColor.Y, ClearColor.Z, 1f));
        _controller.Render(_graphicsDevice, _commandList);
        _commandList.End();
        _graphicsDevice.SubmitCommands(_commandList);
        _graphicsDevice.SwapBuffers(_graphicsDevice.MainSwapchain);
    }

    private void CreateWindow()
    {
        if (_windowOpen)
        {
            throw new Exception("Window already open");
        }
        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, _w, _h, WindowState.Normal, _title),
            new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
            out var window,
            out var gd);
        if (window == null || gd == null)
        {
            throw new Exception("Failed to create window");
        }

        _window = window;
        _graphicsDevice = gd;
        
        _controller = new ImGuiController(_graphicsDevice, _graphicsDevice.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);
        _window.Resized += WindowOnResize;
        _window.Closing += WindowOnClose;
        _commandList = _graphicsDevice.ResourceFactory.CreateCommandList();
        _windowOpen = true;
    }

    private void WindowOnResize()
    {
        if (!_windowOpen)
        {
            throw new Exception("Window not open");
        }
        if (_controller == null)
        {
            throw new Exception("Missing controller");
        }

        _graphicsDevice.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
        _controller.WindowResized(_window.Width, _window.Height);
    }

    private void WindowOnClose()
    {
        _windowOpen = false;
    }
    
    private double NextFrameDelta()
    {
        var prevUpdate = _lastUpdate;
        _lastUpdate = DateTime.UtcNow;
        return (_lastUpdate - prevUpdate).TotalSeconds;
    }

    #region Dispose
    public void Dispose()
    {
        try
        {
            _graphicsDevice.WaitForIdle();
            _graphicsDevice.Dispose();
            _controller.Dispose();
            _commandList.Dispose();
        }
        catch (Exception)
        {
            //non-issue
        }
    }
    #endregion
}