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
    const string EVENTS_TRACK = "EVENTS";           // Sections
    const string GUITAR_TRACK = "PART GUITAR";
    const string BASS_TRACK = "PART BASS";
    const string KEYS_TRACK = "PART KEYS";
    const string DRUMS_TRACK = "PART DRUMS";

    static readonly byte[] END_OF_TRACK = new byte[] { 0, 0xFF, 0x2F, 0x00 };

    public static void WriteToFile(string path, Song song, ExportOptions exportOptions)
    {
        Debug.Log(path);
        short track_count = 1; 

        byte[] track_sync = MakeTrack(GetSyncBytes(song, exportOptions), song.name);

        uint end;
        byte[] track_events = MakeTrack(GetSectionBytes(song, exportOptions, out end), EVENTS_TRACK);
        if (track_events.Length > 0)
            track_count++;

        //byte[] track_beat = MakeTrack(GenerateBeat(end, (uint)exportOptions.targetResolution), "BEAT");
        //song.GetChart(Song.Instrument.Guitar, Song.Difficulty.Expert).Add(new ChartEvent(0, "[idle_realtime]"));

        byte[] track_guitar = GetInstrumentBytes(song, Song.Instrument.Guitar, exportOptions);

        if (track_guitar.Length > 0)
            track_count++;

        byte[] track_bass = GetInstrumentBytes(song, Song.Instrument.Bass, exportOptions);
        if (track_bass.Length > 0)
            track_count++;

        byte[] track_keys = GetInstrumentBytes(song, Song.Instrument.Keys, exportOptions);
        if (track_keys.Length > 0)
            track_count++;

        byte[] track_drums = GetInstrumentBytes(song, Song.Instrument.Drums, exportOptions);
        if (track_drums.Length > 0)
            track_count++;

        byte[] header = GetMidiHeader(1, track_count, (short)(exportOptions.targetResolution));
        
        FileStream file = File.Open(path, FileMode.OpenOrCreate);
        BinaryWriter bw = new BinaryWriter(file);

        bw.Write(header);
        bw.Write(track_sync);
        //bw.Write(track_beat);
        bw.Write(track_events);

        if (track_guitar.Length > 0)
            bw.Write(MakeTrack(track_guitar, GUITAR_TRACK));

        if (track_bass.Length > 0)
            bw.Write(MakeTrack(track_bass, BASS_TRACK));

        if (track_keys.Length > 0)
            bw.Write(MakeTrack(track_keys, KEYS_TRACK));

        if (track_drums.Length > 0)
            bw.Write(MakeTrack(track_drums, DRUMS_TRACK));

        bw.Close();
        file.Close();
    }

    static byte[] GetSyncBytes(Song song, ExportOptions exportOptions)
    {
        List<byte> syncTrackBytes = new List<byte>();

        // Set default bpm and time signature
        if (exportOptions.tickOffset > 0)
        {
            syncTrackBytes.AddRange(TimedEvent(0, TempoEvent(new BPM())));
            syncTrackBytes.AddRange(TimedEvent(0, TimeSignatureEvent(new TimeSignature())));
        }

        float resolutionScaleRatio = song.ResolutionScaleRatio(exportOptions.targetResolution);

        // Loop through all synctrack events
        for (int i = 0; i < song.syncTrack.Length; ++i)
        {
            uint deltaTime = song.syncTrack[i].position;
            if (i > 0)
                deltaTime -= song.syncTrack[i - 1].position;

            deltaTime = (uint)Mathf.Round(deltaTime * resolutionScaleRatio);

            if (i == 0)
                deltaTime += exportOptions.tickOffset;

            var bpm = song.syncTrack[i] as BPM;
            if (bpm != null)
                syncTrackBytes.AddRange(TimedEvent(deltaTime, TempoEvent(bpm)));
            
            var ts = song.syncTrack[i] as TimeSignature;
            if (ts != null)
                syncTrackBytes.AddRange(TimedEvent(deltaTime, TimeSignatureEvent(ts)));             
        }

        return syncTrackBytes.ToArray();
    }

    static byte[] GetSectionBytes(Song song, ExportOptions exportOptions, out uint end)
    {
        List<byte> sectionBytes = new List<byte>();

        const string section_id = "section ";     // "section " is rb2 and former, "prc_" is rb3

        sectionBytes.AddRange(TimedEvent(0, MetaTextEvent(TEXT_EVENT, "[music_start]")));

        uint deltaTickSum = 0;
        float resolutionScaleRatio = song.ResolutionScaleRatio(exportOptions.targetResolution);

        for (int i = 0; i < song.sections.Length; ++i)
        {
            uint deltaTime = song.sections[i].position;
            if (i > 0)
                deltaTime -= song.sections[i - 1].position;

            deltaTime = (uint)Mathf.Round(deltaTime * resolutionScaleRatio);

            if (i == 0)
                deltaTime += exportOptions.tickOffset;

            deltaTickSum += deltaTime;

            sectionBytes.AddRange(TimedEvent(deltaTime, MetaTextEvent(TEXT_EVENT, "[" + section_id + song.sections[i].title + "]")));
        }

        uint music_end = song.TimeToChartPosition(song.length + exportOptions.tickOffset, song.resolution * resolutionScaleRatio, false);

        if (music_end > deltaTickSum)
            music_end -= deltaTickSum;
        else
            music_end = deltaTickSum;

        end = music_end;

        // Add music_end and end text events.
        sectionBytes.AddRange(TimedEvent(music_end, MetaTextEvent(TEXT_EVENT, "[music_end]")));
        sectionBytes.AddRange(TimedEvent(0, MetaTextEvent(TEXT_EVENT, "[end]")));

        return sectionBytes.ToArray();
    }

    static byte[] GetInstrumentBytes(Song song, Song.Instrument instrument, ExportOptions exportOptions)
    {
        // Collect all bytes from each difficulty of the instrument, assigning the position for each event unsorted
        //List<SortableBytes> byteEvents = new List<SortableBytes>();
        SortableBytes[] easyBytes = GetChartSortableBytes(song, instrument, Song.Difficulty.Easy, exportOptions);
        SortableBytes[] mediumBytes = GetChartSortableBytes(song, instrument, Song.Difficulty.Medium, exportOptions);
        SortableBytes[] hardBytes = GetChartSortableBytes(song, instrument, Song.Difficulty.Hard, exportOptions);
        SortableBytes[] expertBytes = GetChartSortableBytes(song, instrument, Song.Difficulty.Expert, exportOptions);

        SortableBytes[] em = SortableBytes.MergeAlreadySorted(easyBytes, mediumBytes);
        SortableBytes[] he = SortableBytes.MergeAlreadySorted(hardBytes, expertBytes);
        SortableBytes[] sortedEvents = SortableBytes.MergeAlreadySorted(em, he);

        // Perform merge sort to re-order everything correctly
        //SortableBytes[] sortedEvents = new SortableBytes[easyBytes.Length + mediumBytes.Length + hardBytes.Length + expertBytes.Length];//byteEvents.ToArray();
        //SortableBytes.Sort(sortedEvents);

        List<byte> bytes = new List<byte>();
        float resolutionScaleRatio = song.ResolutionScaleRatio(exportOptions.targetResolution);

        for (int i = 0; i < sortedEvents.Length; ++i)
        {
            uint deltaTime = sortedEvents[i].position;
            if (i > 0)
                deltaTime -= sortedEvents[i - 1].position;

            deltaTime = (uint)Mathf.Round(deltaTime * resolutionScaleRatio);

            if (i == 0)
                deltaTime += exportOptions.tickOffset;

            // Apply time to the midi event
            bytes.AddRange(TimedEvent(deltaTime, sortedEvents[i].bytes));
        }

        return bytes.ToArray();
    }

    delegate void del(SortableBytes sortableByte);
    static SortableBytes[] GetChartSortableBytes(Song song, Song.Instrument instrument, Song.Difficulty difficulty, ExportOptions exportOptions)
    {
        Chart chart = song.GetChart(instrument, difficulty);

        if (exportOptions.copyDownEmptyDifficulty)
        {
            Song.Difficulty chartDiff = difficulty;
            while (chart.notes.Length <= 0)
            {
                switch (chartDiff)
                {
                    case (Song.Difficulty.Easy):
                        chartDiff = Song.Difficulty.Medium;
                        break;
                    case (Song.Difficulty.Medium):
                        chartDiff = Song.Difficulty.Hard;
                        break;
                    case (Song.Difficulty.Hard):
                        chartDiff = Song.Difficulty.Expert;
                        break;
                    case (Song.Difficulty.Expert):
                    default:
                        return new SortableBytes[0];
                }

                chart = song.GetChart(instrument, chartDiff);
            }
        }

        List<SortableBytes> eventList = new List<SortableBytes>();
        const byte ON_EVENT = 0x91;         // Note on channel 1
        const byte OFF_EVENT = 0x81;
        const byte VELOCITY = 0x64;         // 100
        const byte STARPOWER_NOTE = 0x74;   // 116

        const byte SYSEX_START = 0xF0;
        const byte SYSEX_END = 0xF7;
        const byte SYSEX_ON = 0x01;
        const byte SYSEX_OFF = 0x00;

        del InsertionSort = (sortableByte) =>
        {
            int index = eventList.Count - 1;

            while (index >= 0 && sortableByte.position < eventList[index].position)
                --index;

            eventList.Insert(index + 1, sortableByte);
        };

        foreach (ChartObject chartObject in chart.chartObjects)
        {
            Note note = chartObject as Note;           

            SortableBytes onEvent = null;
            SortableBytes offEvent = null;

            if (note != null)
            {
                Note.Fret_Type fret_type = note.fret_type;
                if (instrument == Song.Instrument.Drums)
                    fret_type = Note.SaveGuitarNoteToDrumNote(fret_type);

                int difficultyNumber;
                int noteNumber;

                switch (difficulty)
                {
                    case (Song.Difficulty.Easy):
                        difficultyNumber = 60;
                        break;
                    case (Song.Difficulty.Medium):
                        difficultyNumber = 72;
                        break;
                    case (Song.Difficulty.Hard):
                        difficultyNumber = 84;
                        break;
                    case (Song.Difficulty.Expert):
                        difficultyNumber = 96;
                        break;
                    default:
                        continue;
                }
 
                switch (fret_type)
                {
                    case (Note.Fret_Type.OPEN):     // Open note highlighted as an SysEx event. Use green as default.
                        if (instrument == Song.Instrument.Drums)
                        {
                            noteNumber = difficultyNumber + 5;
                            break;
                        }
                        else
                            goto case Note.Fret_Type.GREEN;
                    case (Note.Fret_Type.GREEN):
                        noteNumber = difficultyNumber + 0;
                        break;
                    case (Note.Fret_Type.RED):
                        noteNumber = difficultyNumber + 1;
                        break;
                    case (Note.Fret_Type.YELLOW):
                        noteNumber = difficultyNumber + 2;
                        break;
                    case (Note.Fret_Type.BLUE):
                        noteNumber = difficultyNumber + 3;
                        break;
                    case (Note.Fret_Type.ORANGE):
                        noteNumber = difficultyNumber + 4;
                        break;
                    default:
                        continue;
                }
 
                onEvent = new SortableBytes(note.position, new byte[] { ON_EVENT, (byte)noteNumber, VELOCITY });
                offEvent = new SortableBytes(note.position + note.sustain_length, new byte[] { OFF_EVENT, (byte)noteNumber, VELOCITY });

                if (exportOptions.forced)
                {
                    // Forced notes               
                    if ((note.flags & Note.Flags.FORCED) != 0 && (note.previous == null || (note.previous.position != note.position)))     // Don't overlap on chords
                    {
                        // Add a note
                        int forcedNoteNumber;

                        if (note.type == Note.Note_Type.Hopo)
                            forcedNoteNumber = difficultyNumber + 5;
                        else
                            forcedNoteNumber = difficultyNumber + 6;

                        SortableBytes forceOnEvent = new SortableBytes(note.position, new byte[] { ON_EVENT, (byte)forcedNoteNumber, VELOCITY });
                        SortableBytes forceOffEvent = new SortableBytes(note.position + 1, new byte[] { OFF_EVENT, (byte)forcedNoteNumber, VELOCITY });

                        InsertionSort(forceOnEvent);
                        InsertionSort(forceOffEvent);
                    }

                    // Add tap sysex events
                    if (difficulty == Song.Difficulty.Expert && note.fret_type != Note.Fret_Type.OPEN && (note.flags & Note.Flags.TAP) != 0 && (note.previous == null || (note.previous.flags & Note.Flags.TAP) == 0))  // This note is a tap while the previous one isn't as we're creating a range
                    {
                        // Find the next non-tap note
                        Note nextNonTap = note;
                        while (nextNonTap.next != null && nextNonTap.fret_type != Note.Fret_Type.OPEN && (nextNonTap.next.flags & Note.Flags.TAP) != 0)
                            nextNonTap = nextNonTap.next;

                        // Tap event = 08-50-53-00-00-FF-04-01, end with 01 for On, 00 for Off
                        byte[] tapOnEventBytes = new byte[] { SYSEX_START, 0x08, 0x50, 0x53, 0x00, 0x00, 0xFF, 0x04, SYSEX_ON, SYSEX_END };
                        byte[] tapOffEventBytes = new byte[] { SYSEX_START, 0x08, 0x50, 0x53, 0x00, 0x00, 0xFF, 0x04, SYSEX_OFF, SYSEX_END };

                        SortableBytes tapOnEvent = new SortableBytes(note.position, tapOnEventBytes);
                        SortableBytes tapOffEvent = new SortableBytes(nextNonTap.position + 1, tapOffEventBytes);

                        InsertionSort(tapOnEvent);
                        InsertionSort(tapOffEvent);
                    }
                }

                if (difficulty == Song.Difficulty.Expert && note.fret_type == Note.Fret_Type.OPEN && (note.previous == null || (note.previous.fret_type != Note.Fret_Type.OPEN))
                    && instrument != Song.Instrument.Drums)
                {
                    // Find the next non-open note
                    Note nextNonOpen = note;
                    while (nextNonOpen.next != null && nextNonOpen.next.fret_type == Note.Fret_Type.OPEN)
                        nextNonOpen = nextNonOpen.next;

                    byte diff;

                    switch (difficulty)
                    {
                        case (Song.Difficulty.Easy):
                            diff = 0;
                            break;
                        case (Song.Difficulty.Medium):
                            diff = 1;
                            break;
                        case (Song.Difficulty.Hard):
                            diff = 2;
                            break;
                        case (Song.Difficulty.Expert):
                            diff = 3;
                            break;
                        default:
                            continue;
                    }

                    byte[] openOnEventBytes = new byte[] { SYSEX_START, 0x08, 0x50, 0x53, 0x00, 0x00, diff, 0x01, SYSEX_ON, SYSEX_END };
                    byte[] openOffEventBytes = new byte[] { SYSEX_START, 0x08, 0x50, 0x53, 0x00, 0x00, diff, 0x01, SYSEX_OFF, SYSEX_END };

                    SortableBytes openOnEvent = new SortableBytes(note.position, openOnEventBytes);
                    SortableBytes openOffEvent = new SortableBytes(nextNonOpen.position + 1, openOffEventBytes);

                    InsertionSort(openOnEvent);
                    InsertionSort(openOffEvent);
                }
            }

            Starpower sp = chartObject as Starpower;
            if (sp != null && difficulty == Song.Difficulty.Expert)     // Starpower cannot be split up between charts in a midi file
            {
                onEvent = new SortableBytes(sp.position, new byte[] { ON_EVENT, STARPOWER_NOTE, VELOCITY });
                offEvent = new SortableBytes(sp.position + sp.length, new byte[] { OFF_EVENT, STARPOWER_NOTE, VELOCITY });
            }

            ChartEvent chartEvent = chartObject as ChartEvent;
            if (chartEvent != null && difficulty == Song.Difficulty.Expert)     // Text events cannot be split up in the file
            {
                byte[] textEvent = MetaTextEvent(TEXT_EVENT, chartEvent.eventName);
                InsertionSort(new SortableBytes(chartEvent.position, textEvent));
            }

            if (onEvent != null && offEvent != null)
            {
                InsertionSort(onEvent);
                //eventList.Add(onEvent);

                if (offEvent.position == onEvent.position)
                    ++offEvent.position;

                InsertionSort(offEvent);
                //eventList.Add(offEvent);
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
        bytes[4] = EndianBitConverter.Big.GetBytes((short)(Mathf.Log(ts.denominator, 2)))[1];
        bytes[5] = 0x18; // 24, 24 clock ticks in metronome click, so once every quater note. I doubt this is important, but I'm sure irony will strike.
        bytes[6] = 0x08; // 8, a quater note should happen every quarter note.

        return bytes;
    }

    static byte[] GenerateBeat(uint end, uint resolution)
    {
        const byte ON_EVENT = 0x97;         // Note on channel 7
        const byte OFF_EVENT = 0x87;
        const int VELOCITY = 100;
        const int MEASURE_NOTE = 12;
        const int BEAT_NOTE = 13;

        uint length = resolution / 4;
        uint measure = resolution * 4;

        uint tick = 0;

        List<byte> beatBytes = new List<byte>();
        // Add inital beats
        byte[] onEvent = new byte[] { ON_EVENT, (byte)MEASURE_NOTE, VELOCITY };
        byte[] offEvent = new byte[] { OFF_EVENT, (byte)MEASURE_NOTE, VELOCITY };

        beatBytes.AddRange(TimedEvent(0, onEvent));
        beatBytes.AddRange(TimedEvent(length, offEvent));

        tick += resolution;

        while (tick < end)
        {
            tick += resolution;

            int noteNumber = BEAT_NOTE;
            if (tick % measure == 0)
                noteNumber = MEASURE_NOTE;

            onEvent = new byte[] { ON_EVENT, (byte)noteNumber, VELOCITY };
            offEvent = new byte[] { OFF_EVENT, (byte)noteNumber, VELOCITY };

            beatBytes.AddRange(TimedEvent(resolution - length, onEvent));
            beatBytes.AddRange(TimedEvent(length, offEvent));
        }

        return beatBytes.ToArray();
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
