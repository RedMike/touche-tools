using ToucheTools.Models.Instructions;

namespace ToucheTools.Models;

public class ProgramDataModel
{
    public class Rect
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
    }

    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int Order { get; set; }
    }

    public class Walk
    {
        public int Point1 { get; set; }
        public int Point2 { get; set; }
        public int ClipRect { get; set; }
        public int Area1 { get; set; }
        public int Area2 { get; set; }
    }

    public class Area
    {
        public Rect Rect { get; set; }
        public int SrcX { get; set; }
        public int SrcY { get; set; }
        public int Id { get; set; }
        public int State { get; set; }
        public int AnimationCount { get; set; }
        public int AnimationNext { get; set; }
    }

    public class Background
    {
        public Rect Rect { get; set; }
        public int SrcX { get; set; }
        public int SrcY { get; set; }
        public int Type { get; set; }
        public int Offset { get; set; }
        public int ScaleMul { get; set; }
        public int ScaleDiv { get; set; }
    }

    public class Hitbox
    {
        public int Item { get; set; }
        public int Talk { get; set; }
        public int State { get; set; }
        public int String { get; set; }
        public int DefaultString { get; set; }
        public int[] Actions { get; set; } //always length 8
        public Rect Rect1 { get; set; }
        public Rect Rect2 { get; set; }
    }

    public class ActionScriptOffset
    {
        public int Object1 { get; set; }
        public int Action { get; set; }
        public int Object2 { get; set; }
        public int Offset { get; set; }
    }

    public class Conversation
    {
        public int Num { get; set; }
        public int Offset { get; set; }
        public int Message { get; set; }
    }

    public class CharScriptOffset
    {
        public int Character { get; set; }
        public int Offs { get; set; }
    }

    public enum Opcode
    {
        Noop = 0,
        Jnz = 1,
        Jz = 2,
        Jmp = 3,
        True = 4,
        False = 5,
        Push = 6,
        Not = 7,
        Add = 8,
        Sub = 9,
        Mul = 10,
        Div = 11,
        Mod = 12,
        And = 13,
        Or = 14,
        Neg = 15,
        TestGreater = 16,
        TestEquals = 17,
        TestLower = 18,
        FetchScriptWord = 19,
        
        TestGreaterOrEquals = 24,
        TestLowerOrEquals = 25,
        TestNotEquals = 26,
        EndConversation = 27,
        StopScript = 28,
        GetFlag = 29,
        SetFlag = 30,
        
        FetchScriptByte = 35,
        
        GetCharWalkBox = 46,
        StartSound = 47,
        MoveCharToPos = 48,
        
        LoadRoom = 52,
        UpdateRoom = 53,
        StartTalk = 54,
        SetCharBox = 55,
        InitCharScript = 56,
        LoadSprite = 57,
        LoadSequence = 58,
        SetCharFrame = 59,
        SetCharDirection = 60,
        ClearConversationChoices = 61,
        AddConversationChoice = 62,
        RemoveConversationChoice = 63,
        GetInventoryItem = 64,
        SetInventoryItem = 65,
        StartEpisode = 66,
        SetConversation = 67,
        
        EnableInput = 69,
        DisableInput = 70,
        FaceChar = 71,
        GetCharCurrentAnim = 72,
        GetCurrentChar = 73,
        IsCharActive = 74,
        SetPalette = 75,
        ChangeWalkPath = 76,
        LockWalkPath = 77,
        InitChar = 78,
        SetupWaitingChar = 79,
        UpdateRoomAreas = 80,
        UnlockWalkPath = 81,
        
        AddItemToInventoryAndRedraw = 83,
        GiveItemTo = 84,
        SetHitboxText = 85,
        FadePalette = 86,
        
        GetInventoryItemFlags = 97,
        DrawInventory = 98,
        StopCharScript = 99,
        RestartCharScript = 100,
        GetCharCurrentWalkBox = 101,
        GetCharPointsDataNum = 102,
        SetupFollowingChar = 103,
        StartAnimation = 104,
        SetCharTextColor = 105,
        
        StartMusic = 112,
        
        Sleep = 114,
        
        SetCharDelay = 116,
        LockHitbox = 117,
        RemoveItemFromInventory = 118,
        UnlockHitbox = 119,
        AddRoomArea = 120,
        SetCharFlags = 121,
        
        UnsetCharFlags = 128,
        DrawSpriteOnBackdrop = 129,
        LoadSpeechSegment = 130,
        
        StartPaletteFadeIn = 132,
        StartPaletteFadeOut = 133,
        SetRoomAreaState = 134
    }
    
    public List<Rect> Rects { get; set; } = new List<Rect>();
    public List<Point> Points { get; set; } = new List<Point>();
    public List<Walk> Walks { get; set; } = new List<Walk>();
    public List<Area> Areas { get; set; } = new List<Area>();
    public List<Background> Backgrounds { get; set; } = new List<Background>();
    public List<Hitbox> Hitboxes { get; set; } = new List<Hitbox>();
    public List<ActionScriptOffset> ActionScriptOffsets { get; set; } = new List<ActionScriptOffset>();
    public List<Conversation> Conversations { get; set; } = new List<Conversation>();
    public List<CharScriptOffset> CharScriptOffsets { get; set; } = new List<CharScriptOffset>();

    public List<BaseInstruction> Instructions { get; set; } = new List<BaseInstruction>();

    public Dictionary<int, string> Strings { get; set; } = new Dictionary<int, string>();
    
    public int OriginalSize { get; set; }
}