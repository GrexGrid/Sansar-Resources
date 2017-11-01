/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar.Script;
using Sansar.Simulation;

public class CommandExample : SceneObjectScript
{
    #region ScriptParameters
    public readonly string EventCommand;
    public readonly string CoroutineCommand;
    #endregion ScriptParameters

    public override void Init()
    {
    }
#if SIMULATIONSCRIPTAPI_CLIENT_COMMANDS
        // Subscribe to new user events;
        ScenePrivate.User.Subscribe(User.AddUser, NewUser);
    }

    void NewUser(UserData newUser)
    {
        Client client = ScenePrivate.FindAgent(newUser.User).Client;

        // CommandReceived will be called every time the command it triggered on the client
        // CommandCanceled will be called if the subscription fails
        client.SubscribeCommand(EventCommand, CommandAction.Pressed, CommandReceived, CommandCanceled);

        StartCoroutine(ProcessCommands, client);
    }

    void ProcessCommands(Client client)
    {
        try
        {
            WaitFor(client.SubscribeCommand, "SomeInvalidCommand");

            // This is not expected to get hit as waiting on an invalid command should throw a WaitCanceledException
            Log.Write(LogLevel.Error, GetType().Name, "WaitFor on an invalid command did not throw!");
        }
        catch(WaitCanceledException)
        {
            //Ignoring this expected exception
        }
        
        // Wait until the client sends the CoroutineCommand pressed
        WaitFor(client.SubscribeCommand, CoroutineCommand, CommandAction.Pressed);

        // The script will now get the EventCommand when released as well as when it is pressed
        client.SubscribeCommand(EventCommand, CommandAction.Released, CommandReceived, CommandCanceled);

        // Wait until the client sends the CoroutineCommand released
        WaitFor(client.SubscribeCommand, CoroutineCommand, CommandAction.Released);

        // Stop listening to EventCommand Pressed
        client.UnsubscribeFromCommandEvent(EventCommand, CommandAction.Pressed);
        
        // Wait until the client sends the CoroutineCommand pressed or released
        WaitFor(client.SubscribeCommand, CoroutineCommand);

        // Stop listening to EventCommand
        client.UnsubscribeFromCommandEvent(EventCommand);

    }

    void CommandReceived(CommandData command)
    {
        Log.Write(GetType().Name, "Received command " + command.Command +": "+command.Action);
    }

    void CommandCanceled(CancelData data)
    {
        Log.Write(GetType().Name, "Subscription canceled: "+data.Message);
    }
#endif //SIMULATIONSCRIPTAPI_CLIENT_COMMANDS
}