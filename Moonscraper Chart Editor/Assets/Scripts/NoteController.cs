using UnityEngine;
using System.Collections;

public class NoteController : MonoBehaviour {

    public Note noteProperties;
    SpriteRenderer noteRenderer;
    
    void Awake()
    {
        noteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown()
    {
        Debug.Log(noteProperties.position);
    }
}
