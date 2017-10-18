/* 
This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. 
Licensed under the Creative Commons Attribution 4.0 International License
license summary https://creativecommons.org/licenses/by/4.0/ 
complete license terms https://creativecommons.org/licenses/by/4.0/legalcode
*/

using System;
using System.Linq;
using System.Collections.Generic;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class SoundRandomizer : SceneObjectScript
{
    // PUBLIC MEMBERS ------

    // will play sounds globally if an audio component is not provided
    public bool PlayOnAudioComponent;

    public double MinDelayBetweenSounds;
    public double MaxDelayBetweenSounds;

    // FYI: scripts can only take up to 10 parameters, and doesn't support arrays
    // That's why we have these hardcoded 5 slots for sound resources
    public SoundResource Sound1;
    public SoundResource Sound2;
    public SoundResource Sound3;
    public SoundResource Sound4;
    public SoundResource Sound5;


    // PRIVATE MEMBERS ------

    private AudioComponent LocalAudioComponent = null;
    private List<SoundResource> AvailableSounds = new List<SoundResource>();
    private int NextSoundIndex = 0;
    private double DelayRange;
    private Random Rnd = new Random();

    private void ShuffleSounds(SoundResource previousSound = null)
    {
        // NOTE: not efficient for scaling but good enough for <= 5 elements
        AvailableSounds = AvailableSounds.OrderBy<SoundResource, int>((item) => Rnd.Next()).ToList( );

        // make sure we won't play the same sound twice in a row.
        if (previousSound == AvailableSounds[0])
        {
            // swap the next and last sound.
            int finalSoundIndex = AvailableSounds.Count - 1;
            AvailableSounds[0] = AvailableSounds[finalSoundIndex];
            AvailableSounds[finalSoundIndex] = previousSound;
        }
    }

    public void OnSoundFinished()
    {
        if (AvailableSounds.Count > 1)
        {
            SoundResource previousSound = AvailableSounds[NextSoundIndex];
            NextSoundIndex++;
            if (NextSoundIndex >= AvailableSounds.Count)
            {
                ShuffleSounds(previousSound);
                NextSoundIndex = 0;
            }
        }

        double randomDelay = MinDelayBetweenSounds + (DelayRange * Rnd.NextDouble());
        Timer.Create(TimeSpan.FromSeconds(randomDelay), () => { PlayNextSound(); });
    }

    private void PlayNextSound()
    {
        SoundResource nextSound = AvailableSounds[NextSoundIndex];
        if (LocalAudioComponent != null)
        {
            LocalAudioComponent.PlaySoundOnComponent(nextSound, PlaySettings.PlayOnce).OnFinished(OnSoundFinished);
        }
        else
        {
            ScenePrivate.PlaySound(nextSound, PlaySettings.PlayOnce).OnFinished(OnSoundFinished);
        }
    }

    private void AddSoundToListIfExists(SoundResource sound)
    {
        if (sound != null)
        {
            AvailableSounds.Add(sound);
        }
    }

    public override void Init()
    {
        if (PlayOnAudioComponent)
        {
            ObjectPrivate.TryGetFirstComponent(out LocalAudioComponent);
        }

        DelayRange = Math.Max(0.0, MaxDelayBetweenSounds - MinDelayBetweenSounds);

        AddSoundToListIfExists(Sound1);
        AddSoundToListIfExists(Sound2);
        AddSoundToListIfExists(Sound3);
        AddSoundToListIfExists(Sound4);
        AddSoundToListIfExists(Sound5);

        if (AvailableSounds.Count > 0)
        {
            ShuffleSounds();
            PlayNextSound();
        }
    }
}
