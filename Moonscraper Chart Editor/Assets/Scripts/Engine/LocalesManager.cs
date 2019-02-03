using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public static class LocalesManager
{
    static string m_decimalSeperatorStr = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
    public static char decimalSeperator { get; } = m_decimalSeperatorStr.Length > 0 ? m_decimalSeperatorStr[0] : '.';
}
