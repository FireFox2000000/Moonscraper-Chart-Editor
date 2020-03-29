// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public class SelectableClick : MonoBehaviour {
    public virtual void OnSelectableMouseDown() { }
    public virtual void OnSelectableMouseUp() { }
    public virtual void OnSelectableMouseOver() { }
    public virtual void OnSelectableMouseDrag() { }
}
