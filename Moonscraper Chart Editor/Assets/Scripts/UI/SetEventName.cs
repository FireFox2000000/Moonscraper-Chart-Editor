using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetEventName : MonoBehaviour {

    [SerializeField]
    EventPropertiesPanelController ePropCon;
    [SerializeField]
    new UnityEngine.UI.Text name;
	
	public void SetEvent()
    {
        ePropCon.UpdateEventName(name.text);
    }
}
