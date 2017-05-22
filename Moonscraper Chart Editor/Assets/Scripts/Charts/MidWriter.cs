using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;
using NAudio.Midi;

public static class MidWriter {   
    const byte TRACK_NAME_EVENT = 0x03;
    const byte TEXT_EVENT = 0x01;
    const string EVENTS_TRACK = "events";           // Sections
    const string GUITAR_TRACK = "part guitar";
    const string BASS_TRACK = "part bass";
    const string KEYS_TRACK = "part keys";

    static readonly byte[] END_OF_TRACK = new byte[] { 0, 0xFF, 0x2F, 0x00 };

    public static void WriteToFile(string path, Song song)
    {
        short track_count = 1;
        byte[] track_sync = MakeTrack(GetSyncBytes(song), "synctrack");

        byte[] track_events = MakeTrack(GetSectionBytes(song), "events");
        if (track_events.Length > 0)
            track_count++;

        byte[] track_guitar = MakeTrack(GetInstrumentBytes(song, Song.Instrument.Guitar), "part guitar");
        if (track_guitar.Length > 0)
            track_count++;
        /*
        byte[] track_bass = MakeTrack(GetInstrumentBytes(song, Song.Instrument.Bass), "part bass");
        if (track_bass.Length > 0)
            track_count++;*/

        byte[] header = GetMidiHeader(1, track_count, (short)song.resolution);

        FileStream file = File.Open(path, FileMode.OpenOrCreate);
        BinaryWriter bw = new BinaryWriter(file);

        bw.Write(header);
        bw.Write(track_sync);
        bw.Write(track_events);
        bw.Write(track_guitar);

        bw.Close();
        file.Close();
        /*
        Debug.Log(BitConverter.ToString(header));
        Debug.Log(BitConverter.ToString(track_sync));*/
    }

    static byte[] GetChartBytes(Chart chart)
    {
        throw new NotImplementedException();

        // First event is a text event describing the track name
        //MetaTextEvent(TRACK_NAME_EVENT, trackName);
    }

    static byte[] GetSyncBytes(Song song)
    {
        List<byte> syncTrackBytes = new List<byte>();
        syncTrackBytes.AddRange(TimedEvent(0, MetaTextEvent(TEXT_EVENT, song.name)));

        for (int i = 0; i < song.syncTrack.Length; ++i)
        {
            uint deltaTime = song.syncTrack[i].position;
            if (i > 0)
                deltaTime -= song.syncTrack[i - 1].position;
            
            var bpm = song.syncTrack[i] as BPM;
            if (bpm != null)
                syncTrackBytes.AddRange(TimedEvent(deltaTime, TempoEvent(bpm)));
            
            var ts = song.syncTrack[i] as TimeSignature;
            if (ts != null)
                syncTrackBytes.AddRange(TimedEvent(deltaTime, TimeSignatureEvent(ts)));             
        }

        return syncTrackBytes.ToArray();
    }

    static byte[] GetSectionBytes(Song song)
    {
        List<byte> sectionBytes = new List<byte>();

        const string section_id = "section ";     // "section " is rb2 and former, "prc_" is rb3

        for (int i = 0; i < song.sections.Length; ++i)
        {
            uint deltaTime = song.sections[i].position;
            if (i > 0)
                deltaTime -= song.sections[i - 1].position;

            sectionBytes.AddRange(TimedEvent(deltaTime, MetaTextEvent(TEXT_EVENT, "[" + section_id + song.sections[i].title + "]")));        
        }

        return sectionBytes.ToArray();
    }

    static byte[] GetInstrumentBytes(Song song, Song.Instrument instrument)
    {
        // Collect all bytes from each difficulty of the instrument, assigning the position for each event unsorted
        List<SortableBytes> byteEvents = new List<SortableBytes>();
        byteEvents.AddRange(GetChartSortableBytes(song.GetChart(instrument, Song.Difficulty.Easy), Song.Difficulty.Easy));
        byteEvents.AddRange(GetChartSortableBytes(song.GetChart(instrument, Song.Difficulty.Medium), Song.Difficulty.Medium));
        byteEvents.AddRange(GetChartSortableBytes(song.GetChart(instrument, Song.Difficulty.Hard), Song.Difficulty.Hard));
        byteEvents.AddRange(GetChartSortableBytes(song.GetChart(instrument, Song.Difficulty.Expert), Song.Difficulty.Expert));

        // Perform merge sort to re-order everything correctly
        SortableBytes[] sortedEvents = byteEvents.ToArray();
        SortableBytes.Sort(sortedEvents);

        List<byte> bytes = new List<byte>();
        
        for (int i = 0; i < sortedEvents.Length; ++i)
        {
            uint deltaTime = sortedEvents[i].position;
            if (i > 0)
                deltaTime -= sortedEvents[i - 1].position;

            // Apply time to the midi event
            bytes.AddRange(TimedEvent(deltaTime, sortedEvents[i].bytes));
        }

        return bytes.ToArray();
    }

