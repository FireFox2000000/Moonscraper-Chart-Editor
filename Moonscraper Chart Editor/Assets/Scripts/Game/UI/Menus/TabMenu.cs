using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabMenu : DisplayMenu
{
    Button currentButton;
    RectTransform currentContent;

    [SerializeField]
    Button initialMenuItem;

    protected RectTransform menuContextArea = null;

    protected virtual void Start()
    {
        ResetToInitialMenuItem();
    }

    protected void ResetToInitialMenuItem()
    {
        initialMenuItem.onClick.Invoke();
    }

    public void SetTabGroup(RectTransform content)
    {
        if (currentContent)
        {
            currentContent.gameObject.SetActive(false);
        }

        content.gameObject.SetActive(true);

        if (menuContextArea)
        {
            Vector2 size = new Vector2();
            size.x = menuContextArea.sizeDelta.x;
            size.y += content.rect.height - content.localPosition.y;
            menuContextArea.sizeDelta = size;
        }

        currentContent = content;
    }

    public void SetCurrentButton(Button button)
    {
        if (currentButton)
        {
            currentButton.interactable = true;
        }

        currentButton = button;
        currentButton.interactable = false;
    }
}
