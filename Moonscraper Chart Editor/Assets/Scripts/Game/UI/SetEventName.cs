// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetEventName : MonoBehaviour {

    [SerializeField]
    EventPropertiesPanelController ePropCon = null;
    [SerializeField]
    new UnityEngine.UI.Text name = null;
	
	public void SetEvent()
    {
        ePropCon.UpdateEventName(name.text);
    }
}
