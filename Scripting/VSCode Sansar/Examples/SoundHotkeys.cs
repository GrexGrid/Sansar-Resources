using System;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

// THIS SCRIPT LETS THE EXPERIENCE OWNER CONTROL THE START/STOP OF A SOUND WITH HOTKEYS

public class SoundHotkeys : SceneObjectScript
{
    // PUBLIC MEMBERS ------

    public SoundResource Sound;
    public float Volume_Diff_Percent;
    public bool Looped;

    // For supported keys, see:
    // https://help.sansar.com/hc/en-us/articles/115002150603-Example-script-Teleport-Hotkeys
    public string StartKey;
    public string StopKey;

    // PRIVATE MEMBERS ------

    private PlayHandle playHandle = null;
    private AudioComponent audioComp = null;
    private PlaySettings playSettings;


    public override void Init()
    {
        // set up private members
        playSettings.Looping = Looped;
        playSettings.Loudness = RelativePercentToRelativeLoudnessDB(Volume_Diff_Percent);

        ObjectPrivate.TryGetFirstComponent(out audioComp);

        // Listen for new agents to join the scene
        ScenePrivate.User.Subscribe(User.AddUser, NewUser);
    }

    private float RelativePercentToRelativeLoudnessDB(float loudnessPercent)
    {
        // input  = -100 -> +0  -> +100 -> +150
        // output = -48  -> -24 -> +0   -> +12 
        // FS     = 0    -> 24  -> 48   -> 60

        float clampedPercent = Math.Max(-100.0f, Math.Min(150.0f, loudnessPercent));
        float multiplier = 0.01f * clampedPercent;
        return 24.0f * (multiplier - 1.0f);
    }

    private void StartSound(AnimationData obj)
    {
        StopSound(obj);
        if (playHandle == null)
        {
            if (audioComp == null)
            {
                playHandle = ScenePrivate.PlaySound(Sound, playSettings);
            }
            else
            {
                playHandle = audioComp.PlaySoundOnComponent(Sound, playSettings); 
            }
        }
    }

    private void StopSound(AnimationData obj)
    {
        if (playHandle != null)
        {
            playHandle.Stop();
            playHandle = null;
        }
    }

    private void OnOwnerJoined(SessionId userId)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(userId);

        // Lookup the scene object for this agent
        ObjectPrivate agentObejct = ScenePrivate.FindObject(agent.AgentInfo.ObjectId);
        if (agentObejct == null)
        {
            Log.Write($"Unable to find a ObjectPrivate component for user {userId}");
            return;
        }

        // Lookup the animation component. There should be just one, so grab the first
        AnimationComponent animationComponent = null;
        if (!agentObejct.TryGetFirstComponent(out animationComponent))
        {
            Log.Write($"Unable to find an animation component on user {userId}");
            return;
        }

        animationComponent.Subscribe(StartKey, StartSound);
        animationComponent.Subscribe(StopKey, StopSound);
    }

    void NewUser(UserData obj)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(obj.User);
        if (ScenePrivate.SceneInfo.AvatarUuid == agent.AgentInfo.AvatarUuid)
        {
            // This is the experience owner, set up the key mappings for only this person
            OnOwnerJoined(obj.User);
        }
    }
}