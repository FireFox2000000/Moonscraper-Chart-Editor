using MoonscraperChartEditor.Song;

public class GuitarWhammyDetect
{
    public void Update(float time, GuitarSustainHitKnowledge sustainKnowledge)
    {
        var currentSustains = sustainKnowledge.currentSustains;

        float whammyValue = GuitarInput.GetWhammyInput();

        foreach (var sustain in currentSustains)
        {
            foreach (Note note in sustain.note.chord)
            {
                NoteController nCon = note.controller;
                if (nCon != null)
                {
                    nCon.SetDesiredWhammy(whammyValue);
                }
            }
        }
    }
}
