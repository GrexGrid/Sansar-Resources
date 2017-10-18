/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * �    Acknowledge that the content is from the Sansar Knowledge Base.
 * �    Include our copyright notice: "� 2017 Linden Research, Inc."
 * �    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * �    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. � 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

/* Use oroutines as an event callback to easily track visitors to a scene.
 * Every visitor triggers an AddUser event which will start a coroutine for that visitor.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Sansar.Script;
using Sansar.Simulation;

// This example shows how to use coroutines and events to track agents entering and leaving the scene
public class VisitorTrackerExample : SceneObjectScript
{
    public string OwnerName;
    public string VisitorListCommand = "/visitorlist";

    public override void Init()
    {
        // Subscribe to Add User events
        // Events can be handled by anonymous methods
        ScenePrivate.User.Subscribe(User.AddUser, SessionId.Invalid, (action, userSessionId, data) => StartCoroutine(TrackUser, userSessionId), true);

        // listen for commands
        ScenePrivate.Chat.Subscribe(0, null, OwnerCommand);
    }

    private class Visitor
    {
        public TimeSpan TotalTime;
        public DateTime VisitStarted;
        public bool Here = false;

        public TimeSpan ThisVisitSoFar
        {
            get { return DateTime.Now - VisitStarted; }
        }
    }

    private Dictionary<string, Visitor> Visitors = new Dictionary<string, Visitor>();

    // There will be one instance of this coroutine per active user in the scene
    public void TrackUser(SessionId userId)
    {
        // Lookup the name of the agent. This is looked up now since the agent cannot be retrieved after they
        // leave the scene.
        string name = ScenePrivate.FindAgent(userId).AgentInfo.Name;
        Visitor visitor;
        if (Visitors.TryGetValue(name, out visitor))
        {
            visitor.VisitStarted = DateTime.Now;
            visitor.Here = true;
        }
        else
        {
            visitor = new Visitor();
            visitor.TotalTime = TimeSpan.Zero;
            visitor.VisitStarted = DateTime.Now;
            visitor.Here = true;
            Visitors[name] = visitor;
        }

        // Block until the agent leaves the scene
        WaitFor(ScenePrivate.User.Subscribe, User.RemoveUser, userId);

        // This should succeed unless the data has been reset.
        // Even then it _should_ succeed as we re-build it with anyone still in the region.
        if (Visitors.TryGetValue(name, out visitor))
        {
            visitor.TotalTime += visitor.ThisVisitSoFar;
            visitor.Here = false;
        }
    }

    private string getVisitorMessage()
    {
        string message = "There have been " + Visitors.Count + " visitors:\n";
        foreach (var visitor in Visitors)
        {
            message += "   " + visitor.Key + " visited for " + (visitor.Value.TotalTime + visitor.Value.ThisVisitSoFar).TotalMinutes + " minutes. [here now]\n";
        }
        return message;
    }

    public void OwnerCommand(int Channel, string Source, SessionId SourceId, ScriptId SourceScriptId, string Message)
    {
        // Checking the message is actually the fastest thing we could do here. Discard anything that isn't the command we are looking for.
        if (Message != VisitorListCommand)
        {
            return;
        }

        AgentPrivate agent = ScenePrivate.FindAgent(SourceId);
        if (agent == null)
        {   // Possible race condition and they already logged off.
            return;
        }

        // If no OwnerName is set, let anyone get the visitor list, otherwise only if the owner name matches.
        if (OwnerName != null && OwnerName != ""
            && agent.AgentInfo.Name != OwnerName)
        {
            return;
        }

        // Dialogs are much easier in a coroutine with WaitFor
        StartCoroutine(ShowStats, agent);
    }

    void ShowStats(AgentPrivate agent)
    {
        var modelDialog = agent.Client.UI.ModalDialog;
        OperationCompleteEvent result = (OperationCompleteEvent)WaitFor(agent.Client.UI.ModalDialog.Show, getVisitorMessage(), "OK", "Reset");
        if (result.Success && modelDialog.Response == "Reset")
        {
            WaitFor(agent.Client.UI.ModalDialog.Show, "Are you sure you want to reset visitor counts?", "Yes!", "Cancel");
            if (modelDialog.Response == "Yes!")
            {
                // Make a new dictionary of everyone still here to replace the current tracking info.
                Dictionary<string, Visitor> stillHere = Visitors;
                Visitors = new Dictionary<string, Visitor>();

                foreach (var visitor in stillHere)
                {
                    Visitor v = new Visitor();
                    v.TotalTime = TimeSpan.Zero;
                    v.VisitStarted = DateTime.Now;
                    v.Here = true;
                    Visitors[visitor.Key] = v;
                }

                WaitFor(modelDialog.Show, "Visitor times reset.", "", "Ok");
            }
        }
    }
}