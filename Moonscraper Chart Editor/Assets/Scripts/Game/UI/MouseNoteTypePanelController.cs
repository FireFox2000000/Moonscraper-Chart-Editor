using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseNoteTypePanelController : MonoBehaviour
{
    [SerializeField]
    Button m_multiNoteButton;
    [SerializeField]
    Button m_openNoteButton;

    public GameObject noteToolObject;
    PlaceNoteController noteToolController;

    // Start is called before the first frame update
    void Start()
    {
        noteToolController = noteToolObject.GetComponent<PlaceNoteController>();
    }

    // Update is called once per frame
    void Update()
    {
        bool openActive = noteToolController.MouseModeOpenNoteActive();
        m_openNoteButton.interactable = !openActive;
        m_multiNoteButton.interactable = openActive;
    }

    public void SetOpenNote()
    {
        noteToolController.MouseModeSetOpenNoteActive(true);
    }

    public void SetMultiNote()
    {
        noteToolController.MouseModeSetOpenNoteActive(false);
    }
}
