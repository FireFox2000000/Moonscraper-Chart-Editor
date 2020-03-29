// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UITabbing : UpdateableService {
    public static Selectable defaultSelectable = null;
	
	// Update is called once per frame
	public override void OnServiceUpdate() {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = defaultSelectable;
            if (EventSystem.current.currentSelectedGameObject)
            {
                if (Globals.secondaryInputActive)
                    next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
                else
                    next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            }

            if (next != null)
            {
                InputField inputfield = next.GetComponent<InputField>();
                if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(EventSystem.current));  //if it's an input field, also set the text caret

                EventSystem.current.SetSelectedGameObject(next.gameObject, new BaseEventData(EventSystem.current));
            }
        }
    }
}
