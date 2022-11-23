using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;
using ToucheTools.Constants;
using ToucheTools.Models;
using ToucheTools.Models.Instructions;

namespace ToucheTools.App.State;

public class ActiveProgramState
{
    public class InventoryList
    {
        public short DisplayOffset { get; set; } = 0;
        public short LastItem { get; set; } = 0;
        public short ItemsPerLine { get; set; } = 0;
        public List<short> Items { get; set; } = new List<short>();

        public List<short> GetActualItems()
        {
            return Items.Where(i => i > 0).ToList();
        }

        public void RemoveItem(int index)
        {
            Items.RemoveAt(index);
            Items.Add(0);
        }

        public void PrependItem(short item)
        {
            if (Items.All(i => i != 0))
            {
                throw new Exception("No space in inventory");
            }
            Items.Insert(0, item);
            Items.RemoveAt(Items.Count - 1);
        }
    }
    
    public class KeyChar
    {
        public bool Initialised { get; set; } = false;
        public bool IsSelectable { get; set; } = false;
        public bool OffScreen { get; set; } = false;
        public bool IsFollowing { get; set; } = false;
        public uint TextColour { get; set; }
        
        #region Graphics
        public int? SpriteIndex { get; set; }
        public int? SequenceIndex { get; set; }
        public int? Character { get; set; } //within sequence
        #endregion
        
        #region Animation
        public int CurrentDirection { get; set; } //into sequence (direction)
        public int CurrentAnim { get; set; } //into sequence (animation)
        public int CurrentAnimSpeed { get; set; }
        public int CurrentAnimCounter { get; set; } //into sequence (frame)
        
        public int Anim1Start { get; set; }
        public int Anim1Count { get; set; }
        public int Anim2Start { get; set; }
        public int Anim2Count { get; set; }
        public int Anim3Start { get; set; }
        public int Anim3Count { get; set; }
        public List<int> QueuedAnimations { get; set; }
        #endregion
        
        #region Position
        public ushort? LastWalk { get; set; }
        public ushort? LastPoint { get; set; }
        public ushort? TargetPoint { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int PositionZ { get; set; }
        #endregion
        
        #region Items
        public short Money { get; set; }
        public short[] CountedInventoryItems { get; set; } = new short[4];
        #endregion
        
        #region Waiting
        public int? WaitForKeyChar { get; set; } = null;
        public int? WaitForAnimationId { get; set; } = null;
        public int? WaitForPoint { get; set; } = null;
        public int? WaitForWalk { get; set; } = null;
        #endregion
        
        public void OnProgramChange()
        {
            Initialised = false;
            TextColour = 253;
            CurrentDirection = 0;
            CurrentAnim = 0;
            CurrentAnimSpeed = 0;
            CurrentAnimCounter = 0;
            Anim1Count = 1;
            Anim1Start = 0;
            Anim2Count = 1;
            Anim2Start = 0;
            Anim3Count = 1;
            Anim3Start = 0;
            QueuedAnimations = new List<int>();
            LastPoint = null;
            IsFollowing = false;
            IsSelectable = false;
            OffScreen = false;
            PositionX = 10; //from game code
            LastPoint = null;
            LastWalk = null;
            TargetPoint = null;
            //intentional that no position y/z update, from game code
            SpriteIndex = null;
            SequenceIndex = null;
            Character = null;
            WaitForKeyChar = null;
            WaitForPoint = null;
            WaitForWalk = null;
            WaitForAnimationId = null;
        }
    }
    
    public class ProgramState
    {
        #region Scripts
        public enum ScriptType
        {
            Unknown = 0,
            KeyChar = 1, //includes initial main loop
            Action = 2,
            Conversation = 3,
        }

        public enum ScriptStatus
        {
            Unknown = 0,
            NotInit = 1,
            Ready = 2,
            Running = 3,
            Paused = 4,
            Stopped = 5,
        }
        
        public class Script
        {
            public int Id { get; set; }
            public int StartOffset { get; set; }
            public int Offset { get; set; }
            public ScriptStatus Status { get; set; }
            public int Delay { get; set; }
        }

        public List<Script> Scripts { get; set; } = new List<Script>();
        public List<Script> ScriptsForCurrentTick { get; set; } = new List<Script>();
        public int TickCounter { get; set; }

        public Script? GetKeyCharScript(int keyCharId)
        {
            return Scripts.FirstOrDefault(s => s.Id == keyCharId);
        }
        
        public Script? GetRunningScript()
        {
            return Scripts.SingleOrDefault(s => s.Status == ScriptStatus.Running);
        }

        public bool AreScriptsRemainingInCurrentTick()
        {
            return ScriptsForCurrentTick.Count(s => s.Status != ScriptStatus.Stopped && 
                                                    s.Status != ScriptStatus.NotInit) != 0;
        }

        public Script? GetNextScript()
        {
            return ScriptsForCurrentTick.FirstOrDefault(s => s.Status == ScriptStatus.Ready || s.Status == ScriptStatus.Paused);
        }

        public void MarkScriptAsDoneInCurrentTick(Script s)
        {
            ScriptsForCurrentTick.Remove(s);
        }

        public void TickDone()
        {
            ScriptsForCurrentTick = Scripts.ToList();
            TickCounter++;
        }
        #endregion

        #region Room Areas
        public List<(int, ProgramDataModel.AreaState)> ActiveRoomAreas { get; set; } = new List<(int, ProgramDataModel.AreaState)>();
        #endregion
        
        #region Room Sprites
        public List<(int, int, int)> ActiveRoomSprites { get; set; } = new List<(int, int, int)>();
        #endregion
        
        public int CurrentProgram { get; set; } = 0;

        public int? QueuedProgram { get; set; } = null;
        
        #region STK
        public ushort StackPointer { get; private set; } = 0;

        public void MoveStackPointerForwards()
        {
            StackPointer++;
            if (StackPointer >= Stack.Length)
            {
                StackPointer = 0;
            }
        }
        public void MoveStackPointerBackwards()
        {
            StackPointer--;
            if (StackPointer >= Stack.Length)
            {
                StackPointer = (ushort)(Stack.Length - 1);
            }
        }
        private short[] Stack { get; set; } = new short[500];
        public short StackValue => Stack[StackPointer];
        public void SetStackValue(short val)
        {
            Stack[StackPointer] = val;
        }

        public Dictionary<ushort, short> GetFullStackValues()
        {
            return Stack.Select((val, idx) => (idx, val)).Where(pair => pair.val != 0 || pair.idx == StackPointer)
                .ToDictionary(pair => (ushort)pair.idx, pair => pair.val);
        }
        #endregion

        public int? LoadedRoom { get; set; } = null;

        public Dictionary<ushort, (int, int)> BackgroundOffsets { get; set; } = new Dictionary<ushort, (int, int)>();
    }

    private readonly DatabaseModel _model;
    private readonly ActiveProgram _program;
    private readonly LogData _log;
    private readonly GameViewState _viewState; //needed to get rendered sprite sizes (from parts)

    public ActiveProgramState(DatabaseModel model, ActiveProgram program, LogData log, GameViewState viewState)
    {
        _model = model;
        _program = program;
        _log = log;
        _viewState = viewState;

        OnStartup();
        
        _program.ObserveActive(Update);
        Update();
    }

    private void OnStartup()
    {
        //from game code
        #region Inventory Lists
        InventoryLists[0] = new InventoryList()
        {
            DisplayOffset = 0,
            LastItem = 100,
            ItemsPerLine = 6,
            Items = Enumerable.Repeat((short)0, 101).ToList()
        };
        InventoryLists[0].Items[100] = -1;
            
        InventoryLists[1] = new InventoryList()
        {
            DisplayOffset = 0,
            LastItem = 100,
            ItemsPerLine = 6,
            Items = Enumerable.Repeat((short)0, 101).ToList()
        };
        InventoryLists[1].Items[100] = -1;
            
        InventoryLists[2] = new InventoryList()
        {
            DisplayOffset = 0,
            LastItem = 6,
            ItemsPerLine = 6,
            Items = Enumerable.Repeat((short)0, 7).ToList()
        };
        InventoryLists[2].Items[6] = -1;
        #endregion
        
        for (var i = 0; i < 7; i++)
        {
            LoadedSprites[i] = new LoadedSprite();
        }
        
        SetPaletteScale(0, 255, 0, 0, 0);

        SetFlag(ToucheTools.Constants.Flags.Known.TalkFramesEnabled, 1);
    }

    #region Breakpoint/debug
    public List<int> Breakpoints { get; set; } = new List<int>();
    public bool BreakpointHit { get; set; } = false;
    public int LastKnownOffset { get; set; } = -1;
    #endregion
    
    public ProgramState CurrentState { get; set; } = new ProgramState();
    public int DisabledInputCounter { get; set; } = 0;
    public bool AutoPlay { get; set; } = false;
    private DateTime _lastTick = DateTime.MinValue;
    private const int MinimumTimeBetweenTicksInMillis = 50;
    
    #region Palette
    public Dictionary<int, PaletteDataModel.Rgb> LoadedPalette = new Dictionary<int, PaletteDataModel.Rgb>();
    public Dictionary<int, (int, int, int)> LoadedPaletteScale = new Dictionary<int, (int, int, int)>();
    public int PaletteRandomCounter = 0;

    public PaletteDataModel.Rgb GetLoadedColour(int col)
    {
        if (col < 0 || col > 255)
        {
            throw new Exception("Asking for impossible palette colour");
        }

        if (!LoadedPalette.ContainsKey(col))
        {
            return new PaletteDataModel.Rgb()
            {
                R = 255,
                B = 0,
                G = 255
            };
        }

        var paletteCol = LoadedPalette[col];
        var paletteScale = (0, 0, 0);
        if (LoadedPaletteScale.ContainsKey(col))
        {
            paletteScale = LoadedPaletteScale[col];
        }

        return new PaletteDataModel.Rgb()
        {
            R = (byte)(paletteCol.R * paletteScale.Item1 / 255),
            G = (byte)(paletteCol.G * paletteScale.Item2 / 255),
            B = (byte)(paletteCol.B * paletteScale.Item3 / 255),
        };
    }

    public PaletteDataModel GetLoadedPalette()
    {
        var cols = new List<PaletteDataModel.Rgb>();
        for (var i = 0; i < 256; i++)
        {
            cols.Add(GetLoadedColour(i));
        }

        return new PaletteDataModel()
        {
            Colors = cols
        };
    }
    
    public void LoadPalette(PaletteDataModel palette)
    {
        if (palette.Colors.Count < 256)
        {
            throw new Exception("Palette does not contain enough colours");
        }
        
        LoadedPalette = new Dictionary<int, PaletteDataModel.Rgb>();
        for (var i = 0; i < 256; i++)
        {
            LoadedPalette[i] = new PaletteDataModel.Rgb()
            {
                R = palette.Colors[i].R,
                G = palette.Colors[i].G,
                B = palette.Colors[i].B
            };
        }
    }

    public void SetPaletteScale(int fromIdx, int count, int rScale, int gScale, int bScale)
    {
        rScale = Math.Min(255, Math.Max(0, rScale));
        gScale = Math.Min(255, Math.Max(0, gScale));
        bScale = Math.Min(255, Math.Max(0, bScale));
        for (var i = fromIdx; i <= fromIdx + count; i++)
        {
            LoadedPaletteScale[i] = (rScale, gScale, bScale);
        }
    }
    #endregion
    
