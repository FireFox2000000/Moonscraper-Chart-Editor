
public class TempoStream : AudioStream {
    public float frequency
    {
        get { return AudioManager.GetAttribute(this, TempoAudioAttributes.Frequency); }
        set { AudioManager.SetAttribute(this, TempoAudioAttributes.Frequency, value); }
    }

    public float tempo
    {
        get { return AudioManager.GetAttribute(this, TempoAudioAttributes.Tempo); }
        set { AudioManager.SetAttribute(this, TempoAudioAttributes.Tempo, value); }
    }

    public float tempoPitch
    {
        get { return AudioManager.GetAttribute(this, TempoAudioAttributes.TempoPitch); }
        set { AudioManager.SetAttribute(this, TempoAudioAttributes.TempoPitch, value); }
    }

    public TempoStream(int handle) : base(handle)
    {

    }
}
