/* 
 * This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode).
 * This work uses content from LGG. © 2017 Greg Hendrickson. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode).
 */

using Sansar.Script;
using Sansar.Simulation;
using System;

namespace gun
{
    
    public class StaticGun : SceneObjectScript
    {
        private float spacing = 1.0f;
        private int numberOfRez = 20;
        private Sansar.Vector offset;

        public SoundResource Sound_To_Play;
        public ClusterResource Bullet_Object;

        /// <summary>
        /// Runs when the script starts, initialize vars, start coroutines, etc
        /// </summary>
        public override void Init()
        {
            offset =new  Sansar.Vector(0, 0, 1.2f);
            
            Script.UnhandledException += UnhandledException;
            ScenePrivate.User.Subscribe(User.AddUser, NewUser);
        }
        
        #region grabbing the hot keys
        /// <summary>
        /// When a new user enters, grab their keys 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="userSessionId"></param>
        /// <param name="data"></param>
        void NewUser(string action, SessionId userSessionId, string data)
        {
            // Set up the hot key listener.
            SubscribeToHotkeys(userSessionId);
        }
        /// <summary>
        /// Ties the avatars keys to the functions to enter or move our vehicle
        /// </summary>
        /// <param name="userId"></param>
        private void SubscribeToHotkeys(SessionId userId)
        {
            AgentPrivate agent = ScenePrivate.FindAgent(userId);
            // Lookup the scene object for this agent
            ObjectPrivate agentObejct = ScenePrivate.FindObject(agent.AgentInfo.ObjectId);
            
            if (agentObejct == null) { Log.Write($"Unable to find a SceneObject component for user {userId}"); return; }
            // Lookup the animation component. There should be just one, so grab the first, we need this to tie to the keys
            AnimationComponent animationComponent = null;
            if (!agentObejct.TryGetFirstComponent(out animationComponent)) { Log.Write($"Unable to find an animation component on user {userId}"); return; }
            
            //Listen to the f key
            animationComponent.Subscribe("Key_F", (BehaviorName, ComponentId) => fire(1, ComponentId), true);
        }
        #endregion

        #region mostly small helper methods


       
        /// <summary>
        /// We use this mostly to catch error so the whole script does not crash
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnhandledException(object sender, Exception e)
        {
            // Depending on the script scheduling policy, the exception may or may not be recoverable.
            // An unrecoverable exception that can be handled will only be given a short time to run
            // so the handler method needs to be kept small.
            // Any exception thrown from this method will terminate the script regardless of the value
            // of UnhandledExceptionRecoverable
            if (!Script.UnhandledExceptionRecoverable)
            {
                Log.Write("Unrecoverable exception, this is useless.");
                Log.Write(e.ToString());
            }
            else
            {
                Log.Write("Exception!");
                Log.Write(e.ToString());

                // While we have recovered, whatever coroutine had the exception will have stopped.
                // The script will still run so logs can be recovered.
            }
        }

        

        #endregion        

        #region responseTOKeypresses
        
        private void fire(int nothing, ComponentId ComponentId)
        {
            ObjectPrivate avObject = ScenePrivate.FindObject(ComponentId.ObjectId);
            StartCoroutine(fireRez, avObject.Position,avObject.ForwardVector, Sansar.Quaternion.FromLook(avObject.ForwardVector, Sansar.Vector.Up));
        }
        public void fireRez(Sansar.Vector position, Sansar.Vector fwd, Sansar.Quaternion orientation)
        {
            Sansar.Vector rezPosition;
            if(Sound_To_Play!=null) ScenePrivate.PlaySoundAtPosition(Sound_To_Play, position, PlaySettings.PlayOnce);
            for (int i = 0; i < numberOfRez; i++)
            {
                //so i starts at 0, and will eventually be spacing away, if we add .5 that means .5 of fwd which means if the object is centered
                //it will come right out of our av, .6 is 10% more so we dont hit ourself
                rezPosition = position  + offset+
                    (
                    fwd * (spacing * (i + .6f))
                    );

                ScenePrivate.CreateCluster(
                  Bullet_Object,
                  rezPosition,
                  orientation,
                  Sansar.Vector.Zero,
                  CreateClusterHandler
                );
                Wait(TimeSpan.FromSeconds(.1));
            }
        }

        #endregion


        private void CreateClusterHandler(
          bool Success, string Message, Cluster ClusterReference
        )
        {
            try
            {
                StartCoroutine(DestroyCluster, ClusterReference);
            }
            catch (Exception)
            {
            }
        }

        private void DestroyCluster(Cluster ClusterReference)
        {
            Wait(TimeSpan.FromSeconds(.7));
            ClusterReference.Destroy(); // Delete the cluster from the scene
        }
    }
}