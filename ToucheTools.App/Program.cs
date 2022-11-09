using ImGuiNET;
using ToucheTools.App;

using var window = new RenderWindow("ToucheTools", 800, 600);

while (window.IsOpen())
{
    window.ProcessInput();
    if (!window.IsOpen())
    {
        break;
    }
    
    //render IMGUI components
    ImGui.Text("Hello, world!");
    
    window.Render();
}