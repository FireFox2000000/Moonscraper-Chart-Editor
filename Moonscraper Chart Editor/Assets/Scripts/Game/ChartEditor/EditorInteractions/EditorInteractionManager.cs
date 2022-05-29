using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EditorInteractionManager : System.Object
{
    public enum InteractionType
    {
        HighwayObjectEdit,
        BpmCalculator,
        LyricsEditor,
    }

    [System.Serializable]
    public class InteractionConfig : System.Object
    {
        public InteractionType interactionType;
        public GameObject interactionInterface;
    }

    [SerializeField]
    InteractionConfig[] editorInteractions;
    InteractionConfig currentInteraction = null;

    public void Init()
    {
        ChangeInteraction(InteractionType.HighwayObjectEdit);
    }

    public void ChangeInteraction(InteractionType interactionId)
    {
        var config = GetConfigForId(interactionId);

        Debug.Assert(config != null);

        if (currentInteraction != null)
        {
            currentInteraction.interactionInterface.SetActive(false);
        }

        currentInteraction = config;

        if (currentInteraction != null)
        {
            currentInteraction.interactionInterface.SetActive(true);
        }

        ChartEditor.Instance.events.editorInteractionTypeChangedEvent.Fire(interactionId);
    }

    InteractionConfig GetConfigForId(InteractionType id)
    {
        foreach (var config in editorInteractions)
        {
            if (config.interactionType == id)
            {
                return config;
            }
        }

        return null;
    }
}
