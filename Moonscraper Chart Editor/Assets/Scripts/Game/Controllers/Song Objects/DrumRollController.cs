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
    GameObject m_triggerVisualsPlane = null;
    [SerializeField]
    BoxCollider m_collision = null;

    [SerializeField]
    GameObject[] m_laneVisuals = null;

    [SerializeField]
    SustainResources m_resources = null;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    float m_laneVisualAlpha = 0.5f;

    float m_triggerVisualsInitZScale = 1.0f;
    Transform m_triggerVisualsTransform;
    MaterialPropertyBlock m_triggerVisualsPropertyBlock;
    Renderer m_triggerVisualsRenderer;
    Vector4 m_triggerVisualsInitTransform;
    const string c_triggerVisualsTransformVecId = "_MainTex_ST";

    List<Note.DrumPad> m_drumPadRollPriority = new List<Note.DrumPad>();

    const float MinVisualLength = 0.1f;

    protected override void Awake()
    {
        m_triggerVisualsTransform = m_triggerVisualsPlane.transform;
        m_triggerVisualsInitZScale = m_triggerVisualsPlane.transform.localScale.z;
        m_triggerVisualsPropertyBlock = new MaterialPropertyBlock();
        m_triggerVisualsRenderer = m_triggerVisualsPlane.GetComponent<Renderer>();

        m_triggerVisualsRenderer.GetPropertyBlock(m_triggerVisualsPropertyBlock);
        m_triggerVisualsInitTransform = m_triggerVisualsRenderer.sharedMaterial.GetVector(c_triggerVisualsTransformVecId);
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
        length = Mathf.Max(length, MinVisualLength);

        {
            var scale = m_triggerVisualsTransform.localScale;
            scale.z = m_triggerVisualsInitZScale * length;
            m_triggerVisualsTransform.localScale = scale;
            
            var propertyTransform = m_triggerVisualsInitTransform;
            propertyTransform.y *= length;
            m_triggerVisualsRenderer.GetPropertyBlock(m_triggerVisualsPropertyBlock);
            m_triggerVisualsPropertyBlock.SetVector(c_triggerVisualsTransformVecId, propertyTransform);
            m_triggerVisualsRenderer.SetPropertyBlock(m_triggerVisualsPropertyBlock);
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
        var notes = editor.currentChart.notes;

        int start, length;
        SongObjectHelper.GetRange(notes, drumRoll.tick, drumRoll.tick + drumRoll.length, out start, out length);

        m_drumPadRollPriority.Clear();
        GetDrumLanesToRenderPriority(start, length, m_drumPadRollPriority);

        if (drumRollNoteIndex < m_drumPadRollPriority.Count)
        {
            Note.DrumPad pad = m_drumPadRollPriority[drumRollNoteIndex];
            bool leftyFlip = Globals.gameSettings.notePlacementMode == GameSettings.NotePlacementMode.LeftyFlip;
            var lanePosition = ChartEditor.Instance.laneInfo.GetLanePosition((int)pad, leftyFlip);

            // Figure out the total size of the lane
            uint tickMin = uint.MaxValue;
            uint tickMax = uint.MinValue;
            for (int i = start; i < start + length; ++i)
            {
                Note note = notes[i];
                if (note.drumPad == pad)
                {
                    if (note.tick < tickMin)
                    {
                        tickMin = note.tick;
                    }

                    if (note.tick > tickMax)
                    {
                        tickMax = note.tick;
                    }
                }
            }

            Debug.Assert(tickMin <= tickMax);

            Song song = drumRoll.song;
            float yMin = song.TickToWorldYPosition(tickMin);
            float yMax = song.TickToWorldYPosition(tickMax);
            float laneCentrePos = Mathf.Lerp(yMin, yMax, 0.5f);

            laneVisuals.transform.position = new Vector3(CHART_CENTER_POS + lanePosition, laneCentrePos, 0);

            var scale = laneVisuals.transform.localScale;
            scale.y = yMax - yMin;
            laneVisuals.transform.localScale = scale;

            Skin customSkin = SkinManager.Instance.currentSkin;
            int colorIndex = (int)pad;
            if (colorIndex < customSkin.sustain_mats.Length)
            {
                var sprite = laneVisuals.GetComponent<SpriteRenderer>();
                Color color;
                if (customSkin.sustain_mats[colorIndex])
                {
                    color = customSkin.sustain_mats[colorIndex].color;
                }
                else
                {
                    color = m_resources.sustainColours[colorIndex].color;
                }

                Debug.Assert(m_laneVisualAlpha >= 0.0f && m_laneVisualAlpha <= 1.0f, "Lane visual alpha out of range");
                color.a = Mathf.Clamp(m_laneVisualAlpha, 0.0f, 1.0f);
                sprite.color = color;
            }

            laneVisuals.SetActive(true);
        }
        else
        {
            laneVisuals.SetActive(false);
        }
    }

    static void GetDrumLanesToRenderPriority(int start, int length, List<Note.DrumPad> drumPadPriority)
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

            bool padAlreadyAdded = (noteMask & (1 << (int)note.drumPad)) != 0;
            if (!padAlreadyAdded)
            {
                // Sort by tick, left to right
                drumPadPriority.Add(note.drumPad);
            }
            noteMask |= 1 << (int)note.drumPad;
        }
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
