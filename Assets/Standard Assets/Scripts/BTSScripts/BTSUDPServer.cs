using UnityEngine;
using System.Net; // IPEndpoint
using System.Net.Sockets; // UDPClient
using System.Threading; // Thread
using System.Collections; // Hashtable, ArrayList

public class BTSUDPServer : MonoBehaviour 
{
	/*
	 * These events mirror those that can be generated
	 * by MasterServer but they behave slightly differently
	 */
	
	public enum BTSUDPServerEvent
	{
		RegistrationFailedGameName, // This event will never occur
		RegistrationFailedGameType, // This event will never occur
		RegistrationFailedNoServer, // This event will never occur
		RegistrationSucceeded,  	// This event is implemented    
		HostListReceived 			// This event is implemented but deferred, it may occur after the poll list is received
	};
		


	/*
	 * These variables are for the distributed "MasterServer"
	 */
		
	/*
	 * These variables are for this one machine acting as a server
	 */
	
	/* When a RegisterHost is called, this Hashtable is populated
	 * and broadcast by the server_thread in UDP packtes.  When 
	 * UnregisterHost is called this Hashtable is set to null and 
	 * the UDP packet broadcasts will stop
	 */
	
	private static Hashtable g_hostDataHashtable = null;
	
	/* The UDP Client expects an encoded byte array so the
	 * g_hostDataHashtable is put into this byte array encoded
	 * using UTF8 for broadcast
	 */
	
	private static byte[] g_dataToBroadcast;
	
	/*
	 * This is the broadcast IP address for IPv4 
	 */
	
    private static string g_broadcastIPAddress = "255.255.255.255";
	
	
	/* 
	 * These variables are for this one machine acting as a client
	 */

	/*
	 * Send BTSUDPServerEvent to registered gameObjects that are 
	 * stored in this array
	 */
	
	private static ArrayList g_eventListener;
	
	/*
	 * The array of HostData that is returned by PollHostList
	 */
	
	private static HostData[] g_hostdataArray = null;
	
	/*
	 * When a client calls RequestHostList, this value is set so that the
	 * client_thread can filter received UDP packets for just those that
	 * contain the requested gameTypeName
	 */
	
	private static string g_requestedGameTypeName;
	
	/*
	 * When looking for UDP packets, this is how long to wait.  It is also used
	 * in the deferred generation of the HostListReceived event.  It can
	 * be set by the client to tune the time spent looking for packets
	 */
	
	private static float g_pollSeconds = 5.0f;
	
	/* 
	 * The gameServerHostHashtable contains the HostData of all the machines that
	 * are broadcasting that they have a game registered, filtered by the 
	 * requested game type
	 */
	
	private static Hashtable g_gameServerHostHashtable = null;
	
	/*
	 * These variables are needed for more than one role
	 */
	
	/* This is the frequency with which the UDP packets are put on the network.
	 * It is also used in the deferred generation of the HostListReceived event.  
	 * It can be set by the server to tune the frequency of broadcast packets.
	 */
	private static int g_serverThreadSleepMilliseconds = 1000;
	
	/* Some things, like sending events, need to be done on an instance
	 * of an object.  The single instance reference to the BTSUDPServer object is
	 * kept here.
	 */
	
	private static BTSUDPServer g_sharedInstance = null;
		
	/* 
	 * This is the port on which the UDP server is running.  Be very,
	 * very careful about changing this port as it could result
	 * in clients not seeing advertised games because they are
	 * looking for the server on a different port.
	 * 
	 * This is different than the game port, in fact its possible
	 * to have games running on various different ports all being
	 * advertised on the same server.
	 * 
	 */
	
    private static string g_port = "" + (MasterServer.port + 1);
	
	
	/**
	 * For legacy reason we have two UDP clients.  It turns out that if we
	 * want to send and receive UDP datagrams on the same port we MUST use
	 * the same client for sending and receiving.  SO while the code still
	 * contains two client references, they are in fact THE SAME single
	 * reference.  Be careful with this until the code is refactored to use
	 * only a single reference!
	 */

