using MSE;
using MSE.Input;

public enum GameplayAction
{
    // Guitar Actions
    GuitarStrumUp,
    GuitarStrumDown,

    GuitarFretGreen,
    GuitarFretRed,
    GuitarFretYellow,
    GuitarFretBlue,
    GuitarFretOrange,

    // Drum Actions
    DrumPadRed,
    DrumPadYellow,
    DrumPadBlue,
    DrumPadOrange,
    DrumPadGreen,
    DrumPadKick,
}

public class GameplayInput
{
    public static class Category
    {
        // static int to make int conversion way easier. The lack of implicit enum->int conversion is annoying as hell.
        public enum CategoryType
        {
            Guitar,
            Drums,
        }

        public static InteractionMatrix interactionMatrix = new InteractionMatrix(3);

        static Category()
        {
            interactionMatrix.SetInteractable((int)CategoryType.Guitar, (int)CategoryType.Guitar);

            interactionMatrix.SetInteractable((int)CategoryType.Drums, (int)CategoryType.Drums);
        }
    }

    const bool kRebindableDefault = true;
    const bool kHiddenInListsDefault = false;
    const int kCategoryDefault = (int)Category.CategoryType.Guitar;

    static readonly InputAction.Properties kDefaultProperties = new InputAction.Properties { rebindable = kRebindableDefault, hiddenInLists = kHiddenInListsDefault, category = kCategoryDefault };

    public class GameplayActionContainer : InputActionContainer<GameplayAction>
    {
        public GameplayActionContainer() : base(new EnumLookupTable<GameplayAction, InputAction>())
        {
            InputManager inputManager = InputManager.Instance;

            for (int i = 0; i < actionConfigCleanLookup.Count; ++i)
            {
                GameplayAction scEnum = (GameplayAction)i;
                InputAction.Properties properties = kDefaultProperties;
                //if (!inputManager.inputPropertiesConfig.TryGetPropertiesConfig(scEnum, out properties))
                //{
                //    properties = kDefaultProperties;
                //}

                if (string.IsNullOrEmpty(properties.displayName))
                {
                    properties.displayName = scEnum.ToString();
                }

                actionConfigCleanLookup[scEnum] = new InputAction(properties);
            }
        }
    }

    static GameplayActionContainer primaryInputs { get { return GameSettings.gameplayControls; } }

    public static bool GetInputDown(GameplayAction key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetInputDown(InputManager.Instance.devices);
        }

        return false;
    }

    public static bool GetInputUp(GameplayAction key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetInputUp(InputManager.Instance.devices);
        }

        return false;
    }

    public static bool GetInput(GameplayAction key)
    {
        if (ChartEditor.hasFocus && !Services.IsTyping)
        {
            return primaryInputs.GetActionConfig(key).GetInput(InputManager.Instance.devices);
        }

        return false;
    }

    public static bool GetGroupInputDown(GameplayAction[] inputs)
    {
        foreach (GameplayAction key in inputs)
        {
            if (GetInputDown(key))
                return true;
        }

        return false;
    }

    public static bool GetGroupInputUp(GameplayAction[] inputs)
    {
        foreach (GameplayAction key in inputs)
        {
            if (GetInputUp(key))
                return true;
        }

        return false;
    }

    public static bool GetGroupInput(GameplayAction[] inputs)
    {
        foreach (GameplayAction key in inputs)
        {
            if (GetInput(key))
                return true;
        }

        return false;
    }
}
