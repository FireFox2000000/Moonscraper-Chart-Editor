using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHelperFunctions : MonoBehaviour {

	public void ClearCurrentSelectedUI()
    {
        StartCoroutine(DelayUIClear());
    }

    IEnumerator DelayUIClear()
    {
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(null);
    }
}
