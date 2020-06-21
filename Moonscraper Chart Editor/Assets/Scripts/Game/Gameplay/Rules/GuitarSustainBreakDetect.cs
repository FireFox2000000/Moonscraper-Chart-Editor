// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using MoonscraperChartEditor.Song;

public class GuitarSustainBreakDetect {

    public delegate void SustainBreakFactory(float time, Note noteToBreak);

    SustainBreakFactory m_sustainBreakFactory;

    public GuitarSustainBreakDetect(SustainBreakFactory sustainBreakFactory)
    {
        m_sustainBreakFactory = sustainBreakFactory;
    }

    public void Reset()
    {
    }

    public void Update(float time, GuitarSustainHitKnowledge sustainKnowledge, uint noteStreak)
    {
        var currentSustains = sustainKnowledge.currentSustains;

        int inputMask = GuitarInput.GetFretInputMask();
        int extendedSustainsMask = sustainKnowledge.extendedSustainsMask;

        int shiftCount;
        int shiftedExtendedSustainsMask = GameplayInputFunctions.BitshiftToIgnoreLowerUnusedFrets(extendedSustainsMask, out shiftCount);

        foreach (GuitarSustainHitKnowledge.SustainKnowledge sustain in currentSustains.ToArray())     // Take a copy so we can remove as we go
        {
            if (noteStreak == 0)
            {
                BreakSustain(time, sustain);
            }
            else if (extendedSustainsMask != 0)
            {
                int shiftedInputMask = inputMask >> shiftCount;

                if ((shiftedInputMask & ~shiftedExtendedSustainsMask) != 0)
                    BreakSustain(time, sustain);
            }
            else if (!GameplayInputFunctions.ValidateFrets(sustain.note, inputMask, noteStreak))
                BreakSustain(time, sustain);
        }
    }

    void BreakSustain(float time, GuitarSustainHitKnowledge.SustainKnowledge sustain)
    {
        m_sustainBreakFactory(time, sustain.note);
    }
}