    static SortableBytes[] GetChartSortableBytes(Chart chart, Song.Difficulty difficulty)
    {
        List<SortableBytes> eventList = new List<SortableBytes>();
        const byte ON_EVENT = 0x91;         // Note on channel 1
        const byte OFF_EVENT = 0x81;
        const byte VELOCITY = 0x64;         // 100
        const byte STARPOWER_NOTE = 0x74;   // 116

        foreach (ChartObject chartObject in chart.chartObjects)
        {
            Note note = chartObject as Note;
            SortableBytes onEvent = null;
            SortableBytes offEvent = null;

            if (note != null)
            {
                int noteNumber;

                switch (difficulty)
                {
                    case (Song.Difficulty.Easy):
                        noteNumber = 60;
                        break;
                    case (Song.Difficulty.Medium):
                        noteNumber = 72;
                        break;
                    case (Song.Difficulty.Hard):
                        noteNumber = 84;
                        break;
                    case (Song.Difficulty.Expert):
                        noteNumber = 96;
                        break;
                    default:
                        continue;
                }

                switch (note.fret_type)
                {
                    case (Note.Fret_Type.GREEN):
                        noteNumber += 0;
                        break;
                    case (Note.Fret_Type.RED):
                        noteNumber += 1;
                        break;
                    case (Note.Fret_Type.YELLOW):
                        noteNumber += 2;
                        break;
                    case (Note.Fret_Type.BLUE):
                        noteNumber += 3;
                        break;
                    case (Note.Fret_Type.ORANGE):
                        noteNumber += 4;
                        break;
                    case (Note.Fret_Type.OPEN):
                        continue;
                    default:
                        continue;
                }

                onEvent = new SortableBytes(note.position, new byte[] { ON_EVENT, (byte)noteNumber, VELOCITY });
                offEvent = new SortableBytes(note.position + note.sustain_length, new byte[] { OFF_EVENT, (byte)noteNumber, VELOCITY });

                // Add flag and open note Sysex events if we're doing the expert chart
            }

            Starpower sp = chartObject as Starpower;
            if (sp != null && difficulty == Song.Difficulty.Expert)     // Starpower cannot be split up between charts in a midi file
            {
                onEvent = new SortableBytes(sp.position, new byte[] { ON_EVENT, STARPOWER_NOTE, VELOCITY });
                offEvent = new SortableBytes(sp.position + sp.length, new byte[] { OFF_EVENT, STARPOWER_NOTE, VELOCITY });
            }

            if (onEvent != null && offEvent != null)
            {
                eventList.Add(onEvent);

                if (offEvent.position == onEvent.position)
                    ++offEvent.position;

                eventList.Add(offEvent);
            }
        }

        return eventList.ToArray();
    }

    static byte[] GetMidiHeader(short fileFormat, short trackCount, short resolution)
    {
        const string ID = "MThd";   // MThd, 6 bytes, still need to add file format, track count and resolution to the header
        const int headerSize = 6;

        byte[] header = new byte[14];
        byte[] sourceBytes;
        int offset = 0;

        Array.Copy(System.Text.Encoding.UTF8.GetBytes(ID.ToCharArray()), 0, header, offset, ID.Length);
        offset += ID.Length;
        
        sourceBytes = EndianBitConverter.Big.GetBytes(headerSize);
        Array.Copy(sourceBytes, 0, header, offset, sizeof(int));
        offset += sizeof(int);

        sourceBytes = EndianBitConverter.Big.GetBytes(fileFormat);
        Array.Copy(sourceBytes, 0, header, offset, sizeof(short));
        offset += sizeof(short);

        sourceBytes = EndianBitConverter.Big.GetBytes(trackCount);
        Array.Copy(sourceBytes, 0, header, offset, sizeof(short));
        offset += sizeof(short);

        sourceBytes = EndianBitConverter.Big.GetBytes(resolution);
        Array.Copy(sourceBytes, 0, header, offset, sizeof(short));

        return header;
    }

    static byte[] GetTrackHeader(int byteLength)
    {
        const string ID = "MTrk";

        byte[] header = new byte[ID.Length + sizeof(int)];
        int offset = 0;

        Array.Copy(System.Text.Encoding.UTF8.GetBytes(ID.ToCharArray()), 0, header, offset, ID.Length);
        offset += ID.Length;

        byte[] sourceBytes = EndianBitConverter.Big.GetBytes(byteLength);
        Array.Copy(sourceBytes, 0, header, offset, sizeof(int));

        return header;
    }

