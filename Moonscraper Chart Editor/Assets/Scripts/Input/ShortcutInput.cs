using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Shortcut
{
    ActionHistoryRedo,
    ActionHistoryUndo,

    AddSongObject,

    BpmIncrease,
    BpmDecrease,

    ChordSelect,

    ClipboardCopy,
    ClipboardCut,
    ClipboardPaste,

    Delete, 

    FileLoad,
    FileNew,
    FileSave,
    FileSaveAs,

    MoveStepPositive,
    MoveStepNegative,
    MoveMeasurePositive,
    MoveMeasureNegative,

    NoteSetNatural,
    NoteSetStrum,
    NoteSetHopo,
    NoteSetTap,
    
    PlayPause,

    SelectAll,
    SelectAllSection,
    StepDecrease,
    StepIncrease,

    SectionJumpPositive,
    SectionJumpNegative,
    SectionJumpMouseScroll,

    ToggleBpmAnchor,
    ToggleClap,
    ToggleExtendedSustains,
    ToggleMetronome,
    ToggleMouseMode,
    ToggleNoteForced,
    ToggleNoteTap,
    ToggleViewMode, 
    
    ToolNoteBurst,
    ToolNoteHold,
    ToolSelectCursor,
    ToolSelectEraser,
    ToolSelectNote,
    ToolSelectStarpower,
    ToolSelectBpm,
    ToolSelectTimeSignature,
    ToolSelectSection,
    ToolSelectEvent,
}

public static class ShortcutInput {

    class ShortcutLUT
    {
        List<KeyCode>[] keyCodes;

        public ShortcutLUT()
        {
            keyCodes = new List<KeyCode>[System.Enum.GetValues(typeof(Shortcut)).Length];
        }

        public void Insert(Shortcut key, List<KeyCode> entry)
        {
            keyCodes[(int)key] = entry;
        }

        public List<KeyCode> GetKeyCodes(Shortcut key)
        {
            return keyCodes[(int)key];
        }
    }

