using UnityEngine;
using TNet;
using System.IO;
using System.Collections;
using UnityTools = TNet.UnityTools;
using System.Net;

public class PoolTNManager : MonoBehaviour
{
    static public PoolTNManager instance;
    public string serverIPAddress = "52.58.44.209";
    public int serverPort = 5127;

    public int maxPlayerCountPerChannel = 2;

    public string connectLevel;
    public bool persistent = false;
    public string disconnectLevel;

    public UIInput userName;
    public UILabel hintLabel;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        if (Application.isPlaying)
        {
            // Start resolving IPs
            Tools.ResolveIPs(null);

            // We don't want mobile devices to dim their screen and go to sleep while the app is running
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            TNManager.SetPacketHandler((byte)Packet.ResponseChannelList, OnChannelList);
        }
    }

    public void FindGame()
    {
        // set player name
        if(userName.value != null)
            TNManager.playerName = userName.value;

        // We want to connect to the specified destination when the button is clicked on.
        // "OnNetworkConnect" function will be called sometime later with the result.
        TNManager.Connect(serverIPAddress, serverPort);
    }

    /// <summary>
    /// This function is called when a connection is either established or it fails to connect.
    /// Connecting to a server doesn't mean that the connected players are now immediately able
    /// to see each other, as they have not yet joined a channel. Only players that have joined
    /// some channel are able to see and interact with other players in the same channel.
    /// You can call TNManager.JoinChannel here if you like, but in this example we let the player choose.
    /// </summary>

    void OnNetworkConnect(bool success, string message)
    {
        if (success)
        {
            // join a channel
            TNManager.client.BeginSend(Packet.RequestChannelList);
            TNManager.client.EndSend();
        }
        else
        {
            UnityTools.Broadcast("OnJoinFailed", message);
        }
    }

    /// <summary>
    /// Disconnected? Go back to the menu.
    /// </summary>

    void OnNetworkDisconnect()
    {
        if (!string.IsNullOrEmpty(disconnectLevel) && Application.loadedLevelName != disconnectLevel)
        {
            Application.LoadLevel(disconnectLevel);
        }
    }


    /// <summary>
    /// Joined a channel (or failed to).
    /// </summary>

    void OnNetworkJoinChannel(bool result, string message)
    {
        if (result)
        {
             UnityTools.Broadcast("OnJoinSucceed", message);
        }
        else
        {
            UnityTools.Broadcast("OnJoinFailed", message);
            TNManager.Disconnect();
        }
    }

    /// <summary>
    /// We want to return to the menu when we leave the channel.
    /// This message is also sent out when we get disconnected.
    /// </summary>

    void OnNetworkLeaveChannel()
    {
        if (!string.IsNullOrEmpty(disconnectLevel) && Application.loadedLevelName != disconnectLevel)
        {
            Application.LoadLevel(disconnectLevel);
        }
    }

    /// <summary>
    /// List open channels on the server.
    /// int32: number of channels to follow
    /// For each channel:
    /// int32: ID
    /// int32: Number of players
    /// bool: Has a password
    /// bool: Is persistent
    /// string: Level
    /// </summary>
    /// 
    void OnChannelList(Packet response, BinaryReader reader, IPEndPoint source)
    {
        int count = reader.ReadInt32();

        for (int i = 0; i < count; ++i)
        {
            int channelID = reader.ReadInt32();
            int playerCount = reader.ReadInt16();
            int playerLimit = reader.ReadInt16();
            bool password = reader.ReadBoolean();
            bool isPersistent = reader.ReadBoolean();
            string level = reader.ReadString();

            if (playerCount < maxPlayerCountPerChannel)
            {
                JoinChannel(channelID);
                return;
            }
        }

        // should only reach here when all the channels are full
        JoinChannel(count);
    }

    void JoinChannel(int channelID)
    {
        Debug.Log("Joining Channel: " + channelID);
        TNManager.JoinChannel(channelID, null, persistent, maxPlayerCountPerChannel, null);
    }

    void OnNetworkPlayerJoin(Player p)
    {
        // reaches 2 players, load level
        TNManager.LoadLevel(connectLevel);
    }

    void OnJoinSucceed(string message)
    {
        // hide the user name input and show the wait hint
        if (TNManager.players.Count == 0)
        {
            NGUITools.SetActive(userName.gameObject, false);
            NGUITools.SetActive(hintLabel.gameObject, true);
        }
    }

    void OnJoinFailed(string message)
    {
        Debug.LogError(message);
    }
}
