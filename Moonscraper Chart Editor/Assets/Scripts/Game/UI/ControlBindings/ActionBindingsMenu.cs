// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoonscraperEngine.Input;

public class ActionBindingsMenu : MonoBehaviour
{
    [SerializeField]
    bool disablePropertyRestrictions = false;
    [SerializeField]
    RectTransform content;
    [SerializeField]
    Text actionNamePrefab;
    [SerializeField]
    Button actionInputPrefab;
    [SerializeField]
    RebindOverlayInterface rebindInterface;

    [SerializeField]
    float rowHeight = 50;
    [SerializeField]
    float leftSizePadding;
    [SerializeField]
    float xPaddingAfterName;
    [SerializeField]
    float xPaddingBetweenInputButtons;

    RectTransform rectTransform;

    const string kNoInputStr = "-";

    class ActionUIRow
    {
        public const int kMaxInputButtons = 2;
        public Text actionNameText;
        public Button[] actionInputButtons = new Button[kMaxInputButtons];

        public delegate void ButtonClickCallback(IInputMap inputMapToRebind, InputAction inputAction, IEnumerable<InputAction> allActions, IInputDevice device);

        public void SetActive(bool active)
        {
            actionNameText.gameObject.SetActive(active);
            foreach(var button in actionInputButtons)
            {
                button.gameObject.SetActive(active);
            }
        }

        public bool isActive
        {
            get
            {
                return actionNameText.gameObject.activeSelf;
            }
        }

        public void SetupFromAction(InputAction inputAction, IEnumerable<InputAction> allActions, IInputDevice device, ButtonClickCallback callbackFn, bool disablePropertyRestrictions)
        {
            // populate strings and callback fns
            actionNameText.text = inputAction.properties.displayName;

            var maps = inputAction.GetMapsForDevice(device);

            if (maps != null && maps.Count > 0)
            {
                for (int i = 0; i < actionInputButtons.Length; ++i)
                {
                    IInputMap map;
                    if (i >= maps.Count)
                    {
                        var clone = maps[0].Clone();
                        clone.SetEmpty();
                        inputAction.Add(clone);
                        map = clone;
                    }
                    else
                    {
                        map = maps[i];
                    } 
                   
                    var button = actionInputButtons[i];
                    var buttonText = button.GetComponentInChildren<Text>();

                    Debug.Assert(buttonText);
                    Debug.Assert(map != null);

                    if (map != null && !map.IsEmpty)
                    {
                        buttonText.text = map.GetInputStr();
                    }
                    else
                    {
                        buttonText.text = kNoInputStr;
                    }

                    button.interactable = inputAction.properties.rebindable || disablePropertyRestrictions;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(delegate { callbackFn(map, inputAction, allActions, device); });
                }
            }
            else
            {
                Debug.LogError("Unable to find input maps for device " + device);
            }
        }
    }

    List<ActionUIRow> rowPool = new List<ActionUIRow>();
    GameObject menu;

    IInputDevice lastKnownDisplayDevice;
    IEnumerable<InputAction> loadedActions;
    int categoryDisplayMask = ~0;

    bool initialised = false;

    // Start is called before the first frame update
    void Awake()
    {
        //if (!initialised)
            //Setup(InputManager.Instance.devices[0], Globals.gameSettings.controls, MSChartEditorInput.Category.kEditorCategoryMask);
    }

    void Init()
    {
        if (!initialised)
        {
            rectTransform = GetComponent<RectTransform>();
            rebindInterface.rebindCompleteEvent.Register(OnRebindComplete);
        }
        initialised = true;
    }

    public void Setup(IInputDevice device, IEnumerable<InputAction> actionEnumerator, int categoryDisplayMask)
    {
        Init();

        lastKnownDisplayDevice = device;
        LoadActions(actionEnumerator, categoryDisplayMask);
    }

    public void LoadActions(IEnumerable<InputAction> actionEnumerator, int categoryDisplayMask)
    {
        loadedActions = actionEnumerator;
        this.categoryDisplayMask = categoryDisplayMask;
        PopulateFrom(loadedActions, categoryDisplayMask);
    }

    void PopulateFrom(IEnumerable<InputAction> actionEnumerator, int categoryDisplayMask)
    {
        int index = 0;
        foreach(var inputAction in actionEnumerator)
        {
            if (inputAction.properties.hiddenInLists && !disablePropertyRestrictions)
                continue;

            // Not displaying these actions at the moment.
            if (((1 << inputAction.properties.category) & categoryDisplayMask) == 0)
                continue;

            if (index >= rowPool.Count)
                ExtendActionRowPool(20);

            ActionUIRow row = rowPool[index++];
            row.SetupFromAction(inputAction, actionEnumerator, lastKnownDisplayDevice, InvokeRebindState, disablePropertyRestrictions);
            row.SetActive(true);
        }

        for (int i = index; i < rowPool.Count; ++i)
        {
            ActionUIRow row = rowPool[i];
            row.SetActive(false);
        }

        UpdateRowLayout();
    }

    void ExtendActionRowPool(int extendBy)
    {
        Transform parent = transform;

        for (int i = 0; i < extendBy; ++i)
        {
            bool isActive = false;

            ActionUIRow row = new ActionUIRow();

            row.actionNameText = GameObject.Instantiate(actionNamePrefab, parent);

            for (int j = 0; j < row.actionInputButtons.Length; ++j)
            {
                row.actionInputButtons[j] = GameObject.Instantiate(actionInputPrefab, parent);
            }

            row.SetActive(isActive);

            rowPool.Add(row);
        }
    }

    void UpdateRowLayout()
    {
        float columnStartPosition = leftSizePadding - rectTransform.sizeDelta.x / 2;
        Vector2 position = new Vector2(columnStartPosition, 0);

        for (int i = 0; i < rowPool.Count; ++i)
        {
            ActionUIRow row = rowPool[i];
            if (!row.isActive)
                continue;

            RectTransform textTransform = row.actionNameText.GetComponent<RectTransform>();

            position.x += textTransform.sizeDelta.x / 2;
            textTransform.localPosition = position;

            position.x += textTransform.sizeDelta.x + xPaddingAfterName;

            foreach (var button in row.actionInputButtons)
            {
                RectTransform buttonTransform = button.GetComponent<RectTransform>();
                buttonTransform.localPosition = position;

                position.x += buttonTransform.sizeDelta.x + xPaddingBetweenInputButtons;
            }

            position.x = columnStartPosition;
            position.y -= rowHeight;
        }

        if (content)
            content.sizeDelta = new Vector2(content.sizeDelta.x, -position.y);
    }

    void InvokeRebindState(IInputMap inputMapToRebind, InputAction inputAction, IEnumerable<InputAction> allActions, IInputDevice device)
    {
        rebindInterface.Open(inputAction, inputMapToRebind, allActions, device);
    }

    public void OnRebindComplete()
    {
        PopulateFrom(loadedActions, categoryDisplayMask);
    }
}
