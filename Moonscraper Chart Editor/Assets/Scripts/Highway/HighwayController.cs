// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public class HighwayController : MonoBehaviour {
    const int POOL_SIZE = 100;

    public GameObject measureLine;
    public GameObject beatLine;
    public GameObject quarterBeatLine;

    GameObject[] measureLinePool = new GameObject[POOL_SIZE];
    GameObject[] beatLinePool = new GameObject[POOL_SIZE];
    GameObject[] quarterBeatLinePool = new GameObject[POOL_SIZE];

    GameObject beatLineParent;

    ChartEditor editor;

    // Use this for initialization
    void Start () {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        beatLineParent = new GameObject("Beat Lines");

        for (int i = 0; i < POOL_SIZE; ++i)
        {
            measureLinePool[i] = Instantiate(measureLine);
            measureLinePool[i].transform.SetParent(beatLineParent.transform);
            measureLinePool[i].SetActive(false);

            beatLinePool[i] = Instantiate(beatLine);
            beatLinePool[i].transform.SetParent(beatLineParent.transform);
            beatLinePool[i].SetActive(false);

            quarterBeatLinePool[i] = Instantiate(quarterBeatLine);
            quarterBeatLinePool[i].transform.SetParent(beatLineParent.transform);
            quarterBeatLinePool[i].SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update () {
        UpdateBeatLines3();
    }

    // Calculate the beat lines directly from the time signature positions themselves
    void UpdateBeatLines3()
    {
        int measurePoolPos = 0, beatPoolPos = 0, quarterPoolPos = 0;
        const float RESOLUTIONS_PER_MEASURE = 4.0f;

        Song song = editor.currentSong;
        uint startRange = song.WorldPositionToSnappedTick(editor.camYMin.position.y, 8);
        uint endRange = editor.maxPos;
        TimeSignature[] timeSignatures = editor.currentSong.timeSignatures;
        uint standardMeasureLengthTicks = (uint)(RESOLUTIONS_PER_MEASURE * song.resolution);

        int startIndex = SongObjectHelper.FindClosestPositionRoundedDown(startRange, timeSignatures);   

        for (int tsIndex = startIndex; tsIndex < timeSignatures.Length && timeSignatures[tsIndex].tick <= endRange; ++tsIndex)
        {
            TimeSignature ts = timeSignatures[tsIndex];
            uint nextTick = ts.tick;
            uint nextTSTick = tsIndex + 1 < timeSignatures.Length ? timeSignatures[tsIndex + 1].tick : endRange;
            float beatDeltaTick = standardMeasureLengthTicks / ts.beatsPerMeasure;

            uint startDeltaFromTSTick = startRange > ts.tick ? (startRange - ts.tick) : 0;
            int quarterLineIndex = (int)Mathf.Round((float)(startDeltaFromTSTick) / beatDeltaTick);    // Jump to the next reasonable line index rather than looping until we get there
            if (quarterLineIndex > 0)
                --quarterLineIndex;

            while (nextTick < nextTSTick && nextTick <= endRange)
            {
                uint currentTick = nextTick;
                bool tickIsMeasure = quarterLineIndex % ts.quarterNotesPerMeasure == 0;

                if (currentTick >= startRange && currentTick < nextTSTick && currentTick <= endRange)
                {
                    if (tickIsMeasure)
                        SetBeatLinePosition(currentTick, measureLinePool, ref measurePoolPos);
                    else
                        SetBeatLinePosition(currentTick, beatLinePool, ref beatPoolPos);
                }

                nextTick = ts.tick + (uint)Mathf.Round(beatDeltaTick * (++quarterLineIndex));

                uint tickDelta = nextTick - currentTick;
                uint newPosition = currentTick + tickDelta / 2;

                if (newPosition >= startRange && newPosition < nextTSTick && newPosition <= endRange)
                    SetBeatLinePosition(newPosition, quarterBeatLinePool, ref quarterPoolPos);
            }
        }

        DisableBeatLines(measurePoolPos, measureLinePool);
        DisableBeatLines(beatPoolPos, beatLinePool);
        DisableBeatLines(quarterPoolPos, quarterBeatLinePool);
    }

    void UpdateBeatLines2()
    {
        // Update time signature lines SNAPPED
        uint initSnappedLinePos = editor.currentSong.WorldPositionToSnappedTick(editor.camYMin.position.y, 8);
        uint snappedLinePos = initSnappedLinePos;

        int measurePoolPos = 0, quarterPoolPos = 0, eigthPoolPos = 0;
        const float STANDARD_TS_NUMERATOR = 4.0f;

        while (snappedLinePos < editor.maxPos)
        {
            // Get the previous time signature
            TimeSignature prevTS = editor.currentSong.GetPrevTS(snappedLinePos);
            float tsRatio = STANDARD_TS_NUMERATOR / (float)prevTS.denominator;

            // Bold lines
            if ((snappedLinePos - prevTS.tick) % (uint)(editor.currentSong.resolution * prevTS.numerator * tsRatio) == 0)
            {
                SetBeatLinePosition(snappedLinePos, measureLinePool, ref measurePoolPos);
            }
            // Beat lines
            else if (snappedLinePos % (uint)(editor.currentSong.resolution * tsRatio) == 0)
            {
                SetBeatLinePosition(snappedLinePos, beatLinePool, ref quarterPoolPos);
            }
            // Faded lines
            else
            {
                SetBeatLinePosition(snappedLinePos, quarterBeatLinePool, ref eigthPoolPos);
            }

            DisableBeatLines(measurePoolPos, measureLinePool);
            DisableBeatLines(quarterPoolPos, beatLinePool);
            DisableBeatLines(eigthPoolPos, quarterBeatLinePool);

            uint beatSpacing = (uint)(editor.currentSong.resolution * tsRatio / 2.0f);

            snappedLinePos += beatSpacing;
        }
    }

    void SetBeatLinePosition(uint snappedTickPos, GameObject[] beatLinePool, ref int beatLinePoolPos)
    {
        if (beatLinePoolPos < beatLinePool.Length)
        {
            beatLinePool[beatLinePoolPos].transform.position = new Vector3(0, editor.currentSong.TickToWorldYPosition(snappedTickPos), 0);
            beatLinePool[beatLinePoolPos].SetActive(true);
            ++beatLinePoolPos;
        }
    }

    void DisableBeatLines(int offset, GameObject[] beatLinePool)
    {
        // Disable any unused lines
        while (offset < beatLinePool.Length && beatLinePool[offset].activeSelf)
        {
            beatLinePool[offset++].SetActive(false);
        }
    }

    /// <summary>
    /// Depricated
    /// </summary>
    void UpdateBeatLines()
    {
        // Update time signature lines SNAPPED
        uint initSnappedLinePos = editor.currentSong.WorldPositionToSnappedTick(editor.camYMin.position.y, 4);
        uint snappedLinePos = initSnappedLinePos;

        // Place measure beat lines
        int i = 0;      
        while (snappedLinePos < editor.maxPos && i < beatLinePool.Length)
        {
            beatLinePool[i].SetActive(true);

            if (Globals.viewMode == Globals.ViewMode.Song && snappedLinePos % (editor.currentSong.resolution * 4) == 0)
                beatLinePool[i].transform.localScale = new Vector3(1.1f, beatLinePool[i].transform.localScale.y, beatLinePool[i].transform.localScale.z);  // Whole measure beat line
            else
                beatLinePool[i].transform.localScale = new Vector3(1, beatLinePool[i].transform.localScale.y, beatLinePool[i].transform.localScale.z);

            beatLinePool[i].transform.position = new Vector3(0, editor.currentSong.TickToWorldYPosition(snappedLinePos), 0);
            snappedLinePos += (uint)(editor.currentSong.resolution);
            
            ++i;
        }

        // Disable any unused lines
        while (i < beatLinePool.Length && beatLinePool[i].activeSelf)
        {
            beatLinePool[i++].SetActive(false);
        }

        // Place faded beat lines
        i = 0;
        uint offset = (uint)(editor.currentSong.resolution / 2);

        if (offset < initSnappedLinePos)
            snappedLinePos = initSnappedLinePos - offset;
        else
            snappedLinePos = initSnappedLinePos + offset;

        while (snappedLinePos < editor.maxPos && i < quarterBeatLinePool.Length)
        {
            quarterBeatLinePool[i].SetActive(false);
            if (editor.currentSong.GetPrevTS(snappedLinePos).numerator < 7)     // secondary beat lines don't appear in-game if the ts is more than 6
            {
                uint bpm = editor.currentSong.GetPrevBPM(snappedLinePos).value;

                if (bpm < 181000)               //  secondary beat lines don't appear in-game if the bpm is greater than 181
                {
                    if (bpm < 180000)
                    {
                        // Line for every beat
                        quarterBeatLinePool[i].SetActive(true);
                        quarterBeatLinePool[i].transform.position = new Vector3(0, editor.currentSong.TickToWorldYPosition(snappedLinePos), 0);
                    }
                    else
                    {
                        // Line every 3 beats for the range 180-181, offset by 1 beat
                        float factor = editor.currentSong.resolution * 3;
                        if ((int)snappedLinePos - (int)offset - editor.currentSong.resolution >= 0 && (snappedLinePos - offset - editor.currentSong.resolution) % factor == 0)
                        {
                            quarterBeatLinePool[i].SetActive(true);
                            quarterBeatLinePool[i].transform.position = new Vector3(0, editor.currentSong.TickToWorldYPosition(snappedLinePos), 0);
                        }
                    }
                }
            }

            snappedLinePos += (uint)(editor.currentSong.resolution);
            ++i;
        }

        // Disable any unused lines
        while (i < quarterBeatLinePool.Length && quarterBeatLinePool[i].activeSelf)
        {
            quarterBeatLinePool[i++].SetActive(false);
        }
    }
}
