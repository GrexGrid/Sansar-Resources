/**********************************************************************
Purpose: Door opener
Created: 7/17/2017 by Galen

Drop me in an object to be used as a door. Then configure my public
settings to control how the door behaves. You most likely need to enabe 
OpenOnCollision and set HingeOffsetX to something like -0.5, but the
other settings can be left blank (or zero) and they'll auto-configure.

You are free to hack and use this free script in any way you wish, but 
I would appreciate credit. I offer no warranty or service.

Note that this is quite the hack. I fully expect LL to put out hinges 
and other features that will outmode this door script fairly quickly. 
But I hope this also demonstrates a number of techniques in scripting 
for Sansar.

**********************************************************************/

using System;
using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System.Collections.Generic;

// To get full access to the Object API a script must extend from ObjectScript
public class DoorOpener : SceneObjectScript
{
    /************************ User-configurable settings ************************/

    // A user bumping into this door will not trigger it to open unless this is true
    public bool OpenOnCollision;

    // A comma-delimited list of names of remote sensors to listen to that trigger opening
    public string SensorNames;

    // If the origin is not where the door hinge is, this points to it (e.g., -0.5 on a 1m wide door with center pivot)
    public float HingeOffsetX;

    // If the origin is not where the door hinge is, this points to it
    public float HingeOffsetY;

    // How far to open the door, measured in degrees (default = 90 degrees)
    public int OpenAngleDegrees;

    // If true, the door opens out instead of in (with a left-hinged door, out is toward you)
    public bool OpenOut;

    // How many seconds should it take to open the door (default = 1.0 seconds)
    public float SecondsToOpen;

    // How many seconds the door will remain open after it started opening (default = 5 seconds)
    public int SecondsBeforeClose;

    // What sound to trigger when the door starts opening (optional)
    public SoundResource OpenSound;

    // What sound to trigger when the door starts closing (optional)
    public SoundResource CloseSound;

    // What channel to listen to remote sensors on (default = 1000)
    public int Channel;

    // Which rigid body in this object to manipulate (default = first one)
    public RigidBodyComponent DoorBody;


    /************************ Private variables ************************/

    private List<string> SensorNameList = new List<string>();
    private Vector ClosedPosition;
    private Quaternion ClosedOrientation;
    private bool Opening;
    private DateTime LatestOpenSignal;
    private float OpenAngle;
    private Vector HingeOffset;


    // Override Init to set up event handlers and start coroutines.
    public override void Init()
    {

        // Sensible default setting values
        if (OpenAngleDegrees == 0) OpenAngleDegrees = 90;
        if (Channel == 0) Channel = 1000;
        if (SecondsToOpen == 0) SecondsToOpen = 1;
        if (SecondsBeforeClose == 0) SecondsBeforeClose = 5;

        // Parse the pipe-delimited sensor names into a clean list
        if (SensorNames != null)
        {
            foreach (string Name in SensorNames.Split(','))
            {
                if (Name.Trim() == "") continue;
                SensorNameList.Add(Name.Trim());
            }
        }

        // Find the door we'll manipulate.
        if (DoorBody == null) ObjectPrivate.TryGetFirstComponent(out DoorBody);

        // Compute some basics
        OpenAngle = OpenAngleDegrees / Mathf.DegreesPerRadian;
        ClosedOrientation = DoorBody.GetOrientation();
        HingeOffset = new Vector(HingeOffsetX, HingeOffsetY, 0, 0).Rotate(ref ClosedOrientation);
        ClosedPosition = DoorBody.GetPosition() + HingeOffset;

        // Only subscribe to collision events if the experience creator requested it
        if (OpenOnCollision) DoorBody.Subscribe(CollisionEventType.CharacterContact, CollisionEvent);

        // Only listen for chat if the experience creator wants outside sensors to trip this
        if (SensorNames != null) ScenePrivate.Chat.Subscribe(Channel, null, ChatMessage);

        // Start a continuous loop updating the door's position and orientation
        StartCoroutine(UpdateLoop);
    }

    private void CollisionEvent(CollisionEventType EventType, ComponentId ComponentId, ComponentId HitComponentId, ObjectPublic HitObject, CollisionEventPhase Phase, ControlPointType HitControlPoint)
    {
        if (!Opening)
        {
            Opening = true;
            if (OpenSound != null) ScenePrivate.PlaySound(OpenSound, PlaySettings.PlayOnce);
        }
        LatestOpenSignal = DateTime.Now;
    }


    private void ChatMessage(int Channel, string Source, SessionId SourceId, ScriptId SourceScriptId, string Message)
    {

        // Parse the message
        string[] Parts = Message.Split('|');
        if (Parts.Length < 3) return;

        // Wrong kind of message
        if (Parts[0] != "Sensor") return;

        // Is the message from one of the sensors we're listening to?
        if (!SensorNameList.Contains(Parts[1])) return;

        if (Parts[2] != "Collision") return;

        if (!Opening)
        {
            Opening = true;
            if (OpenSound != null) ScenePrivate.PlaySound(OpenSound, PlaySettings.PlayOnce);
        }
        LatestOpenSignal = DateTime.Now;
    }


    private void UpdateLoop()
    {

        float Tick = 0.01f;
        float AngleStep = 5 * OpenAngle * Tick / SecondsToOpen;

        TimeSpan Moment = TimeSpan.FromSeconds(Tick);

        float Angle = 0.0f;

        while (true)
        {
            Wait(Moment);

            // If we're in the process of opening or closing, we'll adjust the angle a little toward the target
            if (Opening)
            {

                if (OpenOut)
                {
                    Angle -= AngleStep;
                    if (Angle < -OpenAngle) Angle = -OpenAngle;
                }
                else
                {
                    Angle += AngleStep;
                    if (Angle > OpenAngle) Angle = OpenAngle;
                }

                // If it's been a while since we've heard an open signal, let's start closing the door
                if (DateTime.Now.Subtract(LatestOpenSignal).TotalSeconds >= SecondsBeforeClose)
                {
                    Opening = false;
                    if (CloseSound != null) ScenePrivate.PlaySound(CloseSound, PlaySettings.PlayOnce);
                }

            }
            else
            {
                if (OpenOut)
                {
                    Angle += AngleStep;
                    if (Angle > 0f) Angle = 0f;
                }
                else
                {
                    Angle -= AngleStep;
                    if (Angle < 0f) Angle = 0f;
                }
            }

            // Compute the new position and rotation of the door
            Quaternion Rotation = Quaternion.FromEulerAngles(new Vector(0, 0, Angle, 0));
            Quaternion NewOri = ClosedOrientation * Rotation;
            Vector NewPos = ClosedPosition - HingeOffset.Rotate(ref Rotation);

            // Update the door's position and orientation
            DoorBody.SetAngularVelocity(Vector.Zero);
            DoorBody.SetLinearVelocity(Vector.Zero);
            DoorBody.SetPosition(NewPos);
            DoorBody.SetOrientation(QuaternionToVector(NewOri));
        }
    }


    private void MessageAllUsers(string message)
    {
        foreach (var Agent in ScenePrivate.GetAgents())
        {
            try
            {
                Agent.SendChat(message);
            }
            catch (Exception) { }
        }
    }


    private Vector QuaternionToVector(Quaternion Q)
    {
        return new Vector(
        Q.X,
        Q.Y,
        Q.Z,
        Q.W
        );
    }

}