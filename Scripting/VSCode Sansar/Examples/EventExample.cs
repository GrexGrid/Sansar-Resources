/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

/* Track visitors to the scene with the AddUser and RemoveUser events.
 */
using System;
using Sansar.Script;
using Sansar.Simulation;
using System.Collections.Generic;

public class EventExample : SceneObjectScript
{
    // Dictionary to keep track of join times with names
    Dictionary<SessionId,Tuple<string,DateTime>> userMap = new Dictionary<SessionId, Tuple<string,DateTime>>();

    public override void Init()
    {
        // Subscribe to Add User events
        ScenePrivate.User.Subscribe(User.AddUser, AddUser);

        // Subscribe to Remove User Events
        ScenePrivate.User.Subscribe(User.RemoveUser, RemoveUser);
    }

    // This event will occur once each time a new agent joins the scene
    void AddUser(UserData data)
    {        
        // Track joined time
        DateTime joined = DateTime.Now;

        // Lookup the name of the agent. This is looked up now since the agent cannot be retrieved after they
        // leave the scene.
        string name = ScenePrivate.FindAgent(data.User).AgentInfo.Name;

        // Store the information to 
        userMap[data.User] = Tuple.Create(name,joined);
    }

    // This event will occur once each time an agent leaves the scene
    void RemoveUser(UserData data)
    {
        // Retrieve the stored name and join time
        Tuple<string,DateTime> info = userMap[data.User];
        string name = info.Item1;
        DateTime joined = info.Item2;

        // Calculate elapsed time and report.
        TimeSpan elapsed = DateTime.Now-joined;
        ScenePrivate.Chat.MessageAllUsers(string.Format("{0} was present for {1} seconds", name, elapsed.TotalSeconds));

        // Remove tracking info for the agent who left
        userMap.Remove(data.User);
    }
}
