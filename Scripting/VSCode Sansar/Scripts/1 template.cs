/*
This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. 
Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ 
and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode).
*/

//------Refrerences--------

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using Mono.Simd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//------Class Name--------

public class template : SceneObjectScript
{

//------Fiddely Bits--------

    
    //[Description("Chat Channel to listen on.")] // not used
    [DefaultValue(0)]
    [Range(0, 1000000)]
    [DisplayName("Listen Channel")]
    public int ChatChannel = 0;

//------Data Storage--------

    Random rnd = new Random();// randoms should be global to prevent duplicats in tight loops

//------Functions--------

    void SendChatMessage(string Message)
    {
        StartCoroutine(DelayedMessage, Message);
    }//SendMessage

    void DelayedMessage(string Message)
    {
        Wait(TimeSpan.FromSeconds(0.75));// has to be in a Coroutine
        ScenePrivate.Chat.MessageAllUsers(Message);
    }//DelayedMessage

//------Events--------

    public override void Init()
    {
        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal
        ScenePrivate.Chat.Subscribe(ChatChannel, Chat.User, OnChat); // Subscribe to user chat
        ScenePrivate.User.Subscribe(User.AddUser, NewUser); // Subscribe to new users
        Timer.Create(TimeSpan.FromSeconds(1 / 60), TimeSpan.FromSeconds(1 / 60), TimerTick); // Start a Timer, minimum timer 1/90 (90 fps)
        Log.Write(LogLevel.Info,"Init", GetType().Name + " loaded"); // Let the end user know its working
        
    }//init

    private void UnhandledException(object Sender, Exception Ex) 
    {
        Log.Write(LogLevel.Error, GetType().Name, Ex.Message + "\n" + Ex.StackTrace.Substring(0, 300));
    }//UnhandledException

    void TimerTick()
    {
        //tick tock
    }//TimerTick

    void OnChat(ChatData Data)
    {
        string Message = Data.Message.ToLower(); // lower case message

        AgentPrivate agent=ScenePrivate.FindAgent(Data.SourceId);
        if (agent == null) return; // only listen to agents

        string[] word = Message.Split( new char[] { ' ' } ); // tokenise
        
        if (word.Length < 1) return; // prevent index out of range exceptions

        if ( word[0] == "stuff" )
        {
            SendChatMessage("heard stuff");
        }
    }//onchat

    private void NewUser(UserData User)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(User.User);
        if (agent == null) return;

        ObjectPrivate agentObject = ScenePrivate.FindObject(agent.AgentInfo.ObjectId);

        AnimationComponent UserAnimation = null;
        
        if (agentObject != null)
        {
            // Lookup the animation component. There should be just one, so grab the first
            if (!agentObject.TryGetFirstComponent(out UserAnimation)) return;
        }
    }//NewUser

    //------End Class--------

}//class

/*
Brought to you by OldVamp Creations
https://store.sansar.com/store/OldVamp
OldVamp's uuid: bc2c162d-2753-44f0-af71-d144e2356281
*/