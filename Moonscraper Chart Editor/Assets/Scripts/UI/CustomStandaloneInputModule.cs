using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CustomStandaloneInputModule : StandaloneInputModule
{
    public PointerEventData GetPointerData()
    {
        return m_PointerData[kMouseLeftId];
    }
}
