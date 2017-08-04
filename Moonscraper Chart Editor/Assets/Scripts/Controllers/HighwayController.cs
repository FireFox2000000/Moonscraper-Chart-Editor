using UnityEngine;
using System.Collections;

public class HighwayController : MonoBehaviour {
    const int POOL_SIZE = 100;

    public GameObject measureBeatLine;
    public GameObject quarterBeatLine;
    public GameObject eigthBeatLine;

    GameObject[] measureBeatLinePool = new GameObject[POOL_SIZE];
    GameObject[] quarterBeatLinePool = new GameObject[POOL_SIZE];
    GameObject[] eigthBeatLinePool = new GameObject[POOL_SIZE];

    GameObject beatLineParent;

    ChartEditor editor;

    // Use this for initialization
    void Start () {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        beatLineParent = new GameObject("Beat Lines");

        for (int i = 0; i < POOL_SIZE; ++i)
        {
            measureBeatLinePool[i] = Instantiate(measureBeatLine);
            measureBeatLinePool[i].transform.SetParent(beatLineParent.transform);
            measureBeatLinePool[i].SetActive(false);

            quarterBeatLinePool[i] = Instantiate(quarterBeatLine);
            quarterBeatLinePool[i].transform.SetParent(beatLineParent.transform);
            quarterBeatLinePool[i].SetActive(false);

            eigthBeatLinePool[i] = Instantiate(eigthBeatLine);
            eigthBeatLinePool[i].transform.SetParent(beatLineParent.transform);
            eigthBeatLinePool[i].SetActive(false);
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

        uint eigthSpacing = (uint)(editor.currentSong.resolution / 2);
        int measurePoolPos = 0, quarterPoolPos = 0, eigthPoolPos = 0;

        while (snappedLinePos < editor.maxPos)
        {
            // Get the previous time signature
            TimeSignature prevTS = editor.currentSong.GetPrevTS(snappedLinePos);

            if ((snappedLinePos - prevTS.position) % (editor.currentSong.resolution * prevTS.numerator) == 0)
            {
                SetBeatLinePosition(snappedLinePos, measureBeatLinePool, ref measurePoolPos);
            }
            else if (snappedLinePos % (editor.currentSong.resolution) == 0)
            {
                SetBeatLinePosition(snappedLinePos, quarterBeatLinePool, ref quarterPoolPos);
            }
            else
            {
                SetBeatLinePosition(snappedLinePos, eigthBeatLinePool, ref eigthPoolPos);
            }

            DisableBeatLines(measurePoolPos, measureBeatLinePool);
            DisableBeatLines(quarterPoolPos, quarterBeatLinePool);
            DisableBeatLines(eigthPoolPos, eigthBeatLinePool);

            snappedLinePos += eigthSpacing;
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

    void UpdateBeatLines()
    {
        // Update time signature lines SNAPPED
        uint initSnappedLinePos = editor.currentSong.WorldPositionToSnappedChartPosition(editor.camYMin.position.y, 4);
        uint snappedLinePos = initSnappedLinePos;

        // Place measure beat lines
        int i = 0;      
        while (snappedLinePos < editor.maxPos && i < quarterBeatLinePool.Length)
        {
            quarterBeatLinePool[i].SetActive(true);

            if (Globals.viewMode == Globals.ViewMode.Song && snappedLinePos % (editor.currentSong.resolution * 4) == 0)
                quarterBeatLinePool[i].transform.localScale = new Vector3(1.1f, quarterBeatLinePool[i].transform.localScale.y, quarterBeatLinePool[i].transform.localScale.z);  // Whole measure beat line
            else
                quarterBeatLinePool[i].transform.localScale = new Vector3(1, quarterBeatLinePool[i].transform.localScale.y, quarterBeatLinePool[i].transform.localScale.z);

            quarterBeatLinePool[i].transform.position = new Vector3(0, editor.currentSong.ChartPositionToWorldYPosition(snappedLinePos), 0);
            snappedLinePos += (uint)(editor.currentSong.resolution);
            
            ++i;
        }

        // Disable any unused lines
        while (i < quarterBeatLinePool.Length && quarterBeatLinePool[i].activeSelf)
        {
            quarterBeatLinePool[i++].SetActive(false);
        }

        // Place faded beat lines
        i = 0;
        uint offset = (uint)(editor.currentSong.resolution / 2);

        if (offset < initSnappedLinePos)
            snappedLinePos = initSnappedLinePos - offset;
        else
            snappedLinePos = initSnappedLinePos + offset;

        while (snappedLinePos < editor.maxPos && i < eigthBeatLinePool.Length)
        {
            eigthBeatLinePool[i].SetActive(false);
            if (editor.currentSong.GetPrevTS(snappedLinePos).numerator < 7)     // secondary beat lines don't appear in-game if the ts is more than 6
            {
                uint bpm = editor.currentSong.GetPrevBPM(snappedLinePos).value;

                if (bpm < 181000)               //  secondary beat lines don't appear in-game if the bpm is greater than 181
                {
                    if (bpm < 180000)
                    {
                        // Line for every beat
                        eigthBeatLinePool[i].SetActive(true);
                        eigthBeatLinePool[i].transform.position = new Vector3(0, editor.currentSong.ChartPositionToWorldYPosition(snappedLinePos), 0);
                    }
                    else
                    {
                        // Line every 3 beats for the range 180-181, offset by 1 beat
                        float factor = editor.currentSong.resolution * 3;
                        if ((int)snappedLinePos - (int)offset - editor.currentSong.resolution >= 0 && (snappedLinePos - offset - editor.currentSong.resolution) % factor == 0)
                        {
                            eigthBeatLinePool[i].SetActive(true);
                            eigthBeatLinePool[i].transform.position = new Vector3(0, editor.currentSong.ChartPositionToWorldYPosition(snappedLinePos), 0);
                        }
                    }
                }
            }

            snappedLinePos += (uint)(editor.currentSong.resolution);
            ++i;
        }

        // Disable any unused lines
        while (i < eigthBeatLinePool.Length && eigthBeatLinePool[i].activeSelf)
        {
            eigthBeatLinePool[i++].SetActive(false);
        }
    }
}
