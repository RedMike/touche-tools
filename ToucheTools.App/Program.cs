using System.Numerics;
using ImGuiNET;
using ToucheTools.App;
using Veldrid;
using Veldrid.StartupUtilities;

VeldridStartup.CreateWindowAndGraphicsDevice(
    new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, "ImGui.NET Sample Program"),
    new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true),
    out var window,
    out var gd);
var controller = new ImGuiController(gd, gd.MainSwapchain.Framebuffer.OutputDescription, window.Width, window.Height);
window.Resized += () =>
{
    gd.MainSwapchain.Resize((uint)window.Width, (uint)window.Height);
    controller.WindowResized(window.Width, window.Height);
};

var cl = gd.ResourceFactory.CreateCommandList();
var clearColor = new Vector3(0.3f, 0.45f, 0.7f);
    
while (window.Exists)
{
    InputSnapshot snapshot = window.PumpEvents();
    if (!window.Exists) { break; }
    controller.Update(1f / 60f, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.
    
    ImGui.Text("Hello, world!");
    
    cl.Begin();
    cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
    cl.ClearColorTarget(0, new RgbaFloat(clearColor.X, clearColor.Y, clearColor.Z, 1f));
    //render
    controller.Render(gd, cl);
    
    cl.End();
    gd.SubmitCommands(cl);
    gd.SwapBuffers(gd.MainSwapchain);
}

// Clean up Veldrid resources
gd.WaitForIdle();
controller.Dispose();
cl.Dispose();
gd.Dispose();