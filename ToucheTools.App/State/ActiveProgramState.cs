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
    
    public class ProgramState
    {
        public class KeyChar
        {
            public bool Initialised { get; set; } = false;

            public void Init()
            {
                Initialised = false;
                Direction = 0;
                Anim1Count = 0;
                Anim1Start = 0;
                Anim2Count = 0;
                Anim2Start = 0;
                Anim3Count = 0;
                Anim3Start = 0;
                ScriptPaused = false;
                ScriptStopped = false;
                LastProgramPoint = null;
                IsFollowing = false;
                IsSelectable = false;
                OffScreen = false;
                PositionX = 10;
                SpriteIndex = null;
                SequenceIndex = null;
                Character = null;
            }

            public bool ScriptPaused { get; set; } = false;
            public bool ScriptStopped { get; set; } = false;
            public bool IsSelectable { get; set; } = false; //unsure?
            public bool OffScreen { get; set; } = false; //unsure?
            public bool IsFollowing { get; set; } = false;
            
            #region Graphics
            public int? SpriteIndex { get; set; }
            public int? SequenceIndex { get; set; }
            public int? Character { get; set; } //within sequence
            #endregion
            
            #region Animation
            public int Anim1Start { get; set; }
            public int Anim1Count { get; set; }
            public int Anim2Start { get; set; }
            public int Anim2Count { get; set; }
            public int Anim3Start { get; set; }
            public int Anim3Count { get; set; }
            #endregion
            
            #region Position
            public ushort? LastProgramPoint { get; set; }
            public int? PositionX { get; set; }
            public int? PositionY { get; set; }
            public int? PositionZ { get; set; }
            public int Direction { get; set; }
            #endregion
            
            //TODO: items shold not be in the program-based state
            #region Items
            public short Money { get; set; }
            /// <summary>
            /// Only used to track amounts (e.g. money)
            /// </summary>
            public short[] CountedInventoryItems { get; set; } = new short[4];
            #endregion
        }


        public enum RunMode
        {
            Unknown = 0,
            DoneCharacterScript = 1,
            CharacterScript = 2,
            DoneActionScript = 3,
            ActionScript = 4,
            WaitingForPlayer = 5,
        }

        public RunMode CurrentRunMode { get; set; } = RunMode.Unknown;
        public int CurrentKeyCharScript { get; set; } = -1;
        
        public int CurrentProgram { get; set; } = 0;
        public int CurrentOffset { get; set; } = 0; //TODO: one per character script

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
        public Dictionary<int, int> SpriteIndexToNum { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, int> SequenceIndexToNum { get; set; } = new Dictionary<int, int>();

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

        Init();
        
        _program.ObserveActive(Update);
        Update();
    }

    private void Init()
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
    }

    public ProgramState CurrentState { get; set; } = new ProgramState();
    
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
    public Dictionary<int, ProgramState.KeyChar> KeyChars { get; set; } = new Dictionary<int, ProgramState.KeyChar>();
    public short CurrentKeyChar => GetFlag(ToucheTools.Constants.Flags.Known.CurrentKeyChar);

    public ProgramState.KeyChar GetKeyChar(int id)
    {
        if (id == 256 && CurrentKeyChar != 256)
        {
            return GetKeyChar(CurrentKeyChar);
        }

        if (!KeyChars.ContainsKey(id))
        {
            KeyChars[id] = new ProgramState.KeyChar()
            {
            };
        }

        return KeyChars[id];
    }
    #endregion
    
    private void Update()
    {
        if (CurrentState.CurrentProgram != _program.Active)
        {
            CurrentState = new ProgramState()
            {
                CurrentProgram = _program.Active,
                CurrentRunMode = ProgramState.RunMode.CharacterScript,
                CurrentKeyCharScript = GetFlag(ToucheTools.Constants.Flags.Known.CurrentKeyChar),
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
            foreach (var (keyCharId, keyChar) in KeyChars)
            {
                keyChar.Init();
            }
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
        
        if (CurrentState.CurrentRunMode == ProgramState.RunMode.DoneCharacterScript)
        {
            //iterate over character scripts
            CurrentState.CurrentOffset = -1;
            CurrentState.CurrentRunMode = ProgramState.RunMode.CharacterScript;
        }

        if (CurrentState.CurrentRunMode == ProgramState.RunMode.CharacterScript)
        {
            if (CurrentState.CurrentOffset < 0)
            {
                //find the next keychar's script to run
                var newOffset = -1;
                var newKeyCharId = -1;
                var program = _model.Programs[_program.Active];
                
                foreach (var (keyCharId, keyChar) in KeyChars.OrderBy(p => p.Key))
                {
                    var cso = program.CharScriptOffsets.FirstOrDefault(x => x.Character == keyCharId);
                    if (cso != null)
                    {
                        if (!keyChar.ScriptStopped)
                        {
                            newKeyCharId = keyCharId;
                            newOffset = cso.Offs;
                        }
                    }
                    else
                    {
                        keyChar.ScriptStopped = true;
                    }
                }

                if (newOffset == -1)
                {
                    CurrentState.CurrentOffset = -1;
                    CurrentState.CurrentRunMode = ProgramState.RunMode.DoneCharacterScript;
                }
                else
                {
                    CurrentState.CurrentKeyCharScript = newKeyCharId;
                    CurrentState.CurrentOffset = newOffset;
                }
            }
            
            if (CurrentState.CurrentRunMode == ProgramState.RunMode.DoneCharacterScript)
            {
                CurrentState.CurrentRunMode = ProgramState.RunMode.WaitingForPlayer;
            }
            else
            {
                return RunStep();
            }
        }

        if (CurrentState.CurrentRunMode == ProgramState.RunMode.WaitingForPlayer)
        {
            //nothing to do
            return true;
        }
        
        throw new Exception($"Unknown run mode: {CurrentState.CurrentRunMode:G}");
    }
    
    public bool RunStep()
    {
        var program = _model.Programs[_program.Active];
        var curOffset = CurrentState.CurrentOffset;
        
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
            if (CurrentState.SpriteIndexToNum.ContainsKey(loadSprite.Index))
            {
                _log.Info("Reloaded same sprite: " + loadSprite.Index + " was: " + CurrentState.SpriteIndexToNum[loadSprite.Index] + " + now: " + loadSprite.Num);
            }
            CurrentState.SpriteIndexToNum[loadSprite.Index] = loadSprite.Num;
        } else if (instruction is LoadSequenceInstruction loadSequence)
        {
            if (CurrentState.SequenceIndexToNum.ContainsKey(loadSequence.Index))
            {
                _log.Info("Reloaded same sequence: " + loadSequence.Index + " was: " + CurrentState.SequenceIndexToNum[loadSequence.Index] + " + now: " + loadSequence.Num);
            }
            CurrentState.SequenceIndexToNum[loadSequence.Index] = loadSequence.Num;
        } else if (instruction is InitCharScriptInstruction initCharScript)
        {
            if (!CurrentState.SpriteIndexToNum.ContainsKey(initCharScript.SpriteIndex))
            {
                throw new Exception("Sprite not loaded yet: " + initCharScript.SpriteIndex);
            }
            if (!CurrentState.SequenceIndexToNum.ContainsKey(initCharScript.SequenceIndex))
            {
                throw new Exception("Sequence not loaded yet: " + initCharScript.SequenceIndex);
            }

            var keyChar = GetKeyChar(initCharScript.Character);
            keyChar.Initialised = true;
            keyChar.SpriteIndex = initCharScript.SpriteIndex;
            keyChar.SequenceIndex = initCharScript.SequenceIndex;
            keyChar.Character = initCharScript.SequenceCharacterId;
        } else if (instruction is LoadRoomInstruction loadRoom)
        {
            CurrentState.LoadedRoom = loadRoom.Num;
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
                _log.Error("Unknown transition type " + setCharFrame.TransitionType.ToString("G"));
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
            keyChar.Init();
            keyChar.Initialised = true;
            keyChar.Anim1Start = 0;
            keyChar.Anim1Count = 1;
            keyChar.Anim2Start = 0;
            keyChar.Anim2Count = 1;
            keyChar.Anim3Start = 0;
            keyChar.Anim3Count = 1;
            keyChar.Direction = 0;
        } else if (instruction is MoveCharToPosInstruction moveCharToPos)
        {
            if (!KeyChars.ContainsKey(moveCharToPos.Character))
            {
                KeyChars[moveCharToPos.Character] = new ProgramState.KeyChar();
            }
            var keyChar = KeyChars[moveCharToPos.Character];

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
            _log.Error("SetCharDelay not implemented yet"); //TODO:
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
                CurrentState.CurrentOffset = jz.NewOffset;
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
            keyChar.ScriptStopped |= (setCharFlags.Flags & 0x01) != 0;
            keyChar.ScriptPaused |= (setCharFlags.Flags & 0x02) != 0;
            keyChar.IsFollowing |= (setCharFlags.Flags & 0x10) != 0;
            keyChar.IsSelectable |= (setCharFlags.Flags & 0x4000) != 0;
            keyChar.OffScreen |= (setCharFlags.Flags & 0x8000) != 0;
        } else if (instruction is UnsetCharFlagsInstruction unsetCharFlags)
        {
            var keyChar = GetKeyChar(unsetCharFlags.Character);
            keyChar.ScriptStopped &= (unsetCharFlags.Flags & 0x01) == 0;
            keyChar.ScriptPaused &= (unsetCharFlags.Flags & 0x02) == 0;
            keyChar.IsFollowing &= (unsetCharFlags.Flags & 0x10) == 0;
            keyChar.IsSelectable &= (unsetCharFlags.Flags & 0x4000) == 0;
            keyChar.OffScreen &= (unsetCharFlags.Flags & 0x8000) == 0;
        }
        else
        {
            _log.Error($"Unhandled instruction type: {instruction.Opcode:G}");
        }

        if (programStopped)
        {
            _log.Info($"Finished running {CurrentState.CurrentRunMode:G}.");

            if (CurrentState.CurrentRunMode == ProgramState.RunMode.CharacterScript)
            {
                if (CurrentState.CurrentKeyCharScript < 0)
                {
                    throw new Exception("Missing key char index");
                }
                var keyChar = GetKeyChar(CurrentState.CurrentKeyCharScript);
                
                keyChar.ScriptStopped = true;
                CurrentState.CurrentOffset = -1;
                justJumped = true;
            }
        }
        
        if (programPaused)
        {
            if (CurrentState.CurrentRunMode == ProgramState.RunMode.CharacterScript)
            {
                if (CurrentState.CurrentKeyCharScript < 0)
                {
                    throw new Exception("Missing key char index");
                }
                var keyChar = GetKeyChar(CurrentState.CurrentKeyCharScript);
                
                keyChar.ScriptPaused = true;
            }
            
            if (GetFlag(ToucheTools.Constants.Flags.Known.DisableRoomScroll) == 0 && CurrentState.LoadedRoom != null)
            {
                //center to current keychar
                var roomImageNum = _model.Rooms[CurrentState.LoadedRoom.Value].RoomImageNum;
                var roomImage = _model.RoomImages[roomImageNum].Value;
                var roomImageWidth = roomImage.RoomWidth;
                var roomImageHeight = roomImage.Height;
                
                var keyChar = GetKeyChar(CurrentKeyChar);
                var (x, y) = (keyChar.PositionX ?? 0, keyChar.PositionY ?? 0);

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
        }
        if (!justJumped)
        {
            idx += 1;
            CurrentState.CurrentOffset = instructionOffsets[idx];
        }   

        return programPaused || programStopped;
    }
}