    static ShortcutLUT generalInputs;
    static ShortcutLUT modifierInputs;
    static ShortcutLUT secondaryInputs;
    static ShortcutLUT secondaryModifierInputs;
    static ShortcutLUT alternativeInputs;
    static ShortcutInput()
    {     
        {
            generalInputs = new ShortcutLUT();
            generalInputs.Insert(Shortcut.AddSongObject,                   new List<KeyCode> { KeyCode.Alpha1, });
            generalInputs.Insert(Shortcut.BpmIncrease,                     new List<KeyCode> { KeyCode.Equals, });
            generalInputs.Insert(Shortcut.BpmDecrease,                     new List<KeyCode> { KeyCode.Minus, });
            generalInputs.Insert(Shortcut.Delete ,                         new List<KeyCode> { KeyCode.Delete });
            generalInputs.Insert(Shortcut.PlayPause ,                      new List<KeyCode> { KeyCode.Space });
            generalInputs.Insert(Shortcut.MoveStepPositive ,               new List<KeyCode> { KeyCode.UpArrow });
            generalInputs.Insert(Shortcut.MoveStepNegative ,               new List<KeyCode> { KeyCode.DownArrow } );
            generalInputs.Insert(Shortcut.MoveMeasurePositive ,            new List<KeyCode> { KeyCode.PageUp } );
            generalInputs.Insert(Shortcut.MoveMeasureNegative ,            new List<KeyCode> { KeyCode.PageDown } );
            generalInputs.Insert(Shortcut.NoteSetNatural ,                 new List<KeyCode> { KeyCode.X } );
            generalInputs.Insert(Shortcut.NoteSetStrum ,                   new List<KeyCode> { KeyCode.S } );
            generalInputs.Insert(Shortcut.NoteSetHopo ,                    new List<KeyCode> { KeyCode.H } );
            generalInputs.Insert(Shortcut.NoteSetTap ,                     new List<KeyCode> { KeyCode.T } );
            generalInputs.Insert(Shortcut.StepIncrease ,                   new List<KeyCode> { KeyCode.W, KeyCode.RightArrow } );
            generalInputs.Insert(Shortcut.StepDecrease ,                   new List<KeyCode> { KeyCode.Q, KeyCode.LeftArrow } );
            generalInputs.Insert(Shortcut.ToggleBpmAnchor ,                new List<KeyCode> { KeyCode.A } );
            generalInputs.Insert(Shortcut.ToggleClap ,                     new List<KeyCode> { KeyCode.N } );
            generalInputs.Insert(Shortcut.ToggleExtendedSustains,          new List<KeyCode> { KeyCode.E } );
            generalInputs.Insert(Shortcut.ToggleMetronome ,                new List<KeyCode> { KeyCode.M } );
            generalInputs.Insert(Shortcut.ToggleMouseMode ,                new List<KeyCode> { KeyCode.BackQuote } );
            generalInputs.Insert(Shortcut.ToggleNoteForced ,               new List<KeyCode> { KeyCode.F } );
            generalInputs.Insert(Shortcut.ToggleNoteTap ,                  new List<KeyCode> { KeyCode.T } );
            generalInputs.Insert(Shortcut.ToggleViewMode ,                 new List<KeyCode> { KeyCode.G } );
            generalInputs.Insert(Shortcut.ToolNoteBurst ,                  new List<KeyCode> { KeyCode.B } );
            generalInputs.Insert(Shortcut.ToolNoteHold ,                   new List<KeyCode> { KeyCode.H } );
            generalInputs.Insert(Shortcut.ToolSelectCursor ,               new List<KeyCode> { KeyCode.J } );
            generalInputs.Insert(Shortcut.ToolSelectEraser ,               new List<KeyCode> { KeyCode.K } );
            generalInputs.Insert(Shortcut.ToolSelectNote ,                 new List<KeyCode> { KeyCode.Y } );
            generalInputs.Insert(Shortcut.ToolSelectStarpower ,            new List<KeyCode> { KeyCode.U } );
            generalInputs.Insert(Shortcut.ToolSelectBpm ,                  new List<KeyCode> { KeyCode.I } );
            generalInputs.Insert(Shortcut.ToolSelectTimeSignature ,        new List<KeyCode> { KeyCode.O } );
            generalInputs.Insert(Shortcut.ToolSelectSection ,              new List<KeyCode> { KeyCode.P } );
            generalInputs.Insert(Shortcut.ToolSelectEvent ,                new List<KeyCode> { KeyCode.L } );
        }

        {
            modifierInputs = new ShortcutLUT();
            modifierInputs.Insert(Shortcut.ClipboardCopy,                   new List<KeyCode> { KeyCode.C } );
            modifierInputs.Insert(Shortcut.ClipboardCut,                    new List<KeyCode> { KeyCode.X } );
            modifierInputs.Insert(Shortcut.ClipboardPaste,                  new List<KeyCode> { KeyCode.V } );
            modifierInputs.Insert(Shortcut.FileLoad,                        new List<KeyCode> { KeyCode.O } );
            modifierInputs.Insert(Shortcut.FileNew,                         new List<KeyCode> { KeyCode.N } );
            modifierInputs.Insert(Shortcut.FileSave,                        new List<KeyCode> { KeyCode.S } );
            modifierInputs.Insert(Shortcut.ActionHistoryRedo,               new List<KeyCode> { KeyCode.Y } );
            modifierInputs.Insert(Shortcut.ActionHistoryUndo,               new List<KeyCode> { KeyCode.Z } );
            modifierInputs.Insert(Shortcut.SelectAll,                       new List<KeyCode> { KeyCode.A });
        }

        {
            secondaryInputs = new ShortcutLUT();
            secondaryInputs.Insert(Shortcut.ChordSelect, new List<KeyCode> { KeyCode.LeftShift, KeyCode.RightShift, });
        }

        {
            secondaryModifierInputs = new ShortcutLUT();
            secondaryModifierInputs.Insert(Shortcut.FileSaveAs,                      new List<KeyCode> { KeyCode.S });
            secondaryModifierInputs.Insert(Shortcut.ActionHistoryRedo,               new List<KeyCode> { KeyCode.Z });
        }

        {
            alternativeInputs = new ShortcutLUT();
            alternativeInputs.Insert(Shortcut.SectionJumpPositive, new List<KeyCode> { KeyCode.UpArrow });
            alternativeInputs.Insert(Shortcut.SectionJumpNegative, new List<KeyCode> { KeyCode.DownArrow });
            alternativeInputs.Insert(Shortcut.SelectAllSection, new List<KeyCode> { KeyCode.A });
            alternativeInputs.Insert(Shortcut.SectionJumpMouseScroll, new List<KeyCode> { KeyCode.LeftAlt, KeyCode.RightAlt });
        }
    }
    /*
    static Dictionary<Shortcut, List<KeyCode>> generalInputs = new Dictionary<Shortcut, List<KeyCode>>
    {
        { Shortcut.AddSongObject ,                  new List<KeyCode> { KeyCode.Alpha1,                         } },

        { Shortcut.BpmIncrease ,                    new List<KeyCode> { KeyCode.Equals,                         } },
        { Shortcut.BpmDecrease ,                    new List<KeyCode> { KeyCode.Minus,                          } },

        { Shortcut.Delete ,                         new List<KeyCode> { KeyCode.Delete                          } },
        { Shortcut.PlayPause ,                      new List<KeyCode> { KeyCode.Space                           } },

        { Shortcut.MoveStepPositive ,               new List<KeyCode> { KeyCode.UpArrow                         } },
        { Shortcut.MoveStepNegative ,               new List<KeyCode> { KeyCode.DownArrow                       } },
        { Shortcut.MoveMeasurePositive ,            new List<KeyCode> { KeyCode.PageUp                          } },
        { Shortcut.MoveMeasureNegative ,            new List<KeyCode> { KeyCode.PageDown                        } },

        { Shortcut.NoteSetNatural ,                 new List<KeyCode> { KeyCode.X                               } },
        { Shortcut.NoteSetStrum ,                   new List<KeyCode> { KeyCode.S                               } },
        { Shortcut.NoteSetHopo ,                    new List<KeyCode> { KeyCode.H                               } },
        { Shortcut.NoteSetTap ,                     new List<KeyCode> { KeyCode.T                               } },

        { Shortcut.StepIncrease ,                   new List<KeyCode> { KeyCode.W,          KeyCode.RightArrow  } },
        { Shortcut.StepDecrease ,                   new List<KeyCode> { KeyCode.Q,          KeyCode.LeftArrow   } },

        { Shortcut.ToggleBpmAnchor ,                new List<KeyCode> { KeyCode.A                               } },
        { Shortcut.ToggleClap ,                     new List<KeyCode> { KeyCode.N                               } },
        { Shortcut.ToggleExtendedSustains,          new List<KeyCode> { KeyCode.E                               } },
        { Shortcut.ToggleMetronome ,                new List<KeyCode> { KeyCode.M                               } },
        { Shortcut.ToggleMouseMode ,                new List<KeyCode> { KeyCode.BackQuote                       } },
        { Shortcut.ToggleNoteForced ,               new List<KeyCode> { KeyCode.F                               } },
        { Shortcut.ToggleNoteTap ,                  new List<KeyCode> { KeyCode.T                               } },
        { Shortcut.ToggleViewMode ,                 new List<KeyCode> { KeyCode.G                               } },

        { Shortcut.ToolNoteBurst ,                  new List<KeyCode> { KeyCode.B                               } },
        { Shortcut.ToolNoteHold ,                   new List<KeyCode> { KeyCode.H                               } },
        { Shortcut.ToolSelectCursor ,               new List<KeyCode> { KeyCode.J                               } },
        { Shortcut.ToolSelectEraser ,               new List<KeyCode> { KeyCode.K                               } },
        { Shortcut.ToolSelectNote ,                 new List<KeyCode> { KeyCode.Y                               } },
        { Shortcut.ToolSelectStarpower ,            new List<KeyCode> { KeyCode.U                               } },
        { Shortcut.ToolSelectBpm ,                  new List<KeyCode> { KeyCode.I                               } },
        { Shortcut.ToolSelectTimeSignature ,        new List<KeyCode> { KeyCode.O                               } },
        { Shortcut.ToolSelectSection ,              new List<KeyCode> { KeyCode.P                               } },
        { Shortcut.ToolSelectEvent ,                new List<KeyCode> { KeyCode.L                               } },
    };
    static Dictionary<Shortcut, List<KeyCode>> modifierInputs = new Dictionary<Shortcut, List<KeyCode>>
    {
        { Shortcut.ClipboardCopy,                   new List<KeyCode> { KeyCode.C } },
        { Shortcut.ClipboardCut,                    new List<KeyCode> { KeyCode.X } },
        { Shortcut.ClipboardPaste,                  new List<KeyCode> { KeyCode.V } },

        { Shortcut.FileLoad,                        new List<KeyCode> { KeyCode.O } },
        { Shortcut.FileNew,                         new List<KeyCode> { KeyCode.N } },
        { Shortcut.FileSave,                        new List<KeyCode> { KeyCode.S } },

        { Shortcut.ActionHistoryRedo,               new List<KeyCode> { KeyCode.Y } },
        { Shortcut.ActionHistoryUndo,               new List<KeyCode> { KeyCode.Z } },

        { Shortcut.SelectAll,                       new List<KeyCode> { KeyCode.A } },
    };
    static Dictionary<Shortcut, List<KeyCode>> secondaryInputs = new Dictionary<Shortcut, List<KeyCode>>
    {
        { Shortcut.ChordSelect ,                    new List<KeyCode> { KeyCode.LeftShift,  KeyCode.RightShift, } },
    };
    static Dictionary<Shortcut, List<KeyCode>> secondaryModifierInputs = new Dictionary<Shortcut, List<KeyCode>>
    {
        { Shortcut.FileSaveAs,                      new List<KeyCode> { KeyCode.S } },
        { Shortcut.ActionHistoryRedo,               new List<KeyCode> { KeyCode.Z } },      
    };
    static Dictionary<Shortcut, List<KeyCode>> alternativeInputs = new Dictionary<Shortcut, List<KeyCode>>
    {
        { Shortcut.SectionJumpPositive ,            new List<KeyCode> { KeyCode.UpArrow } },
        { Shortcut.SectionJumpNegative ,            new List<KeyCode> { KeyCode.DownArrow } },
        { Shortcut.SelectAllSection ,               new List<KeyCode> { KeyCode.A } },
        { Shortcut.SectionJumpMouseScroll ,         new List<KeyCode> { KeyCode.LeftAlt, KeyCode.RightAlt } },
    };*/

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public static bool modifierInput { get { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); } }
    public static bool secondaryInput { get { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); } }
    public static bool alternativeInput { get { return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt); } }

    static bool TryGetKeyCodes(Shortcut key, out List<KeyCode> keyCode)
    {
        keyCode = null;

        bool modifierInputActive = modifierInput;
        bool secondaryInputActive = secondaryInput;
        bool alternativeInputActive = alternativeInput;

        ShortcutLUT inputDict = generalInputs;

        if (modifierInputActive && secondaryInputActive)
        {
            inputDict = secondaryModifierInputs;
        }
        else if (modifierInputActive)
        {
            inputDict = modifierInputs;
        }
        else if (secondaryInputActive)
        {
            inputDict = secondaryInputs;
        }
        else if (alternativeInputActive)
        {
            inputDict = alternativeInputs;
        }
        else if (Services.IsTyping)
        {
            return false;
        }

        keyCode = inputDict.GetKeyCodes(key);
        return keyCode != null;
    }

    delegate bool InputFn(KeyCode keyCode);
    static bool CheckInput(Shortcut key, InputFn InputFn)
    {
        List<KeyCode> keyCodes;

        if (TryGetKeyCodes(key, out keyCodes))
        {
            foreach (KeyCode keyCode in keyCodes)
            {
                if (InputFn(keyCode))
                    return true;
            }
        }

        return false;
    }

    public static bool GetInputDown(Shortcut key)
    {
        return CheckInput(key, Input.GetKeyDown);
    }

    public static bool GetInputUp(Shortcut key)
    {
        return CheckInput(key, Input.GetKeyUp);
    }

    public static bool GetInput(Shortcut key)
    {
        return CheckInput(key, Input.GetKey);
    }

    public static bool GetGroupInputDown(Shortcut[] keys)
    {
        foreach (Shortcut key in keys)
        {
            if (CheckInput(key, Input.GetKeyDown))
                return true;
        }

        return false;
    }

    public static bool GetGroupInputUp(Shortcut[] keys)
    {
        foreach (Shortcut key in keys)
        {
            if (CheckInput(key, Input.GetKeyUp))
                return true;
        }

        return false;
    }

    public static bool GetGroupInput(Shortcut[] keys)
    {
        foreach (Shortcut key in keys)
        {
            if (CheckInput(key, Input.GetKey))
                return true;
        }

        return false;
    }
}
