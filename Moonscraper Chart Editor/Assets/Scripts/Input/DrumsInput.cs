using UnityEngine;

namespace DrumsInput
{
    public static class GamepadInputExtension
    {
        public static bool GetHitInput(this GamepadInput gamepad, Note.Drum_Fret_Type drumFret)
        {
            switch (drumFret)
            {
                case (Note.Drum_Fret_Type.RED):
                    return gamepad.GetButton(GamepadInput.Button.B);

                case (Note.Drum_Fret_Type.YELLOW):
                    return gamepad.GetButton(GamepadInput.Button.Y);

                case (Note.Drum_Fret_Type.BLUE):
                    return gamepad.GetButton(GamepadInput.Button.X);

                case (Note.Drum_Fret_Type.ORANGE):
                    return gamepad.GetButton(GamepadInput.Button.LB);

                case (Note.Drum_Fret_Type.GREEN):
                    return gamepad.GetButton(GamepadInput.Button.A);

                case (Note.Drum_Fret_Type.KICK):
                    return gamepad.GetButton(GamepadInput.Button.RB);

                default:
                    Debug.LogError("Unhandled note type for drum input: " + drumFret);
                    break;
            }

            return false;
        }

        /******************************** Keyboard Alts ********************************************/

        public static bool GetHitInputKeyboard(Note.Drum_Fret_Type drumFret)
        {
            switch (drumFret)
            {
                case (Note.Drum_Fret_Type.RED):
                    return Input.GetKey(KeyCode.Alpha1);

                case (Note.Drum_Fret_Type.YELLOW):
                    return Input.GetKey(KeyCode.Alpha2);

                case (Note.Drum_Fret_Type.BLUE):
                    return Input.GetKey(KeyCode.Alpha3);

                case (Note.Drum_Fret_Type.ORANGE):
                    return Input.GetKey(KeyCode.Alpha4);

                case (Note.Drum_Fret_Type.GREEN):
                    return Input.GetKey(KeyCode.Alpha5);

                case (Note.Drum_Fret_Type.KICK):
                    return Input.GetKey(KeyCode.Alpha0);

                default:
                    Debug.LogError("Unhandled note type for drum input: " + drumFret);
                    break;
            }

            return false;
        }

        public static bool GetHitInputControllerOrKeyboard(this GamepadInput gamepad, Note.Drum_Fret_Type drumFret)
        {
            return GetHitInput(gamepad, drumFret) || GetHitInputKeyboard(drumFret);
        }
    }
}
