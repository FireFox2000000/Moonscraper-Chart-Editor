using UnityEngine;
using MoonscraperChartEditor.Song;

[RequireComponent(typeof(Renderer))]
public class StarpowerVisualsManager : MonoBehaviour
{
    [SerializeField]
    StarpowerController spCon;
    Renderer spRenderer;

    [SerializeField]
    MeshNoteResources resources;
    Material[] resourceSharedMatsSp;
    Material[] resourceSharedMatsSpDrumFill;

    // Use this for initialization
    void Awake()
    {
        if (spRenderer)
            return;

        spRenderer = GetComponent<Renderer>();
        resourceSharedMatsSp = resources.starpowerMaterials;
        resourceSharedMatsSpDrumFill = resources.starpowerDrumFillMaterials;
    }

    public void UpdateVisuals()
    {
        if (!spRenderer)
            Awake();

        Starpower sp = spCon.starpower;
        if (sp != null)
        {
            Starpower.Flags flags = sp.flags;
            if (flags.HasFlag(Starpower.Flags.ProDrums_Activation))
            {
                spRenderer.sharedMaterials = resourceSharedMatsSpDrumFill;
            }
            else
            {
                // Default visuals
                spRenderer.sharedMaterials = resourceSharedMatsSp;
            }
        }
    }
}
