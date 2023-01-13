using System.Numerics;
using ImGuiNET;
using ToucheTools.App.Models;
using ToucheTools.App.State;
using ToucheTools.App.Utils;
using ToucheTools.App.ViewModels;
using ToucheTools.Constants;

namespace ToucheTools.App.Windows;

public class RoomEditorWindow : BaseWindow
{
    private readonly OpenedManifest _manifest;
    private readonly MainWindowState _state;
    private readonly RoomManagementState _roomManagementState;
    private readonly RenderWindow _render;
    private readonly PackageImages _images;
    private readonly PackageRooms _rooms;

    public RoomEditorWindow(OpenedManifest manifest, MainWindowState state, RoomManagementState roomManagementState, RenderWindow render, PackageImages images, PackageRooms rooms)
    {
        _manifest = manifest;
        _state = state;
        _roomManagementState = roomManagementState;
        _render = render;
        _images = images;
        _rooms = rooms;
    }

    public override void Render()
    {
        if (_state.State != MainWindowState.States.RoomManagement)
        {
            return;
        }

        if (!_roomManagementState.EditorOpen)
        {
            return;
        }

        if (_roomManagementState.SelectedRoom == null)
        {
            return;
        }

        var game = _manifest.GetGame();
        var room = _rooms.GetRoom(_roomManagementState.SelectedRoom);
        
        ImGui.Begin("Room Editor", ImGuiWindowFlags.NoCollapse);
        var windowSize = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin();
        var childWindowSize = new Vector2(windowSize.X / 2.0f, windowSize.Y);
        var childWidowWidth = windowSize.X / 2.0f - 5.0f;

        var startPos = ImGui.GetCursorPos();
        ImGui.BeginChild("Room Controls", childWindowSize, false, ImGuiWindowFlags.ChildWindow);

        //room image
        var roomImages = _manifest.GetIncludedImages()
            .Where(p => p.Value.Type == OpenedManifest.ImageType.Room)
            .Select(p => (p.Key, p.Value.Index))
            .ToList();
        var roomImageList = roomImages.Select(r => $"Room {r.Index} ({r.Key.ShortenPath()})").ToArray();
        var origSelectedRoomImage = roomImages.FindIndex(p => p.Index == room.RoomImageIndex);
        var selectedRoomImage = origSelectedRoomImage;
        ImGui.PushID("RoomImage");
        ImGui.SetNextItemWidth(childWidowWidth);
        ImGui.Combo("", ref selectedRoomImage, roomImageList, roomImageList.Length);
        if (selectedRoomImage != origSelectedRoomImage)
        {
            room.RoomImageIndex = roomImages[selectedRoomImage].Index;
        }
        ImGui.PopID();

        var roomWidth = 0;
        var roomHeight = 0;
        if (room.RoomImageIndex != null)
        {
            var roomImagePath = _manifest.GetIncludedImages().First(p =>
                p.Value.Type == OpenedManifest.ImageType.Room && p.Value.Index == room.RoomImageIndex).Key;
            (roomWidth, roomHeight, _) = _images.GetImage(roomImagePath);
        }
        
        ImGui.Separator();

        if (ImGui.TreeNodeEx("Walkable Points"))
        {
            foreach (var (walkablePointId, (origX, origY, origZ)) in room.WalkablePoints)
            {
                ImGui.Text($"{walkablePointId} -");
                ImGui.SameLine();
                var x = origX;
                var y = origY;
                var z = origZ;

                ImGui.PushID($"RoomWalkPoints{walkablePointId}X");
                ImGui.SetNextItemWidth(childWidowWidth / 4.0f);
                ImGui.SliderInt("", ref x, 0, roomWidth, $"X {x}");
                if (x != origX)
                {
                    room.WalkablePoints[walkablePointId] = (x, y, z);
                }

                ImGui.PopID();
                ImGui.SameLine();

                ImGui.PushID($"RoomWalkPoints{walkablePointId}Y");
                ImGui.SetNextItemWidth(childWidowWidth / 4.0f);
                ImGui.SliderInt("", ref y, 0, roomHeight, $"Y {y}");
                if (y != origY)
                {
                    room.WalkablePoints[walkablePointId] = (x, y, z);
                }

                ImGui.PopID();
                ImGui.SameLine();

                ImGui.PushID($"RoomWalkPoints{walkablePointId}Z");
                ImGui.SetNextItemWidth(childWidowWidth / 4.0f);
                ImGui.SliderInt("", ref z, ToucheTools.Constants.Game.ZDepthMin, ToucheTools.Constants.Game.ZDepthMax,
                    $"Z {z}");
                if (z != origZ)
                {
                    room.WalkablePoints[walkablePointId] = (x, y, z);
                }

                ImGui.PopID();
            }

            ImGui.SetNextItemWidth(childWidowWidth);
            if (ImGui.Button("Add Walkable Point"))
            {
                var newId = 1;
                if (room.WalkablePoints.Count > 0)
                {
                    newId = room.WalkablePoints.Select(p => p.Key).Max() + 1;
                }

                room.WalkablePoints.Add(newId, (0, 0, ToucheTools.Constants.Game.ZDepthEven));
            }
            
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx("Walkable Lines"))
        {
            foreach (var ((p1, p2), (clip, area1, area2)) in room.WalkableLines.ToList())
            {
                ImGui.Text($"Point {p1} to point {p2}");
                ImGui.SameLine();
                ImGui.PushID($"RoomWalkLine{p1}-{p2}Delete");
                //TODO: rest
                if (ImGui.Button("Delete"))
                {
                    room.WalkableLines.Remove((p1, p2));
                }
                ImGui.PopID();
            }
            
            var points = room.WalkablePoints.Keys.ToList();
            var pointList = points.Select(p => $"Point {p}").ToArray();
            
            var origPoint1 = _roomManagementState.WalkableLinePoint1;
            var point1 = origPoint1;
            ImGui.PushID($"RoomWalkLineP1");
            ImGui.SetNextItemWidth(childWidowWidth / 4.0f);
            ImGui.Combo("", ref point1, pointList, pointList.Length);
            if (point1 != origPoint1)
            {
                _roomManagementState.WalkableLinePoint1 = point1;
            }
            ImGui.PopID();
            ImGui.SameLine();
            
            var origPoint2 = _roomManagementState.WalkableLinePoint2;
            var point2 = origPoint2;
            ImGui.PushID($"RoomWalkLineP2");
            ImGui.SetNextItemWidth(childWidowWidth / 4.0f);
            ImGui.Combo("", ref point2, pointList, pointList.Length);
            if (point2 != origPoint2)
            {
                _roomManagementState.WalkableLinePoint2 = point2;
            }
            ImGui.PopID();
            ImGui.SameLine();

            ImGui.SetNextItemWidth(childWidowWidth / 4.0f);
            if (ImGui.Button("Add"))
            {
                var p = (points[point1], points[point2]);
                if (room.WalkableLines.ContainsKey(p))
                {
                    throw new Exception("Line already exists");
                }

                room.WalkableLines[p] = (0, -1, -1);
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx("Hitboxes"))
        {
            foreach (var (hitboxId, hitbox) in room.Hitboxes)
            {
                if (ImGui.TreeNodeEx($"Hitbox{hitboxId}", ImGuiTreeNodeFlags.None, $"{hitboxId} - {hitbox.Item} {hitbox.Type:G} - {hitbox.Label}"))
                {
                    var item = hitbox.Item;
                    ImGui.PushID($"Hitbox{hitboxId}Item");
                    var text = "ID";
                    if (hitbox.Type == HitboxModel.HitboxType.KeyChar)
                    {
                        text = "KeyChar";
                    } else if (hitbox.Type == HitboxModel.HitboxType.Inventory)
                    {
                        text = "Item";
                    }
                    ImGui.InputInt(text, ref item, 1);
                    if (item != hitbox.Item)
                    {
                        hitbox.Item = item;
                    }
                    ImGui.PopID();
                    
                    var types = Enum.GetValues<HitboxModel.HitboxType>().ToList();
                    var typeList = types.Select(t => $"{t:G}").ToArray();
                    var origType = types.FindIndex(t => t == hitbox.Type);
                    var type = origType;
                    
                    ImGui.PushID($"Hitbox{hitboxId}Type");
                    ImGui.Combo("", ref type, typeList, typeList.Length);
                    if (type != origType)
                    {
                        hitbox.Type = types[type];
                    }
                    ImGui.PopID();

                    var label = hitbox.Label;
                    ImGui.InputText("Label", ref label, 32);
                    if (label != hitbox.Label)
                    {
                        hitbox.Label = label;
                    }
                    
                    var secondaryLabel = hitbox.SecondaryLabel;
                    ImGui.InputText("Secondary Label", ref secondaryLabel, 32);
                    if (secondaryLabel != hitbox.SecondaryLabel)
                    {
                        hitbox.SecondaryLabel = secondaryLabel;
                    }

                    if (hitbox.Type == HitboxModel.HitboxType.KeyChar)
                    {
                        
                    } else if (hitbox.Type == HitboxModel.HitboxType.Inventory)
                    {
                        //TODO: inventory hitboxes
                    } else if (hitbox.Type == HitboxModel.HitboxType.Normal)
                    {
                        var x = hitbox.X;
                        ImGui.PushID($"Hitbox{hitboxId}X");
                        ImGui.SliderInt("", ref x, 0, roomWidth, $"X {x}");
                        if (x != hitbox.X)
                        {
                            hitbox.X = x;
                        }
                        ImGui.PopID();
                    
                        var y = hitbox.Y;
                        ImGui.PushID($"Hitbox{hitboxId}Y");
                        ImGui.SliderInt("", ref y, 0, roomHeight, $"Y {y}");
                        if (y != hitbox.Y)
                        {
                            hitbox.Y = y;
                        }
                        ImGui.PopID();
                    
                        var w = hitbox.W;
                        ImGui.PushID($"Hitbox{hitboxId}W");
                        ImGui.SliderInt("", ref w, 0, roomWidth, $"W {w}");
                        if (w != hitbox.W)
                        {
                            hitbox.W = w;
                        }
                        ImGui.PopID();
                    
                        var h = hitbox.H;
                        ImGui.PushID($"Hitbox{hitboxId}H");
                        ImGui.SliderInt("", ref h, 0, roomHeight, $"H {h}");
                        if (h != hitbox.H)
                        {
                            hitbox.H = h;
                        }
                        ImGui.PopID();
                    }

                    var actions = game.ActionDefinitions
                        .Select(a => (a.Key, a.Value))
                        .Where(a => !Actions.BuiltInActions.Contains(-a.Key))
                        .ToList();
                    actions.Insert(0, (-1, "-"));
                    var actionList = actions.Select(a => a.Key < 0 ? $"-" : $"Action {a.Key} ({a.Value})").ToArray();
                    var fallbackActionList = actionList.Select(s => $"Fallback {s}").ToArray();

                    var origFallbackAction = actions.FindIndex(a => a.Key == hitbox.FallbackAction);
                    if (origFallbackAction == -1)
                    {
                        origFallbackAction = 0;
                    }

                    var fallbackAction = origFallbackAction;
                    ImGui.PushID($"Hitbox{hitboxId}ActionFallback");
                    ImGui.Combo("", ref fallbackAction, fallbackActionList, fallbackActionList.Length);
                    if (fallbackAction != origFallbackAction)
                    {
                        var actionId = actions[fallbackAction].Key;
                        hitbox.FallbackAction = actionId;
                    }
                    ImGui.PopID();

                    for (var i = 0; i < 8; i++)
                    {
                        var origSelectedAction = actions.FindIndex(a => a.Key == hitbox.Actions[i]);
                        if (origSelectedAction == -1)
                        {
                            origSelectedAction = 0;
                        }
                        var selectedAction = origSelectedAction;
                        ImGui.PushID($"Hitbox{hitboxId}Action{i}");
                        ImGui.Combo("", ref selectedAction, actionList, actionList.Length);
                        if (selectedAction != origSelectedAction)
                        {
                            var actionId = actions[selectedAction].Key;
                            hitbox.Actions[i] = actionId;
                        }
                        ImGui.PopID();
                    }
                        
                    ImGui.TreePop();
                }
            }

            ImGui.SetNextItemWidth(childWidowWidth);
            if (ImGui.Button("Add Hitbox"))
            {
                var newId = 1;
                if (room.Hitboxes.Count > 0)
                {
                    newId = room.Hitboxes.Select(h => h.Key).Max() + 1;
                }

                room.Hitboxes.Add(newId, new HitboxModel()
                {
                    Item = -1
                });
            }
            
            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx("Background Areas"))
        {
            foreach (var (bgAreaId, bgArea) in room.BackgroundAreas)
            {
                if (ImGui.TreeNodeEx($"BackgroundArea{bgAreaId}", ImGuiTreeNodeFlags.None,
                        $"{bgAreaId} - ({bgArea.SourceX}, {bgArea.SourceY}) x ({bgArea.Width}, {bgArea.Height})"))
                {
                    var origEnabled = bgArea.DestX != null && bgArea.DestY != null;
                    var enabled = origEnabled;
                    ImGui.PushID($"BackgroundArea{bgAreaId}Enabled");
                    ImGui.Checkbox("", ref enabled);
                    ImGui.PopID();
                    ImGui.SameLine();
                    ImGui.Text("Start Enabled?");
                    if (enabled != origEnabled)
                    {
                        if (enabled)
                        {
                            bgArea.DestX = 0;
                            bgArea.DestY = 0;
                        }
                        else
                        {
                            bgArea.DestX = null;
                            bgArea.DestY = null;
                        }
                    }

                    if (enabled)
                    {
                        if (bgArea.DestX == null || bgArea.DestY == null)
                        {
                            throw new Exception("Missing destination values");
                        }
                        var origDestX = bgArea.DestX.Value;
                        var destX = origDestX;
                        ImGui.PushID($"BackgroundArea{bgAreaId}DestX");
                        ImGui.SliderInt("", ref destX, 0, roomWidth, $"Dest X {destX}");
                        ImGui.PopID();
                        if (destX != origDestX)
                        {
                            bgArea.DestX = destX;
                        }

                        var origDestY = bgArea.DestY.Value;
                        var destY = origDestY;
                        ImGui.PushID($"BackgroundArea{bgAreaId}DestY");
                        ImGui.SliderInt("", ref destY, 0, roomHeight, $"Dest Y {destY}");
                        ImGui.PopID();
                        if (destY != origDestY)
                        {
                            bgArea.DestY = destY;
                        }
                    }
                    
                    var origSrcX = bgArea.SourceX;
                    var srcX = origSrcX;
                    ImGui.PushID($"BackgroundArea{bgAreaId}SrcX");
                    ImGui.SliderInt("", ref srcX, 0, roomWidth, $"Source X {srcX}");
                    ImGui.PopID();
                    if (srcX != origSrcX)
                    {
                        bgArea.SourceX = srcX;
                    }
                    
                    var origSrcY = bgArea.SourceY;
                    var srcY = origSrcY;
                    ImGui.PushID($"BackgroundArea{bgAreaId}SrcY");
                    ImGui.SliderInt("", ref srcY, 0, roomHeight, $"Source Y {srcY}");
                    ImGui.PopID();
                    if (srcY != origSrcY)
                    {
                        bgArea.SourceY = srcY;
                    }
                    
                    var origWidth = bgArea.Width;
                    var width = origWidth;
                    ImGui.PushID($"BackgroundArea{bgAreaId}Width");
                    ImGui.SliderInt("", ref width, 0, roomWidth, $"Width {width}");
                    ImGui.PopID();
                    if (width != origWidth)
                    {
                        bgArea.Width = width;
                    }
                    
                    var origHeight = bgArea.Height;
                    var height = origHeight;
                    ImGui.PushID($"BackgroundArea{bgAreaId}Height");
                    ImGui.SliderInt("", ref height, 0, roomHeight, $"Height {height}");
                    ImGui.PopID();
                    if (height != origHeight)
                    {
                        bgArea.Height = height;
                    }
                    
                    var origDynamic = bgArea.Dynamic;
                    var dynamic = origDynamic;
                    ImGui.PushID($"BackgroundArea{bgAreaId}Dynamic");
                    ImGui.Checkbox("", ref dynamic);
                    ImGui.PopID();
                    ImGui.SameLine();
                    ImGui.Text("Dynamic?");
                    if (dynamic != origDynamic)
                    {
                        bgArea.Dynamic = dynamic;
                    }
                    
                    ImGui.TreePop();
                }
            }

            ImGui.SetNextItemWidth(childWidowWidth);
            if (ImGui.Button("Add Background Area"))
            {
                var newId = 1;
                if (room.BackgroundAreas.Count > 0)
                {
                    newId = room.BackgroundAreas.Select(h => h.Key).Max() + 1;
                }

                room.BackgroundAreas.Add(newId, new BackgroundAreaModel());
            }
            
            ImGui.TreePop();
        }
        
        if (ImGui.TreeNodeEx("Texts"))
        {
            foreach (var (textId, origTextStr) in room.Texts)
            {
                ImGui.Text($"{textId} -");
                ImGui.SameLine();

                var textStr = origTextStr;
                
                ImGui.PushID($"RoomTexts{textId}");
                ImGui.SetNextItemWidth(3.0f * childWidowWidth/4.0f);
                ImGui.InputText("", ref textStr, 128);
                if (textStr != origTextStr)
                {
                    room.Texts[textId] = textStr;
                }
                ImGui.PopID();
            }

            ImGui.SetNextItemWidth(childWidowWidth);
            if (ImGui.Button("Add Text"))
            {
                var newId = 1;
                if (room.Texts.Count > 0)
                {
                    newId = room.Texts.Select(p => p.Key).Max() + 1;
                }

                room.Texts.Add(newId, "");
            }
            
            ImGui.TreePop();
        }

        ImGui.Separator();
        if (ImGui.Button("Save"))
        {
            _rooms.SaveRoom(_roomManagementState.SelectedRoom);
        }
        if (ImGui.Button("Close Editor"))
        {
            _roomManagementState.EditorOpen = false;
        }
        
        ImGui.EndChild();

        ImGui.SetCursorPos(startPos + new Vector2(windowSize.X/2.0f, 0.0f));
        ImGui.BeginChild("Room View", childWindowSize, true, ImGuiWindowFlags.ChildWindow | ImGuiWindowFlags.AlwaysHorizontalScrollbar);
        var imagePos = ImGui.GetCursorPos();
        if (room.RoomImageIndex != null)
        {
            var roomImagePath = _manifest.GetIncludedImages().First(p =>
                    p.Value.Type == OpenedManifest.ImageType.Room && p.Value.Index == room.RoomImageIndex).Key;
            var (roomImageWidth, roomImageHeight, roomImageBytes) = _images.GetImage(roomImagePath);

            //blank texture
            var blankTexture = _render.RenderCheckerboardRectangle(25, roomImageWidth, roomImageHeight,
                (40, 30, 40, 255), (50, 40, 50, 255));
            ImGui.SetCursorPos(imagePos);
            ImGui.Image(blankTexture, new Vector2(roomImageWidth, roomImageHeight));

            //room texture
            var roomTexture = _render.RenderImage(RenderWindow.RenderType.Primitive, roomImagePath, roomImageWidth,
                roomImageHeight, roomImageBytes);
            ImGui.SetCursorPos(imagePos);
            ImGui.Image(roomTexture, new Vector2(roomImageWidth, roomImageHeight));
            
            //points
            var pw = 10;
            var pointTexture = _render.RenderRectangle(1, pw, pw,
                (255, 255, 255, 50), (255, 255, 255, 255));
            foreach (var (pointId, (x, y, z)) in room.WalkablePoints)
            {
                var zFactor = Game.GetZFactor(z);
                var ow = pw * zFactor;
                var ox = x;
                var oy = y;
                ImGui.SetCursorPos(imagePos + new Vector2(ox - ow/2, oy - ow/2));
                ImGui.Image(pointTexture, new Vector2(ow, ow));
                
                ImGui.SetCursorPos(imagePos + new Vector2(ox, oy));
                ImGui.Text($"{pointId}");
            }
            
            //walk lines
            var drawList = ImGui.GetWindowDrawList();
            foreach (var ((point1, point2), (_, _, _)) in room.WalkableLines)
            {
                var (p1X, p1Y, _) = room.WalkablePoints[point1];
                var (p2X, p2Y, _) = room.WalkablePoints[point2];
                var curPos = ImGui.GetWindowPos() - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY()) + imagePos;
                drawList.AddLine(curPos + new Vector2(p1X, p1Y), curPos + new Vector2(p2X, p2Y), ImGui.GetColorU32(new Vector4(0.7f, 0.9f, 0.6f, 1.0f)), 1.0f);
            }
            
            //hitboxes
            foreach (var (hitboxId, hitbox) in room.Hitboxes)
            {
                if (hitbox.W == 0 || hitbox.H == 0)
                {
                    continue;
                }

                (byte, byte, byte, byte) col = (255, 0, 0, 50);
                var rectTexture = _render.RenderRectangle(1, hitbox.W, hitbox.H,
                    col, (255, 255, 255, 255));
                
                ImGui.SetCursorPos(imagePos + new Vector2(hitbox.X, hitbox.Y));
                ImGui.Image(rectTexture, new Vector2(hitbox.W, hitbox.H));

                var type = $"\n{hitbox.Type:G}";
                var secondary = "";
                if (!string.IsNullOrEmpty(hitbox.SecondaryLabel))
                {
                    secondary = $"\n({hitbox.SecondaryLabel})";
                }
                
                var rectText = $"Hitbox {hitboxId}\nItem {hitbox.Item}\n{hitbox.Label}{secondary}{type}";
                ImGui.SetCursorPos(imagePos + new Vector2(hitbox.X, hitbox.Y));
                ImGui.Text(rectText);
            }
            
            //background areas
            foreach (var (bgAreaId, bgArea) in room.BackgroundAreas)
            {
                if (bgArea.Width == 0 || bgArea.Height == 0)
                {
                    continue;
                }
                
                //source
                (byte, byte, byte, byte) col = (100, 255, 100, 50);
                var rectTexture = _render.RenderRectangle(1, bgArea.Width, bgArea.Height,
                    col, (255, 255, 255, 255));
                ImGui.SetCursorPos(imagePos + new Vector2(bgArea.SourceX, bgArea.SourceY));
                ImGui.Image(rectTexture, new Vector2(bgArea.Width, bgArea.Height));

                var type = "\nStatic";
                if (bgArea.Dynamic)
                {
                    type = "\nDynamic";
                }
                var rectText = $"BG Area {bgAreaId}\nSource{type}";
                ImGui.SetCursorPos(imagePos + new Vector2(bgArea.SourceX, bgArea.SourceY));
                ImGui.Text(rectText);
                
                //dest
                if (bgArea.DestX != null && bgArea.DestY != null)
                {
                    col = (50, 50, 255, 50);
                    rectTexture = _render.RenderRectangle(1, bgArea.Width, bgArea.Height,
                        col, (255, 255, 255, 255));
                    ImGui.SetCursorPos(imagePos + new Vector2(bgArea.DestX.Value, bgArea.DestY.Value));
                    ImGui.Image(rectTexture, new Vector2(bgArea.Width, bgArea.Height));

                    rectText = $"BG Area {bgAreaId}\nDestination{type}";
                    ImGui.SetCursorPos(imagePos + new Vector2(bgArea.DestX.Value, bgArea.DestY.Value));
                    ImGui.Text(rectText);
                }
            }
        }
        ImGui.EndChild();
        
        ImGui.End();
    }
}