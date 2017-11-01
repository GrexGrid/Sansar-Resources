/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * �    Acknowledge that the content is from the Sansar Knowledge Base.
 * �    Include our copyright notice: "� 2017 Linden Research, Inc."
 * �    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * �    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. � 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */
 
using Sansar.Simulation;

/// <summary>
/// TriggerAnnounce announces on chat when someone (or some thing) enters the volume the script is attached to.
/// </summary>
public class TriggerAnnounce : SceneObjectScript
{
    public override void Init()
    {
        RigidBodyComponent rigidBody;
        // Collision events are related to the RigidBody component so we must find it.
        // See if this object has a RigidBodyComponent and grab the first one.
        if (ObjectPrivate.TryGetFirstComponent(out rigidBody)
            && rigidBody.IsTriggerVolume())
        {
            // Subscribe to TriggerVolume collisions on our rigid body: our callback (OnCollide) will get called
            // whenever a character or character vr hand or dynamic object collides with our trigger volume
            rigidBody.Subscribe(CollisionEventType.Trigger, OnCollide);
        }
        else
        {
            // This will write to the script console
            Log.Write("Couldn't find rigid body, or rigid body is not a trigger volume!");
        }
    }

    // "Collision" events are really RigidBody events, and they get RigidBodyData.
    private void OnCollide(CollisionData obj)
    {
        
        // Ignore hands:
        if (obj.HitControlPoint != ControlPointType.Invalid) return;

        if (obj.Phase == CollisionEventPhase.TriggerEnter)
        {
            ScenePrivate.Chat.MessageAllUsers($"Object {obj.HitComponentId.ObjectId} has entered my volume!");
        }
        else
        {
            // HitObject might be null if the object or avatar is no longer in the scene, here we are just reporting the object id.
            ScenePrivate.Chat.MessageAllUsers($"Object {obj.HitComponentId.ObjectId} has left my volume!");
        }
    }
}