	private static UdpClient g_server = null;
	private static UdpClient g_client = null;
	
	/** The number of players connected to the server */
	
	private static int g_connectedPlayers = 0;
	
	
	/**
	 * The shared instance property
	 *
	 * This method returns the shared instance if it exists.  If it
	 * does not exist then a new gameObject is created and the 
	 * component is added to it.  This new gameObject is assigned to 
	 * the sharedInstance and flagged as DontDestroyOnLoad.
	 */

	public static BTSUDPServer sharedInstance
    {
        get 
		{ 
			if (null == g_sharedInstance)
			{
				string l_className = typeof(BTSUDPServer).Name;
				GameObject l_go = new GameObject(l_className +  " (" + Time.time + ")");
				g_sharedInstance = l_go.AddComponent(l_className) as BTSUDPServer;
				
				// Parent this object into the BTSGroup
				l_go.transform.parent = BTSManager.sharedInstance.transform;

				DontDestroyOnLoad(g_sharedInstance);
			}

			return g_sharedInstance;
		}
    }
	
	/** The list of gameObjects to which events are sent property
	  * 
	  * This method always returns an object, so if the reference is 
	  * null the object will be created.
	  */
	
	public static ArrayList eventListener
    {
        get 
		{
			if (null == g_eventListener)
			{
				g_eventListener = new ArrayList();
			}
			
			return g_eventListener; 
		}
    }
	
	
	/** The BTSUDPServer IP port property
	 * 
	 * This is the port on which the UDP server is running.  Be very,
	 * very careful about changing this port as it could result
	 * in clients not seeing advertised games because they are
	 * looking for the server on a different port.
	 * 
	 * This is different than the game port, in fact its possible
	 * to have games running on various different ports all being
	 * advertised on the same server.
	 * 
	 */

	public static string port
    {
        get { return g_port; }
        set { BTSUDPServer.g_port = value; }
    }
		
	/** The UdpClient server property
	 * 
	 * The name says it all
	 */

	public static UdpClient server
    {
        get { return BTSUDPServer.g_server; }
        set { BTSUDPServer.g_server = value; }
    }

	/** The UdpClient client property
	 * 
	 * The name says it all
	 */

	public static UdpClient client
    {
        get { return BTSUDPServer.g_client; }
        set { BTSUDPServer.g_client = value; }
    }

	/** The server thread sleep time property
	 * 
	 * The frequency with which UDP packets are sent and also a component
	 * of the delay used to send the HostListReceived event
	 */

	public static int serverThreadSleepMilliseconds
    {
        get { return g_serverThreadSleepMilliseconds; }
        set { BTSUDPServer.g_serverThreadSleepMilliseconds = value; }
   }
	
	/** The receive timeout for the socket.
	 * 
	 * This is the millisecond value representation of the pollSeconds
	 */

	public static int receiveTimeoutMilliseconds
    {
        get { return (int)(1000 * g_pollSeconds); }
    }


	/** The game server host list property
	 * 
	 * When UDP packets are read, they are filtered by the requested gameType and
	 * put into this hashtable
	 */

	public static Hashtable gameServerHostHashtable
    {
        get { return g_gameServerHostHashtable; }
		set { g_gameServerHostHashtable = value; }

    }


	/** The broadcast IPv4 Address property
	 * 
	 * The name says it all
	 */

	public static string broadcastIPAddress
    {
        get { return g_broadcastIPAddress; }
    }
	
	/** The number of seconds to poll for servers property
	 * 
	 * Its also used as the receive timeout and as a componet of the time used
	 * to send the deferred HostListReceived event
	 */

	public static float pollSeconds
    {
        get { return g_pollSeconds; }
       	set { g_pollSeconds = value; }
    }
		
	/** The requested game type name property
	 * 
	 * Receive UDP packets are filtered by this value which is set in the
	 * RequestHostList method
	 */