    #region Talk Entries
    public class TalkEntry
    {
        public int TalkingKeyChar { get; set; }
        public int OtherKeyChar { get; set; }
        public int Num { get; set; }
        public string Text { get; set; } = "";
        public int Counter { get; set; }
    }

    public List<TalkEntry> TalkEntries { get; set; } = new List<TalkEntry>();
    #endregion
    
    #region Loaded Sprites
    public class LoadedSprite //includes sequence info
    {
        public int? SpriteNum { get; set; } = null;
        public int? SequenceNum { get; set; } = null;
    }
    public LoadedSprite[] LoadedSprites { get; set; } = new LoadedSprite[7];
    #endregion
    
    #region Inventory
    public static class InventoryHitboxType
    {
        public const int Character = 0;
        public const int MoneyDisplay = 1;
        public const int GoldCoins = 2;
        public const int SilverCoins = 3;
        public const int Money = 4;
        public const int Scroller1 = 5;
        public const int Object1 = 6;
        public const int Object2 = 7;
        public const int Object3 = 8;
        public const int Object4 = 9;
        public const int Object5 = 10;
        public const int Object6 = 11;
        public const int Scroller2 = 12;
        
    }

    public static readonly Dictionary<int, (int, int, int, int)> InventoryHitboxes = new Dictionary<int, (int, int, int, int)>
    {
        { InventoryHitboxType.Character , (0, 354, 50, 46)},
        { InventoryHitboxType.MoneyDisplay , (66, 354, 58, 26)},
        { InventoryHitboxType.GoldCoins , (74, 380, 42, 18)},
        { InventoryHitboxType.SilverCoins , (116, 380, 42, 18)},
        { InventoryHitboxType.Money , (144, 354, 44, 26)},
        { InventoryHitboxType.Scroller1 , (202, 354, 36, 26)},
        { InventoryHitboxType.Object1 , (242, 354, 58, 26)},
        { InventoryHitboxType.Object2 , (300, 354, 58, 26)},
        { InventoryHitboxType.Object3 , (358, 354, 58, 26)},
        { InventoryHitboxType.Object4 , (416, 354, 58, 26)},
        { InventoryHitboxType.Object5 , (474, 354, 58, 26)},
        { InventoryHitboxType.Object6 , (532, 354, 58, 26)},
        { InventoryHitboxType.Scroller2 , (594, 354, 46, 25)}
    };

    public InventoryList[] InventoryLists { get; set; } = new InventoryList[3];
    public Dictionary<int, short> InventoryFlags { get; set; } = new Dictionary<int, short>();
    public short GetInventoryFlag(int item)
    {
        if (!InventoryFlags.ContainsKey(item))
        {
            return 0x20;
        }

        return InventoryFlags[item];
    }
    public short RemovedMoney { get; set; } = 0;
    public short GrabbedItem { get; set; } = 0;
    
    private void RemoveGrabbedItem()
    {
        if (GrabbedItem != 0)
        {
            if (GrabbedItem != 1)
            {
                //add item to inventory
                AddItemToInventory(CurrentKeyChar, GrabbedItem);
            }

            GrabbedItem = 0;
        }
    }

    private void RemoveItemFromInventory(int ch, short item)
    {
        if (item == 1)
        {
            //it's really about money
            RemovedMoney = 0;
        }
        else
        {
            //it's about removing an item
            if (ch >= InventoryLists.Length)
            {
                throw new Exception("Removing inventory to non-existent list");
            }
            var inventoryList = InventoryLists[ch];
            InventoryFlags.Remove(item);
            if (inventoryList.Items.Contains(item))
            {
                inventoryList.RemoveItem(inventoryList.Items.FindIndex(x => x == item));
            }
        }
    }
    
    private void AddItemToInventory(int ch, short item)
    {
        if (item == 0)
        {
            _log.Error("TODO: re-sort inventory items"); //TODO: just re-sort the inventory items
        } else if (item == 1)
        {
            //it's really about money
            RemovedMoney += GetFlag(ToucheTools.Constants.Flags.Known.CurrentMoney);
        }
        else
        {
            //it's about adding an item
            if (ch >= InventoryLists.Length)
            {
                throw new Exception("Adding inventory to non-existent list");
            }
            
            var inventoryList = InventoryLists[ch];
            InventoryFlags[item] = (short)(ch | 0x10);
            //don't re-add if it already exists
            if (inventoryList.Items.Contains(item))
            {
                return;
            }
            inventoryList.PrependItem(item);
        }
    }
    #endregion
    
