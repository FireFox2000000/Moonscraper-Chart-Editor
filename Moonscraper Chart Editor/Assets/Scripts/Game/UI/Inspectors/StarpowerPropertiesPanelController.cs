using UnityEngine;
using UnityEngine.UI;
using MoonscraperChartEditor.Song;

public class StarpowerPropertiesPanelController : PropertiesPanelController
{
    public Starpower currentSp { get { return (Starpower)currentSongObject; } set { currentSongObject = value; } }
    bool toggleBlockingActive;

    [SerializeField]
    Text sustainText;
    [SerializeField]
    Toggle drumFillToggle;

    [SerializeField]
    PlaceStarpower starpowerToolController;

    Starpower prevSp = new Starpower(0, 0);

    // Start is called before the first frame update
    void Start()
    {
        ChartEditor.Instance.events.drumsModeOptionChangedEvent.Register(UpdateTogglesInteractable);
    }

    protected override void Update()
    {
        UpdateTogglesInteractable();
        UpdateTogglesDisplay();

        UpdateStringsInfo();
        Controls();

        prevSp = currentSp;
    }

    bool IsInTool()
    {
        return editor.toolManager.currentToolId == EditorObjectToolManager.ToolID.Starpower;
    }

    void Controls()
    {
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ToggleStarpowerDrumsFillActivation) && drumFillToggle.gameObject.activeSelf && drumFillToggle.interactable)
        {
            drumFillToggle.isOn = !drumFillToggle.isOn;
        }
    }

    void UpdateTogglesInteractable()
    {
        // Prevent users from forcing notes when they shouldn't be forcable but retain the previous user-set forced property when using the note tool
        bool drumsMode = Globals.drumMode;
        bool proDrumsMode = drumsMode && Globals.gameSettings.drumsModeOptions == GameSettings.DrumModeOptions.ProDrums;

        drumFillToggle.gameObject.SetActive(drumsMode);
    }

    void UpdateStringsInfo()
    {
        positionText.text = "Position: " + currentSp.tick.ToString();
        sustainText.text = "Length: " + currentSp.length.ToString();
    }

    void UpdateTogglesDisplay()
    {
        toggleBlockingActive = true;

        Starpower.Flags flags = currentSp.flags;
        bool inTool = IsInTool();

        if (!inTool && currentSp == null)
        {
            gameObject.SetActive(false);
            Debug.LogError("No starpower loaded into note inspector");
        }

        drumFillToggle.isOn = flags.HasFlag(Starpower.Flags.ProDrums_Activation);

        toggleBlockingActive = false;
    }

    public void SetDrumFill()
    {
        if (toggleBlockingActive)
            return;

        if (IsInTool())
        {
            if (drumFillToggle.interactable)
            {
                SetToolFlag(drumFillToggle, Starpower.Flags.ProDrums_Activation);
            }
        }
        else
        {
            if (currentSp == prevSp)
            {
                var newFlags = currentSp.flags;

                if (drumFillToggle.isOn)
                    newFlags |= Starpower.Flags.ProDrums_Activation;
                else
                    newFlags &= ~Starpower.Flags.ProDrums_Activation;

                SetNewFlags(currentSp, newFlags);
            }
        }
    }

    void SetNewFlags(Starpower sp, Starpower.Flags newFlags)
    {
        if (sp.flags == newFlags)
            return;

        if (editor.toolManager.currentToolId == EditorObjectToolManager.ToolID.Cursor)
        {
            Starpower newSp = new Starpower(sp.tick, sp.length, newFlags);
            SongEditModify<Starpower> command = new SongEditModify<Starpower>(sp, newSp);
            editor.commandStack.Push(command);
        }
        else
        {
            // Updating sp tool parameters and visuals
            starpowerToolController.starpower.flags = newFlags;
            starpowerToolController.controller.SetDirty();
        }
    }

    void SetToolFlag(Toggle uiToggle, Starpower.Flags flagsToToggle)
    {
        ref Starpower.Flags flags = ref starpowerToolController.starpower.flags;

        if ((flags & flagsToToggle) == 0)
            flags |= flagsToToggle;
        else
            flags &= ~flagsToToggle;

        starpowerToolController.controller.SetDirty();
    }
}
