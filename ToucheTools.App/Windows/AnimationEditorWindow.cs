using System.Numerics;
using ImGuiNET;
using ToucheTools.App.Services;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;
using ToucheTools.Constants;
using ToucheTools.Models;

namespace ToucheTools.App.Windows;

public class AnimationEditorWindow : BaseWindow
{
    private readonly OpenedPackage _package;
    private readonly MainWindowState _state;
    private readonly AnimationManagementState _animationManagementState;
    private readonly RenderWindow _render;
    private readonly PackageImages _images;
    private readonly PackageAnimations _animations;
    private readonly SpriteSheetRenderer _spriteRenderer;

    public AnimationEditorWindow(OpenedPackage package, MainWindowState state, AnimationManagementState animationManagementState, RenderWindow render, PackageImages images, PackageAnimations animations, SpriteSheetRenderer spriteRenderer)
    {
        _package = package;
        _state = state;
        _animationManagementState = animationManagementState;
        _render = render;
        _images = images;
        _animations = animations;
        _spriteRenderer = spriteRenderer;
    }

    public override void Render()
    {
        if (_state.State != MainWindowState.States.AnimationManagement)
        {
            return;
        }

        if (!_animationManagementState.EditorOpen)
        {
            return;
        }

        if (_animationManagementState.SelectedAnimationPath == null)
        {
            return;
        }
        
        if (!_package.Value.Animations.ContainsKey(_animationManagementState.SelectedAnimationPath))
        {
            //error?
            return;
        }
        
        ImGui.Begin("Animation Editor", ImGuiWindowFlags.NoCollapse);
        var windowSize = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin();

        //sprite selection
        var sprites = _package.GetIncludedImages()
            .Where(p => p.Value.Type == OpenedPackage.ImageType.Sprite)
            .Select(p => (p.Key, p.Value.Index))
            .ToList();
        var spriteList = sprites.Select(s => $"Sprite {s.Index} ({s.Key})").ToArray();
        var origSelectedSprite = sprites.FindIndex(p => p.Index == _animationManagementState.SelectedSpriteIndex);
        if (origSelectedSprite == -1)
        {
            origSelectedSprite = 0;
        }
        var selectedSprite = origSelectedSprite;
        ImGui.PushID("AnimationSprite");
        ImGui.SetNextItemWidth(windowSize.X/2.0f);
        ImGui.Combo("", ref selectedSprite, spriteList, spriteList.Length);
        if (selectedSprite != origSelectedSprite)
        {
            _animationManagementState.SelectedSpriteIndex = sprites[selectedSprite].Index;
        }
        ImGui.PopID();
        var (spriteWidth, spriteHeight, spriteBytes) = _images.GetImage(sprites[selectedSprite].Key);
        var spriteTexture = _render.RenderImage(RenderWindow.RenderType.Sprite, sprites[selectedSprite].Key, spriteWidth, spriteHeight, spriteBytes);
        
        //palette selection
        ImGui.SameLine();
        var palettes = _package.GetIncludedImages()
            .Where(p => p.Value.Type == OpenedPackage.ImageType.Room)
            .Select(p => (p.Key, p.Value.Index))
            .ToList();
        var paletteList = palettes.Select(s => $"Room {s.Index} ({s.Key})").ToArray();
        var origSelectedPalette = palettes.FindIndex(p => p.Index == _animationManagementState.SelectedPaletteIndex);
        if (origSelectedPalette == -1)
        {
            origSelectedPalette = 0;
        }
        var selectedPalette = origSelectedPalette;
        ImGui.PushID("AnimationPalette");
        ImGui.SetNextItemWidth(windowSize.X/2.0f);
        ImGui.Combo("", ref selectedPalette, paletteList, paletteList.Length);
        if (selectedPalette != origSelectedPalette)
        {
            _animationManagementState.SelectedPaletteIndex = palettes[selectedPalette].Index;
        }
        ImGui.PopID();
        
        ImGui.Separator();
        var isNew = false;
        var imagePos = ImGui.GetCursorPos() + new Vector2(windowSize.X / 2.0f, 0.0f);
        var animation = _animations.GetAnimation(_animationManagementState.SelectedAnimationPath);
        
        //character field
        var characters = animation.CharToFrameFlag.Select(p => p.Key).OrderBy(p => p).ToList();
        if (characters.Count == 0)
        {
            isNew = true;
            characters = new List<int>() { 0 };
        }
        else
        {
            characters.Add(characters.Max() + 1);
        }
        var characterList = characters.Select((c, i) => $"{DisplayCharacter(c)}{((i == characters.Count - 1) ? " (new)" : "")}").ToArray();
        var origSelectedCharacter = _animationManagementState.SelectedCharacter;
        var selectedCharacter = origSelectedCharacter;
        ImGui.PushID("AnimationCharacter");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        ImGui.Combo("", ref selectedCharacter, characterList, characterList.Length);
        if (selectedCharacter != origSelectedCharacter)
        {
            _animationManagementState.SelectedCharacter = characters[selectedCharacter];
            _animationManagementState.SelectedAnimation = 0;
            _animationManagementState.SelectedDirection = 0;
            _animationManagementState.SelectedFrame = 0;
            _animationManagementState.SelectedPart = 0;
        }

        if (selectedCharacter == characters.Count - 1)
        {
            isNew = true;
        }
        ImGui.PopID();
        
        //animation field
        var animations = animation.FrameMappings
            .Where(p => p.Key.Item1 == selectedCharacter)
            .Select(p => p.Key.Item2)
            .Distinct()
            .OrderBy(p => p)
            .ToList();
        if (animations.Count == 0)
        {
            isNew = true;
            animations = new List<int>() { 0 };
        }
        else
        {
            animations.Add(animations.Max() + 1);
        }
        var animationList = animations.Select((a, i) => $"{DisplayAnimation(a)}{((i == animations.Count - 1) ? " (new)" : "")}").ToArray();
        var origSelectedAnimation = _animationManagementState.SelectedAnimation;
        var selectedAnimation = origSelectedAnimation;
        ImGui.PushID("AnimationAnimation");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        ImGui.Combo("", ref selectedAnimation, animationList, animationList.Length);
        if (selectedAnimation != origSelectedAnimation)
        {
            _animationManagementState.SelectedAnimation = animations[selectedAnimation];
            _animationManagementState.SelectedDirection = 0;
            _animationManagementState.SelectedFrame = 0;
            _animationManagementState.SelectedPart = 0;
        }
        if (selectedAnimation == animations.Count - 1)
        {
            isNew = true;
        }
        ImGui.PopID();
        
        //direction field
        var directions = new List<int>()
        {
            Directions.Right,
            Directions.Down,
            Directions.Up,
            Directions.Left
        };
        var directionList = directions.Select(d => $"{DisplayDirection(d)}").ToArray();
        var origSelectedDirection = _animationManagementState.SelectedDirection;
        var selectedDirection = origSelectedDirection;
        ImGui.PushID("AnimationDirection");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        ImGui.Combo("", ref selectedDirection, directionList, directionList.Length);
        if (selectedDirection != origSelectedDirection)
        {
            _animationManagementState.SelectedDirection = directions[selectedDirection];
            _animationManagementState.SelectedFrame = 0;
            _animationManagementState.SelectedPart = 0;
        }
        ImGui.PopID();
        
        //frame field
        var startFrameId = 1;
        if (animation.Frames.Count > 0)
        {
            startFrameId = animation.Frames.Keys.Max() + 1;
        }
        var frameIdentifier = (characters[selectedCharacter], animations[selectedAnimation],
            directions[selectedDirection]);
        if (animation.FrameMappings.ContainsKey(frameIdentifier))
        {
            startFrameId = animation.FrameMappings[frameIdentifier];
        }
        var frames = new List<int>() { 0 };
        if (animation.Frames.ContainsKey(startFrameId))
        {
            frames = Enumerable.Range(0, animation.Frames[startFrameId].Count + 1).ToList();
        }
        else
        {
            isNew = true;
        }
        var frameList = frames.Select((f, i) => $"{DisplayFrame(f)}{((i == frames.Count - 1) ? " (new)" : "")}").ToArray();
        var origSelectedFrame = _animationManagementState.SelectedFrame;
        var selectedFrame = origSelectedFrame;
        ImGui.PushID("AnimationFrame");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        ImGui.Combo("", ref selectedFrame, frameList, frameList.Length);
        if (selectedFrame != origSelectedFrame)
        {
            _animationManagementState.SelectedFrame = frames[selectedFrame];
            _animationManagementState.SelectedPart = 0;
        }
        if (selectedFrame == frames.Count - 1)
        {
            isNew = true;
        }
        ImGui.PopID();
        
        //part field
        var startPartId = 1;
        if (animation.Parts.Count > 0)
        {
            startPartId = animation.Parts.Keys.Max() + 1;
        }
        else
        {
            isNew = true;
        }
        var partIdentifier = (characters[selectedCharacter], animations[selectedAnimation], directions[selectedDirection], frames[selectedFrame]);
        if (animation.PartMappings.ContainsKey(partIdentifier))
        {
            startPartId = animation.PartMappings[partIdentifier];
        }
        var parts = new List<int>() { 0 };
        if (animation.Parts.ContainsKey(startPartId))
        {
            parts = Enumerable.Range(0, animation.Parts[startPartId].Count + 1).ToList();
        }
        var partList = parts.Select((p, i) => $"{DisplayPart(p)}{((i == parts.Count - 1) ? " (new)" : "")}").ToArray();
        var origSelectedPart = _animationManagementState.SelectedPart;
        var selectedPart = origSelectedPart;
        ImGui.PushID("AnimationPart");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        ImGui.Combo("", ref selectedPart, partList, partList.Length);
        if (selectedPart != origSelectedPart)
        {
            _animationManagementState.SelectedPart = parts[selectedPart];
        }
        if (selectedPart == parts.Count - 1)
        {
            isNew = true;
        }
        ImGui.PopID();
        
        //controls
        var frameChanged = false;
        
        var partInformation = new List<SequenceDataModel.PartInformation>()
        {
        };
        if (animation.Parts.ContainsKey(startPartId))
        {
            partInformation = animation.Parts[startPartId];
        }
        else
        {
            isNew = true;
        }

        var selectedPartInformation = new SequenceDataModel.PartInformation();
        if (partInformation.Count > parts[selectedPart])
        {
            selectedPartInformation = partInformation[parts[selectedPart]];
        }
        else
        {
            partInformation.Add(selectedPartInformation);
            isNew = true;
        }
        
        ImGui.Text("");
        ImGui.PushID("AnimationPartFromSheet");
        if (ImGui.Button("Grab From Sheet"))
        {
            //TODO: select part from sprite sheet
            frameChanged = true;
        }
        ImGui.PopID();
        
        ImGui.PushID("AnimationPartDestX");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        var origDestX = selectedPartInformation.DestX;
        var destX = origDestX;
        ImGui.SliderInt("", ref destX, -100, 100, "Dest X {0}");
        if (destX != origDestX)
        {
            selectedPartInformation.DestX = destX;
            frameChanged = true;
        }
        ImGui.PopID();
        
        ImGui.PushID("AnimationPartDestY");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        var origDestY = selectedPartInformation.DestY;
        var destY = origDestY;
        ImGui.SliderInt("", ref destY, -100, 100, "Dest Y {0}");
        if (destY != origDestY)
        {
            selectedPartInformation.DestY = destY;
            frameChanged = true;
        }
        ImGui.PopID();

        if (frameChanged)
        {
            if (isNew)
            {
                //create any missing bits of data
                if (!animation.CharToFrameFlag.ContainsKey(characters[selectedCharacter]))
                {
                    animation.CharToFrameFlag[characters[selectedCharacter]] = (ushort)0;
                }

                if (!animation.Frames.ContainsKey(frames[selectedFrame]))
                {
                    animation.Frames[frames[selectedFrame]] = new List<SequenceDataModel.FrameInformation>()
                    {
                        new SequenceDataModel.FrameInformation()
                    };
                }
                if (!animation.FrameMappings.ContainsKey(frameIdentifier))
                {
                    animation.FrameMappings[frameIdentifier] = frames[selectedFrame];
                }

                if (!animation.Parts.ContainsKey(startPartId))
                {
                    animation.Parts[startPartId] = new List<SequenceDataModel.PartInformation>()
                    {
                        new SequenceDataModel.PartInformation()
                    };
                }

                if (!animation.PartMappings.ContainsKey(partIdentifier))
                {
                    animation.PartMappings[partIdentifier] = startPartId;
                }
            }

            animation.Parts[startPartId] = partInformation;
        }
        
        //image background
        var w = (int)(windowSize.X / 3);
        var h = w;
        var blankBackground = _render.RenderCheckerboardRectangle(20, w, h,
            (40, 30, 40, 255), (50, 40, 50, 255)
        );
        ImGui.SetCursorPos(imagePos);
        ImGui.Image(blankBackground, new Vector2(w, h));

        var spriteTileWidth = spriteWidth / 5;
        var spriteTileHeight = spriteHeight / 5;
        var spritePos = imagePos + new Vector2(w / 2.0f - spriteTileWidth/2.0f, h / 2.0f - spriteTileHeight/2.0f);
        foreach (var partToDraw in partInformation)
        {
            var tileWidthRatio = (float)spriteTileWidth / spriteWidth;
            var tileHeightRatio = (float)spriteTileHeight / spriteHeight;
            var tilesPerRow = (int)Math.Floor((float)spriteWidth / spriteTileWidth);
            var tileX = partToDraw.FrameIndex % tilesPerRow;
            var tileY = (int)Math.Floor((float)partToDraw.FrameIndex / tilesPerRow);
            var spriteUv1 = new Vector2(tileX * tileWidthRatio, tileY * tileHeightRatio);
            var spriteUv2 = new Vector2((tileX + 1) * tileWidthRatio, (tileY + 1) * tileHeightRatio);
            if (partToDraw.HFlipped)
            {
                (spriteUv1.X, spriteUv2.X) = (spriteUv2.X, spriteUv1.X);
            }
            if (partToDraw.VFlipped)
            {
                (spriteUv1.Y, spriteUv2.Y) = (spriteUv2.Y, spriteUv1.Y);
            }
        
            //fix the position based on the direction
            var ox = 0;
            if (directions[selectedDirection] == ToucheTools.Constants.Directions.Left)
            {
                ox = -spriteTileWidth;
            }
            ImGui.SetCursorPos(spritePos + new Vector2(partToDraw.DestX + ox, partToDraw.DestY));
            ImGui.Image(spriteTexture, new Vector2(spriteTileWidth, spriteTileHeight), spriteUv1, spriteUv2);
        }

        ImGui.SetCursorPos(new Vector2(0.0f, imagePos.Y + h));
        ImGui.Separator();
        
        if (ImGui.Button("Close editor"))
        {
            _animationManagementState.EditorOpen = false;
        }
        
        ImGui.End();
    }

    private static string DisplayCharacter(int character)
    {
        return $"Character {character}";
    }

    private static string DisplayAnimation(int animation)
    {
        if (animation == 0)
        {
            return $"Idle animation";
        }

        if (animation == 1)
        {
            return $"Move animation";
        }
        return $"Animation {animation}";
    }

    private static string DisplayDirection(int direction)
    {
        return $"{Directions.DirectionName(direction)}";
    }

    private static string DisplayFrame(int frameId)
    {
        return $"Frame {frameId}";
    }

    private static string DisplayPart(int partId)
    {
        return $"Part {partId}";
    }
}