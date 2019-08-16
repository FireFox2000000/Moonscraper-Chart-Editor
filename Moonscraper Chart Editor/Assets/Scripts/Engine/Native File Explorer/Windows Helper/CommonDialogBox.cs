using System;
using System.Runtime.InteropServices;

public static class CommonDialogBox {
    public enum ErrorCodes
    {
        None = 0x00,

        // General
        CDERR_STRUCTSIZE        = 0x0001,
        CDERR_INITIALIZATION    = 0x0002,
        CDERR_NOTEMPLATE        = 0x0003,
        CDERR_NOHINSTANCE       = 0x0004,
        CDERR_LOADSTRFAILURE    = 0x0005,
        CDERR_FINDRESFAILURE    = 0x0006,
        CDERR_LOADRESFAILURE    = 0x0007,
        CDERR_LOCKRESFAILURE    = 0x0008,
        CDERR_MEMALLOCFAILURE   = 0x0009,
        CDERR_MEMLOCKFAILURE    = 0x000A,     
        CDERR_NOHOOK            = 0x000B,
        CDERR_REGISTERMSGFAIL   = 0x000C,
        CDERR_DIALOGFAILURE     = 0xFFFF,


        // Open Filename/Save Filename
        FNERR_BUFFERTOOSMALL    = 0x3003,
        FNERR_INVALIDFILENAME   = 0x3002,
        FNERR_SUBCLASSFAILURE   = 0x3001,
    }

    public static ErrorCodes GetErrorCode()
    {
        return (ErrorCodes)CommonWindowsBindings.CommDlgExtendedError();
    }
}
