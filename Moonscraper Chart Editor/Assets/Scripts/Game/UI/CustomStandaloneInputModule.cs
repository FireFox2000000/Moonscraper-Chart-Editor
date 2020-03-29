// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

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
