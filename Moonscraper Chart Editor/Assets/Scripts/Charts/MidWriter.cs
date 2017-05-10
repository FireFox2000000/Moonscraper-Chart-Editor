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

    static readonly byte[] END_OF_TRACK = new byte[] { 0xFF, 0x2F, 0x00 };

    public static void WriteToFile(string path, Song song)
    {
        byte[] header = GetMidiHeader(0, 1, (short)song.resolution);
        byte[] track_sync = MakeTrack(GetSyncBytes(song), "synctrack");
        //byte[] track_events = MakeTrack(GetSectionBytes(song), "events");

        FileStream file = File.Open(path, FileMode.OpenOrCreate);
        BinaryWriter bw = new BinaryWriter(file);

        bw.Write(header);
        bw.Write(track_sync);
        //bw.Write(track_events);

        bw.Close();
        file.Close();

        // Write header
        // Write synctrack
        // Write sections
        // Write instuments
        /*
        Debug.Log(BitConverter.ToString(header));
        Debug.Log(BitConverter.ToString(MetaTextEvent(TRACK_NAME_EVENT, "Test")));
        Debug.Log(BitConverter.ToString(TimedEvent(64, TempoEvent(new BPM()))));
        Debug.Log(BitConverter.ToString(GetSyncBytes(song))); */
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

        for (int i = 0; i < song.sections.Length; ++i)
        {
            uint deltaTime = song.sections[i].position;
            if (i > 0)
                deltaTime -= song.sections[i - 1].position;

            sectionBytes.AddRange(TimedEvent(deltaTime, MetaTextEvent(TEXT_EVENT, "[section " + song.sections[i].title)));
        }

        return sectionBytes.ToArray();
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
        byte[] trackNameEvent = MetaTextEvent(TEXT_EVENT, trackName);

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
        byte[] bytes = new byte[5];

        bytes[0] = TEMPO_EVENT;
        bytes[1] = 0x03;            // Size

        // Microseconds per quarter note for the last 3 bytes stored as a 24-bit binary
        byte[] microPerSec = EndianBitConverter.Big.GetBytes((uint)(6.0f * Mathf.Pow(10, 10) / bpm.value));

        Array.Copy(microPerSec, 1, bytes, 2, 3);        // Offset of 1 and length of 3 cause 24 bit

        return bytes;
    }

    static byte[] TimeSignatureEvent(TimeSignature ts)
    {
        const byte TIME_SIGNATURE_EVENT = 0x58;
        byte[] bytes = new byte[6];

        bytes[0] = TIME_SIGNATURE_EVENT;
        bytes[1] = 0x04;            // Size
        bytes[2] = EndianBitConverter.Big.GetBytes((short)ts.numerator)[1];
        bytes[3] = EndianBitConverter.Big.GetBytes((short)ts.denominator)[1];
        bytes[4] = 0x18; // 24, 24 clock ticks in metronome click, so once every quater note. I doubt this is important, but I'm sure irony will strike.
        bytes[5] = 0x08; // 8, a quater note should happen every quarter note.

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
