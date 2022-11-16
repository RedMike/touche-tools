using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;
using ToucheTools.Constants;
using ToucheTools.Models;
using ToucheTools.Models.Instructions;

namespace ToucheTools.App.State;

public class ActiveProgramState
{
    public class ProgramState
    {
        public class KeyChar
        {
            public bool Initialised { get; set; } = false;
            
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
            
            #region Items
            public short Money { get; set; }
            /// <summary>
            /// Only used to track amounts (e.g. money)
            /// </summary>
            public short[] CountedInventoryItems { get; set; } = new short[4];
            #endregion
        }

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
        
        public enum JumpReason
        {
            Unknown = 0,
            CharacterScript = 1,
            ActionScript = 2
        }
        
        public int CurrentProgram { get; set; } = 0;
        public int CurrentOffset { get; set; } = 0;
        
        #region Jumps
        public readonly List<(JumpReason, int)> JumpFrames = new List<(JumpReason, int)>();
        
        public bool IsInAJump()
        {
            return JumpFrames.Count != 0;
        }

        public void Jump(JumpReason reason, int offset)
        {
            var curOffset = CurrentOffset;
            CurrentOffset = offset;
            JumpFrames.Add((reason, curOffset));
            if (JumpFrames.Count > 500)
            {
                throw new Exception("Recursion overflow");
            }
        }

        public void JumpReturn()
        {
            if (!IsInAJump())
            {
                throw new Exception("Tried to return outside of a jump frame");
            }

            var (reason, offset) = JumpFrames.Last();
            JumpFrames.RemoveAt(JumpFrames.Count-1);
            CurrentOffset = offset;
        }
        #endregion
        
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
                KeyChars[id] = new KeyChar()
                {
                };
            }

