/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */
using Sansar.Simulation;
using System.ComponentModel;

// To get full access to the Agent API a script must extend from AgentScript
public class AgentScriptExample : AgentScript
{
    // Public fields of supported types are reflected in the editor. 
    [DefaultValue("The script attached to this agent has been initialized.")]
    public string WelcomeMessage = null;

    public AgentScriptExample()
    {
        // WelcomeMessage will be null in the constructor, as will Agent.
        // Any value assigned to WelcomeMessage will be overwritten before Init is called.
    }

    // Init will be called by the script loader after the constructor and after any public fields have been initialized.
    public override void Init()
    {
        // Agent is now valid and can be used to display a message on the client.
        // Even though there is no code shown which assigns to WelcomeMessage, it will have a value assigned by the editor.
        AgentPrivate.Client.UI.ModalDialog.Show(WelcomeMessage, "Cancel", "Okay", OnDialogResponse);
    }

    // This method will be called when the client presses a button on the dialog.
    void OnDialogResponse(bool Success, string Message)
    {
        AgentPrivate.SendChat("The client pressed " + Message);
    }
}