	public static string requestedGameTypeName
    {
        get { return g_requestedGameTypeName; }
       	set { g_requestedGameTypeName = value; }
    }

	/** The host data array property
	 * 
	 * This list is returned to the client in the PollHostList
	 * method
	 */

	public static HostData[] hostdataArray
    {
        get { return g_hostdataArray; }
        set { g_hostdataArray = value; }
    }

		
	/** The data to broadcast for this server
	 * 
	 * When a host is registered, all the vital data is put into
	 * this Hashtable for broadcast by the server_thread
	 */

	public static Hashtable hostDataHashtable
	{
		get { return g_hostDataHashtable; }
		set { g_hostDataHashtable = value; }
	}
		
	/** The data to broadcase byte array property
	 * 
	 * The UDPClient expects a byte array, so we provide one.  Its a UTF8 encoding 
	 * of the hostDataHashtable
	 */
	
	public static byte[] dataToBroadcast
	{
		set { g_dataToBroadcast = value; }
		get { return g_dataToBroadcast; }
	}
	
	/** The number of players connected to the server property */
	
	public static int connectedPlayers
	{
		set { g_connectedPlayers = value; }
		get { return g_connectedPlayers; }
	}
	
	/** server_thread
	 * 
	 * This method broadcasts UDP packets that contain the host data
	 */
	
	private static void server_thread() 
	{

		Debug.Log("server_thread - Entered");
		
		try
    	{
			/*
			 * So long as there is something in the hostData the ipAddress of
			 * this machine will be broadcast.
			 */
			Debug.Log("BTSUDPServer.port=" + BTSUDPServer.port);
        	while (null != BTSUDPServer.hostDataHashtable)
        	{
          		BTSUDPServer.server.Send(BTSUDPServer.dataToBroadcast,BTSUDPServer.dataToBroadcast.Length, BTSUDPServer.broadcastIPAddress,System.Convert.ToInt32(BTSUDPServer.port));
          		Thread.Sleep(BTSUDPServer.serverThreadSleepMilliseconds);
			}
			
    	} 
		catch (System.Exception l_ex)
		{
			Debug.Log("Exception server_thread got: " + l_ex);
		}
		
		lock (BTSUDPServer.server)
		{
			if (null == BTSUDPServer.client)
			{
				BTSUDPServer.server.Close();
			}
		
			BTSUDPServer.server = null;
		}
			

		Debug.Log("server_thread - Exited");
    }

	/** client_thread
	 * 
	 * This method receives and filters UDP packets that contain the host data
	 */
	
	private static void client_thread() 
	{
		Debug.Log("client_thread - Entered");

        System.Text.UTF8Encoding encode = new System.Text.UTF8Encoding();

		IPEndPoint l_receivePoint = null;
		
		client.Client.ReceiveTimeout = BTSUDPServer.receiveTimeoutMilliseconds;
		
		System.DateTime l_pollTimeStart = System.DateTime.Now;
				
 		if (null == BTSUDPServer.gameServerHostHashtable)
		{
			BTSUDPServer.gameServerHostHashtable = new Hashtable();
		}
		
		BTSUDPServer.gameServerHostHashtable.Clear();

		try
      	{
          	while (System.DateTime.Now < l_pollTimeStart.AddMilliseconds(BTSUDPServer.receiveTimeoutMilliseconds))
          	{
				// receivePoint tell us which host sent the datagram
				// This call blocks
          		byte[] recData = BTSUDPServer.client.Receive(ref l_receivePoint);
				
				string l_ipAddress = l_receivePoint.Address.ToString();
				string l_port = l_receivePoint.Port.ToString();
				
				Hashtable l_plist = new Hashtable();
				BTSTinySerializer.LoadPlistFromString(encode.GetString(recData), l_plist);
				
				if ((string)l_plist[Literals.kGameType] == BTSUDPServer.requestedGameTypeName)
				{
				
					l_plist.Add(Literals.kLastUpdateTime, System.DateTime.Now);
				
					string l_gameKey = l_ipAddress + ":" + l_port;
				
					if (BTSUDPServer.gameServerHostHashtable.ContainsKey(l_gameKey))
					{
						BTSUDPServer.gameServerHostHashtable[l_gameKey] = l_plist;
					}
					else
					{
						BTSUDPServer.gameServerHostHashtable.Add(l_gameKey, l_plist);
					}	
				}
          	}
			
      	} 
		catch (System.Exception l_ex)
		{
			Debug.Log("Exception client_thread got: " + l_ex);
		}
				
		lock (BTSUDPServer.client)
		{
			if (null == BTSUDPServer.server)
			{
				BTSUDPServer.client.Close();
			}
		
			BTSUDPServer.client = null;
		}
				
		Debug.Log("client_thread - Exited");
    }
	
