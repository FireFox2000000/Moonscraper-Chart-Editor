// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;

namespace MoonscraperEngine.Input
{
    public class InputRebinder
    {
        IEnumerable<InputAction> allActions;
        IInputMap mapCopy;
        IInputMap mapToRebind;
        InputAction actionToRebind;
        IInputDevice device;

        public InputRebinder(InputAction actionToRebind, IInputMap mapToRebind, IEnumerable<InputAction> allActions, IInputDevice device)
        {
            mapCopy = mapToRebind.Clone();
            mapToRebind.SetEmpty();

            this.actionToRebind = actionToRebind;
            this.mapToRebind = mapToRebind;
            this.allActions = allActions;
            this.device = device;
        }

        public bool TryMap(out InputAction conflict, out IInputMap attemptedInput)
        {
            conflict = null;

            IInputMap currentInput = device.GetCurrentInput(actionToRebind.properties);
            attemptedInput = currentInput;

            if (currentInput != null)
            {
                foreach (InputAction inputAction in allActions)
                {
                    if (MSChartEditorInput.Category.interactionMatrix.TestInteractable(inputAction.properties.category, actionToRebind.properties.category) && inputAction.HasConflict(currentInput))
                    {
                        conflict = inputAction;
                        return false;
                    }
                }

                // Do rebind and exit
                mapToRebind.SetFrom(currentInput);
                return true;
            }

            return false;
        }

        public void RevertMapBeingRebound()
        {
            mapToRebind.SetFrom(mapCopy);
        }

        public static void SetToDefault<TEnum>(
            InputActionContainer<TEnum> actions, 
            InputActionContainer<TEnum> defaultActions, 
            int categoryMask,
            IInputDevice device
            ) where TEnum : System.Enum
        {
            foreach (TEnum actionEnum in EnumX<TEnum>.Values)
            {
                InputAction action = actions.GetActionConfig(actionEnum);

                if (((1 << actions.GetActionConfig(actionEnum).properties.category) & categoryMask) == 0)
                {
                    continue;
                }

                InputAction defaultAction = defaultActions.GetActionConfig(actionEnum);
                action.RemoveMapsForDevice(device);

                var defaultMaps = defaultAction.GetMapsForDevice(device);
                foreach (var map in defaultMaps)
                {
                    action.Add(map.Clone());
                }
            }
        }
    }
}
