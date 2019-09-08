using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MSE.Input;

public class ActionBindingsMenu : MonoBehaviour
{
    [SerializeField]
    RectTransform content;
    [SerializeField]
    Text actionNamePrefab;
    [SerializeField]
    Button actionInputPrefab;

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
                return actionNameText.IsActive();
            }
        }

        public void SetupFromAction(InputAction inputAction, InputAction.Device device)
        {
            // populate strings and callback fns
            actionNameText.text = inputAction.displayName;

            var maps = inputAction.GetMapsForDevice(device);

            if (maps != null)
            {
                for (int i = 0; i < maps.Length && i < actionInputButtons.Length; ++i)
                {
                    var map = maps[i];
                   
                    var button = actionInputButtons[i];
                    var buttonText = button.GetComponentInChildren<Text>();

                    Debug.Assert(buttonText);
                    if (map != null)
                    {
                        buttonText.text = map.GetInputStr();
                    }
                    else
                    {
                        buttonText.text = kNoInputStr;
                    }

                    button.interactable = inputAction.properties.rebindable;
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
    InputAction.Device lastKnownDisplayDevice = InputAction.Device.Keyboard;
    IEnumerable<InputAction> loadedActions;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // For testing
        ShortcutInput.ShortcutActionContainer actions = new ShortcutInput.ShortcutActionContainer();
        GameSettings.LoadDefaultControls(actions);

        LoadActions(actions);
        SetDevice(InputAction.Device.Keyboard);
    }

    public void SetDevice(InputAction.Device device)
    {
        lastKnownDisplayDevice = device;
        PopulateFrom(loadedActions);
    }

    public void LoadActions(IEnumerable<InputAction> actionEnumerator)
    {
        loadedActions = actionEnumerator;
        PopulateFrom(loadedActions);
    }

    void PopulateFrom(IEnumerable<InputAction> actionEnumerator)
    {
        int index = 0;
        foreach(var inputAction in actionEnumerator)
        {
            if (inputAction.properties.hiddenInLists)
                continue;

            if (index >= rowPool.Count)
                ExtendActionRowPool(20);

            ActionUIRow row = rowPool[index++];
            row.SetupFromAction(inputAction, lastKnownDisplayDevice);
            row.SetActive(true);
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
}
