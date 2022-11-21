using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;
using ToucheTools.Constants;
using ToucheTools.Models;
using ToucheTools.Models.Instructions;
using Veldrid;

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
        public bool IsSelectable { get; set; } = false; //unsure?
        public bool OffScreen { get; set; } = false; //unsure?
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
            public ScriptType Type { get; set; }
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
            return Scripts.FirstOrDefault(s =>
                s.Type == ProgramState.ScriptType.KeyChar &&
                s.Id == keyCharId
            );
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
            // if (ScriptsForCurrentTick
            //         .Count(s => s.Status != ScriptStatus.Stopped && 
            //                     s.Status != ScriptStatus.NotInit) != 0)
            // {
            //     throw new Exception("Scripts not all done in tick");
            // }

            ScriptsForCurrentTick = Scripts.ToList();
            TickCounter++;
        }
        #endregion

        #region Room Areas
        public Dictionary<int, ProgramDataModel.AreaState> ActiveRoomAreas { get; set; } = new Dictionary<int, ProgramDataModel.AreaState>();
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

    public ActiveProgramState(DatabaseModel model, ActiveProgram program, LogData log)
    {
        _model = model;
        _program = program;
        _log = log;

        OnStartup();
        
        _program.ObserveActive(Update);
        Update();
    }

    private void OnStartup()
    {
        //from game code
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

        for (var i = 0; i < 7; i++)
        {
            LoadedSprites[i] = new LoadedSprite();
        }
    }

    #region Breakpoint/debug
    public List<int> Breakpoints { get; set; } = new List<int>();
    public bool BreakpointHit { get; set; } = false;
    public int LastKnownOffset { get; set; } = -1;
    #endregion
    
    public ProgramState CurrentState { get; set; } = new ProgramState();
    public bool AutoPlay { get; set; } = false;
    private DateTime _lastTick = DateTime.MinValue;
    private const int MinimumTimeBetweenTicksInMillis = 50;
    
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
    private InventoryList[] InventoryLists { get; set; } = new InventoryList[3];
    private short GlobalMoney { get; set; } = 0;
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
    private short CurrentKeyChar => GetFlag(ToucheTools.Constants.Flags.Known.CurrentKeyChar);

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

        SetFlag(ToucheTools.Constants.Flags.Known.RndPalMinColour, 240);
        SetFlag(ToucheTools.Constants.Flags.Known.RndPalRandomRange, 16);
        SetFlag(ToucheTools.Constants.Flags.Known.RndPalMinDelay, 0);
        SetFlag(ToucheTools.Constants.Flags.Known.RndPalRandomDelay, 1);
        SetFlag(ToucheTools.Constants.Flags.Known.CurrentKeyChar, 0);

        CurrentState.Scripts.Add(new ProgramState.Script()
        {
            Type = ProgramState.ScriptType.KeyChar,
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
                    Type = ProgramState.ScriptType.KeyChar,
                    Id = keyCharId,
                    StartOffset = cso.Offs,
                    Offset = cso.Offs,
                    Status = ProgramState.ScriptStatus.NotInit
                });
            }
        }

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

                    var sequenceNum = LoadedSprites[keyChar.SequenceIndex.Value].SequenceNum;
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

            if (script.Type == ProgramState.ScriptType.KeyChar)
            {
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
        }

        #endregion

        #region Talk Entries

        if (TalkEntries.Count > 0)
        {
            var lastTalkEntry = TalkEntries.Last();
            lastTalkEntry.Counter--;
            if (lastTalkEntry.Counter <= 0)
            {
                foreach (var talkEntry in TalkEntries)
                {
                    if (talkEntry.OtherKeyChar != -1)
                    {
                        var script = CurrentState.GetKeyCharScript(talkEntry.OtherKeyChar);
                        if (script != null)
                        {
                            if (script.Status == ProgramState.ScriptStatus.Paused)
                            {
                                script.Status = ProgramState.ScriptStatus.Ready;
                            }
                        }
                    }

                    var keyChar = GetKeyChar(talkEntry.TalkingKeyChar);
                    if (keyChar.CurrentAnim >= keyChar.Anim1Start &&
                        keyChar.CurrentAnim < keyChar.Anim1Start + keyChar.Anim1Count)
                    {
                        keyChar.CurrentAnim = keyChar.Anim2Start;
                        keyChar.CurrentAnimCounter = 0;
                        keyChar.CurrentAnimSpeed = 0;
                    }
                }

                TalkEntries = new List<TalkEntry>();
            }
            else
            {
                SetFlag(ToucheTools.Constants.Flags.Known.GameCycleCounter3, 0);
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
                                                            || t.TalkingKeyChar == id2) && 
                                GetFlag(901) == 1)
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
    
    private int TickCounter = 0;
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
            if (CurrentState.TickCounter != TickCounter || !CurrentState.AreScriptsRemainingInCurrentTick())
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
        else
        {
            StepUntilPaused(false);
        }
    }
    
    private void Update()
    {
        if (CurrentState.CurrentProgram != _program.Active)
        {
            OnProgramChange();
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
            if (nextScript.Type != ProgramState.ScriptType.KeyChar)
            {
                throw new Exception($"Non-char scripts not implemented yet: {nextScript.Type:G}");
            }

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
            else
            {
                return true;
            }
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
            if (currentScript.Type != ProgramState.ScriptType.KeyChar || currentScript.Id != CurrentKeyChar)
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
                                                                  s.Type == ProgramState.ScriptType.KeyChar && 
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
            LoadedSprites[5].SpriteNum = null;
            LoadedSprites[5].SequenceNum = null;
            LoadedSprites[6].SpriteNum = null;
            LoadedSprites[6].SequenceNum = null;
            CurrentState.ActiveRoomAreas = new Dictionary<int, ProgramDataModel.AreaState>();
            CurrentState.BackgroundOffsets = new Dictionary<ushort, (int, int)>();
            CurrentState.ActiveRoomSprites = new List<(int, int, int)>();
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
            
        } else if (instruction is SetFlagInstruction setFlag)
        {
            var val = CurrentState.StackValue;
            SetFlag(setFlag.Flag, val);
            
            if (setFlag.Flag == 611 && val != 0)
            {
                //TODO: quits game
            } else if (setFlag.Flag == 612)
            {
                var newVal = 999; //TODO: randomly generate
                SetFlag(setFlag.Flag, (short)newVal);
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

            if (currentScript.Type == ProgramState.ScriptType.KeyChar && currentScript.Id == charId)
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
                int otherChar = CurrentKeyChar;
                if (currentScript.Type == ProgramState.ScriptType.KeyChar)
                {
                    otherChar = currentScript.Id;
                }
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

                    var str = "";
                    if (num < 0)
                    {
                        if (!_model.Text?.Strings.ContainsKey(num) ?? false)
                        {
                            throw new Exception("Missing global string: " + (-num));
                        }
                        str = _model.Text?.Strings[-num];
                    }
                    else
                    {
                        if (!program.Strings.ContainsKey(num))
                        {
                            throw new Exception("Missing program string: " + num);
                        }
                        str = program.Strings[num];
                    }

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
            if (currentScript.Type != ProgramState.ScriptType.KeyChar)
            {
                throw new Exception("Unknown script type for delay");
            }

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
                if (currentScript.Type != ProgramState.ScriptType.KeyChar)
                {
                    throw new Exception("Setup waiting chars outside of keychar script");
                }

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
        } else if (instruction is TestEqualsInstruction)
        {
            var val = CurrentState.StackValue;
            CurrentState.MoveStackPointerForwards();
            short newVal = 0;
            if (val == CurrentState.StackValue)
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
                currentKeyChar.Money = GlobalMoney;
                GlobalMoney = 0;
                
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
            if (item == 0)
            {
                _log.Error("TODO: re-sort inventory items"); //TODO: just re-sort the inventory items
            } else if (item == 1)
            {
                //it's really about money
                GlobalMoney += GetFlag(ToucheTools.Constants.Flags.Known.CurrentMoney);
            }
            else
            {
                //it's about adding an item
                if (addItemToInventory.Character >= InventoryLists.Length)
                {
                    throw new Exception("Adding inventory to non-existent list");
                }

                var inventoryList = InventoryLists[addItemToInventory.Character];
                inventoryList.PrependItem(item);
            }
        } else if (instruction is StartEpisodeInstruction startEpisode)
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
                CurrentState.ActiveRoomAreas = new Dictionary<int, ProgramDataModel.AreaState>();
            }

            CurrentState.ActiveRoomAreas[areaId] = area.InitialState;
        } else if (instruction is UpdateRoomInstruction updateRoom)
        {
            var areaId = updateRoom.Area;
            var area = program.Areas.First(a => a.Id == areaId);
            if (CurrentState.ActiveRoomAreas.Count == 199)
            {
                CurrentState.ActiveRoomAreas = new Dictionary<int, ProgramDataModel.AreaState>();
            }

            CurrentState.ActiveRoomAreas[areaId] = area.InitialState;
        } else if (instruction is DrawSpriteOnBackdropInstruction drawSpriteOnBackdrop)
        {
            var spriteId = drawSpriteOnBackdrop.Num;
            if (!_model.Sprites.ContainsKey(spriteId))
            {
                throw new Exception("Unknown sprite referenced");
            }
            CurrentState.ActiveRoomSprites.Add((spriteId, drawSpriteOnBackdrop.X, drawSpriteOnBackdrop.Y));
        }
        else
        {
            _log.Error($"Unhandled instruction type: {instruction.Opcode:G}");
        }
        
        if (programStopped)
        {
            _log.Info($"Finished running {currentScript.Type:G} {currentScript.Id}.");

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