    static byte[] MakeTrack(byte[] trackEvents, string trackName)
    {
        byte[] trackNameEvent = TimedEvent(0, MetaTextEvent(TRACK_NAME_EVENT, trackName));

        byte[] header = GetTrackHeader(trackNameEvent.Length + trackEvents.Length + END_OF_TRACK.Length);
        byte[] fullTrack = new byte[header.Length + trackNameEvent.Length + trackEvents.Length + END_OF_TRACK.Length];

        int offset = 0;
        Array.Copy(header, fullTrack, header.Length);
        offset += header.Length;

        Array.Copy(trackNameEvent, 0, fullTrack, offset, trackNameEvent.Length);
        offset += trackNameEvent.Length;

        Array.Copy(trackEvents, 0, fullTrack, offset, trackEvents.Length);
        offset += trackEvents.Length;

        Array.Copy(END_OF_TRACK, 0, fullTrack, offset, END_OF_TRACK.Length);

        return fullTrack;
    }

    // Joins delta-time byte information onto the front of a byte array
    static byte[] TimedEvent(uint tick, byte[] midiEvent)
    {
        byte[] deltaTime = VLVCompressedBytes(tick);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(deltaTime);

        byte[] timedEvent = new byte[deltaTime.Length + midiEvent.Length];
        Array.Copy(deltaTime, 0, timedEvent, 0, deltaTime.Length);
        Array.Copy(midiEvent, 0, timedEvent, deltaTime.Length, midiEvent.Length);

        return timedEvent;
    }

    static byte[] MetaTextEvent(byte m_event, string text)
    {
        if (text.Length > 255)
            throw new Exception("Text cannot be longer than 255 characters");

        char[] chars = text.ToCharArray();
        int byteLength = chars.Length;
        
        byte[] bytes = new byte[3 + byteLength];       // FF xx nn then whatever data

        byte[] header_event = EndianBitConverter.Big.GetBytes((short)(0xFF00 | (short)m_event));                // FF xx
        byte[] header_byte_length = EndianBitConverter.Big.GetBytes((sbyte)byteLength);    // nn
        byte[] header_text = System.Text.Encoding.UTF8.GetBytes(chars);            // dd

        int offset = 0;
        Array.Copy(header_event, 0, bytes, offset, sizeof(short));

        offset += sizeof(short);
        Array.Copy(header_byte_length, sizeof(sbyte), bytes, offset, sizeof(sbyte));

        offset += sizeof(sbyte);
        Array.Copy(header_text, 0, bytes, offset, header_text.Length);

        return bytes;
    }

    static byte[] TempoEvent(BPM bpm)
    {
        const byte TEMPO_EVENT = 0x51;
        byte[] bytes = new byte[6];

        bytes[0] = 0xFF;
        bytes[1] = TEMPO_EVENT;
        bytes[2] = 0x03;            // Size

        // Microseconds per quarter note for the last 3 bytes stored as a 24-bit binary
        byte[] microPerSec = EndianBitConverter.Big.GetBytes((uint)(6.0f * Mathf.Pow(10, 10) / bpm.value));

        Array.Copy(microPerSec, 1, bytes, 3, 3);        // Offset of 1 and length of 3 cause 24 bit

        return bytes;
    }

    static byte[] TimeSignatureEvent(TimeSignature ts)
    {
        const byte TIME_SIGNATURE_EVENT = 0x58;
        byte[] bytes = new byte[7];

        bytes[0] = 0xFF;
        bytes[1] = TIME_SIGNATURE_EVENT;
        bytes[2] = 0x04;            // Size
        bytes[3] = EndianBitConverter.Big.GetBytes((short)ts.numerator)[1];
        bytes[4] = EndianBitConverter.Big.GetBytes((short)ts.denominator)[1];
        bytes[5] = 0x18; // 24, 24 clock ticks in metronome click, so once every quater note. I doubt this is important, but I'm sure irony will strike.
        bytes[6] = 0x08; // 8, a quater note should happen every quarter note.

        return bytes;
    }

    static byte[] VLVCompressedBytes(uint value)
    {
        List<byte> vlvEncodedBytesList = new List<byte>();

        bool first = true;
        while (first || value > 0)
        {           
            byte lower7bits;

            lower7bits = (byte)(value & 0x7F);
            
            if (!first)
                lower7bits |= 128;      // Change lsb to a value of 1
            value >>= 7;

            first = false;
            vlvEncodedBytesList.Add(lower7bits);
        }

        return vlvEncodedBytesList.ToArray();
    }

    
}
