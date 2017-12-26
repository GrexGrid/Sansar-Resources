/*
This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. 
Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ 
and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode).
*/


/* 
Command	Default         Key Binding

PrimaryAction	        F
SecondaryAction	        R
Modifier	            Shift
Action1 to Action0	    Number keys 1 to 0
Confirm	                Enter
Cancel	                Escape
SelectLeft	            Left arrow
SelectRight	            Right arrow
SelectUp	            Up arrow
SelectDown	            Down arrow
Keypad0 to Keypad9	    Numberpad keys 0 to 9
KeypadEnter	            Numberpad Enter
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
        // Subscribe to new user events;
        ScenePrivate.User.Subscribe(User.AddUser, NewUser);
    }//init

    void NewUser(UserData newUser)
    {
        Client client = ScenePrivate.FindAgent(newUser.User).Client;

        // CommandReceived will be called every time the command it triggered on the client
        // CommandCanceled will be called if the subscription fails
        client.SubscribeToCommand(EventCommand, CommandAction.Pressed, CommandReceived, CommandCanceled);

        StartCoroutine(ProcessCommands, client);
    }//NewUser

    void ProcessCommands(Client client)
    {
        try
        {
            WaitFor(client.SubscribeToCommand, "SomeInvalidCommand");

            // This is not expected to get hit as waiting on an invalid command should throw a WaitCanceledException
            Log.Write(LogLevel.Error, GetType().Name, "WaitFor on an invalid command did not throw!");
        }
        catch(WaitCanceledException)
        {
            //Ignoring this expected exception
        }
        
        // Wait until the client sends the CoroutineCommand pressed
        WaitFor(client.SubscribeToCommand, CoroutineCommand, CommandAction.Pressed);

        // The script will now get the EventCommand when released as well as when it is pressed
        client.SubscribeToCommand(EventCommand, CommandAction.Released, CommandReceived, CommandCanceled);

        // Wait until the client sends the CoroutineCommand released
        WaitFor(client.SubscribeToCommand, CoroutineCommand, CommandAction.Released);

        // Stop listening to EventCommand Pressed
        client.UnsubscribeFromCommandEvent(EventCommand, CommandAction.Pressed);
        
        // Wait until the client sends the CoroutineCommand pressed or released
        WaitFor(client.SubscribeToCommand, CoroutineCommand);

        // Stop listening to EventCommand
        client.UnsubscribeFromCommandEvent(EventCommand);

    }//ProcessCommands

    void CommandReceived(CommandData command)
    {
        Log.Write(GetType().Name, "Received command " + command.Command +": "+command.Action);
    }//CommandReceived

    void CommandCanceled(CancelData data)
    {
        Log.Write(GetType().Name, "Subscription canceled: "+data.Message);
    }//CommandCanceled

}//class