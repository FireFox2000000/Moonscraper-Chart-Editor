using UnityEngine;
using System.Collections;

public abstract class MovementController : MonoBehaviour {
    [HideInInspector]
    public ApplicationMode applicationMode = ApplicationMode.Editor;

    // Jump to a chart position
    public abstract void SetPosition(uint chartPosition);

    public enum ApplicationMode
    {
        Editor, Playing
    }
}