	/** The BTSUPDServerEvents
	 * 
	 * These methods manage the BTSUPDServerEvents.
	 * 
	 * RegisterForEvents: If a gameObject wants BTSUDPServer events it needs to register
	 * DeregisterForEvents: If a gameObject is no longer interested in BTSUDPServer it can deregister
	 * SendOnBTSUDPServerEvent: Send the a_event now
	 * SendDelayedOnBTSUDPServerEvent: Send the a_event later
	 * SendOnBTSUDPServerEventUsingObject: Use an instance to send the a_event
	 * SendHostListReceived: Use an instance to send the HostListReceived event
	 */
	
	public static void RegisterForEvents (GameObject a_gameObject)
	{
		if (false == BTSUDPServer.eventListener.Contains(a_gameObject))
		{
			BTSUDPServer.eventListener.Add(a_gameObject);
		}
	}
	
	public static void DeregisterForEvents (GameObject a_gameObject)
	{
		if (true == BTSUDPServer.eventListener.Contains(a_gameObject))
		{
			BTSUDPServer.eventListener.Remove(a_gameObject);
		}
	}

	private static void SendOnBTSUDPServerEvent (BTSUDPServerEvent a_event)
	{
		Debug.Log("Sending OnBTSUDPServerEvent: " + a_event);
		BTSUDPServer.sharedInstance.SendOnBTSUDPServerEventUsingObject (a_event);
	}
	
	private static void SendDelayedOnBTSUDPServerEvent (BTSUDPServerEvent a_event, float a_delay)
	{
		if (BTSUDPServerEvent.HostListReceived == a_event)
		{
			BTSUDPServer.sharedInstance.Invoke("SendHostListReceived", a_delay);
		}
	}
	
	private void SendOnBTSUDPServerEventUsingObject (BTSUDPServerEvent a_event)
	{
		if (0 == BTSUDPServer.eventListener.Count)
		{
			Debug.LogWarning("A BTSUDPServerEvent occurred but no one was interested: " + a_event);
		}
		
		foreach (GameObject l_gameObject in BTSUDPServer.eventListener)
		{
			l_gameObject.SendMessage("OnBTSUDPServerEvent", a_event, SendMessageOptions.DontRequireReceiver);
		}
	}
	
	private void SendHostListReceived ()
	{
		if (0 == BTSUDPServer.eventListener.Count)
		{
			Debug.LogWarning("A BTSUDPServerEvent occurred but no one was interested: " + BTSUDPServerEvent.HostListReceived);
		}
		
		foreach (GameObject l_gameObject in BTSUDPServer.eventListener)
		{
			l_gameObject.SendMessage("OnBTSUDPServerEvent", BTSUDPServerEvent.HostListReceived, SendMessageOptions.DontRequireReceiver);
		}
	}
	

	
	/** ClearHostList()
	 * 
	 * This method clears the hostdataList array
	 */
	
	public static void ClearHostList()
	{
		BTSUDPServer.gameServerHostHashtable = null;
	}
	
	/** PollHostList()
	 * 
	 * This method gets the host data from every machine that was publishing
	 * its IP address and stores the data in an internal array.
	 * 
	 * Then it returns that as a native .NET array
	 **/
	
