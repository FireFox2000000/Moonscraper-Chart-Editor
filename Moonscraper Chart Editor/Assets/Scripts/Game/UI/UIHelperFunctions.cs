// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

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
