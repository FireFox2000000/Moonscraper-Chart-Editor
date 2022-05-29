// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugStringFunctions {

    public static bool ValidateString(string str, out System.Exception exceptionResult)
    {
        exceptionResult = null;

        try
        {
            System.Text.StringBuilder saveString = new System.Text.StringBuilder();
            string s_sectionFormat = " = E \"section {0}\"";
            saveString.AppendFormat(s_sectionFormat, str);
        }
        catch (System.Exception e)
        {
            exceptionResult = e;
            return false;
        }

        return true;
    }
}