	public static HostData[] PollHostList()
	{
		// If there is no gameServerHostHashtable then return 
		// an empty array
		if (null == gameServerHostHashtable)
		{
			return new HostData[0];
		}
		
		// If the client is still running then there may still be some hosts that
		// have not been registered.  So we simulate the case that nothing has come 
		// back yet.
		
		// This makes the BTSUDPServer very predictable, it will always find the host
		// data in the amount of time specified by pollSeconds
		
		if (null != BTSUDPServer.client)
		{
			return new HostData[0];
		}
			
		// Recreate the hostdataArray
		// We have no idea what host data is in BTSUDPServer.gameServerHostHashtable so
		// we MUST always perform this task
		BTSUDPServer.hostdataArray = new HostData[BTSUDPServer.gameServerHostHashtable.Count];
		
		int i = 0;
		foreach (Hashtable l_plist in BTSUDPServer.gameServerHostHashtable.Values)
		{
			HostData l_hostData = new HostData();
			
			l_hostData.useNat = false;
			l_hostData.gameType = (string)l_plist[Literals.kGameType];
			l_hostData.gameName = (string)l_plist[Literals.kGameName];
				
			// These need to be set on the server using the Network class
			// The network must be initialized before the master server
			// or bad things will happen
			l_hostData.connectedPlayers = (int)l_plist[Literals.kConnectedPlayers];			
			
			l_hostData.playerLimit = (int)l_plist[Literals.kPlayerLimit];
			l_hostData.passwordProtected = (bool)l_plist[Literals.kPasswordProtected];
								
			string[] l_ips = new string[1];
			l_ips[0] = (string)l_plist[Literals.kIPAddress];
			l_hostData.ip = l_ips;
				
			l_hostData.port = System.Convert.ToInt32(l_plist[Literals.kPort]);
			
			l_hostData.comment = (string)l_plist[Literals.kGameComment];
			
			l_hostData.guid = "";
				
			BTSUDPServer.hostdataArray[i++] = l_hostData;

		}
			
		
		/*
		 * This returns a Static Array copy of the ArrayList because
		 * that is what is expected.
		 */
		
		return BTSUDPServer.hostdataArray;
	}


	/** UnregisterHost()
	 * 
	 * This method unregisters a host by removing its hostData
	 * object.  As a side effect it will stop broadcasting its IP
	 * address in a UDP datagram (see the server_thread)
	 */

	public static void UnregisterHost()
	{
		// Garbage collection will clean up this unreferenced object
		BTSUDPServer.hostDataHashtable = null;
		BTSUDPServer.connectedPlayers = 0;
	}
	
	/** RegisterHost(string a_gameType, string a_gameName, string a_comment)
	 * 
	 * This method registers the host creating a hostDataHashtable object and
	 * populating it.  Note only 1 host is supported to be compatible 
	 * with the MasterServer.
	 * 
	 * Once he hostDataHashtable object exists and is populated, this method starts a 
	 * server_thread which broadcasts the IP address of this machine until
	 * the host is deregistered (at which time the hostDataHashtable object will be
	 * set to null).
	 */
	
