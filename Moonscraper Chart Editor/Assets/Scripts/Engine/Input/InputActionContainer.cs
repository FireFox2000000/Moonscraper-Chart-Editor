// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoonscraperEngine.Input
{
    public interface IInputActionContainer : IEnumerable<InputAction>
    {
    }

    /// <summary>
    /// Stores a list of input actions that can be accessed via a generic input enum for looking up actions with array indexing speed, rather then via string parsing and storing it in a dictionary
    /// </summary>
    public class InputActionContainer<TEnum> : IInputActionContainer where TEnum : System.Enum
    {
        [SerializeField]
        List<InputAction.SaveData> saveData = new List<InputAction.SaveData>();    // Safer save data format, to handle cases where the Shortcut enum list may get updated or values are shifted around
        protected EnumLookupTable<TEnum, InputAction> actionConfigCleanLookup;

        public InputActionContainer(EnumLookupTable<TEnum, InputAction> actionConfig)
        {
            actionConfigCleanLookup = actionConfig;
        }

        public void Insert(TEnum key, InputAction entry)
        {
            actionConfigCleanLookup[key] = entry;
        }

        public InputAction GetActionConfig(TEnum key)
        {
            return actionConfigCleanLookup[key];
        }

        public void LoadFromSaveData(InputActionContainer<TEnum> that)
        {
            saveData = that.saveData;
            foreach (var data in saveData)
            {
                TEnum enumVal;
                if (EnumX<TEnum>.GenericTryParse(data.action, out enumVal))
                {
                    actionConfigCleanLookup[enumVal].LoadFrom(data);
                    // Add more maps as needed
                }
                else
                {
                    Debug.LogError("Unable to parse " + data.action + " as an input action");
                }
            }
        }

        public void UpdateSaveData(bool saveRebindable = false)
        {
            saveData.Clear();
            for (int i = 0; i < actionConfigCleanLookup.Count; ++i)
            {
                TEnum enumVal = EnumX<TEnum>.FromInt(i);

                if (!actionConfigCleanLookup[enumVal].properties.rebindable && !saveRebindable)
                    continue;

                var newItem = new InputAction.SaveData();
                actionConfigCleanLookup[enumVal].SaveTo(enumVal.ToString(), newItem);

                saveData.Add(newItem);
            }
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            foreach (var val in actionConfigCleanLookup)
            {
                yield return val;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
