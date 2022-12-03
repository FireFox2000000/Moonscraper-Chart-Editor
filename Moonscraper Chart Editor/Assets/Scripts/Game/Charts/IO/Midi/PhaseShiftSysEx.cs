// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using NAudio.Midi;

namespace MoonscraperChartEditor.Song.IO
{
    public class PhaseShiftSysEx : MidiEvent
    {
        public byte Type;
        public byte Difficulty;
        public byte Code;
        public byte Value;

        protected PhaseShiftSysEx()
        { }

        protected PhaseShiftSysEx(PhaseShiftSysEx other)
        {
            AbsoluteTime = other.AbsoluteTime;
            Type = other.Type;
            Difficulty = other.Difficulty;
            Code = other.Code;
            Value = other.Value;
        }

        public PhaseShiftSysEx(SysexEvent sysex) : base(sysex.AbsoluteTime, sysex.Channel, sysex.CommandCode)
        {
            if (!TryParseInternal(sysex, this))
            {
                throw new ArgumentException("The given event data is not a Phase Shift SysEx event.", nameof(sysex));
            }
        }

        public static bool TryParse(SysexEvent sysex, out PhaseShiftSysEx psSysex)
        {
            psSysex = new PhaseShiftSysEx();
            return TryParseInternal(sysex, psSysex);
        }

        protected static bool TryParseInternal(SysexEvent sysex, PhaseShiftSysEx psSysex)
        {
            byte[] sysexData = sysex.GetData();
            if (IsPhaseShiftSysex(sysexData))
            {
                psSysex.AbsoluteTime = sysex.AbsoluteTime;
                psSysex.Type = sysexData[MidIOHelper.SYSEX_INDEX_TYPE];
                psSysex.Difficulty = sysexData[MidIOHelper.SYSEX_INDEX_DIFFICULTY];
                psSysex.Code = sysexData[MidIOHelper.SYSEX_INDEX_CODE];
                psSysex.Value = sysexData[MidIOHelper.SYSEX_INDEX_VALUE];

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
            return Type == otherEvent.Type && Difficulty == otherEvent.Difficulty && Code == otherEvent.Code;
        }

        public override string ToString()
        {
            return $"AbsoluteTime: {AbsoluteTime}, Type: {Type}, Difficulty: {Difficulty}, Code: {Code}, Value: {Value}";
        }
    }

    public class PhaseShiftSysExStart : PhaseShiftSysEx
    {
        private PhaseShiftSysEx endEvent;
        public PhaseShiftSysEx EndEvent
        {
            get => endEvent;
            set
            {
                if (value.AbsoluteTime < AbsoluteTime)
                    throw new ArgumentException($"The end event of a SysEx pair must occur after the start event.\nStart: {this}\nEnd: {value}", nameof(value));

                endEvent = value;
            }
        }

        public long Length
        {
            get
            {
                if (EndEvent != null)
                {
                    return EndEvent.AbsoluteTime - AbsoluteTime;
                }

                // No end event to get a length from
                return 0;
            }
        }

        public PhaseShiftSysExStart(PhaseShiftSysEx sysex) : base(sysex)
        { }

        public PhaseShiftSysExStart(PhaseShiftSysEx start, PhaseShiftSysEx end) : base(start)
        {
            if (start.AbsoluteTime < end.AbsoluteTime)
                throw new ArgumentException($"The start event of a SysEx pair must occur before the end event.\nStart: {start}\nEnd: {end}", nameof(start));

            EndEvent = end;
        }

        public override string ToString()
        {
            if (EndEvent != null)
                return $"AbsoluteTime: {AbsoluteTime}, Length: {Length}, Type: {Type}, Difficulty: {Difficulty}, Code: {Code}, Value: {Value}";
            else
                return base.ToString();
        }
    }
}
