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
        #endregion
        
        #region Position
        public ushort? LastProgramPoint { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int PositionZ { get; set; }
        #endregion
        
        #region Items
        public short Money { get; set; }
        public short[] CountedInventoryItems { get; set; } = new short[4];
        #endregion
        
        public void OnProgramChange()
        {
            Initialised = false;
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
            LastProgramPoint = null;
            IsFollowing = false;
            IsSelectable = false;
            OffScreen = false;
            PositionX = 10; //from game code
            //intentional that no position y/z update, from game code
            SpriteIndex = null;
            SequenceIndex = null;
            Character = null;
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

        public Script? GetRunningScript()
        {
            return Scripts.SingleOrDefault(s => s.Status == ScriptStatus.Running);
        }

        public Script? GetNextScript()
        {
            return Scripts.FirstOrDefault(s => s.Status == ScriptStatus.Ready);
        }
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

    public ProgramState CurrentState { get; set; } = new ProgramState();
    public bool AutoPlay { get; set; } = false;
    private DateTime _lastTick = DateTime.MinValue;
    private const int MinimumTimeBetweenTicksInMillis = 50;
    
    #region Loaded Sprites
    public class LoadedSprite //includes sequence info
    {
        public int? SpriteNum { get; set; } = null;
        public int? SequenceNum { get; set; } = null;
    }
    public LoadedSprite[] LoadedSprites { get; set; } = new LoadedSprite[7];
    #endregion
    
    #region Inventory
    public InventoryList[] InventoryLists { get; set; } = new InventoryList[3];
    public short GlobalMoney { get; set; } = 0;
    #endregion
    
    #region Flags
    public Dictionary<ushort, short> Flags { get; set; } = new Dictionary<ushort, short>();

    public short GetFlag(ushort flag)
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

    public void SetFlag(ushort flag, short val)
    {
        Flags[flag] = val;
    }

    public void SetFlag(Flags.Known known, short val)
    {
        SetFlag((ushort)known, val);
    }
    #endregion

    #region KeyChars
    public Dictionary<int, KeyChar> KeyChars { get; set; } = new Dictionary<int, KeyChar>();
    public short CurrentKeyChar => GetFlag(ToucheTools.Constants.Flags.Known.CurrentKeyChar);

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

    public void Tick()
    {
        if (!AutoPlay)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if ((now - _lastTick).TotalMilliseconds >= MinimumTimeBetweenTicksInMillis)
        {
            _lastTick = now;
            Step(); //debug way
            //StepUntilPaused(); //correct way
        }
    }

    private void OnProgramChange()
    {
        for (var i = 0; i < 32; i++)
        {
            if (!KeyChars.ContainsKey(i))
            {
                KeyChars[i] = new KeyChar();
            }
        }
        var program = _model.Programs[_program.Active];
        CurrentState = new ProgramState()
        {
            CurrentProgram = _program.Active,
        };
        
        Flags[0] = (short)_program.Active;
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
            
            var frames = _model.Sequences[LoadedSprites[keyChar.SequenceIndex.Value].SequenceNum.Value]
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
                var animStart = 0;
                var animCount = 0;
                if (keyChar.CurrentAnim != 1)
                {
                    if (false && GetFlag(901) == 1) //TODO: if current char talking?
                    {
                        animStart = keyChar.Anim1Start;
                        animCount = keyChar.Anim1Count;
                    } else if (false) //TODO: framesListCount random then stop?
                    {
                        //animStart is looked up in a list of 16?
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
        #endregion
        
        #region Delay
        foreach (var script in CurrentState.Scripts)
        {
            if (script.Status != ProgramState.ScriptStatus.NotInit &&
                script.Status != ProgramState.ScriptStatus.Stopped)
            {
                if (script.Delay > 0)
                {
                    script.Delay--;
                }
            }

            if (script.Status == ProgramState.ScriptStatus.Paused)
            {
                if (script.Delay == 0)
                {
                    script.Status = ProgramState.ScriptStatus.Ready;
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

    public void StepUntilPaused()
    {
        while (!Step())
        {
            
        }
    }

    public bool Step()
    {
        if (CurrentState.QueuedProgram != null)
        {
            //starting new episode
            var newProgram = CurrentState.QueuedProgram.Value;
            CurrentState.QueuedProgram = null;
            _program.SetActive(newProgram);
            return true;
        }

        var currentScript = CurrentState.GetRunningScript();
        if (currentScript != null)
        {
            return RunStep();
        }

        var nextScript = CurrentState.GetNextScript();
        if (nextScript == null)
        {
            if (CurrentState.Scripts.All(s => s.Status != ProgramState.ScriptStatus.Paused))
            {
                throw new Exception("All scripts in non-paused state");
            }
            
            OnGraphicalUpdate();

            return false;
        }

        if (nextScript.Type != ProgramState.ScriptType.KeyChar)
        {
            throw new Exception($"Non-char scripts not implemented yet: {nextScript.Type:G}");
        }
        
        nextScript.Status = ProgramState.ScriptStatus.Running;
        return true;
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
        
        var instructionOffsets = program.Instructions.Keys.OrderBy(k => k).ToList();
        var idx = instructionOffsets.FindIndex(k => k == curOffset);
        if (idx == instructionOffsets.Count - 1)
        {
            throw new Exception("Reached end of script");
        }
        
        var programPaused = false;
        var programStopped = false;
        var justJumped = false;

        var instruction = program.Instructions[curOffset];
        if (instruction is StopScriptInstruction)
        {
            programStopped = true;
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
                _log.Error("Unknown transition type " + setCharFrame.TransitionType.ToString("G"));
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
            programPaused = true;
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
            keyChar.LastProgramPoint = setCharBox.Num;
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

            if (moveCharToPos.TargetingAnotherCharacter)
            {
                throw new Exception("Targeting another character, not implemented");
            }

            var point = program.Points[moveCharToPos.Num];
            keyChar.PositionX = point.X;
            keyChar.PositionY = point.Y;
            keyChar.PositionZ = point.Z;
            keyChar.LastProgramPoint = moveCharToPos.Num;
            keyChar.IsFollowing = false;
            programPaused = true;
        } else if (instruction is StartTalkInstruction startTalk)
        {
            _log.Error("StartTalk not implemented yet"); //TODO:
            programPaused = true;
        } else if (instruction is SetCharDelayInstruction setCharDelay)
        {
            var keyCharScript = CurrentState.Scripts.FirstOrDefault(s => 
                s.Type == ProgramState.ScriptType.KeyChar && 
                s.Id == CurrentKeyChar
            );
            if (keyCharScript != null)
            {
                keyCharScript.Delay = setCharDelay.Delay;
            }
            programPaused = true;
        } else if (instruction is SetupWaitingCharInstruction setupWaitingChar)
        {
            _log.Error("SetupWaitingChar not implemented yet"); //TODO:
            programPaused = true;
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
            currentScript.Status = ProgramState.ScriptStatus.Paused;
            OnGraphicalUpdate();
        }
        if (!justJumped)
        {
            idx += 1;
            currentScript.Offset = instructionOffsets[idx];
        }

        return programPaused || programStopped;
    }
}