	public static void RegisterHost(string a_gameType, string a_gameName, string a_comment)
	{
		
		/*
		 * Make sure we have a shared instance to tear down the
		 * connections.  If we don't do this, the server_thread
		 * will keep on broadcasting when we quit in the editor.
		 */
		
		if (null == BTSUDPServer.sharedInstance)
		{
			Debug.LogError("Could not create share instance of BTSUDPServer");
		}
		
		if (null == BTSUDPServer.hostDataHashtable)
		{
			BTSUDPServer.hostDataHashtable = new Hashtable();
			BTSUDPServer.connectedPlayers = 0;
						
			BTSUDPServer.hostDataHashtable.Add(Literals.kGameType, a_gameType);
			BTSUDPServer.hostDataHashtable.Add(Literals.kGameName, a_gameName);
			BTSUDPServer.hostDataHashtable.Add(Literals.kGameComment, a_comment);

			BTSUDPServer.hostDataHashtable.Add(Literals.kConnectedPlayers, BTSUDPServer.connectedPlayers + 1);
			BTSUDPServer.hostDataHashtable.Add(Literals.kPlayerLimit, Network.maxConnections + 1);
						
			if ("" == Network.incomingPassword)
			{
				BTSUDPServer.hostDataHashtable.Add(Literals.kPasswordProtected, false);
			}
			else
			{
				BTSUDPServer.hostDataHashtable.Add(Literals.kPasswordProtected, true);
			}

			BTSUDPServer.hostDataHashtable.Add(Literals.kIPAddress, Network.player.ipAddress);
			BTSUDPServer.hostDataHashtable.Add(Literals.kPort, Network.player.port);
			
		}
		else
		{
			// Things that change are updated here
			BTSUDPServer.hostDataHashtable[Literals.kGameType] = a_gameType;
			BTSUDPServer.hostDataHashtable[Literals.kGameName] = a_gameType;
			BTSUDPServer.hostDataHashtable[Literals.kGameComment] = a_gameType;			
		}
				
		encodeHostData();		

		if (null == BTSUDPServer.server)
		{
			if (null == BTSUDPServer.client)
			{
				try
				{
      				BTSUDPServer.server = new UdpClient(System.Convert.ToInt32(BTSUDPServer.port));
				}
				catch (System.Exception l_ex)
				{
					Debug.Log("BTSUDPServer.server System.Exception = " + l_ex);
					BTSUDPServer.server = null;
					SendOnBTSUDPServerEvent (BTSUDPServerEvent.RegistrationFailedNoServer);
				}
			}
			else
			{
				BTSUDPServer.server = BTSUDPServer.client;
			}
			
			if (null != BTSUDPServer.server)
			{
      			Thread serverThread = new Thread(new ThreadStart(server_thread));
      			serverThread.Start();
			}
		}
			
		if (null != BTSUDPServer.server)
		{
			SendOnBTSUDPServerEvent (BTSUDPServerEvent.RegistrationSucceeded);
		}
		
	}
	
	private static void encodeHostData()
	{
		lock (BTSUDPServer.hostDataHashtable)
		{
			System.Text.UTF8Encoding encode = new System.Text.UTF8Encoding();
       	 	BTSUDPServer.dataToBroadcast = encode.GetBytes(BTSTinySerializer.PlistToString(hostDataHashtable));
		}

	}

	/** RequestHostList ( string a_gameTypeName)
	 * 
	 * This method looks for machines that are broadcasting UDP
	 * packets and assembles a host list from them.  It only looks for a 
	 * limited amount of time (g_pollSeconds)
	 */
	
	public static void RequestHostList ( string a_gameTypeName)
	{
		/*
		 * Make sure we have a shared instance to tear down the
		 * connections.  If we don't do this, the server_thread
		 * will keep on broadcasting when we quit in the editor.
		 */
		
		if (null == BTSUDPServer.sharedInstance)
		{
			Debug.LogError("Could not create share instance of BTSUDPServer");
		}
		
		/*
		 * We know the host list will be received after the pre-determined
		 * time so we schedule the event now
		 */
		BTSUDPServer.SendDelayedOnBTSUDPServerEvent (BTSUDPServerEvent.HostListReceived, BTSUDPServer.pollSeconds + (serverThreadSleepMilliseconds / 1000));
		
		/* 
		 * We want to filter for a specific gameTypeName when we poll
		 * so we need to remember the gameTypeName
		 */
		
		BTSUDPServer.requestedGameTypeName = a_gameTypeName;
		
		/*
		 * We clear the array.  This will be the trigger that PollHostList
		 * needs to know that it needs to gather the HostData from
		 * the Hashtable again.
		 */
		
		BTSUDPServer.hostdataArray = null;

		/* 
		 * If a client already exists, we can get out of here
		 */
		
		if (null != BTSUDPServer.client)
		{
			return;
		}
				
		/*
		 * Create a new UDP Client on the game UDP Port
		 */
		
		Debug.Log("BTSUDPServer.port=" + BTSUDPServer.port);
		
		if (null == BTSUDPServer.server)
		{
			BTSUDPServer.client = new UdpClient(System.Convert.ToInt32(BTSUDPServer.port));
		}
		else
		{
			BTSUDPServer.client = BTSUDPServer.server;
		}
				
		/*
		 * Since we will use client.Receive, which blocks, we run it
		 * on a thread so that the program does not block
		 */
		
		Thread clientThread = new Thread(new ThreadStart(client_thread));
   		clientThread.Start();
				
	}


