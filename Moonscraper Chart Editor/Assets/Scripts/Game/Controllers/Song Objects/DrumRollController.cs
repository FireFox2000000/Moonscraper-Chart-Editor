using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

public class DrumRollController : SongObjectController
{
    public DrumRoll drumRoll { get { return (DrumRoll)songObject; } set { Init(value); } }
    public const float position = 0.0f;
    bool m_wantPop = false;

    [SerializeField]
    GameObject m_triggerVisualsPlane;
    [SerializeField]
    BoxCollider m_collision;

    [SerializeField]
    GameObject[] m_laneVisuals;

    float m_triggerVisualsInitZScale = 1.0f;
    Transform m_triggerVisualsTransform;

    protected override void Awake()
    {
        m_triggerVisualsTransform = m_triggerVisualsPlane.transform;
        m_triggerVisualsInitZScale = m_triggerVisualsPlane.transform.localScale.z;
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Reset();
    }

    public override void UpdateSongObject()
    {
        if (drumRoll.song != null)
        {
            transform.position = new Vector3(CHART_CENTER_POS + position, desiredWorldYPosition, 0);

            if (isDirty)
            {
                UpdateLength();

                int laneVisualIndex = 0;
                switch (drumRoll.type)
                {
                    case DrumRoll.Type.Standard:
                        {
                            Debug.Assert(m_laneVisuals.Length > 0);
                            SetLanePosition(m_laneVisuals[laneVisualIndex], laneVisualIndex);
                            ++laneVisualIndex;

                            break;
                        }
                    case DrumRoll.Type.Special:
                        {
                            Debug.Assert(m_laneVisuals.Length > 1);
                            SetLanePosition(m_laneVisuals[laneVisualIndex], laneVisualIndex);
                            ++laneVisualIndex;

                            SetLanePosition(m_laneVisuals[laneVisualIndex], laneVisualIndex);
                            ++laneVisualIndex;

                            break;
                        }
                }

                for (int i = laneVisualIndex; i < m_laneVisuals.Length; ++i)
                {
                    m_laneVisuals[laneVisualIndex].SetActive(false);
                }
            }
        }

        isDirty = false;
    }

    void Init(DrumRoll drumRoll)
    {
        base.Init(drumRoll, this);
        SetDirty();
        UpdateSongObject();
    }

    public override void OnSelectableMouseDown()
    {
        Reset();
        base.OnSelectableMouseDown();
    }

    public override void OnSelectableMouseDrag()
    {
        if (!DragCheck())
        {
            base.OnSelectableMouseDrag();
        }
    }

    public override void OnSelectableMouseUp()
    {
        Reset();
    }

    public void Reset()
    {
        m_wantPop = false;
    }

    bool DragCheck()
    {
        if (editor.currentState == ChartEditor.State.Editor && Input.GetMouseButton(1))
        {
            TailDrag();
            return true;
        }

        return false;
    }

    void UpdateLength()
    {
        float length = drumRoll.song.TickToWorldYPosition(drumRoll.tick + drumRoll.length) - desiredWorldYPosition;
        length = Mathf.Max(length, 0.1f);

        {
            var scale = m_triggerVisualsTransform.localScale;
            scale.z = m_triggerVisualsInitZScale * length;
            m_triggerVisualsTransform.localScale = scale;
        }

        {
            Vector3 position = transform.position;
            position.y += length / 2.0f;
            m_triggerVisualsTransform.transform.position = position;
        }

        {
            var collisionSize = m_collision.size;
            collisionSize.z = length;
            m_collision.size = collisionSize;

            Vector3 position = m_collision.center;
            position.z = length / 2.0f;
            m_collision.center = position;
        }
    }

    void SetLanePosition(GameObject laneVisuals, int drumRollNoteIndex)
    {
        // Find all the notes that are meant to be within this lane
        var notes = ChartEditor.Instance.currentChart.notes;
        int start, length;
        SongObjectHelper.GetRange(notes, drumRoll.tick, drumRoll.tick + drumRoll.length, out start, out length);

        int noteMask = GetNoteMaskInRange(start, length);
        int totalLanesActive = 0;
        int currentLane = 0;
        int activeLanesTarget = drumRollNoteIndex + 1;

        while (currentLane < MoonscraperEngine.EnumX<Note.DrumPad>.Count)
        {
            int currentLaneMask = 1 << currentLane;
            if ((noteMask & currentLaneMask) != 0)
            {
                ++totalLanesActive;
            }

            if (totalLanesActive == activeLanesTarget)
            {
                break;
            }
            else
            {
                ++currentLane;
            }
        }

        // Successfully found a lane to render
        if (totalLanesActive == activeLanesTarget)
        {
            laneVisuals.SetActive(true);
        }
        else
        {
            laneVisuals.SetActive(false);
        }
    }

    int GetNoteMaskInRange(int start, int length)
    {
        int noteMask = 0;
        var notes = ChartEditor.Instance.currentChart.notes;
        for (int i = start; i < start + length; ++i)
        {
            var note = notes[i];
            if (note.drumPad == Note.DrumPad.Kick)
            {
                continue;
            }

            noteMask |= 1 << (int)note.drumPad;
        }

        return noteMask;
    }

    void TailDrag()
    {
        uint snappedChartPos;

        if (editor.services.mouseMonitorSystem.world2DPosition != null && ((Vector2)editor.services.mouseMonitorSystem.world2DPosition).y < editor.mouseYMaxLimit.position.y)
        {
            snappedChartPos = Snapable.TickToSnappedTick(drumRoll.song.WorldYPositionToTick(((Vector2)editor.services.mouseMonitorSystem.world2DPosition).y), Globals.gameSettings.step, drumRoll.song);
        }
        else
        {
            snappedChartPos = Snapable.TickToSnappedTick(drumRoll.song.WorldYPositionToTick(editor.mouseYMaxLimit.position.y), Globals.gameSettings.step, drumRoll.song);
        }

        // Cap to within the range of the song
        snappedChartPos = (uint)Mathf.Min(editor.maxPos, snappedChartPos);

        uint newLength = drumRoll.GetCappedLengthForPos(snappedChartPos);
        if (newLength != drumRoll.length)
        {
            if (m_wantPop)
            {
                editor.commandStack.Pop();
            }

            editor.commandStack.Push(new SongEditModify<DrumRoll>(drumRoll, new DrumRoll(drumRoll.tick, newLength, drumRoll.type)));

            m_wantPop = true;
        }
    }
}
