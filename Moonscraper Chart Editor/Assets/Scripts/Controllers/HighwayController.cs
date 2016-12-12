using UnityEngine;
using System.Collections;

public class HighwayController : MonoBehaviour {
    const int POOL_SIZE = 100;

    public GameObject beatLine1;
    public GameObject beatLine2;

    GameObject[] beatLinePool1 = new GameObject[POOL_SIZE];
    GameObject[] beatLinePool2 = new GameObject[POOL_SIZE];

    GameObject beatLineParent;

    ChartEditor editor;

    // Use this for initialization
    void Start () {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();

        beatLineParent = new GameObject("Beat Lines");

        for (int i = 0; i < POOL_SIZE; ++i)
        {
            beatLinePool1[i] = Instantiate(beatLine1);
            beatLinePool1[i].transform.SetParent(beatLineParent.transform);
            beatLinePool1[i].SetActive(false);

            beatLinePool2[i] = Instantiate(beatLine2);
            beatLinePool2[i].transform.SetParent(beatLineParent.transform);
            beatLinePool2[i].SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update () {
        UpdateBeatLines();
    }

    void UpdateBeatLines()
    {
        // Update time signature lines SNAPPED
        uint initSnappedLinePos = editor.currentSong.WorldPositionToSnappedChartPosition(editor.camYMin.position.y, 4);
        uint snappedLinePos = initSnappedLinePos;

        // Place main beat lines
        int i = 0;
        while (snappedLinePos < editor.maxPos && i < beatLinePool1.Length)
        {
            beatLinePool1[i].SetActive(true);
            beatLinePool1[i].transform.position = new Vector3(0, editor.currentSong.ChartPositionToWorldYPosition(snappedLinePos), 0);
            snappedLinePos += (uint)(editor.currentSong.resolution);
            ++i;
        }

        // Disable any unused lines
        while (i < beatLinePool1.Length)
        {
            beatLinePool1[i++].SetActive(false);
        }

        // Place faded beat lines
        i = 0;
        uint offset = (uint)(editor.currentSong.resolution / 2);

        if (offset < initSnappedLinePos)
            snappedLinePos = initSnappedLinePos - offset;
        else
            snappedLinePos = initSnappedLinePos + offset;

        while (snappedLinePos < editor.maxPos && i < beatLinePool2.Length)
        {
            beatLinePool2[i].SetActive(false);
            if (editor.currentSong.GetPrevTS(snappedLinePos) < 7)     // secondary beat lines don't appear in-game if the ts is more than 6
            {
                uint bpm = editor.currentSong.GetPrevBPM(snappedLinePos);

                if (bpm < 181000)               //  secondary beat lines don't appear in-game if the bpm is greater than 181
                {
                    if (bpm < 180000)
                    {
                        // Line for every beat
                        beatLinePool2[i].SetActive(true);
                        beatLinePool2[i].transform.position = new Vector3(0, editor.currentSong.ChartPositionToWorldYPosition(snappedLinePos), 0);
                    }
                    else
                    {
                        // Line every 3 beats for the range 180-181, offset by 1 beat
                        float factor = editor.currentSong.resolution * 3;
                        if ((int)snappedLinePos - (int)offset - editor.currentSong.resolution >= 0 && (snappedLinePos - offset - editor.currentSong.resolution) % factor == 0)
                        {
                            beatLinePool2[i].SetActive(true);
                            beatLinePool2[i].transform.position = new Vector3(0, editor.currentSong.ChartPositionToWorldYPosition(snappedLinePos), 0);
                        }
                    }
                }
            }

            snappedLinePos += (uint)(editor.currentSong.resolution);
            ++i;
        }

        // Disable any unused lines
        while (i < beatLinePool2.Length)
        {
            beatLinePool2[i++].SetActive(false);
        }
    }
}
