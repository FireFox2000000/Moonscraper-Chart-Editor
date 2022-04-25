// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Linq;

public static class LocalesManager
{
    static string m_decimalSeperatorStr { get { return Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator; } }
    public static char decimalSeperator { get { return m_decimalSeperatorStr.Length > 0 ? m_decimalSeperatorStr[0] : '.'; } }

    public static char ValidateDecimalInput(string text, int charIndex, char addedChar)
    {
        bool newDecimalSeperator = addedChar == decimalSeperator && !text.Contains(decimalSeperator);
        bool initialCharacter = text.Length <= 0;
        bool negativeInput = (initialCharacter || charIndex <= 0) && addedChar == '-';
        bool numericalInput = addedChar >= '0' && addedChar <= '9';

        if (negativeInput || (newDecimalSeperator && !initialCharacter) || numericalInput)
        {
            return addedChar;
        }

        return '\0';
    }
}
