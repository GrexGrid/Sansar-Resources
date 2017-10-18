using System;
using System.Collections.Generic;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class CannonGameExample : SceneObjectScript
{
    // It is not yet possible to create dynamic cluster resources for rezzing.
    // When it is, set this to 'public' so that the projectile can be chosen at edit time.
    private ClusterResource Projectile = null;

    // Public fields are shown in the object editor and can be set in the UI.
    public float InitialSpeed = 20;
    public float Delay = 0.0f;

    public SoundResource Fire_Sound = null;
    public SoundResource Hit_Sound = null;
    public float Loudness = 0.0f;

    public Vector Spawn_Point_1;
    public Vector Spawn_Point_2;
    public Vector Spawn_Point_3;
    public Vector Spawn_Point_4;

    private string ListenEvent = "src_fire_event";  // Magic string for the "fire" button for scripts.
    private Random rng = new Random();
    private List<Vector> SpawnPoints = new List<Vector>();
    private PlaySettings SoundSettings = PlaySettings.PlayOnce;
    private bool Teleport_On_Hit = false;

    public override void Init()
    {
        SoundSettings.Loudness = Loudness;

        if (Spawn_Point_1.IsNotZero()) SpawnPoints.Add(Spawn_Point_1);
        if (Spawn_Point_2.IsNotZero()) SpawnPoints.Add(Spawn_Point_2);
        if (Spawn_Point_3.IsNotZero()) SpawnPoints.Add(Spawn_Point_3);
        if (Spawn_Point_4.IsNotZero()) SpawnPoints.Add(Spawn_Point_4);

        // If any spawn points are entered then TP targets that are hit.
        Teleport_On_Hit = (SpawnPoints.Count > 0);
        ScenePrivate.User.Subscribe(User.AddUser, (string action, SessionId user, string data) => SubscribeToHotkey(user));
    }

    private void SubscribeToHotkey(SessionId userId)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(userId);

        // Lookup the scene object for this agent
        ObjectPrivate agentObject = ScenePrivate.FindObject(agent.AgentInfo.ObjectId);
        if (agentObject != null)
        {
            // Lookup the animation component. There should be just one, so grab the first
            AnimationComponent animationComponent = null;
            if (agentObject.TryGetFirstComponent(out animationComponent))
            {
                // Listen for the correct event to fire the projectile.
                animationComponent.Subscribe(ListenEvent, (string name, ComponentId id) => { OnFire(animationComponent); }, false);
            }
        }
    }

    private bool RezThrottled = false;
    public void OnFire(AnimationComponent animationComponent)
    {
        if (RezThrottled == true)
        {   // TODO: Play a "click" / misfire sound.
            return;
        }

        ObjectPrivate characterObject = ScenePrivate.FindObject(animationComponent.ComponentId.ObjectId);

        if (Fire_Sound != null)
        {
            ScenePrivate.PlaySoundAtPosition(Fire_Sound, characterObject.Position, SoundSettings);
        }

        Sansar.Vector cameraForward = animationComponent.GetVectorAnimationVariable("LLCameraForward");
        cameraForward.W = 0.0f;
        cameraForward = cameraForward.Normalized();

        Sansar.Vector offset = new Sansar.Vector(cameraForward[0], cameraForward[1], 0, 0).Normalized();

        Sansar.Vector new_pos = new Sansar.Vector(0f, 0f, 1.5f, 0f);
        new_pos += characterObject.Position;   // This script is on the Avatar, so the Avatar is the owning object.
        new_pos += (offset * 0.6f);  // Add to the world just in front of the avatar.

        float speed = InitialSpeed;

        if (InitialSpeed < 1.0f || InitialSpeed > 200.0f)
        {
            const float defaultSpeed = 10.0f;
            Log.Write(string.Format("Bad CannonBall Speed: {0}, using default {1}", InitialSpeed, defaultSpeed));
            speed = defaultSpeed;
        }

        Sansar.Vector vel = cameraForward * speed;

        StartCoroutine(RezCannonball, new_pos, Quaternion.FromLook(cameraForward, Vector.Up), vel);

        Timer.Create(TimeSpan.FromSeconds(Delay), () => { animationComponent.Subscribe(ListenEvent, (string name, ComponentId id) => { OnFire(animationComponent); }, false); });
    }

    public void RezCannonball(Sansar.Vector initial_pos, Sansar.Quaternion rotation, Sansar.Vector vel)
    {
        ScenePrivate.CreateClusterData createData = null;
        try
        {
            try
            {
                if (Projectile != null)
                {
                    createData = (ScenePrivate.CreateClusterData)WaitFor(ScenePrivate.CreateCluster, Projectile,
                            initial_pos,
                            rotation,
                            vel);
                }
                else
                {   // No projectile override - just use cannon balls.
                    createData = (ScenePrivate.CreateClusterData)WaitFor(ScenePrivate.CreateCluster, "Export/CannonBall/",
                            initial_pos,
                            rotation,
                            vel);
                }
            }
            catch(ThrottleException e)
            {
                Log.Write($"Too many rezzes! Throttle rate is {e.MaxEvents} per {e.Interval.TotalSeconds}.");
                RezThrottled = true;
                Wait(e.Interval - e.Elapsed); // Wait out the rest of the interval before trying again.
                RezThrottled = false;
                return;
            }
            
            if (createData.Success)
            {
                ObjectPrivate obj = createData.ClusterReference.GetObjectPrivate(0);
                if (obj != null)
                {
                    RigidBodyComponent RigidBody = null;
                    if (obj.TryGetFirstComponent(out RigidBody))
                    {
                        //exit after 3 hits or 1 character hit
                        for (int i = 0; i < 3; i++)
                        {
                            CollisionData data = (CollisionData)WaitFor(RigidBody.Subscribe, CollisionEventType.AllCollisions, ComponentId.Invalid);

                            if ((data.EventType & CollisionEventType.CharacterContact) != 0)
                            {
                                // Use the scene to find the full object API of the other object.
                                AgentPrivate hit = ScenePrivate.FindAgent(data.HitComponentId.ObjectId);
                                if (hit != null)
                                {
                                    ObjectPrivate otherObject = ScenePrivate.FindObject(data.HitComponentId.ObjectId);
                                    if (otherObject != null)
                                    {
                                        if (Hit_Sound != null)
                                        {
                                            ScenePrivate.PlaySoundAtPosition(Hit_Sound, otherObject.Position, SoundSettings);
                                        }

                                        if (Teleport_On_Hit)
                                        {
                                            AnimationComponent anim;
                                            if (otherObject.TryGetFirstComponent(out anim))
                                            {
                                                anim.SetPosition(SpawnPoints[rng.Next(SpawnPoints.Count)]);
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Write(e.ToString());
        }
        finally
        {
            if (createData.ClusterReference != null)
            {
                try
                {
                    createData.ClusterReference.Destroy();
                }
                catch (Exception) { }
            }
        }
    }
}

static public class VectorExtensions
{
    public static bool IsNotZero(this Vector v)
    {
        return v.X != 0.0f || v.Y != 0.0f || v.Z != 0.0f || v.W != 0.0f;
    }
}
