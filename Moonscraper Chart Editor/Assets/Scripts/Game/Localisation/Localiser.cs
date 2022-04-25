// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

[UnitySingleton(UnitySingletonAttribute.Type.CreateOnNewGameObject, false)]
public class Localiser : UnitySingleton<Localiser>
{
    const string c_inputStartFlag = "|";
    const string c_inputEndFlag = "|";

    public string Localise(string input)
    {
        var currentControls = Globals.gameSettings.controls;
        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < input.Length; index += c_inputStartFlag.Length)
        {
            int startInputindex = input.IndexOf(c_inputStartFlag, index);
            if (startInputindex == -1)
            {
                sb.Append(input, index, input.Length - index);
                break;
            }

            int endInputindex = input.IndexOf(c_inputEndFlag, startInputindex + 1);
            if (endInputindex == -1)
            {
                sb.Append(input, index, input.Length - index);
                break;
            }

            sb.Append(input, index, startInputindex - index);

            int subStringStart = startInputindex + c_inputStartFlag.Length;
            int subStringEnd = endInputindex - subStringStart;

            string inputActionString = input.Substring(subStringStart, subStringEnd);
            int nextIndexOffset = endInputindex + c_inputEndFlag.Length - c_inputStartFlag.Length;

            MSChartEditorInputActions result;

            if (System.Enum.TryParse(inputActionString, out result))
            {
                string inputStrReplacement = string.Empty;

                var actionConfig = currentControls.GetActionConfig(result);

                var map = actionConfig.GetFirstActiveInputMap();
                if (map != null)
                {
                    inputStrReplacement = map.GetInputStr();
                    sb.Append(inputStrReplacement);
                }
            }
            else
            {
                sb.Append(input, index, nextIndexOffset);
            }

            index += nextIndexOffset;
        }

        // Apply tabbing
        for (int i = 10; i >= 0; --i)
        {
            string oldValue = "\\t" + i;
            string newValue = string.Empty;

            for (int j = 0; j < i; ++j)
                newValue += "\t";

            sb.Replace(oldValue, newValue);
        }

        // Apply line breaks
        sb.Replace("\\n", "\n");

        return sb.ToString();
    }

    public static void LocaliseScene()
    {
        GameObject[] gos = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject go in gos)
        {
            if (go && go.transform.parent == null)
            {
                go.gameObject.BroadcastMessage("OnLocalise", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
