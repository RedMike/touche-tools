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
        ImGui.SetNextItemWidth(windowSize.X);
        ImGui.Combo("", ref selectedSprite, spriteList, spriteList.Length);
        if (selectedSprite != origSelectedSprite)
        {
            _animationManagementState.SelectedSpriteIndex = sprites[selectedSprite].Index;
        }
        ImGui.PopID();
        
        ImGui.Separator();
        //set up sprite image
        var (spriteWidth, spriteHeight, spriteBytes) = _images.GetImage(sprites[selectedSprite].Key);
        var spriteProcessedBytes = new byte[spriteHeight * spriteWidth * 4];
        var spriteTileWidth = spriteWidth;
        var spriteTileHeight = spriteHeight;
        for (var x = 0; x < spriteWidth; x++)
        {
            var r = spriteBytes[x * 4 + 0];
            var g = spriteBytes[x * 4 + 1];
            var b = spriteBytes[x * 4 + 2];
            var a = spriteBytes[x * 4 + 3];
            if (r == 255 && g == 0 && b == 255 && a == 255)
            {
                spriteTileWidth = x;
                break;
            }
        }
        for (var y = 0; y < spriteHeight; y++)
        {
            var r = spriteBytes[(y * spriteWidth) * 4 + 0];
            var g = spriteBytes[(y * spriteWidth) * 4 + 1];
            var b = spriteBytes[(y * spriteWidth) * 4 + 2];
            var a = spriteBytes[(y * spriteWidth) * 4 + 3];
            if (r == 255 && g == 0 && b == 255 && a == 255)
            {
                spriteTileHeight = y;
                break;
            }
        }

        for (var x = 0; x < spriteWidth; x++)
        {
            for (var y = 0; y < spriteHeight; y++)
            {
                var r = spriteBytes[(y * spriteWidth + x) * 4 + 0];
                var g = spriteBytes[(y * spriteWidth + x) * 4 + 1];
                var b = spriteBytes[(y * spriteWidth + x) * 4 + 2];
                var a = spriteBytes[(y * spriteWidth + x) * 4 + 3];
                if (a < 255)
                {
                    r = 0;
                    g = 0;
                    b = 0;
                    a = 0;
                }

                if (r == 255 && g == 0 && b == 255 && a == 255)
                {
                    r = 0;
                    g = 0;
                    b = 0;
                    a = 0;
                }

                spriteProcessedBytes[(y * spriteWidth + x) * 4 + 0] = r;
                spriteProcessedBytes[(y * spriteWidth + x) * 4 + 1] = g;
                spriteProcessedBytes[(y * spriteWidth + x) * 4 + 2] = b;
                spriteProcessedBytes[(y * spriteWidth + x) * 4 + 3] = a;
            }
        }
        var spriteTexture = _render.RenderImage(RenderWindow.RenderType.Sprite, sprites[selectedSprite].Key, spriteWidth, spriteHeight, spriteProcessedBytes);
        
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

        var frameInformation = new List<SequenceDataModel.FrameInformation>()
        {
        };
        if (animation.Frames.ContainsKey(startFrameId))
        {
            frameInformation = animation.Frames[startFrameId];
        }
        else
        {
            isNew = true;
        }
        var selectedFrameInformation = new SequenceDataModel.FrameInformation();
        if (frameInformation.Count > frames[selectedFrame])
        {
            selectedFrameInformation = frameInformation[frames[selectedFrame]];
        }
        else
        {
            frameInformation.Add(selectedFrameInformation);
            isNew = true;
        }
        
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
        
        //frame controls
        ImGui.Text("");
        ImGui.PushID("AnimationFrameDx");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        var origWalkDx = selectedFrameInformation.WalkDx;
        var walkDx = origWalkDx;
        ImGui.SliderInt("", ref walkDx, -20, 20, $"Walk DX {walkDx}");
        if (walkDx != origWalkDx)
        {
            selectedFrameInformation.WalkDx = walkDx;
            frameChanged = true;
        }
        ImGui.PopID();
        //TODO: should Y even be included if it's not really used in the game?
        ImGui.PushID("AnimationFrameDz");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        var origWalkDz = selectedFrameInformation.WalkDz;
        var walkDz = origWalkDz;
        ImGui.SliderInt("", ref walkDz, -20, 20, $"Walk DZ {walkDz}");
        if (walkDz != origWalkDz)
        {
            selectedFrameInformation.WalkDz = walkDz;
            frameChanged = true;
        }
        ImGui.PopID();
        ImGui.PushID("AnimationFrameDelay");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        var origDelay = selectedFrameInformation.Delay;
        var delay = origDelay;
        ImGui.SliderInt("", ref delay, 0, 20, $"Delay {delay}");
        if (delay != origDelay)
        {
            selectedFrameInformation.Delay = delay;
            frameChanged = true;
        }
        ImGui.PopID();
        
        //part controls
        ImGui.Text("");
        //TODO: select part from sprite sheet
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        ImGui.PushID("AnimationPartIndex");
        var partIndexes = Enumerable.Range(0, (spriteWidth / spriteTileWidth) * (spriteHeight / spriteTileHeight)).ToList();
        var partIndexList = partIndexes.Select(p => $"{DisplayPartIndex(p, spriteWidth, spriteTileWidth)}").ToArray();
        var origSelectedPartIndex = (int)selectedPartInformation.FrameIndex;
        var selectedPartIndex = origSelectedPartIndex;
        ImGui.Combo("", ref selectedPartIndex, partIndexList, partIndexList.Length);
        if (selectedPartIndex != origSelectedPartIndex)
        {
            var wasHFlipped = selectedPartInformation.HFlipped;
            var wasVFlipped = selectedPartInformation.VFlipped;
            selectedPartInformation.RawFrameIndex = (short)partIndexes[selectedPartIndex];
            if (wasVFlipped)
            {
                selectedPartInformation.RawFrameIndex = (short)(selectedPartInformation.RawFrameIndex | 0x4000);
            }
            if (wasHFlipped)
            {
                selectedPartInformation.RawFrameIndex = (short)(selectedPartInformation.RawFrameIndex | 0x8000);
            }
            frameChanged = true;
        }
        ImGui.PopID();
        
        ImGui.PushID("AnimationPartDestX");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        var origDestX = selectedPartInformation.DestX;
        var destX = origDestX;
        ImGui.SliderInt("", ref destX, -100, 100, $"Dest X {destX}");
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
        ImGui.SliderInt("", ref destY, -100, 100, $"Dest Y {destY}");
        if (destY != origDestY)
        {
            selectedPartInformation.DestY = destY;
            frameChanged = true;
        }
        ImGui.PopID();
        
        ImGui.PushID("AnimationPartHFlip");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        var origHFlipped = selectedPartInformation.HFlipped;
        var hFlipped = origHFlipped;
        ImGui.Checkbox("Horizontal Flip", ref hFlipped);
        if (hFlipped != origHFlipped)
        {
            if (hFlipped)
            {
                selectedPartInformation.RawFrameIndex = (short)(selectedPartInformation.RawFrameIndex | 0x8000);
            }
            else
            {
                selectedPartInformation.RawFrameIndex = (short)(selectedPartInformation.RawFrameIndex & ~0x8000);
            }
            frameChanged = true;
        }
        ImGui.PopID();
        
        ImGui.PushID("AnimationPartVFlip");
        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        var origVFlipped = selectedPartInformation.VFlipped;
        var vFlipped = origVFlipped;
        ImGui.Checkbox("Vertical Flip", ref vFlipped);
        if (vFlipped != origVFlipped)
        {
            if (vFlipped)
            {
                selectedPartInformation.RawFrameIndex = (short)(selectedPartInformation.RawFrameIndex | 0x4000);
            }
            else
            {
                selectedPartInformation.RawFrameIndex = (short)(selectedPartInformation.RawFrameIndex & ~0x4000);
            }
            frameChanged = true;
        }
        ImGui.PopID();

        ImGui.SetNextItemWidth(windowSize.X/3.0f);
        if (ImGui.Button("Copy Frame"))
        {
            _animationManagementState.CopiedParts = partInformation.Select(p => new SequenceDataModel.PartInformation()
            {
                DestX = p.DestX,
                DestY = p.DestY,
                RawFrameIndex = p.RawFrameIndex
            }).ToList();
        }

        var hasCopied = _animationManagementState.CopiedParts != null;
        if (hasCopied)
        {
            ImGui.SetNextItemWidth(windowSize.X / 3.0f);
            if (ImGui.Button("Paste Frame"))
            {
                partInformation = _animationManagementState.CopiedParts;
                _animationManagementState.CopiedParts = null;
                frameChanged = true;
            }
        }

        if (frameChanged)
        {
            if (isNew)
            {
                //create any missing bits of data
                if (!animation.CharToFrameFlag.ContainsKey(characters[selectedCharacter]))
                {
                    animation.CharToFrameFlag[characters[selectedCharacter]] = (ushort)0;
                }

                if (!animation.Frames.ContainsKey(startFrameId))
                {
                    animation.Frames[startFrameId] = new List<SequenceDataModel.FrameInformation>()
                    {
                        new SequenceDataModel.FrameInformation()
                    };
                }
                if (!animation.FrameMappings.ContainsKey(frameIdentifier))
                {
                    animation.FrameMappings[frameIdentifier] = startFrameId;
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
            animation.Frames[startFrameId] = frameInformation;
        }
        
        //image background
        var w = (int)(windowSize.X / 3);
        var h = w;
        var blankBackground = _render.RenderCheckerboardRectangle(20, w, h,
            (40, 30, 40, 255), (50, 40, 50, 255)
        );
        ImGui.SetCursorPos(imagePos);
        ImGui.Image(blankBackground, new Vector2(w, h));

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
        
        if (ImGui.Button("Save"))
        {
            _animations.SaveAnimation(_animationManagementState.SelectedAnimationPath);
        }
        
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

    private static string DisplayPartIndex(int partIndex, int spriteWidth, int spriteTileWidth)
    {
        var cols = (int)Math.Ceiling((float)spriteWidth / spriteTileWidth);
        return $"Sprite part {partIndex} ({(partIndex % cols)}, {(partIndex / cols)})";
    }
}