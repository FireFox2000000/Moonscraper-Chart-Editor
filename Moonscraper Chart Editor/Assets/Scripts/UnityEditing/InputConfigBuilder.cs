// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputConfigBuilder : MonoBehaviour
{
    [System.Serializable]
    public class InputProperties
    {
        public ShortcutInputConfig[] shortcutInputs;
    }

    public InputProperties inputProperties;
}
