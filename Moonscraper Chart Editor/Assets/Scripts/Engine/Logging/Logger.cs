// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Logger {

    public static string LogException(System.Exception e, string errorContextMessage)
    {
        string fullMessage = string.Format("Exception Logged-\nContext: {0}\nMessage: {1} \nStack Trace: {2}", errorContextMessage, e.Message, e.StackTrace.ToString());
        Debug.LogError(fullMessage);

        return fullMessage;
    }
}