    #region Action Menus
    public class ActionMenu
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; } = "";
        public List<int> Actions { get; set; } = new List<int>();
        public int Item { get; set; }
        public int FallbackAction { get; set; }
    }

    public ActionMenu? ActiveMenu { get; set; } = null;

    public void ChooseMenuOption(int action)
    {
        if (ActiveMenu == null || !ActiveMenu.Actions.Contains(action))
        {
            throw new Exception("Choosing menu option from missing menu");
        }

        var item = ActiveMenu.Item;
        if (action == 0)
        {
            action = ActiveMenu.FallbackAction;
        }
        ActiveMenu = null;
        if (action != 0)
        {
            TryTriggerAction(action, item, 0);
        }
    }
    #endregion
    
    #region Flags
    public Dictionary<ushort, short> Flags { get; set; } = new Dictionary<ushort, short>();

    private short GetFlag(ushort flag)
    {
        if (!Flags.ContainsKey(flag))
        {
            return 0;
        }
        return Flags[flag];
    }

    public short GetFlag(Flags.Known known)
    {
        return GetFlag((ushort)known);
    }

    private void SetFlag(ushort flag, short val)
    {
        Flags[flag] = val;
    }

    private void SetFlag(Flags.Known known, short val)
    {
        SetFlag((ushort)known, val);
    }
    #endregion

    #region KeyChars
    public Dictionary<int, KeyChar> KeyChars { get; set; } = new Dictionary<int, KeyChar>();
    public short CurrentKeyChar { get; set; } = 0;

    public KeyChar GetKeyChar(int id)
    {
        if (id == 256 && CurrentKeyChar != 256)
        {
            return GetKeyChar(CurrentKeyChar);
        }

        if (!KeyChars.ContainsKey(id))
        {
            throw new Exception("Unknown keychar");
        }

        return KeyChars[id];
    }
    #endregion

    #region Conversations
    public int? QueuedConversation { get; set; }
    public int? CurrentConversation { get; set; }
    public List<(int, string)> CurrentConversationChoices { get; set; }

    private void AddConversationChoice(int num)
    {
        var program = _model.Programs[_program.Active];
        if (CurrentConversation == null)
        {
            throw new Exception("Missing conversation");
        }

        var idx = program.Conversations.FindIndex(c => c.Num == CurrentConversation.Value);
        var choice = program.Conversations[idx + num];
        if (choice == null)
        {
            throw new Exception("Missing conversation choice");
        }

        var msg = GetString(choice.Message);

        if (CurrentConversationChoices.Any(c => c.Item2 == msg))
        {
            //already in list
            return;
        }

        CurrentConversationChoices.Add((num, msg));
    }

    private void RemoveConversationChoice(int num)
    {
        CurrentConversationChoices.RemoveAll(x => x.Item1 == num);
    }

    public void ChooseConversationChoice(int num)
    {
        var program = _model.Programs[_program.Active];
        if (CurrentConversation == null)
        {
            throw new Exception("Missing conversation");
        }
        RemoveConversationChoice(num);

        var idx = program.Conversations.FindIndex(c => c.Num == CurrentConversation.Value);
        var conversationChoice = program.Conversations[idx + num];
        if (conversationChoice == null)
        {
            throw new Exception("Missing conversation");
        }
        
        var script = CurrentState.GetKeyCharScript(CurrentKeyChar);
        if (script == null)
        {
            throw new Exception("Missing script");
        }

        script.Offset = conversationChoice.Offset;
        script.Status = ProgramState.ScriptStatus.Ready;
        //TODO: reset STK?
    }
    #endregion
    
    public string GetString(int num)
    {
        var program = _model.Programs[_program.Active];
        var str = "";
        if (num < 0)
        {
            if (!_model.Text?.Strings.ContainsKey(-num) ?? false)
            {
                return "";
            }
            str = _model.Text?.Strings[-num];
        }
        else
        {
            if (!program.Strings.ContainsKey(num))
            {
                return "";
            }
            str = program.Strings[num];
        }

        return str;
    }
    
    private void OnProgramChange()
    {
        Breakpoints = new List<int>();
        BreakpointHit = false;
        LastKnownOffset = -1;
        
        for (var i = 0; i < 32; i++)
        {
            if (!KeyChars.ContainsKey(i))
            {
                KeyChars[i] = new KeyChar();
            }
        }

        TalkEntries = new List<TalkEntry>();

        var program = _model.Programs[_program.Active];
        CurrentState = new ProgramState()
        {
            CurrentProgram = _program.Active,
        };

        //values set from game code
        for (ushort i = 200; i < 300; i++)
        {
            if (GetFlag(i) != 0)
            {
                SetFlag(i, 0);
            }
        }

        SetFlag(ToucheTools.Constants.Flags.Known.RndPalMinColourScale, 240);
        SetFlag(ToucheTools.Constants.Flags.Known.RndPalRandomExtraColourScale, 16);
        SetFlag(ToucheTools.Constants.Flags.Known.RndPalMinPeriod, 0);
        SetFlag(ToucheTools.Constants.Flags.Known.RndPalRandomExtraPeriod, 1);
        SetFlag(ToucheTools.Constants.Flags.Known.CurrentKeyChar, 0);

        CurrentState.Scripts.Add(new ProgramState.Script()
        {
            Id = CurrentKeyChar,
            Offset = 0,
            StartOffset = 0,
            Status = ProgramState.ScriptStatus.Running
        });
        foreach (var (keyCharId, keyChar) in KeyChars)
        {
            keyChar.OnProgramChange();
            if (keyCharId == CurrentKeyChar)
            {
                continue;
            }

            var cso = program.CharScriptOffsets.FirstOrDefault(x => x.Character == keyCharId);
            if (cso != null)
            {
                CurrentState.Scripts.Add(new ProgramState.Script()
                {
                    Id = keyCharId,
                    StartOffset = cso.Offs,
                    Offset = cso.Offs,
                    Status = ProgramState.ScriptStatus.NotInit
                });
            }
        }

        DisabledInputCounter = 0;
        CurrentState.TickDone();
    }

    private static int GetDirection(int x1, int y1, int z1, int x2, int y2, int z2) {
        int ret = -1;
        var dx = x2 - x1;
        var dy = y2 - y1;
        var dz = z2 - z1;
        if (dx == 0 && dy == 0 && dz == 0) {
            ret = -2;
        } else {
            if (Math.Abs(dx) >= Math.Abs(dz)) {
                //if x is further apart than z is
                if (Math.Abs(dx) > Math.Abs(dy)) {
                    //if x is further apart than y is
                    if (dx > 0) {
                        ret = 0;
                    } else {
                        ret = 3;
                    }
                } else {
                    //if y is further apart than x is
                    if (dy > 0) {
                        ret = 1;
                    } else {
                        ret = 2;
                    }
                }
            } else {
                //if z is further apart than x is
                if (dz != 0) {
                    //if moving in z direction at all, use z different to determine direction
                    if (dz > 0) {
                        ret = 1;
                    } else {
                        ret = 2;
                    }
                } else {
                    if (dy > 0) {
                        ret = 1;
                    } else {
                        ret = 2;
                    }
                }
            }
        }
        return ret;
    }
    
    private void OnGameTick()
    {
        var program = _model.Programs[_program.Active];

        #region Conversations
        if (QueuedConversation != null)
        {
            if (CurrentConversation != null)
            {
                throw new Exception("Queued conversation when already current");
            }

            CurrentConversation = QueuedConversation.Value;
            CurrentConversationChoices = new List<(int, string)>();
            var conversation = program.Conversations.FirstOrDefault(c => c.Num == CurrentConversation.Value);
            if (conversation == null)
            {
                throw new Exception("Unknown conversation");
            }

            var script = CurrentState.GetKeyCharScript(CurrentKeyChar); //TODO: this may not be the right approach?
            if (script == null)
            {
                throw new Exception("Missing script");
            }
            script.Offset = conversation.Offset;
            script.Status = ProgramState.ScriptStatus.Ready;
            QueuedConversation = null;
        }
        else
        {
            //if we're already in a conversation and it's ended
            if (CurrentConversation != null)
            {
                if (CurrentConversationChoices.Count == 0)
                {
                    CurrentConversation = null;
                }
            }
        }
        #endregion
        
        #region Random numbers
        var rndMod = GetFlag(ToucheTools.Constants.Flags.Known.RandomNumberMod);
        if (rndMod > 0)
        {
            var newVal = new Random().Next(0, rndMod);
            SetFlag(ToucheTools.Constants.Flags.Known.LastRandomNumber, (short)newVal);
        }
        #endregion
        
        #region Walking
        foreach (var (keyCharId, keyChar) in KeyChars)
        {
            while (true)
            {
                if (keyChar.TargetPoint != null)
                {
                    var lastX = keyChar.PositionX;
                    var lastY = keyChar.PositionY;
                    var lastZ = keyChar.PositionZ;
                    var targetPoint = program.Points[keyChar.TargetPoint.Value];
                    if (keyChar.TargetPoint == keyChar.LastPoint &&
                        targetPoint.X == lastX &&
                        targetPoint.Y == lastY &&
                        targetPoint.Z == lastZ)
                    {
                        break;
                    }

                    var nextPointId = -1;
                    if (keyChar.LastPoint != null && keyChar.TargetPoint == keyChar.LastPoint)
                    {
                        //we're moving back to our last point
                        nextPointId = keyChar.LastPoint.Value;
                    }
                    else if (keyChar.LastPoint == null)
                    {
                        //we're not at a point yet, so path directly to the nearest point
                        int nearestPointId = -1;
                        var closestDistance = int.MaxValue;
                        for (var i = 0; i < program.Points.Count; i++)
                        {
                            var point = program.Points[i];
                            var distance = Math.Abs(point.X - keyChar.PositionX) +
                                           Math.Abs(point.Y - keyChar.PositionY) +
                                           Math.Abs(point.Z - keyChar.PositionZ);
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                nearestPointId = i;
                            }
                        }

                        nextPointId = nearestPointId;
                    }
                    else
                    {
                        //we were at a point already, so path from it to the next one
                        var lastPoint = program.Points[keyChar.LastPoint.Value];
                        lastX = lastPoint.X;
                        lastY = lastPoint.Y;
                        lastZ = lastPoint.Z;
                        //use a basic breadth first search to figure out the correct path, game precalculates this and saves
                        var searchedPoints = new HashSet<int>() { keyChar.TargetPoint.Value };
                        var foundLastPoint = false;
                        var iterationCount = 0;
                        var iterations = new Dictionary<int, int>() { { keyChar.TargetPoint.Value, 0 } };
                        while (!foundLastPoint)
                        {
                            iterationCount++;
                            var walksWithSearchPoint = program.Walks.Where(w =>
                                    (searchedPoints.Contains(w.Point1) || searchedPoints.Contains(w.Point2))
                                    && !(searchedPoints.Contains(w.Point1) &&
                                         searchedPoints.Contains(w.Point2)) //remove cycles
                            ).ToList();
                            if (walksWithSearchPoint.Count == 0)
                            {
                                break;
                            }

                            foreach (var walk in walksWithSearchPoint)
                            {
                                var p = walk.Point1;
                                if (searchedPoints.Contains(p))
                                {
                                    p = walk.Point2;
                                }

                                if (!iterations.ContainsKey(p))
                                {
                                    iterations[p] = iterationCount;
                                }

                                searchedPoints.Add(p);
                            }

                            foundLastPoint = walksWithSearchPoint.Any(w =>
                                w.Point1 == keyChar.LastPoint.Value ||
                                w.Point2 == keyChar.LastPoint.Value
                            );
                        }

                        var curWalk = program.Walks.Where(w =>
                            w.Point1 == keyChar.LastPoint.Value ||
                            w.Point2 == keyChar.LastPoint.Value
                        ).MinBy(w =>
                            iterations.GetValueOrDefault(w.Point1, 999) +
                            iterations.GetValueOrDefault(w.Point2, 999) -
                            iterations.GetValueOrDefault(keyChar.LastPoint.Value, 999));
                        if (curWalk == null)
                        {
                            throw new Exception("No possible path");
                        }

                        keyChar.LastWalk = (ushort)program.Walks.FindIndex(w => w == curWalk);

                        nextPointId = curWalk.Point1;
                        if (curWalk.Point1 == keyChar.LastPoint)
                        {
                            nextPointId = curWalk.Point2;
                        }
                    }

                    if (nextPointId == -1)
                    {
                        throw new Exception("No next point");
                    }

                    var nextPoint = program.Points[nextPointId];

                    var x = keyChar.PositionX;
                    var y = keyChar.PositionY;
                    var z = keyChar.PositionZ;
                    if (z < Game.ZDepthMin)
                    {
                        z = Game.ZDepthMin;
                    }

                    if (z > Game.ZDepthMax)
                    {
                        z = Game.ZDepthMax;
                    }

                    //var zFactor = Game.GetZFactor(z); //weirdly the walk calculations use a simplified z factor

                    var tx = nextPoint.X;
                    var ty = nextPoint.Y;
                    var tz = nextPoint.Z;
                    
                    if (keyChar.SequenceIndex == null)
                    {
                        throw new Exception("Missing sequence");
                    }

                    var sequenceNum = LoadedSprites[keyChar.SequenceIndex.Value].SequenceNum;
                    if (sequenceNum == null)
                    {
                        throw new Exception("Missing sequence num");
                    }

                    if (keyChar.Character == null)
                    {
                        throw new Exception("Missing character");
                    }
                    
                    var frames = _model.Sequences[sequenceNum.Value]
                        .Characters[keyChar.Character.Value]
                        .Animations[keyChar.CurrentAnim]
                        .Directions[keyChar.CurrentDirection]
                        .Frames;

                    var frame = frames[keyChar.CurrentAnimCounter];

                    int dx = 0;
                    if (frame.WalkDx != 0)
                    {
                        dx = -frame.WalkDx * z / 160;
                        if (keyChar.CurrentDirection == 3)
                        {
                            dx = -dx;
                        }

                        if (dx == 0)
                        {
                            dx = 1;
                        }
                        if (dx < 0)
                        {
                            dx = Math.Min(dx, -1); 
                        }
                        if (dx > 0)
                        {
                            dx = Math.Max(dx, 1);
                        }
                    }

                    int dy = 0;
                    if (frame.WalkDy != 0)
                    {
                        dy = -frame.WalkDy * z / 160;
                        if (dy == 0)
                        {
                            dy = 1;
                        }
                        if (dy < 0)
                        {
                            dy = Math.Min(dy, -1);
                        }
                        if (dy > 0)
                        {
                            dy = Math.Max(dy, 1);
                        }
                    }

                    int dz = 0;
                    if (frame.WalkDz != 0)
                    {
                        dz = -frame.WalkDz * z / 160;
                        if (dz == 0)
                        {
                            dz = 1;
                        }
                        if (dz < 0)
                        {
                            dz = Math.Min(dz, -1);
                        }
                        if (dz > 0)
                        {
                            dz = Math.Max(dz, 1);
                        }
                    }

                    if (keyChar.CurrentAnim > 1)
                    {
                        //not a walk animation, so just cancel out the dy/dz movement
                        if (Math.Abs(tx - keyChar.PositionX) < Math.Abs(dx))
                        {
                            dx = tx - keyChar.PositionX; //exact movement
                        }

                        keyChar.PositionX = (int)(x - dx);
                        if (keyChar.PositionX == nextPoint.X && keyChar.PositionY == nextPoint.Y &&
                            keyChar.PositionZ == nextPoint.Z)
                        {
                            if (keyChar.CurrentAnim == 1)
                            {
                                keyChar.CurrentAnimCounter = 0;
                                keyChar.CurrentAnimSpeed = 0;
                                var rnd = new Random();
                                keyChar.CurrentAnim = keyChar.Anim2Start + rnd.Next(0, keyChar.Anim2Count);
                            }
                            keyChar.LastPoint = (ushort)nextPointId;
                            continue;
                        }

                        break;
                    }

                    var direction = GetDirection(x, y, z, tx, ty, tz);
                    if (direction < 0)
                    {
                        direction = keyChar.CurrentDirection;
                    }

                    if (direction != keyChar.CurrentDirection)
                    {
                        keyChar.CurrentAnimCounter = 0;
                        keyChar.CurrentDirection = direction;
                        //don't want to process the movement in this frame
                        continue;
                    }

                    if (keyChar.CurrentAnim < 1)
                    {
                        keyChar.CurrentAnimCounter = 0;
                        keyChar.CurrentAnim = 1;
                        if (dx == 0 && dy == 0 && dz == 0)
                        {
                            continue;
                        }
                    }

                    var ddx = nextPoint.X - lastX;
                    var ddy = nextPoint.Y - lastY;
                    var ddz = nextPoint.Z - lastZ;

                    if (keyChar.CurrentDirection == 0 || keyChar.CurrentDirection == 3)
                    {
                        //x movement only
                        if (Math.Abs(tx - keyChar.PositionX) < Math.Abs(dx))
                        {
                            keyChar.PositionX = tx;
                        }
                        else
                        {
                            keyChar.PositionX = (int)(keyChar.PositionX - dx);
                            //if the path moves along x at all, then
                            if (ddx != 0)
                            {
                                //adjust y/z based on x change
                                keyChar.PositionY = ddy * (keyChar.PositionX - lastX) / ddx + lastY;
                                keyChar.PositionZ = ddz * (keyChar.PositionX - lastX) / ddx + lastZ;
                            }
                        }
                    }
                    else
                    {
                        //y/z movement
                        if (nextPoint.Z != keyChar.PositionZ)
                        {
                            //first move along z, 
                            if (Math.Abs(tz - z) < Math.Abs(dz))
                            {
                                keyChar.PositionZ = tz;
                            }
                            else
                            {
                                keyChar.PositionZ = (int)(keyChar.PositionZ - dz);
                                //if the path moves along z at all, then
                                if (ddz != 0)
                                {
                                    //adjust x/y based on z change
                                    keyChar.PositionX = ddx * (keyChar.PositionZ - lastZ) / ddz + lastX;
                                    keyChar.PositionY = ddy * (keyChar.PositionZ - lastZ) / ddz + lastY;
                                }
                            }
                        }
                        else
                        {
                            //then move along y
                            if (Math.Abs(ty - y) < Math.Abs(dz))
                            {
                                keyChar.PositionY = ty;
                            }
                            else
                            {
                                keyChar.PositionY =
                                    (int)(keyChar.PositionY -
                                          dz); //intentional that it uses dz! so y isn't actually used
                                //if the path moves along y at all, then
                                if (ddy != 0)
                                {
                                    //adjust x/z based on y change
                                    keyChar.PositionX = ddx * (keyChar.PositionY - lastY) / ddy + lastX;
                                    keyChar.PositionZ = ddz * (keyChar.PositionY - lastY) / ddy + lastZ;
                                }
                            }
                        }
                    }

                    if (x == keyChar.PositionX && y == keyChar.PositionY && z == keyChar.PositionZ)
                    {
                        throw new Exception("No movement");
                    }
                    
                    //TODO: when moving from an intermediate point to the next one there's a slight hitch

                    if (keyChar.PositionX == nextPoint.X && keyChar.PositionY == nextPoint.Y &&
                        keyChar.PositionZ == nextPoint.Z)
                    {
                        if (keyChar.CurrentAnim == 1)
                        {
                            keyChar.CurrentAnimCounter = 0;
                            keyChar.CurrentAnimSpeed = 0;
                            var rnd = new Random();
                            keyChar.CurrentAnim = keyChar.Anim2Start + rnd.Next(0, keyChar.Anim2Count);
                        }

                        keyChar.LastPoint = (ushort)nextPointId;
                        continue;
                    }
                }

                break;
            }
        }
        #endregion
        
        #region Delay
        foreach (var script in CurrentState.Scripts)
        {
            if (script.Status != ProgramState.ScriptStatus.NotInit &&
                script.Status != ProgramState.ScriptStatus.Stopped)
            {
                if (script.Delay > 0)
                {
                    script.Status = ProgramState.ScriptStatus.Paused;
                    script.Delay--;
                    if (script.Status == ProgramState.ScriptStatus.Paused && script.Delay == 0)
                    {
                        script.Status = ProgramState.ScriptStatus.Ready;
                    }
                }
            }

            if (script.Status == ProgramState.ScriptStatus.Paused && script.Delay == 0)
            {
                //no delay was set, so maybe keychar waiting
                var keyChar = GetKeyChar(script.Id);
                if (keyChar.WaitForKeyChar != null)
                {
                    var otherKeyChar = GetKeyChar(keyChar.WaitForKeyChar.Value);
                    if ((
                            keyChar.WaitForAnimationId != null &&
                            otherKeyChar.CurrentAnim == keyChar.WaitForAnimationId.Value
                        ) ||
                        (
                            keyChar.WaitForPoint != null &&
                            otherKeyChar.LastPoint == keyChar.WaitForPoint.Value
                        ) ||
                        (
                            keyChar.WaitForWalk != null &&
                            otherKeyChar.LastWalk == keyChar.WaitForWalk.Value
                        )
                       )
                    {
                        keyChar.WaitForKeyChar = null;
                        script.Status = ProgramState.ScriptStatus.Ready;
                    }
                }
                else
                {
                    //expecting talk to remove it
                    if (script.Id != CurrentKeyChar && TalkEntries.All(t => t.OtherKeyChar != script.Id))
                    {
                        throw new Exception("Pause waiting for talk with no talk scripts");
                    }
                }
            }
        }

        #endregion

        #region Talk Entries

        if (TalkEntries.Count > 0)
        {
            var lastTalkEntry = TalkEntries.First();
            lastTalkEntry.Counter--;
            if (lastTalkEntry.Counter <= 0)
            {
                if (lastTalkEntry.OtherKeyChar != -1)
                {
                    var script = CurrentState.GetKeyCharScript(lastTalkEntry.OtherKeyChar);
                    if (script != null)
                    {
                        if (script.Status == ProgramState.ScriptStatus.Paused)
                        {
                            script.Status = ProgramState.ScriptStatus.Ready;
                        }
                    }
                }

                var keyChar = GetKeyChar(lastTalkEntry.TalkingKeyChar);
                if (keyChar.CurrentAnim >= keyChar.Anim1Start &&
                    keyChar.CurrentAnim < keyChar.Anim1Start + keyChar.Anim1Count)
                {
                    keyChar.CurrentAnim = keyChar.Anim2Start;
                    keyChar.CurrentAnimCounter = 0;
                    keyChar.CurrentAnimSpeed = 0;
                }

                TalkEntries.RemoveAt(0);
            }
            else
            {
                SetFlag(ToucheTools.Constants.Flags.Known.GameCycleCounter3, 0);
            }
        }

        #endregion

        #region Palettes
        if (GetFlag(ToucheTools.Constants.Flags.Known.ProcessRandomPalette) != 0)
        {
            if (PaletteRandomCounter != 0)
            {
                PaletteRandomCounter--;
            }
            else
            {
                var rnd = new Random();
                var minScale = GetFlag(ToucheTools.Constants.Flags.Known.RndPalMinColourScale);
                var rndScale = GetFlag(ToucheTools.Constants.Flags.Known.RndPalRandomExtraColourScale);
                var minPeriod = GetFlag(ToucheTools.Constants.Flags.Known.RndPalMinPeriod);
                var rndPeriod = GetFlag(ToucheTools.Constants.Flags.Known.RndPalRandomExtraPeriod);
                var scale = minScale + rnd.Next(0, rndScale);
                var period = minPeriod + rnd.Next(0, rndPeriod);
                SetPaletteScale(0, 240, scale, scale, scale);
                PaletteRandomCounter = period;
            }
        }
        #endregion
        
        #region Counters

        SetFlag(ToucheTools.Constants.Flags.Known.GameCycleCounter1,
            (short)(GetFlag(ToucheTools.Constants.Flags.Known.GameCycleCounter1) + 1));
        SetFlag(ToucheTools.Constants.Flags.Known.GameCycleCounter2,
            (short)(GetFlag(ToucheTools.Constants.Flags.Known.GameCycleCounter2) + 1));
        SetFlag(ToucheTools.Constants.Flags.Known.GameCycleCounter3,
            (short)(GetFlag(ToucheTools.Constants.Flags.Known.GameCycleCounter3) + 1));
        var f1 = GetFlag(ToucheTools.Constants.Flags.Known.GameCycleCounterDec1);
        if (f1 > 0)
        {
            SetFlag(ToucheTools.Constants.Flags.Known.GameCycleCounterDec1, (short)(f1 - 1));
        }

        var f2 = GetFlag(ToucheTools.Constants.Flags.Known.GameCycleCounterDec2);
        if (f2 > 0)
        {
            SetFlag(ToucheTools.Constants.Flags.Known.GameCycleCounterDec2, (short)(f2 - 1));
        }

        #endregion
    }
    
    private void OnGraphicalUpdate()
    {
        #region Fade palette update
        var increment = GetFlag(ToucheTools.Constants.Flags.Known.FadePaletteScaleIncrement);
        if (increment != 0)
        {
            var start = GetFlag(ToucheTools.Constants.Flags.Known.FadePaletteFirstColour);
            var count = GetFlag(ToucheTools.Constants.Flags.Known.FadePaletteColourCount);
            var scale = GetFlag(ToucheTools.Constants.Flags.Known.FadePaletteScale);
            SetPaletteScale(start, count, scale, scale, scale);

            if (increment > 0)
            {
                if (scale >= GetFlag(ToucheTools.Constants.Flags.Known.FadePaletteMaxScale))
                {
                    SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteScaleIncrement, 0);
                }
            }
            else
            {
                if (scale <= GetFlag(ToucheTools.Constants.Flags.Known.FadePaletteMinScale))
                {
                    SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteScaleIncrement, 0);
                }
            }

            scale += increment;
            if (scale < 0)
            {
                scale = 0;
            } else if (scale > 255)
            {
                scale = 255;
            }
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteScale, scale);
        }
        #endregion
        
        #region Room Scrolling
        if (GetFlag(ToucheTools.Constants.Flags.Known.DisableRoomScroll) == 0 && CurrentState.LoadedRoom != null)
        {
            //center to current keychar
            var roomImageNum = _model.Rooms[CurrentState.LoadedRoom.Value].RoomImageNum;
            var roomImage = _model.RoomImages[roomImageNum].Value;
            var roomImageWidth = roomImage.RoomWidth;
            var roomImageHeight = roomImage.Height;
            
            var keyChar = GetKeyChar(CurrentKeyChar);
            var (x, y) = (keyChar.PositionX, keyChar.PositionY);

            //center to keychar
            var fx = x - Constants.GameScreenWidth / 2;
            var fy = y - Constants.GameScreenHeight / 2;
            if (fy < 0)
            {
                fy = 0;
            }
            if (fy > roomImageHeight - Constants.RoomHeight)
            {
                fy = roomImageHeight - Constants.RoomHeight;
            }
            SetFlag(ToucheTools.Constants.Flags.Known.RoomScrollX, (short)fx);
            SetFlag(ToucheTools.Constants.Flags.Known.RoomScrollY, (short)fy);

            //scroll room y
            fy = y + 32 - Constants.GameScreenHeight / 2;
            var roomHeight = Constants.RoomHeight;
            if (GetFlag(ToucheTools.Constants.Flags.Known.DisableInventoryDraw) != 0)
            {
                roomHeight = Constants.GameScreenHeight;
            }
            if (fy < 0)
            {
                fy = 0;
            }
            if (fy > roomImageHeight - roomHeight)
            {
                fy = roomImageHeight - roomHeight;
            }
            SetFlag(ToucheTools.Constants.Flags.Known.RoomScrollY, (short)fy);
            
            //scroll room x
            var prevDx = (int)GetFlag(ToucheTools.Constants.Flags.Known.RoomScrollX);
            if (x > prevDx + Constants.GameScreenWidth - 160)
            {
                var dx = x - (prevDx + Constants.GameScreenWidth - 160);
                prevDx += dx;
            }
            else if (x < prevDx + 160)
            {
                var dx = prevDx + 160 - x;
                prevDx -= dx;
                if (prevDx < 0)
                {
                    prevDx = 0;
                }
            }
            if (prevDx < 0)
            {
                prevDx = 0;
            }
            if (prevDx > roomImageWidth - Constants.GameScreenWidth)
            {
                prevDx = roomImageWidth - Constants.GameScreenWidth;
            }
            
            SetFlag(ToucheTools.Constants.Flags.Known.RoomScrollX, (short)(prevDx));
        }
        #endregion
        
        #region Animation updates

        foreach (var (keyCharId, keyChar) in KeyChars)
        {
            if (!keyChar.Initialised || keyChar.OffScreen || keyChar.SpriteIndex == null ||
                keyChar.SequenceIndex == null)
            {
                continue;
            }

            var sequenceNum = LoadedSprites[keyChar.SequenceIndex.Value].SequenceNum;
            if (sequenceNum != null)
            {
                if (keyChar.Character != null)
                {
                    var frames = _model.Sequences[sequenceNum.Value]
                        .Characters[keyChar.Character.Value]
                        .Animations[keyChar.CurrentAnim]
                        .Directions[keyChar.CurrentDirection]
                        .Frames;

                    if (keyChar.CurrentAnimSpeed <= 0)
                    {
                        var frame = frames[keyChar.CurrentAnimCounter];
                        keyChar.CurrentAnimSpeed = frame.Delay;
                    }

                    keyChar.CurrentAnimSpeed -= 1;
                    if (keyChar.CurrentAnimSpeed <= 0)
                    {
                        keyChar.CurrentAnimCounter += 1;
                    }
                    if (keyChar.CurrentAnimCounter >= frames.Count)
                    {
                        keyChar.CurrentAnimSpeed = 0;
                        keyChar.CurrentAnimCounter = 0;
                        if (keyChar.CurrentAnim != 1)
                        {
                            var animStart = 0;
                            var animCount = 0;

                            var id2 = -2;
                            if (CurrentKeyChar == keyCharId)
                            {
                                id2 = -1;
                            }
                            if (TalkEntries.Count > 0 && 
                                TalkEntries.Any(t => t.TalkingKeyChar == keyCharId 
                                                            || t.TalkingKeyChar == id2) 
                                && GetFlag(ToucheTools.Constants.Flags.Known.TalkFramesEnabled) == 1
                                )
                            {
                                animStart = keyChar.Anim1Start;
                                animCount = keyChar.Anim1Count;
                            } else if (keyChar.QueuedAnimations.Any())
                            {
                                var nextQueuedAnimation = keyChar.QueuedAnimations[0];
                                keyChar.QueuedAnimations.RemoveAt(0);
                                animStart = nextQueuedAnimation;
                                animCount = 0;
                            }
                            else
                            {
                                animStart = keyChar.Anim2Start;
                                animCount = keyChar.Anim2Count;

                                if (keyChar.CurrentAnim >= animStart && keyChar.CurrentAnim < animStart + animCount)
                                {
                                    var rnd = new Random().Next(0, 100);
                                    if (keyChar.IsFollowing)
                                    {
                                        //TODO:
                                    }
                                    else
                                    {
                                        if (rnd >= 50 && rnd <= 51)
                                        {
                                            animStart = keyChar.Anim3Start;
                                            animCount = keyChar.Anim3Count;
                                        }
                                    }
                                }
                            }

                            if (animCount != 0)
                            {
                                animCount = new Random().Next(0, animCount);
                            }

                            keyChar.CurrentAnim = animStart + animCount;
                        }
                    }
                }
            }
        }
        #endregion
    }

    private void Update()
    {
        if (CurrentState.CurrentProgram != _program.Active)
        {
            OnProgramChange();
        }
    }

    public void PressEscape()
    {
        SetFlag(ToucheTools.Constants.Flags.Known.LastAsciiKeyPress, (short)27);
    }

    public void RightClicked(int screenX, int screenY, int globalX, int globalY)
    {
        if (DisabledInputCounter != 0)
        {
            return;
        }

        var program = _model.Programs[CurrentState.CurrentProgram];
        if (screenX < 0 || screenX > Constants.GameScreenWidth || screenY < 0 || screenY > Constants.GameScreenHeight)
        {
            return;
        }

        if (screenY > Game.RoomHeight)
        {
            //was it within the inventory?
            var inventoryHitbox = -1;
            var inventoryHitboxX = -1;
            foreach (var (inventoryHitboxId, (hitboxX, hitboxY, hitboxW, hitboxH)) in InventoryHitboxes)
            {
                if (screenX < hitboxX || screenX > hitboxX + hitboxW || screenY < hitboxY ||
                    screenY > hitboxY + hitboxH)
                {
                    continue;
                }

                inventoryHitbox = inventoryHitboxId;
                inventoryHitboxX = hitboxX;
                break;
            }

            if (inventoryHitbox != -1)
            {
                var keyChar = GetKeyChar(CurrentKeyChar);
                var inventoryList = InventoryLists[CurrentKeyChar]; //TODO: this may need to be 0 instead, code uses pointers
                var firstEmptyItem = inventoryList.Items.FindIndex(i => i == 0);
                switch (inventoryHitbox)
                {
                    case InventoryHitboxType.Character:
                    case InventoryHitboxType.MoneyDisplay: 
                    case InventoryHitboxType.GoldCoins:
                    case InventoryHitboxType.SilverCoins:
                    case InventoryHitboxType.Money:
                    case InventoryHitboxType.Scroller1:
                    case InventoryHitboxType.Scroller2:
                        //nothing happens
                        break;

                    case InventoryHitboxType.Object1:
                    case InventoryHitboxType.Object2:
                    case InventoryHitboxType.Object3:
                    case InventoryHitboxType.Object4:
                    case InventoryHitboxType.Object5:
                    case InventoryHitboxType.Object6:
                        var obj = (inventoryHitbox - InventoryHitboxType.Object1);
                        var item = inventoryList.Items[inventoryList.DisplayOffset + obj] | 0x1000;
                        //find the "hitbox" for the item (stores actions and strings)
                        foreach (var hitbox in program.Hitboxes)
                        {
                            if (hitbox.Item == item)
                            {
                                var customActions = false;
                                var len = 0;
                                foreach (var action in hitbox.Actions)
                                {
                                    if (action == 0)
                                    {
                                        break;
                                    }
                                    len++;

                                    if (action != Actions.LeftClick && action != Actions.LeftClickWithItem)
                                    {
                                        customActions = true;
                                    }
                                }

                                if (!customActions)
                                {
                                    TryTriggerAction(Actions.DoNothing, item, 0);
                                    return;
                                }

                                var actions = hitbox.Actions.Take(len).Where(a =>
                                    a != Actions.LeftClick && a != Actions.LeftClickWithItem).ToList();
                                
                                ActiveMenu = new ActionMenu()
                                {
                                    Item = hitbox.Item,
                                    X = inventoryHitboxX,
                                    Y = Game.RoomHeight,
                                    Name = GetString(hitbox.String),
                                    Actions = actions,
                                    FallbackAction = 0
                                };
                            }
                        }

                        break;
                    default:
                        throw new Exception("Not implemented yet");
                }
            }
            return;
        }
        
        //find any hitbox that was clicked
        foreach (var hitbox in program.Hitboxes)
        {
            if (!hitbox.IsDrawable)
            {
                continue;
            }

            if (hitbox.IsInventory)
            {
                var flag = GetInventoryFlag(hitbox.InventoryItem);
                if (flag != 0x20)
                {
                    continue;
                }
            }

            var x = hitbox.Rect1.X;
            var y = hitbox.Rect1.Y;
            var w = hitbox.Rect1.W;
            var h = hitbox.Rect1.H;

            if (w == 0 || h == 0)
            {
                continue;
            }
            
            if (hitbox.IsCharacter)
            {
                var keyChar = GetKeyChar(hitbox.KeyChar);
                if (keyChar.Initialised)
                {
                    if (_viewState.KeyCharRenderedRects.ContainsKey(hitbox.KeyChar))
                    {
                        (x, y, w, h) = _viewState.KeyCharRenderedRects[hitbox.KeyChar];
                    }

                    var offsetX = GetFlag(ToucheTools.Constants.Flags.Known.RoomScrollX);
                    var offsetY = GetFlag(ToucheTools.Constants.Flags.Known.RoomScrollY);
                    x += offsetX;
                    y += offsetY;
                }
            }
            
            if (globalX < x || globalX > x + w || globalY < y || globalY > y + h)
            {
                continue;
            }

            if (hitbox.IsDisabled)
            {
                continue;
            }

            var customActions = false;
            var len = 0;
            foreach (var action in hitbox.Actions)
            {
                if (action == 0)
                {
                    break;
                }
                len++;

                if (action != Actions.LeftClick && action != Actions.LeftClickWithItem)
                {
                    customActions = true;
                }
            }

            if (!customActions)
            {
                TryTriggerAction(Actions.DoNothing, hitbox.Item, 0);
                return;
            }

            var actions = hitbox.Actions.Take(len).Where(a =>
                a != Actions.LeftClick && a != Actions.LeftClickWithItem).ToList();
                            
            ActiveMenu = new ActionMenu()
            {
                Item = hitbox.Item,
                X = screenX,
                Y = screenY,
                Name = GetString(hitbox.String),
                Actions = actions,
                FallbackAction = hitbox.Talk
            };

            var curKeyChar = GetKeyChar(CurrentKeyChar);
            curKeyChar.CurrentDirection = (curKeyChar.PositionX < x) ? 0 : 3;

            return;
        }
        
        //no hitboxes
        //TODO:
    }

    public void LeftClicked(int screenX, int screenY, int globalX, int globalY)
    {
        if (DisabledInputCounter != 0)
        {
            return;
        }
        var program = _model.Programs[CurrentState.CurrentProgram];
        if (screenX < 0 || screenX > Constants.GameScreenWidth || screenY < 0 || screenY > Constants.GameScreenHeight)
        {
            return;
        }
        
        if (screenY > Game.RoomHeight)
        {
            //was it within the inventory?
            var inventoryHitbox = -1;
            foreach (var (inventoryHitboxId, (hitboxX, hitboxY, hitboxW, hitboxH)) in InventoryHitboxes)
            {
                if (screenX < hitboxX || screenX > hitboxX + hitboxW || screenY < hitboxY || screenY > hitboxY + hitboxH)
                {
                    continue;
                }

                inventoryHitbox = inventoryHitboxId;
                break;
            }

            if (inventoryHitbox != -1)
            {
                var keyChar = GetKeyChar(CurrentKeyChar);
                var inventoryList = InventoryLists[CurrentKeyChar]; //TODO: this may need to be 0 instead, code uses pointers
                var firstEmptyItem = inventoryList.Items.FindIndex(i => i == 0);
                switch (inventoryHitbox)
                {
                    case InventoryHitboxType.Character:
                    case InventoryHitboxType.MoneyDisplay: //in game code this is handled separately?
                        if (RemovedMoney != 0)
                        {
                            keyChar.Money += RemovedMoney;
                            RemovedMoney = 0;
                        }

                        if (GrabbedItem == 1)
                        {
                            RemoveGrabbedItem();
                        }
                        break;
                    case InventoryHitboxType.GoldCoins:
                        if (keyChar.Money >= 10)
                        {
                            keyChar.Money -= 10;
                            RemovedMoney += 10;
                        }

                        break;
                    case InventoryHitboxType.SilverCoins:
                        if (keyChar.Money > 0)
                        {
                            keyChar.Money -= 1;
                            RemovedMoney += 1;
                        }
                        break;
                    case InventoryHitboxType.Money:
                        if (RemovedMoney != 0)
                        {
                            RemoveGrabbedItem();
                            
                            GrabbedItem = 1;
                        }
                        break;
                    case InventoryHitboxType.Scroller1:
                        if (inventoryList.DisplayOffset <= inventoryList.ItemsPerLine)
                        {
                            inventoryList.DisplayOffset = 0;
                        }
                        else
                        {
                            inventoryList.DisplayOffset -= inventoryList.ItemsPerLine;
                        }
                        break;
                    
                    case InventoryHitboxType.Scroller2:
                        if (inventoryList.DisplayOffset + inventoryList.ItemsPerLine + 1 < firstEmptyItem) 
                        {
                            inventoryList.DisplayOffset += inventoryList.ItemsPerLine;
                        }
                        else
                        {
                            inventoryList.DisplayOffset = (short)Math.Max(0, firstEmptyItem - 6 - 1);
                        }
                        break;

                    case InventoryHitboxType.Object1:
                    case InventoryHitboxType.Object2:
                    case InventoryHitboxType.Object3:
                    case InventoryHitboxType.Object4:
                    case InventoryHitboxType.Object5:
                    case InventoryHitboxType.Object6:
                        var obj = inventoryHitbox - InventoryHitboxType.Object1;
                        SetFlag(ToucheTools.Constants.Flags.Known.CurrentCursorObject, GrabbedItem);
                        if (GrabbedItem == 1)
                        {
                            keyChar.Money += RemovedMoney;
                            RemovedMoney = 0;
                            SetFlag(ToucheTools.Constants.Flags.Known.CurrentMoney, 0);
                        }
                        
                        var item = inventoryList.Items[inventoryList.DisplayOffset + obj];
                        if (item != 0 && GrabbedItem != 0)
                        {
                            //clicked an item onto another item
                            if (TryTriggerAction(Actions.LeftClickWithItem, (item | 0x1000), 0))
                            {
                                RemoveGrabbedItem();
                            }
                        }
                        else
                        {
                            //clicked an item
                            inventoryList.RemoveItem(inventoryList.DisplayOffset + obj);
                            
                            RemoveGrabbedItem();

                            if (item != 0)
                            {
                                GrabbedItem = item;
                            }
                        }
                        break;
                    default:
                        throw new Exception("Not implemented yet");
                }
            }
            return;
        }

        //find any hitbox that was clicked
        foreach (var hitbox in program.Hitboxes)
        {
            if (!hitbox.IsDrawable)
            {
                continue;
            }

            if (hitbox.IsInventory)
            {
                var flag = GetInventoryFlag(hitbox.InventoryItem);
                if (flag != 0x20)
                {
                    continue;
                }
            }

            var x = hitbox.Rect1.X;
            var y = hitbox.Rect1.Y;
            var w = hitbox.Rect1.W;
            var h = hitbox.Rect1.H;

            if (w == 0 || h == 0)
            {
                continue;
            }
            
            if (hitbox.IsCharacter)
            {
                var keyChar = GetKeyChar(hitbox.KeyChar);
                if (keyChar.Initialised)
                {
                    if (_viewState.KeyCharRenderedRects.ContainsKey(hitbox.KeyChar))
                    {
                        (x, y, w, h) = _viewState.KeyCharRenderedRects[hitbox.KeyChar];
                    }

                    var offsetX = GetFlag(ToucheTools.Constants.Flags.Known.RoomScrollX);
                    var offsetY = GetFlag(ToucheTools.Constants.Flags.Known.RoomScrollY);
                    x += offsetX;
                    y += offsetY;
                }
            }
            
            if (globalX < x || globalX > x + w || globalY < y || globalY > y + h)
            {
                continue;
            }

            if (hitbox.IsDisabled)
            {
                continue;
            }
            
            if (GrabbedItem != 0)
            {
                SetFlag(ToucheTools.Constants.Flags.Known.CurrentCursorObject, GrabbedItem);
                if (GrabbedItem == 1)
                {
                    SetFlag(ToucheTools.Constants.Flags.Known.CurrentMoney, RemovedMoney);
                    RemovedMoney = 0;
                }

                InventoryFlags.Remove(GrabbedItem);
                RemoveGrabbedItem();

                if (true) //TODO: if not giving item?
                {
                    if (!TryTriggerAction(Actions.LeftClickWithItem, hitbox.Item, 0))
                    {
                        var prevGrabbedItem = GetFlag(ToucheTools.Constants.Flags.Known.CurrentCursorObject);
                        if (prevGrabbedItem == 1)
                        {
                            RemovedMoney = GetFlag(ToucheTools.Constants.Flags.Known.CurrentMoney);
                        }
                        else
                        {
                            AddItemToInventory(CurrentKeyChar, prevGrabbedItem);
                        }
                    }
                }
                else
                {
                    SetFlag(ToucheTools.Constants.Flags.Known.ItemBeingGiven, (short)(hitbox.Item - 1));
                    //TODO: set flag 117 to hitbox item - 1, then mark give item -1
                }
                
                if (GrabbedItem != 0)
                {
                    RemoveGrabbedItem();
                }

                return;
            }

            if (GrabbedItem != 0)
            {
                RemoveGrabbedItem();
            }
            
            //if there's no action on clicking the hitbox, then just walk
            if (!TryTriggerAction(Actions.LeftClick, hitbox.Item, 0))
            {
                //no script offset so just walk
                WalkTo(globalX, globalY);
            }

            return;
        }
        
        //no hitboxes
        WalkTo(globalX, globalY);
    }

    private bool TryTriggerAction(int action, int object1, int object2)
    {
        var program = _model.Programs[_program.Active];
        var aso = program.ActionScriptOffsets.FirstOrDefault(aso =>
            aso.Action == action && aso.Object1 == object1 && aso.Object2 == object2
        );
        if (aso == null)
        {
            //_log.Error($"Failed to find ASO with action {action} and object {object1}");
            return false;
        }
        
        var script = CurrentState.GetKeyCharScript(CurrentKeyChar);
        if (script == null)
        {
            throw new Exception("Missing key char script");
        }

        script.Offset = aso.Offset;
        script.Status = ProgramState.ScriptStatus.Ready;
        //TODO: reset STK?
        return true;
    }

    private void WalkTo(int x, int y)
    {
        //find closest point
        var program = _model.Programs[_program.Active];
        
        int closestPointIdx = -1;
        double lowestDistance = int.MaxValue;
        for (var i = 0; i < program.Points.Count; i++)
        {
            var point = program.Points[i];
            var distance = Math.Sqrt(
                Math.Pow(x - point.X, 2) +
                Math.Pow(y - point.Y, 2)
            ); //ignore Z distance because it'll even out
            if (distance < lowestDistance)
            {
                closestPointIdx = i;
                lowestDistance = distance;
            }
        }

        if (closestPointIdx == -1)
        {
            _log.Error("No point to path to");
            return;
        }

        var keyChar = GetKeyChar(CurrentKeyChar);
        keyChar.TargetPoint = (ushort)closestPointIdx;
        keyChar.IsFollowing = false;
    }

    public int TickCounter = 0;
    public void Tick()
    {
        if (!AutoPlay)
        {
            return;
        }

        if (BreakpointHit && AutoPlay)
        {
            AutoPlay = false;
            return;
        }

        var now = DateTime.UtcNow;
        
        if ((now - _lastTick).TotalMilliseconds >= MinimumTimeBetweenTicksInMillis)
        {
            if (CurrentState.TickCounter != TickCounter)
            {
                TickCounter = CurrentState.TickCounter;
                _lastTick = now;
                
                StepUntilPaused(true);
            }
            else
            {
                if (!CurrentState.AreScriptsRemainingInCurrentTick())
                {
                    TickCounter = CurrentState.TickCounter;
                    _lastTick = now;
                    
                    StepUntilPaused(true);
                }
                else
                {
                    StepUntilPaused(false);
                }
            }
        }
        else
        {
            StepUntilPaused(false);
        }
    }
    
    public void StepUntilPaused(bool allowGameTick = true)
    {
        BreakpointHit = false;
        while (!Step(allowGameTick) && !BreakpointHit)
        {
            
        }
    }

    public bool Step(bool allowGameTick = true)
    {
        BreakpointHit = false;
        if (CurrentState.QueuedProgram != null)
        {
            //starting new episode
            var newProgram = CurrentState.QueuedProgram.Value;
            CurrentState.QueuedProgram = null;
            _program.SetActive(newProgram);
            return true;
        }
        
        //if we're in the middle fo running a script, just run it
        var currentScript = CurrentState.GetRunningScript();
        bool ret = false;
        if (currentScript != null)
        {
            ret = RunStep();
            if (ret)
            {
                CurrentState.MarkScriptAsDoneInCurrentTick(currentScript);
            }

            return ret;
        }
        
        var nextScript = CurrentState.GetNextScript();
        if (nextScript != null)
        {
            if (nextScript.Status == ProgramState.ScriptStatus.Ready)
            {
                nextScript.Status = ProgramState.ScriptStatus.Running;
            }
            else
            {
                CurrentState.MarkScriptAsDoneInCurrentTick(nextScript);
            }
        }

        if (!CurrentState.AreScriptsRemainingInCurrentTick())
        {
            if (allowGameTick)
            {
                OnGameTick();
                OnGraphicalUpdate();
                CurrentState.TickDone();
            }
            return true;
        }

        return false;
    }
    
    public bool RunStep()
    {
        var program = _model.Programs[_program.Active];
        
        var currentScript = CurrentState.GetRunningScript();
        if (currentScript == null)
        {
            throw new Exception("No running script found");
        }

        var curOffset = currentScript.Offset;
        if (!program.Instructions.ContainsKey(curOffset))
        {
            throw new Exception("Unknown program offset: " + curOffset);
        }

        var prevLastKnown = LastKnownOffset;
        LastKnownOffset = curOffset;
        if (Breakpoints.Contains(curOffset) && curOffset != prevLastKnown)
        {
            BreakpointHit = true;
            return false; //not a game pause, just a breakpoint hit
        }
        
        var instructionOffsets = program.Instructions.Keys.OrderBy(k => k).ToList();
        var idx = instructionOffsets.FindIndex(k => k == curOffset);
        if (idx == instructionOffsets.Count - 1)
        {
            throw new Exception("Reached end of script");
        }

        var programRestart = false;
        var programPaused = false;
        var programStopped = false;
        var justJumped = false;

        var instruction = program.Instructions[curOffset];
        if (instruction is StopScriptInstruction)
        {
            programPaused = true;
            if (currentScript.Id != CurrentKeyChar)
            {
                //the current keychar script always only pauses
                programRestart = true;
                currentScript.Offset = currentScript.StartOffset;
            }
            justJumped = true;
        } else if (instruction is NoopInstruction)
        {
            
        } else if (instruction is FetchScriptWordInstruction fetchScriptWord)
        {
            CurrentState.SetStackValue((short)fetchScriptWord.Val);
        } else if (instruction is LoadSpriteInstruction loadSprite)
        {
            _log.Info($"Loaded sprite {loadSprite.Num} into slot {loadSprite.Index}.");
            LoadedSprites[loadSprite.Index].SpriteNum = loadSprite.Num;
        } else if (instruction is LoadSequenceInstruction loadSequence)
        {
            _log.Info($"Loaded sequence {loadSequence.Num} into slot {loadSequence.Index}.");
            LoadedSprites[loadSequence.Index].SequenceNum = loadSequence.Num;
        } else if (instruction is InitCharScriptInstruction initCharScript)
        {
            var sprite = LoadedSprites[initCharScript.SpriteIndex];
            if (sprite.SpriteNum == null)
            {
                throw new Exception("Sprite not loaded yet: " + initCharScript.SpriteIndex);
            }

            var sequence = LoadedSprites[initCharScript.SequenceIndex];
            if (sequence.SequenceNum == null)
            {
                throw new Exception("Sequence not loaded yet: " + initCharScript.SequenceIndex);
            }
            
            _log.Info($"Initialising key char {initCharScript.Character} with sprite {initCharScript.SpriteIndex} ({sprite.SpriteNum}) and sequence {initCharScript.SequenceIndex} ({sequence.SequenceNum}).");
            
            var keyChar = GetKeyChar(initCharScript.Character);
            keyChar.Initialised = true;
            keyChar.TextColour = initCharScript.Color;
            keyChar.SpriteIndex = initCharScript.SpriteIndex;
            keyChar.SequenceIndex = initCharScript.SequenceIndex;
            keyChar.Character = initCharScript.SequenceCharacterId;
            var keyCharScript = CurrentState.Scripts.FirstOrDefault(s => 
                                                                  s.Id == initCharScript.Character && 
                                                                  s.Status != ProgramState.ScriptStatus.Running &&
                                                                  s.Status != ProgramState.ScriptStatus.Stopped
            );
            if (keyCharScript != null)
            {
                keyCharScript.Offset = keyCharScript.StartOffset;
                keyCharScript.Status = ProgramState.ScriptStatus.Ready;
            }
        } else if (instruction is LoadRoomInstruction loadRoom)
        {
            CurrentState.LoadedRoom = loadRoom.Num;
            
            //from game code
            var palette = _model.Palettes[loadRoom.Num];
            LoadPalette(palette);
            LoadedSprites[5].SpriteNum = null;
            LoadedSprites[5].SequenceNum = null;
            LoadedSprites[6].SpriteNum = null;
            LoadedSprites[6].SequenceNum = null;
            CurrentState.ActiveRoomAreas = new List<(int, ProgramDataModel.AreaState)>();
            CurrentState.BackgroundOffsets = new Dictionary<ushort, (int, int)>();
            CurrentState.ActiveRoomSprites = new List<(int, int, int)>();
            if (GetFlag(ToucheTools.Constants.Flags.Known.KeepPaletteOnRoomLoad) == 0)
            {
                //reset to full scale
                SetPaletteScale(0, 255, 255, 255, 255);
            }
            else
            {
                //set scale to 0
                SetPaletteScale(0, 255, 0, 0, 0);
            }
        } else if (instruction is SetCharFrameInstruction setCharFrame)
        {
            var keyChar = GetKeyChar(setCharFrame.Character);
            
            if (setCharFrame.TransitionType == SetCharFrameInstruction.Type.Loop) // 0
            {
                keyChar.Anim2Start = setCharFrame.Val2;
                keyChar.Anim2Count = setCharFrame.Val3;
                keyChar.Anim3Start = setCharFrame.Val2;
                keyChar.Anim3Count = setCharFrame.Val3;
            } else if (setCharFrame.TransitionType == SetCharFrameInstruction.Type.RandomCountThenStop) //1
            {
                var rnd = new Random();
                int v = setCharFrame.Val3;
                if (setCharFrame.Val3 != 0)
                {
                    v = rnd.Next(0, v);
                }

                keyChar.QueuedAnimations.Add(v + setCharFrame.Val2);
                if (keyChar.QueuedAnimations.Count > 15)
                {
                    throw new Exception("Too many queued animations");
                }
            }
            else if (setCharFrame.TransitionType == SetCharFrameInstruction.Type.TalkFrames) //2
            {
                keyChar.Anim1Start = setCharFrame.Val2;
                keyChar.Anim1Count = setCharFrame.Val3;
            } 
            else if (setCharFrame.TransitionType == SetCharFrameInstruction.Type.StartPaused) //3
            {
                keyChar.CurrentAnim = setCharFrame.Val2;
                keyChar.CurrentAnimSpeed = 0;
                keyChar.CurrentAnimCounter = 0;
            }
            else if (setCharFrame.TransitionType == SetCharFrameInstruction.Type.Todo4) //4
            {
                keyChar.Anim3Start = setCharFrame.Val2;
                keyChar.Anim3Count = setCharFrame.Val3;
            }
            else
            {
                throw new Exception("Unknown transition type: " + setCharFrame.TransitionType);
            }
        } else if (instruction is EnableInputInstruction)
        {
            DisabledInputCounter++;
        } else if (instruction is DisableInputInstruction)
        {
            if (DisabledInputCounter < 0)
            {
                DisabledInputCounter = 0;
            }
            if (DisabledInputCounter > 0)
            {
                DisabledInputCounter--;
            }  
        } else if (instruction is SetFlagInstruction setFlag)
        {
            var val = CurrentState.StackValue;
            SetFlag(setFlag.Flag, val);

            if (setFlag.Flag == (short)ToucheTools.Constants.Flags.Known.CurrentKeyChar)
            {
                CurrentKeyChar = val;
            } else  if (setFlag.Flag == (short)ToucheTools.Constants.Flags.Known.QuitGame && val != 0)
            {
                //TODO: quits game
            } else if (setFlag.Flag == (short)ToucheTools.Constants.Flags.Known.RandomNumberMod)
            {
                var newVal = new Random().Next(0, val);
                SetFlag(setFlag.Flag, (short)val);
                SetFlag(ToucheTools.Constants.Flags.Known.LastRandomNumber, (short)newVal);
            } //TODO: more
        } else if (instruction is SetCharBoxInstruction setCharBox)
        {
            var keyChar = GetKeyChar(setCharBox.Character);
            var point = program.Points[setCharBox.Num];
            keyChar.PositionX = point.X;
            keyChar.PositionY = point.Y;
            keyChar.PositionZ = point.Z;
            keyChar.LastPoint = setCharBox.Num;
            keyChar.TargetPoint = setCharBox.Num;
            keyChar.LastWalk = (ushort)program.Walks.FindIndex(w => w.Point1 == setCharBox.Num || w.Point2 == setCharBox.Num);
        } else if (instruction is InitCharInstruction initChar)
        {
            var keyChar = GetKeyChar(initChar.Character);
            keyChar.OnProgramChange();
            keyChar.Initialised = true;
            keyChar.Anim1Start = 0;
            keyChar.Anim1Count = 1;
            keyChar.Anim2Start = 0;
            keyChar.Anim2Count = 1;
            keyChar.Anim3Start = 0;
            keyChar.Anim3Count = 1;
            keyChar.CurrentDirection = 0;
        } else if (instruction is MoveCharToPosInstruction moveCharToPos)
        {
            var keyChar = GetKeyChar(moveCharToPos.Character);
            int charId = moveCharToPos.Character;
            if (moveCharToPos.Character == 256)
            {
                charId = CurrentKeyChar;
            }

            if (moveCharToPos.TargetingAnotherCharacter)
            {
                throw new Exception("Targeting another character, not implemented");
            }

            var point = program.Points[moveCharToPos.Num];
            keyChar.TargetPoint = moveCharToPos.Num;
            keyChar.IsFollowing = false;

            if (currentScript.Id == charId)
            {
                foreach (var talkEntry in TalkEntries)
                {
                    if (talkEntry.OtherKeyChar == charId)
                    {
                        talkEntry.OtherKeyChar = -1;
                    }
                }

                keyChar.WaitForAnimationId = null;
                keyChar.WaitForWalk = null;
                keyChar.WaitForKeyChar = charId;
                keyChar.WaitForPoint = moveCharToPos.Num;
                programPaused = true;
            }
        } else if (instruction is StartTalkInstruction startTalk)
        {
            if (!startTalk.DoNothing)
            {
                int talkingChar = startTalk.Character;
                int num = startTalk.Num;
                int otherChar = currentScript.Id;
                
                if (startTalk.CurrentCharacter)
                {
                    talkingChar = CurrentKeyChar;
                    num += CurrentKeyChar & 1;
                }

                var skip = false;
                if (TalkEntries.Count > 0)
                {
                    var talkEntry = TalkEntries.Last();
                    if (talkEntry.TalkingKeyChar == talkingChar &&
                        talkEntry.OtherKeyChar == otherChar &&
                        talkEntry.Num == num)
                    {
                        skip = true;
                    }
                }

                if (!skip)
                {
                    foreach (var talkEntry in TalkEntries)
                    {
                        if (talkEntry.OtherKeyChar == otherChar)
                        {
                            talkEntry.OtherKeyChar = -1;
                        }
                    }

                    var str = GetString(num);

                    var counter = (int)((str.Length * 16.0f) / 32 + 20); //from game code but with a fixed letter size
                    var keyChar = GetKeyChar(talkingChar);
                    if (keyChar.CurrentAnim != 1)
                    {
                        keyChar.CurrentAnim = keyChar.Anim1Start;
                        keyChar.CurrentAnimCounter = 0;
                        keyChar.CurrentAnimSpeed = 0;
                    }
                    
                    TalkEntries.Add(new TalkEntry()
                    {
                        TalkingKeyChar = talkingChar,
                        OtherKeyChar = otherChar,
                        Num = num,
                        Text = str,
                        Counter = counter
                    });
                    
                }

                programPaused = true;   
            }
        } else if (instruction is SetCharDelayInstruction setCharDelay)
        {
            currentScript.Delay = setCharDelay.Delay;
            programPaused = true;
        } else if (instruction is SetupWaitingCharInstruction setupWaitingChar)
        {
            if (setupWaitingChar.Val1 == ushort.MaxValue)
            {
                _log.Error("Some waiting logic not implemented yet"); //TODO:
                //TODO: waiting logic
                programPaused = true;
            }
            else
            {
                var keyChar = GetKeyChar(currentScript.Id);
                keyChar.WaitForPoint = null;
                keyChar.WaitForAnimationId = null;
                keyChar.WaitForWalk = null;
                if (setupWaitingChar.Val1 == 0)
                {
                    keyChar.WaitForAnimationId = setupWaitingChar.Val2;
                } else if (setupWaitingChar.Val1 == 1)
                {
                    keyChar.WaitForPoint = setupWaitingChar.Val2;
                } else if (setupWaitingChar.Val1 == 2)
                {
                    keyChar.WaitForWalk = setupWaitingChar.Val2;
                }
                else
                {
                    throw new Exception("Unknown wait property: " + setupWaitingChar.Val1);
                }

                keyChar.WaitForKeyChar = setupWaitingChar.CurrentCharacter ? CurrentKeyChar : setupWaitingChar.Character;
                programPaused = true;
            }
        }
        else if (instruction is AddRoomAreaInstruction addRoomArea)
        {
            if (!Flags.ContainsKey(addRoomArea.Flag))
            {
                _log.Error($"Flag {addRoomArea.Flag} value required but not known");
            }
            if (!Flags.ContainsKey((ushort)(addRoomArea.Flag + 1)))
            {
                _log.Error($"Flag {(addRoomArea.Flag + 1)} value required but not known");
            }

            var x = GetFlag(addRoomArea.Flag);
            var y = GetFlag((ushort)(addRoomArea.Flag + 1));
            CurrentState.BackgroundOffsets[addRoomArea.Num] = (x, y);
        } else if (instruction is GetFlagInstruction getFlag)
        {
            var flagVal = GetFlag(getFlag.Flag);
            CurrentState.SetStackValue((short)flagVal);
        } else if (instruction is PushInstruction)
        {
            CurrentState.MoveStackPointerBackwards();
            CurrentState.SetStackValue(0);
        } else if (instruction is AddInstruction)
        {
            var val = CurrentState.StackValue;
            CurrentState.MoveStackPointerForwards();
            CurrentState.SetStackValue((short)(CurrentState.StackValue + val));
        } else if (instruction is SubInstruction)
        {
            var val = CurrentState.StackValue;
            CurrentState.MoveStackPointerForwards();
            CurrentState.SetStackValue((short)(CurrentState.StackValue - val));
        } else if (instruction is AndInstruction)
        {
            var val = CurrentState.StackValue;
            var uVal = BitConverter.ToUInt16(BitConverter.GetBytes(val), 0);//game does it this way
            CurrentState.MoveStackPointerForwards();
            CurrentState.SetStackValue((short)(CurrentState.StackValue & uVal));
        } else if (instruction is OrInstruction)
        {
            var val = CurrentState.StackValue;
            var uVal = BitConverter.ToUInt16(BitConverter.GetBytes(val), 0);//game does it this way
            CurrentState.MoveStackPointerForwards();
#pragma warning disable CS0675
            CurrentState.SetStackValue((short)(CurrentState.StackValue | uVal));
#pragma warning restore CS0675
        }
        else if (instruction is TestEqualsInstruction)
        {
            var val = CurrentState.StackValue;
            CurrentState.MoveStackPointerForwards();
            short newVal = 0;
            if (val == CurrentState.StackValue)
            {
                newVal = -1;
            }
            CurrentState.SetStackValue(newVal);
        } else if (instruction is TestNotEqualsInstruction)
        {
            var val = CurrentState.StackValue;
            CurrentState.MoveStackPointerForwards();
            short newVal = 0;
            if (val != CurrentState.StackValue)
            {
                newVal = -1;
            }
            CurrentState.SetStackValue(newVal);
        } else if (instruction is TestLowerInstruction)
        {
            var val = CurrentState.StackValue;
            CurrentState.MoveStackPointerForwards();
            short newVal = 0;
            if (val < CurrentState.StackValue)
            {
                newVal = -1;
            }
            CurrentState.SetStackValue(newVal);
        } else if (instruction is JzInstruction jz)
        {
            if (CurrentState.StackValue == 0)
            {
                currentScript.Offset = jz.NewOffset;
                justJumped = true;
            }
        } else if (instruction is GetInventoryItemInstruction getInventoryItem)
        {
            var keyChar = GetKeyChar(getInventoryItem.Character);
            var val = keyChar.Money;
            if (!getInventoryItem.MoneyItem)
            {
                val = keyChar.CountedInventoryItems[getInventoryItem.Item];
            }
            CurrentState.SetStackValue(val);
        } else if (instruction is SetInventoryItemInstruction setInventoryItem)
        {
            var keyChar = GetKeyChar(setInventoryItem.Character);
            var val = CurrentState.StackValue;
            if (setInventoryItem.MoneyItem)
            {
                //first, dump any 'global' money into the current keychar
                var currentKeyChar = GetKeyChar(CurrentKeyChar);
                currentKeyChar.Money = RemovedMoney;
                RemovedMoney = 0;
                
                //second, set the keychar money
                keyChar.Money = val;
            }
            else
            {
                keyChar.CountedInventoryItems[setInventoryItem.Item] = val;
            }
        } else if (instruction is AddItemToInventoryAndRedrawInstruction addItemToInventory)
        {
            var item = CurrentState.StackValue;
            AddItemToInventory(addItemToInventory.Character, item);
        } else if (instruction is RemoveItemFromInventoryInstruction removeItemFromInventory)
        {
            var item = CurrentState.StackValue;
            RemoveItemFromInventory(removeItemFromInventory.Character, item);
        }
        else if (instruction is StartEpisodeInstruction startEpisode)
        {
            if (CurrentState.QueuedProgram != null)
            {
                throw new Exception("Trying to start episode that's already queued");
            }
            CurrentState.QueuedProgram = startEpisode.Num;
            Flags[0] = (short)startEpisode.Flag;
        } else if (instruction is SetCharFlagsInstruction setCharFlags)
        {
            var keyChar = GetKeyChar(setCharFlags.Character);
            keyChar.IsSelectable |= (setCharFlags.Flags & 0x4000) != 0;
            keyChar.OffScreen |= (setCharFlags.Flags & 0x8000) != 0;
        } else if (instruction is UnsetCharFlagsInstruction unsetCharFlags)
        {
            var keyChar = GetKeyChar(unsetCharFlags.Character);
            keyChar.IsSelectable &= (unsetCharFlags.Flags & 0x4000) == 0;
            keyChar.OffScreen &= (unsetCharFlags.Flags & 0x8000) == 0;
        } else if (instruction is UpdateRoomAreasInstruction updateRoomAreas)
        {
            var areaId = updateRoomAreas.Area;
            var area = program.Areas.First(a => a.Id == areaId);
            if (CurrentState.ActiveRoomAreas.Count == 199)
            {
                CurrentState.ActiveRoomAreas = new List<(int, ProgramDataModel.AreaState)>();
            }

            CurrentState.ActiveRoomAreas.Add((areaId, area.InitialState));
        } else if (instruction is UpdateRoomInstruction updateRoom)
        {
            var areaId = updateRoom.Area;
            var area = program.Areas.First(a => a.Id == areaId);
            if (CurrentState.ActiveRoomAreas.Count == 199)
            {
                CurrentState.ActiveRoomAreas = new List<(int, ProgramDataModel.AreaState)>();
            }

            CurrentState.ActiveRoomAreas.Add((areaId, area.InitialState));
        } else if (instruction is DrawSpriteOnBackdropInstruction drawSpriteOnBackdrop)
        {
            var spriteId = drawSpriteOnBackdrop.Num;
            if (spriteId >= LoadedSprites.Length)
            {
                throw new Exception("Unknown sprite referenced");
            }

            if (LoadedSprites[spriteId].SpriteNum == null)
            {
                throw new Exception("Referenced sprite not loaded");
            }
            
            CurrentState.ActiveRoomSprites.Add((spriteId, drawSpriteOnBackdrop.X, drawSpriteOnBackdrop.Y));
        } else if (instruction is SetPaletteInstruction setPalette)
        {
            //from game code
            var from = 0; 
            var count = 240;
            SetPaletteScale(from, count, setPalette.R, setPalette.G, setPalette.B);
        } else if (instruction is StartPaletteFadeInInstruction startPaletteFadeIn)
        {
            SetFlag(ToucheTools.Constants.Flags.Known.ProcessRandomPalette, 0);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteScale, 0);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteFirstColour, 0);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteColourCount, 255);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteMaxScale, 255);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteMinScale, 0);
            var increment = startPaletteFadeIn.Num;
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteScaleIncrement, (short)increment);
        } else if (instruction is StartPaletteFadeOutInstruction startPaletteFadeOut)
        {
            SetFlag(ToucheTools.Constants.Flags.Known.ProcessRandomPalette, 0);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteScale, 255);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteFirstColour, 0);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteColourCount, 255);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteMaxScale, 255);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteMinScale, 0);
            var increment = startPaletteFadeOut.Num;
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteScaleIncrement, (short)(-increment));
        } else if (instruction is FadePaletteInstruction fadePalette)
        {
            var scaleMin = fadePalette.FadeOut ? 255 : 0;
            var scaleMax = fadePalette.FadeOut ? 0 : 255;
            var increment = fadePalette.FadeOut ? 2 : -2;
            var steps = 128;
            
            SetFlag(ToucheTools.Constants.Flags.Known.ProcessRandomPalette, 1);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteScale, (short)scaleMin);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteFirstColour, 0);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteColourCount, 240);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteMaxScale, (short)scaleMax);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteMinScale, (short)scaleMin);
            SetFlag(ToucheTools.Constants.Flags.Known.FadePaletteScaleIncrement, (short)increment);
        } else if (instruction is StartSoundInstruction)
        {
            //nothing to do yet
        } else if (instruction is SetCharDirectionInstruction setCharDirection)
        {
            var keyChar = GetKeyChar(setCharDirection.Character);
            var dir = setCharDirection.Direction;
            keyChar.CurrentDirection = dir;
        } else if (instruction is StartMusicInstruction)
        {
            //nothing to do yet
        } else if (instruction is LoadSpeechSegmentInstruction)
        {
            //nothing to do yet
        } else if (instruction is FaceCharInstruction faceChar)
        {
            var keyChar1 = GetKeyChar(faceChar.Character1);
            var keyChar2 = GetKeyChar(faceChar.Character2);

            if (keyChar1.PositionX <= keyChar2.PositionX)
            {
                keyChar2.CurrentDirection = 3;
            }
            else
            {
                keyChar2.CurrentDirection = 0;
            }
        } else if (instruction is SetCharTextColorInstruction setCharTextColor)
        {
            var keyChar = GetKeyChar(setCharTextColor.Character);
            keyChar.TextColour = setCharTextColor.Color;
        } else if (instruction is GetCharPointsDataNumInstruction getCharPoint)
        {
            var keyChar = GetKeyChar(getCharPoint.Character);
            CurrentState.SetStackValue((short)(keyChar.LastPoint ?? 0));
        } else if (instruction is GetCharCurrentWalkBoxInstruction getCharCurrentWalkBox)
        {
            var keyChar = GetKeyChar(getCharCurrentWalkBox.Character);
            CurrentState.SetStackValue((short)(keyChar.TargetPoint ?? -1));
        } else if (instruction is GetCharWalkBoxInstruction getCharWalk)
        {
            var keyChar = GetKeyChar(getCharWalk.Character);
            CurrentState.SetStackValue((short)(keyChar.LastWalk ?? -1));
        } else if (instruction is SetConversationInstruction setConversation)
        {
            QueuedConversation = setConversation.Num;
        } else if (instruction is ClearConversationChoicesInstruction)
        {
            CurrentConversationChoices = new List<(int, string)>();
        } else if (instruction is AddConversationChoiceInstruction addConversationChoice)
        {
            AddConversationChoice(addConversationChoice.Num);
        } else if (instruction is RemoveConversationChoiceInstruction removeConversationChoice)
        {
            RemoveConversationChoice(removeConversationChoice.Num);
        } else if (instruction is EndConversationInstruction)
        {
            CurrentConversationChoices = new List<(int, string)>();
            programPaused = true;
            programRestart = true;
            DisabledInputCounter = 0;
        }
        else
        {
            _log.Error($"Unhandled instruction type: {instruction.Opcode:G}");
        }
        
        if (programStopped)
        {
            _log.Info($"Finished running script {currentScript.Id}.");

            currentScript.Status = ProgramState.ScriptStatus.Stopped;
            justJumped = true;
        }
        
        if (programPaused)
        {
            if (programRestart)
            {
                currentScript.Status = ProgramState.ScriptStatus.Ready;
            }
            else
            {
                currentScript.Status = ProgramState.ScriptStatus.Paused;
            }
        }
        if (!justJumped)
        {
            idx += 1;
            currentScript.Offset = instructionOffsets[idx];
        }

        return programPaused || programStopped;
    }
}