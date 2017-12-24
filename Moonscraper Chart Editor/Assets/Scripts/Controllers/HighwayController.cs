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
        UpdateBeatLines2();
    }
    void UpdateBeatLines2()
    {
        // Update time signature lines SNAPPED
        uint initSnappedLinePos = editor.currentSong.WorldPositionToSnappedChartPosition(editor.camYMin.position.y, 8);
        uint snappedLinePos = initSnappedLinePos;

        int measurePoolPos = 0, quarterPoolPos = 0, eigthPoolPos = 0;
        const float STANDARD_TS_NUMERATOR = 4.0f;

        while (snappedLinePos < editor.maxPos)
        {
            // Get the previous time signature
            TimeSignature prevTS = editor.currentSong.GetPrevTS(snappedLinePos);
            float tsRatio = STANDARD_TS_NUMERATOR / (float)prevTS.denominator;

            // Bold lines
            if ((snappedLinePos - prevTS.position) % (editor.currentSong.resolution * prevTS.numerator * tsRatio) == 0)
            {
                SetBeatLinePosition(snappedLinePos, measureLinePool, ref measurePoolPos);
            }
            // Beat lines
            else if (snappedLinePos % (editor.currentSong.resolution * tsRatio) == 0)
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

            uint beatSpacing = (uint)(tsRatio * editor.currentSong.resolution / 2.0f);

            snappedLinePos += beatSpacing;
        }
    }

    void SetBeatLinePosition(uint snappedTickPos, GameObject[] beatLinePool, ref int beatLinePoolPos)
    {
        if (beatLinePoolPos < beatLinePool.Length)
        {
            beatLinePool[beatLinePoolPos].transform.position = new Vector3(0, editor.currentSong.ChartPositionToWorldYPosition(snappedTickPos), 0);
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
        uint initSnappedLinePos = editor.currentSong.WorldPositionToSnappedChartPosition(editor.camYMin.position.y, 4);
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

            beatLinePool[i].transform.position = new Vector3(0, editor.currentSong.ChartPositionToWorldYPosition(snappedLinePos), 0);
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
                        quarterBeatLinePool[i].transform.position = new Vector3(0, editor.currentSong.ChartPositionToWorldYPosition(snappedLinePos), 0);
                    }
                    else
                    {
                        // Line every 3 beats for the range 180-181, offset by 1 beat
                        float factor = editor.currentSong.resolution * 3;
                        if ((int)snappedLinePos - (int)offset - editor.currentSong.resolution >= 0 && (snappedLinePos - offset - editor.currentSong.resolution) % factor == 0)
                        {
                            quarterBeatLinePool[i].SetActive(true);
                            quarterBeatLinePool[i].transform.position = new Vector3(0, editor.currentSong.ChartPositionToWorldYPosition(snappedLinePos), 0);
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
