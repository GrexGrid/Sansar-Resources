/* 
 * This work uses content from the Sansar Knowledge Base. � 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode).
 * This work uses content from LGG. � 2017 Greg Hendrickson. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode).
 */
using System;
using Sansar;
using Sansar.Simulation;

// To get full access to the Object API a script must extend from ObjectScript
public class SpotlightRotation : SceneObjectScript
{
    Quaternion startRot;
    Sansar.Vector lightPositionOffset;
    RigidBodyComponent RigidBody;
    Quaternion lightOnObjectDirection;

    public override void Init()
    {
        if (RigidBody == null)
        {
            if (!ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                return;
            }
        }
        lightPositionOffset = new Sansar.Vector(0, 0, 2);
        startRot = RigidBody.GetOrientation();
        //we want this to float
        RigidBody.SetMass(0);
        //the default light when you add it is off by -90 degrees
        lightOnObjectDirection = Quaternion.FromEulerAngles(new Sansar.Vector(0, 0, (float)(Math.PI / -2.0), 0));
        StartCoroutine(UpdateLoop);
    }

    private void UpdateLoop()
    {
        while (true)
        {
            //this is only one object, so we have it hard coded in to only follow the first agent., you could split this up later
            Boolean got1 = false;
            foreach (AgentPrivate agent in ScenePrivate.GetAgents())
            {
                if (got1 == false)
                {
                    ObjectPrivate agentObejct = ScenePrivate.FindObject(agent.AgentInfo.ObjectId);
                    AnimationComponent anim;
                    if (agentObejct.TryGetFirstComponent(out anim))
                    {
                        Sansar.Vector fwd = anim.GetVectorAnimationVariable("LLCameraForward");
                        //Builds a rotation from the fwd vector
                        Quaternion newRot = Quaternion.FromLook(fwd, Sansar.Vector.Up);
                        //This is a basic check to make sure the camera rotation isnt all 0s
                        if (fwd.LengthSquared() > 0)
                        {
                            try
                            {
                                RigidBody.SetAngularVelocity(Vector.Zero);
                                RigidBody.SetLinearVelocity(Vector.Zero);
                                RigidBody.SetPosition(agentObejct.Position + lightPositionOffset);
                                //Order of multiplication matters here I think, start with the base rotation,
                                //multiply by and base offset rotation for the light, then multiply by the rotation of the fwd
                                //Keep in mind that multiplying quad A by quad b will rotate quad A by quad b
                                RigidBody.SetOrientation(QuaternionToVector(startRot * lightOnObjectDirection * newRot).Normalized());
                            }
                            catch
                            {

                            }
                        }
                    }
                    got1 = true;
                }
            }
            Wait(TimeSpan.FromSeconds(.05));
        }
    }
    private Vector QuaternionToVector(Quaternion Q) { return new Vector(Q.X, Q.Y, Q.Z, Q.W); }
}