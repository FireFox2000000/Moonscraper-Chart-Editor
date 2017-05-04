using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using NAudio.Midi;

public static class MidWriter {
    const byte TEXT_EVENT = 0x01;
    const byte TRACK_NAME_EVENT = 0x03;
    const sbyte TRACK_END_EVENT = 0x2F;
    const sbyte TIME_SIGNATURE_EVENT = 0x58;

    const string EVENTS_TRACK = "events";           // Sections
    const string GUITAR_TRACK = "part guitar";
    const string BASS_TRACK = "part bass";
    const string KEYS_TRACK = "part keys";

    static readonly byte[] endOfTrack = new byte[] { 0xFF, 0x2F, 0x00 };

    public static void WriteToFile(string path, Song song)
    {
        byte[] header = GetMidiHeader(1, 3, 192);

        Debug.Log(BitConverter.ToString(header));
        Debug.Log(BitConverter.ToString(MetaTextEvent(TRACK_NAME_EVENT, "Test")));
        Debug.Log(BitConverter.ToString(TimedEvent(64, TempoEvent(new BPM()))));
        // First track will always be synctrack events
        // Write the song name as a text event, bpm as tempo event and time signature as a time signature event in the first track  
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
            /*
            var ts = song.syncTrack[i] as TimeSignature;
            if (ts != null)
                syncTrackBytes.AddRange(TimedEvent(deltaTime, MetaTextEvent(TEMPO_EVENT, ts.numerator.ToString())));*/
        }

        return syncTrackBytes.ToArray();
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

        sourceBytes = BitConverter.GetBytes(headerSize);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(sourceBytes);
        Array.Copy(sourceBytes, 0, header, offset, sizeof(int));
        offset += sizeof(int);

        sourceBytes = BitConverter.GetBytes(fileFormat);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(sourceBytes);
        Array.Copy(sourceBytes, 0, header, offset, sizeof(short));
        offset += sizeof(short);

        sourceBytes = BitConverter.GetBytes(trackCount);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(sourceBytes);
        Array.Copy(sourceBytes, 0, header, offset, sizeof(short));
        offset += sizeof(short);

        sourceBytes = BitConverter.GetBytes(resolution);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(sourceBytes);
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

        byte[] sourceBytes = BitConverter.GetBytes(byteLength);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(sourceBytes);
        Array.Copy(sourceBytes, 0, header, offset, sizeof(int));

        return header;
    }

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

        byte[] header_event = BitConverter.GetBytes((short)(0xFF00 | (short)m_event));                // FF xx
        if (BitConverter.IsLittleEndian)
            Array.Reverse(header_event);

        byte[] header_byte_length = BitConverter.GetBytes((sbyte)byteLength);    // nn
        if (BitConverter.IsLittleEndian)
            Array.Reverse(header_byte_length);

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
        bytes[1] = 0x03;

        // Microseconds per quarter note for the last 3 bytes stored as a 24-bit binary
        byte[] microPerSec = BitConverter.GetBytes((uint)(6.0f * Mathf.Pow(10, 10) / bpm.value));
        if (BitConverter.IsLittleEndian)
            Array.Reverse(microPerSec);

        Array.Copy(microPerSec, 1, bytes, 2, 3);        // Offset of 1 and length of 3 cause 24 bit

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
