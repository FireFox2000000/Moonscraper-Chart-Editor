// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using Melanchall.DryWetMidi.Core;

namespace MoonscraperChartEditor.Song.IO
{
    public class PhaseShiftSysEx : SysExEvent
    {
        public byte type;
        public byte difficulty;
        public byte code;
        public byte value;

        protected PhaseShiftSysEx() : base(MidiEventType.NormalSysEx)
        { }

        protected PhaseShiftSysEx(PhaseShiftSysEx other) : this()
        {
            DeltaTime = other.DeltaTime;
            type = other.type;
            difficulty = other.difficulty;
            code = other.code;
            value = other.value;
        }

        public PhaseShiftSysEx(SysExEvent sysex) : this()
        {
            if (!TryParseInternal(sysex, this))
            {
                throw new ArgumentException("The given event data is not a Phase Shift SysEx event.", nameof(sysex));
            }
        }

        public static bool TryParse(SysExEvent sysex, out PhaseShiftSysEx psSysex)
        {
            psSysex = new PhaseShiftSysEx();
            return TryParseInternal(sysex, psSysex);
        }

        protected static bool TryParseInternal(SysExEvent sysex, PhaseShiftSysEx psSysex)
        {
            byte[] sysexData = sysex.Data;
            if (IsPhaseShiftSysex(sysexData))
            {
                psSysex.DeltaTime = sysex.DeltaTime;
                psSysex.type = sysexData[MidIOHelper.SYSEX_INDEX_TYPE];
                psSysex.difficulty = sysexData[MidIOHelper.SYSEX_INDEX_DIFFICULTY];
                psSysex.code = sysexData[MidIOHelper.SYSEX_INDEX_CODE];
                psSysex.value = sysexData[MidIOHelper.SYSEX_INDEX_VALUE];

                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsPhaseShiftSysex(byte[] sysexData)
        {
            if (sysexData == null)
                throw new ArgumentNullException(nameof(sysexData));

            return (
                sysexData.Length == MidIOHelper.SYSEX_LENGTH &&
                sysexData[MidIOHelper.SYSEX_INDEX_HEADER_1] == MidIOHelper.SYSEX_HEADER_1 &&
                sysexData[MidIOHelper.SYSEX_INDEX_HEADER_2] == MidIOHelper.SYSEX_HEADER_2 &&
                sysexData[MidIOHelper.SYSEX_INDEX_HEADER_3] == MidIOHelper.SYSEX_HEADER_3
            );
        }

        public bool MatchesWith(PhaseShiftSysEx otherEvent)
        {
            return type == otherEvent.type && difficulty == otherEvent.difficulty && code == otherEvent.code;
        }

        public override string ToString()
        {
            return $"DeltaTime: {DeltaTime}, Type: {type}, Difficulty: {difficulty}, Code: {code}, Value: {value}";
        }

        protected override MidiEvent CloneEvent()
        {
            return new PhaseShiftSysEx(this);
        }
    }
}