            return KeyChars[id];
        }
        #endregion

        #region Inventory

        public InventoryList[] InventoryLists { get; set; } = new InventoryList[3];
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

        public void SetFlag(Flags.Known known, short val)
        {
            Flags[(ushort)known] = val;
        }
        #endregion

        public Dictionary<ushort, (int, int)> BackgroundOffsets { get; set; } = new Dictionary<ushort, (int, int)>();

        public ProgramState()
        {
            //values set from game code
            SetFlag(ToucheTools.Constants.Flags.Known.RndPalMinColour, 240);
            SetFlag(ToucheTools.Constants.Flags.Known.RndPalRandomRange, 16);
            SetFlag(ToucheTools.Constants.Flags.Known.RndPalMinDelay, 0);
            SetFlag(ToucheTools.Constants.Flags.Known.RndPalRandomDelay, 1);

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
    }

    private readonly DatabaseModel _model;
    private readonly ActiveProgram _program;
    private readonly LogData _log;

    public ActiveProgramState(DatabaseModel model, ActiveProgram program, LogData log)
    {
        _model = model;
        _program = program;
        _log = log;
        _program.ObserveActive(Update);
        Update();
    }

    public ProgramState CurrentState { get; set; } = new ProgramState();

    private void Update()
    {
        if (CurrentState.CurrentProgram != _program.Active)
        {
            CurrentState = new ProgramState()
            {
                CurrentProgram = _program.Active
            };
            CurrentState.Flags[0] = (short)_program.Active;
        }
    }
    
    public void Step()
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
        var justJumped = false;

        var instruction = program.Instructions[curOffset];
        if (instruction is StopScriptInstruction)
        {
            if (!CurrentState.IsInAJump())
            {
                throw new Exception("Reached end of script");
            }
            
            CurrentState.JumpReturn();
            justJumped = true;
        } else if (instruction is NoopInstruction)
        {
            
        } else if (instruction is FetchScriptWordInstruction fetchScriptWord)
        {
            CurrentState.SetStackValue((short)fetchScriptWord.Val);
        } else if (instruction is LoadSpriteInstruction loadSprite)
        {
            if (CurrentState.SpriteIndexToNum.ContainsKey(loadSprite.Index))
            {
                throw new Exception("Reloaded same sprite: " + loadSprite.Index);
            }
            CurrentState.SpriteIndexToNum[loadSprite.Index] = loadSprite.Num;
        } else if (instruction is LoadSequenceInstruction loadSequence)
        {
            if (CurrentState.SequenceIndexToNum.ContainsKey(loadSequence.Index))
            {
                throw new Exception("Reloaded same sequence: " + loadSequence.Index);
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

            var keyChar = CurrentState.GetKeyChar(initCharScript.Character);
            keyChar.Initialised = true;
            keyChar.SpriteIndex = initCharScript.SpriteIndex;
            keyChar.SequenceIndex = initCharScript.SequenceIndex;
            keyChar.Character = initCharScript.SequenceCharacterId;

            var cso = program.CharScriptOffsets.FirstOrDefault(x => x.Character == initCharScript.Character);
            if (cso != null)
            {
                var nextOffset = instructionOffsets[idx + 1];
                CurrentState.CurrentOffset = nextOffset;
                var jumpOffset = cso.Offs;
                CurrentState.Jump(ProgramState.JumpReason.CharacterScript, jumpOffset);
                justJumped = true;
            }
        } else if (instruction is LoadRoomInstruction loadRoom)
        {
            CurrentState.LoadedRoom = loadRoom.Num;
        } else if (instruction is SetCharFrameInstruction setCharFrame)
        {
            var keyChar = CurrentState.GetKeyChar(setCharFrame.Character);
            
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
            
        } else if (instruction is SetFlagInstruction setFlag)
        {
            var val = CurrentState.StackValue;
            CurrentState.Flags[setFlag.Flag] = (short)val;
            
            if (setFlag.Flag == 611 && val != 0)
            {
                //TODO: quits game
            } else if (setFlag.Flag == 612)
            {
                var newVal = 999; //TODO: randomly generate
                CurrentState.Flags[613] = (short)newVal;
            } //TODO: more
        } else if (instruction is SetCharBoxInstruction setCharBox)
        {
            var keyChar = CurrentState.GetKeyChar(setCharBox.Character);
            var point = program.Points[setCharBox.Num];
            keyChar.PositionX = point.X;
            keyChar.PositionY = point.Y;
            keyChar.PositionZ = point.Z;
            keyChar.LastProgramPoint = setCharBox.Num;
        } else if (instruction is InitCharInstruction initChar)
        {
            var keyChar = CurrentState.GetKeyChar(initChar.Character);
            keyChar.Anim1Start = 0;
            keyChar.Anim1Count = 1;
            keyChar.Anim2Start = 0;
            keyChar.Anim2Count = 1;
            keyChar.Anim3Start = 0;
            keyChar.Anim3Count = 1;
            keyChar.Direction = 0;
        } else if (instruction is MoveCharToPosInstruction moveCharToPos)
        {
            if (!CurrentState.KeyChars.ContainsKey(moveCharToPos.Character))
            {
                CurrentState.KeyChars[moveCharToPos.Character] = new ProgramState.KeyChar();
            }
            var keyChar = CurrentState.KeyChars[moveCharToPos.Character];

            if (moveCharToPos.TargetingAnotherCharacter)
            {
                throw new Exception("Targeting another character, not implemented");
            }

            var point = program.Points[moveCharToPos.Num];
            keyChar.PositionX = point.X;
            keyChar.PositionY = point.Y;
            keyChar.PositionZ = point.Z;
            keyChar.LastProgramPoint = moveCharToPos.Num;
        } else if (instruction is AddRoomAreaInstruction addRoomArea)
        {
            if (!CurrentState.Flags.ContainsKey(addRoomArea.Flag))
            {
                _log.Error($"Flag {addRoomArea.Flag} value required but not known");
            }
            if (!CurrentState.Flags.ContainsKey((ushort)(addRoomArea.Flag + 1)))
            {
                _log.Error($"Flag {(addRoomArea.Flag + 1)} value required but not known");
            }

            if (CurrentState.Flags[addRoomArea.Flag] == 20000)
            {
                throw new Exception("Room scroll offset roll back");
                //TODO: room scroll offset roll back
            }

            var x = CurrentState.Flags[addRoomArea.Flag];
            var y = CurrentState.Flags[(ushort)(addRoomArea.Flag + 1)];
            CurrentState.BackgroundOffsets[addRoomArea.Num] = (x, y);
        } else if (instruction is GetFlagInstruction getFlag)
        {
            var flagVal = 0;
            if (CurrentState.Flags.ContainsKey(getFlag.Flag))
            {
                flagVal = CurrentState.Flags[getFlag.Flag];
            }
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
            var keyChar = CurrentState.GetKeyChar(getInventoryItem.Character);
            var val = keyChar.Money;
            if (!getInventoryItem.MoneyItem)
            {
                val = keyChar.CountedInventoryItems[getInventoryItem.Item];
            }
            CurrentState.SetStackValue(val);
        } else if (instruction is SetInventoryItemInstruction setInventoryItem)
        {
            var keyChar = CurrentState.GetKeyChar(setInventoryItem.Character);
            var val = CurrentState.StackValue;
            if (setInventoryItem.MoneyItem)
            {
                keyChar.Money = val;
            }
            else
            {
                keyChar.CountedInventoryItems[setInventoryItem.Item] = val;
            }
        } else if (instruction is AddItemToInventoryAndRedrawInstruction addItemToInventory)
        {
            var keyChar = CurrentState.GetKeyChar(addItemToInventory.Character);
            var item = CurrentState.StackValue;
            if (item == 0)
            {
                _log.Error("TODO: re-sort inventory items"); //TODO: just re-sort the inventory items
            } else if (item == 1)
            {
                //it's really about money
                keyChar.Money += CurrentState.GetFlag(Flags.Known.CurrentMoney);
            }
            else
            {
                //it's about adding an item
                if (addItemToInventory.Character >= CurrentState.InventoryLists.Length)
                {
                    throw new Exception("Adding inventory to non-existent list");
                }

                var inventoryList = CurrentState.InventoryLists[addItemToInventory.Character];
                inventoryList.PrependItem(item);
            }
        }
        else
        {
            _log.Error($"Unhandled instruction type: {instruction.Opcode:G}");
        }

        if (!justJumped)
        {
            idx += 1;
            CurrentState.CurrentOffset = instructionOffsets[idx];
        }
    }
}