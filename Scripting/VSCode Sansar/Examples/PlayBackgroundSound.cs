/*
This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. 
Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ 
and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode).
*/

using System;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

// THIS SCRIPT PLAYS A SOUND IN SYNC

public class SynchedSoundBackground : SceneObjectScript
{
    // PUBLIC MEMBERS ------

    public SoundResource Sound;
    [DefaultValue(100f)]
    [Range(-100f, 150f)]
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
        PlaySettings playSettings = Play_Once ? PlaySettings.PlayOnce : PlaySettings.Looped;
        playSettings.Loudness = RelativePercentToRelativeLoudnessDB(Volume_Diff_Percent);
        playSettings.DontSync = Dont_Sync;
        ScenePrivate.PlaySound(Sound, playSettings);
    }
}
