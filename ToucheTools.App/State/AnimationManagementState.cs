﻿using ToucheTools.Models;

namespace ToucheTools.App.State;

public class AnimationManagementState
{
    public int SelectedSpriteIndex { get; set; }
    public int SelectedPaletteIndex { get; set; }
    
    
    public string? SelectedAnimationPath { get; set; } = null;
    public bool PreviewOpen { get; set; } = false;
    public bool EditorOpen { get; set; } = false;
    
    
    public int SelectedCharacter { get; set; }
    public int SelectedAnimation { get; set; }
    public int SelectedDirection { get; set; }
    public int SelectedFrame { get; set; }
    public int SelectedPart { get; set; }

    public List<SequenceDataModel.PartInformation>? CopiedParts { get; set; } = null;
}