	void OnPlayerConnected(NetworkPlayer player) 
	{
		// Oddly we sometimes get a player connecting after we
		// have shutdown our server.  Sounds like a Unity or
		// Facilitator bug, in any case we ignore the connection
		
		if (null == BTSUDPServer.hostDataHashtable)
		{
			Network.Disconnect();
        	BTSUDPServer.UnregisterHost();
			return;
		}
		
		
    	Debug.Log("BTSUDPServer: Player " + BTSUDPServer.connectedPlayers + 
              " connected from " + player.ipAddress + 
              ":" + player.port);
		
		BTSUDPServer.connectedPlayers++;
		BTSUDPServer.hostDataHashtable[Literals.kConnectedPlayers] = BTSUDPServer.connectedPlayers + 1;		
		encodeHostData();
		
	}
		
	void OnPlayerDisconnected(NetworkPlayer a_player) 
	{

		// Oddly we sometimes get a player connecting after we
		// have shutdown our server.  Sounds like a Unity or
		// Facilitator bug, I have not seen a late disconnect
		// but just in case one happesn, we want to ignore it too
		
		if (null == BTSUDPServer.hostDataHashtable)
		{
 			Network.Disconnect();
        	BTSUDPServer.UnregisterHost();
			return;
		}
		
		if (BTSUDPServer.connectedPlayers > 0)
		{
			BTSUDPServer.connectedPlayers--;
		}
		BTSUDPServer.hostDataHashtable[Literals.kConnectedPlayers] = BTSUDPServer.connectedPlayers + 1;		
		encodeHostData();
		
    	Network.RemoveRPCs(a_player);
    	Network.DestroyPlayerObjects(a_player);
	}
	
	
	/**
	 * Because this class is static, we need some object that can shut things down
	 * when the Application quits.  In this case we use our own shared instance so
	 * it is very, very important that the shared instace exists before we
	 * do anything else.
	 * 
	 * Notice the static functions test to make sure BTSUDPServer.sharedInstance
	 * is not null.  
	 * 
	 * This is why.
	 */
	
	void OnApplicationQuit()
	{		
		if (null != BTSUDPServer.client)
		{
			
			BTSUDPServer.client.Close();
			BTSUDPServer.client = null;
			BTSUDPServer.server = null;
		}

		if (null != BTSUDPServer.server)
		{
			BTSUDPServer.server.Close();
			BTSUDPServer.server = null;
		}
			

	}


	
}

/*
 * Below this point is some sample code.  Its kept here because it is
 * relevant to this class as a potential alternative to the timeouts
 * used on the UDPClient 

 // You can also use the async methods to do this, but manually block execution:
 
	var timeToWait = TimeSpan.FromSeconds(10);

	var udpClient = new UdpClient( portNumber );
	var asyncResult = udpClient.BeginReceive( null, null );
	asyncResult.AsyncWaitHandle.WaitOne( timeToWait );
	try
	{
    	IPEndPoint remoteEP = null;
    	byte[] receivedData = udpClient.EndReceive( asyncResult, ref remoteEP );
    	// EndReceive worked and we have received data and remote endpoint
	}
	catch (Exception ex)
	{
    	// EndReceive failed and we ended up here
	}

*/
