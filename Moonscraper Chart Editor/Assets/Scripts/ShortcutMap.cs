using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Shortcut
{
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

    NoteSetNatural,
    NoteSetStrum,
    NoteSetHopo,
    NoteSetTap,
    
    PlayPause,
    Redo,

    SelectAll,
    StepDecrease,
    StepIncrease,

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

    Undo,
}

public static class ShortcutMap {
    static Dictionary<Shortcut, KeyCode[]> generalInputs = new Dictionary<Shortcut, KeyCode[]>
    {
        { Shortcut.AddSongObject ,                  new KeyCode[] { KeyCode.Alpha1,                         } },

        { Shortcut.BpmIncrease ,                    new KeyCode[] { KeyCode.Equals,                         } },
        { Shortcut.BpmDecrease ,                    new KeyCode[] { KeyCode.Minus,                          } },

        { Shortcut.Delete ,                         new KeyCode[] { KeyCode.Delete                          } },
        { Shortcut.PlayPause ,                      new KeyCode[] { KeyCode.Space                           } },

        { Shortcut.NoteSetNatural ,                 new KeyCode[] { KeyCode.X                               } },
        { Shortcut.NoteSetStrum ,                   new KeyCode[] { KeyCode.S                               } },
        { Shortcut.NoteSetHopo ,                    new KeyCode[] { KeyCode.H                               } },
        { Shortcut.NoteSetTap ,                     new KeyCode[] { KeyCode.T                               } },

        { Shortcut.StepIncrease ,                   new KeyCode[] { KeyCode.W,          KeyCode.RightArrow  } },
        { Shortcut.StepDecrease ,                   new KeyCode[] { KeyCode.Q,          KeyCode.LeftArrow   } },

        { Shortcut.ToggleBpmAnchor ,                new KeyCode[] { KeyCode.A                               } },
        { Shortcut.ToggleClap ,                     new KeyCode[] { KeyCode.N                               } },
        { Shortcut.ToggleExtendedSustains,          new KeyCode[] { KeyCode.E                               } },
        { Shortcut.ToggleMetronome ,                new KeyCode[] { KeyCode.M                               } },
        { Shortcut.ToggleMouseMode ,                new KeyCode[] { KeyCode.BackQuote                       } },
        { Shortcut.ToggleNoteForced ,               new KeyCode[] { KeyCode.F                               } },
        { Shortcut.ToggleNoteTap ,                  new KeyCode[] { KeyCode.T                               } },
        { Shortcut.ToggleViewMode ,                 new KeyCode[] { KeyCode.G                               } },

        { Shortcut.ToolNoteBurst ,                  new KeyCode[] { KeyCode.B                               } },
        { Shortcut.ToolNoteHold ,                   new KeyCode[] { KeyCode.H                               } },
        { Shortcut.ToolSelectCursor ,               new KeyCode[] { KeyCode.J                               } },
        { Shortcut.ToolSelectEraser ,               new KeyCode[] { KeyCode.K                               } },
        { Shortcut.ToolSelectNote ,                 new KeyCode[] { KeyCode.Y                               } },
        { Shortcut.ToolSelectStarpower ,            new KeyCode[] { KeyCode.U                               } },
        { Shortcut.ToolSelectBpm ,                  new KeyCode[] { KeyCode.I                               } },
        { Shortcut.ToolSelectTimeSignature ,        new KeyCode[] { KeyCode.O                               } },
        { Shortcut.ToolSelectSection ,              new KeyCode[] { KeyCode.P                               } },
        { Shortcut.ToolSelectEvent ,                new KeyCode[] { KeyCode.L                               } },
    };
    static Dictionary<Shortcut, KeyCode[]> modifierInputs = new Dictionary<Shortcut, KeyCode[]>
    {
        { Shortcut.ClipboardCopy,                   new KeyCode[] { KeyCode.C } },
        { Shortcut.ClipboardCut,                    new KeyCode[] { KeyCode.X } },
        { Shortcut.ClipboardPaste,                  new KeyCode[] { KeyCode.V } },

        { Shortcut.FileLoad,                        new KeyCode[] { KeyCode.O } },
        { Shortcut.FileNew,                         new KeyCode[] { KeyCode.N } },
        { Shortcut.FileSave,                        new KeyCode[] { KeyCode.S } },

        { Shortcut.Redo,                            new KeyCode[] { KeyCode.Y } },
        
        { Shortcut.SelectAll,                       new KeyCode[] { KeyCode.A } },
        { Shortcut.Undo,                            new KeyCode[] { KeyCode.Z } },
    };
    static Dictionary<Shortcut, KeyCode[]> secondaryInputs = new Dictionary<Shortcut, KeyCode[]>
    {
        { Shortcut.ChordSelect ,                    new KeyCode[] { KeyCode.LeftShift,  KeyCode.RightShift, } },
    };
    static Dictionary<Shortcut, KeyCode[]> secondaryModifierInputs = new Dictionary<Shortcut, KeyCode[]>
    {
        { Shortcut.FileSaveAs,                      new KeyCode[] { KeyCode.S } },

        { Shortcut.Redo,                            new KeyCode[] { KeyCode.Z } },      
    };

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    static bool TryGetKeyCodes(Shortcut key, out KeyCode[] keyCode)
    {
        keyCode = new KeyCode[0];

        bool modifierInputActive = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightCommand);
        bool secondaryInputActive = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        Dictionary<Shortcut, KeyCode[]> inputDict = generalInputs;

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
        else if (Services.IsTyping)
        {
            return false;
        }

        return (inputDict.TryGetValue(key, out keyCode));
    }

    delegate bool InputFn(KeyCode keyCode);
    static bool CheckInput(Shortcut key, InputFn InputFn)
    {
        KeyCode[] keyCodes;

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
}
