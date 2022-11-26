// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using MoonscraperChartEditor.Song;

// Stores all collider information for group select collision detection
public class PrefabGlobals {
    static bool hasBeenInitialised = false;
    static Vector2 noteColliderSize, 
        spColliderSize, 
        chartEventColliderSize, 
        bpmColliderSize, 
        tsColliderSize, 
        sectionColliderSize, 
        eventColliderSize,
        drumRollColliderSize;

    static void Init()
    {
        ChartEditor editor = ChartEditor.Instance;

        // Collect prefab collider sizes
        noteColliderSize = GetColliderSize(editor.assets.notePrefab);
        spColliderSize = GetColliderSize(editor.assets.starpowerPrefab);
        chartEventColliderSize = GetColliderSize(editor.assets.chartEventPrefab);
        bpmColliderSize = GetColliderSize(editor.assets.bpmPrefab);
        tsColliderSize = GetColliderSize(editor.assets.tsPrefab);
        sectionColliderSize = GetColliderSize(editor.assets.sectionPrefab);
        eventColliderSize = GetColliderSize(editor.assets.songEventPrefab);
        drumRollColliderSize = GetColliderSize(editor.assets.drumRollPrefab);

        hasBeenInitialised = true;
    }

    static void TryInit()
    {
        if (!hasBeenInitialised)
            Init();
    }

    static Vector2 GetColliderSize(GameObject gameObject)
    {
        Vector2 size;

        GameObject copy = GameObject.Instantiate(gameObject);
        copy.SetActive(true);
        Collider col3d = copy.GetComponent<Collider>();
        Collider2D col2d = copy.GetComponent<Collider2D>();

        /************ Note Unity documentation- ************/
        // Bounds: The world space bounding volume of the collider.
        // Note that this will be an empty bounding box if the collider is disabled or the game object is inactive.
        if (col3d)
            size = col3d.bounds.size;
        else if (col2d)
            size = col2d.bounds.size;
        else
            size = Vector2.zero;

        GameObject.Destroy(copy);
        return size;
    }

    public static Rect GetCollisionRect(SongObject songObject, float posOfChart = 0, float offset = 0)
    {
        TryInit();

        Vector2 colliderSize;
        Vector2 position;

        switch ((SongObject.ID)songObject.classID)
        {
            case (SongObject.ID.Note):
                colliderSize = noteColliderSize;
                if (((Note)songObject).IsOpenNote())
                    colliderSize.x = NoteController.OPEN_NOTE_COLLIDER_WIDTH;
                position = new Vector2(NoteController.NoteToXPos((Note)songObject), 0);
                break;
            case (SongObject.ID.Starpower):
                colliderSize = spColliderSize;
                position = new Vector2(StarpowerController.position, 0);
                break;
            case (SongObject.ID.ChartEvent):
                colliderSize = chartEventColliderSize;
                position = new Vector2(ChartEventController.position, 0);
                break;
            case (SongObject.ID.BPM):
                colliderSize = bpmColliderSize;
                position = new Vector2(BPMController.position, 0);
                break;
            case (SongObject.ID.TimeSignature):
                colliderSize = tsColliderSize;
                position = new Vector2(TimesignatureController.position, 0);
                break;
            case (SongObject.ID.Section):
                colliderSize = sectionColliderSize;
                position = new Vector2(SectionController.position, 0);
                break;
            case (SongObject.ID.Event):
                colliderSize = eventColliderSize;
                position = new Vector2(EventController.position, 0);
                break;
            case (SongObject.ID.DrumRoll):
                colliderSize = drumRollColliderSize;
                position = new Vector2(DrumRollController.position, 0);
                break;
            default:
                return new Rect();
        }

        Vector2 min = new Vector2(position.x + posOfChart + offset - colliderSize.x / 2, position.y - colliderSize.y / 2);
        return new Rect(min, colliderSize);
    }

    public static bool HorizontalCollisionCheck(Rect rectA, Rect rectB)
    {
        TryInit();

        if (rectA.width == 0 || rectB.width == 0)
            return false;

        // AABB, check for any gaps
        if (rectA.x <= rectB.x + rectB.width &&
               rectA.x + rectA.width >= rectB.x)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
