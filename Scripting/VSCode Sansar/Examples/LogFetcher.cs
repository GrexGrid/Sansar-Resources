using Sansar.Script;
using Sansar.Simulation;
using System.Collections.Generic;

public class find_logs : SceneObjectScript
{
    private void LogFetcher()
    {// Run in a coroutine, code from kelly
        while (true)
        {
            // Wait for the first time the scene owner logs in
            UserData data = (UserData)WaitFor(ScenePrivate.User.Subscribe, User.AddUser, Sansar.Script.SessionId.Invalid);
            AgentPrivate agent = ScenePrivate.FindAgent(data.User);
            if (agent != null && agent.AgentInfo.AvatarUuid == ScenePrivate.SceneInfo.AvatarUuid)
            {
                break;
            }
        }//while

        // Wait a couple extra seconds
        Wait(System.TimeSpan.FromSeconds(5));
        var messages = new Queue<Log.Message>(Log.Messages);

        // Re-spam all the logs
        Log.Write("Log replay :");
        foreach (var log in messages)
        {
            Log.Write(log.LogLevel, "REPLAY", $"{log.Tag} : {log.Text}");
        }
    }//logfetcher

    public override void Init()
    {
        StartCoroutine(LogFetcher);//fetch old logs
    }//init

}//class