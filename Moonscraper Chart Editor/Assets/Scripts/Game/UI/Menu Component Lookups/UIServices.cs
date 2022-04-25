// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIServices : MonoBehaviour {

    public EditorPanels editorPanels { get; private set; }

    [SerializeField]
    GameObject gameplayUICanvas;

    MenuBar m_menuBar = null;
    public MenuBar menuBar
    {
        get
        {
            if (!m_menuBar)
            {
                m_menuBar = GetComponentInChildren<MenuBar>();
            }

            return m_menuBar;
        }
    }

    LyricEditor2Controller m_lyricEditor = null;
    public LyricEditor2Controller lyricEditor
    {
        get
        {
            if (!m_lyricEditor)
            {
                m_lyricEditor = GetComponentInChildren<LyricEditor2Controller>(true);
            }

            return m_lyricEditor;
        }
    }

    bool _popupBlockerEnabled = false;
    Camera _uiCamera;
    public Camera uiCamera
    {
        get
        {
            if (_uiCamera == null)
                _uiCamera = GetComponent<Canvas>().worldCamera;

            return _uiCamera;
        }
    }

	// Use this for initialization
	void Start () {
        editorPanels = GetComponentInChildren<EditorPanels>();

        Debug.Assert(editorPanels, "Unable to locate Editor Panels script");

        // Every inputfield needs this attached to it
        InputField[] allInputFields = GetComponentsInChildren<InputField>(true);
        foreach (InputField inputField in allInputFields)
        {
            if (!inputField.gameObject.GetComponent<InputFieldDoubleClick>())
            {
                inputField.gameObject.AddComponent<InputFieldDoubleClick>();
            }
        }

        SetGameplayUIActive(false);
    }

    public Vector2 GetUIMousePosition()
    {
        return uiCamera.ScreenToWorldPoint(Input.mousePosition);
    }

    public bool popupBlockerEnabled
    {
        get
        {
            return _popupBlockerEnabled || ChartEditor.Instance.currentState == ChartEditor.State.Loading;
        }
    }

    public void SetPopupBlockingEnabled(bool enabled)
    {
        _popupBlockerEnabled = enabled;
    }

    public void SetGameplayUIActive(bool active)
    {
        gameplayUICanvas.SetActive(active);
    }
}
