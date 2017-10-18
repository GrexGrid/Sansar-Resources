using System;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

// THIS SCRIPT PLAYS A SOUND IN SYNC

public class SynchedSoundOnObject : SceneObjectScript
{
    // PUBLIC MEMBERS ------

    public SoundResource Sound;
    public float Volume_Diff_Percent;
    public bool Play_Once;
    public bool Dont_Sync;

    private float RelativePercentToRelativeLoudnessDB(float loudnessPercent)
    {
        // input  = -100 -> +0  -> +100 -> +150
        // output = -48  -> -24 -> +0   -> +12 
        // FS     = 0    -> 24  -> 48   -> 60

        float clampedPercent = Math.Max(-100.0f, Math.Min(150.0f, loudnessPercent));
        float multiplier = 0.01f * clampedPercent;
        return 24.0f * (multiplier - 1.0f);
    }

    public override void Init()
    {
        AudioComponent audioComp = null;
        ObjectPrivate.TryGetFirstComponent(out audioComp);
        if (audioComp != null)
        {
            PlaySettings playSettings = Play_Once ? PlaySettings.PlayOnce : PlaySettings.Looped;
            playSettings.Loudness = RelativePercentToRelativeLoudnessDB(Volume_Diff_Percent);
            playSettings.DontSync = Dont_Sync;
            audioComp.PlaySoundOnComponent(Sound, playSettings); 
        }
    }
}