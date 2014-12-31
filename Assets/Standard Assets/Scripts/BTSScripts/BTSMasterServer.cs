using UnityEngine;
using System.Collections;

/**
 * A replacement class for the builtin unity MasterServer
 */
//public static class BTSMasterServer
public class BTSMasterServer : MonoBehaviour
{
	private static BTSMasterServer g_sharedInstance = null;
	private static GameObject g_GameObject;
	
	private static bool useMySQL = false;
	//TODO use when not using built-in.
	//private static string ipAddress = ;
	//private static ushort port;
	//private static int updateRate;
	private static bool isDedicated;
	private static string s_serverRestartScript = "restart.php";
	private static string s_serverRegistrationScript = "registerScript.php";
	private static HostData[] lastKnownHosts;
	
	private static string s_gameType;
	private static string s_gameName;
	private static string s_gameComment;
	
	
	//Properties//
	public static string serverRestartScript
    {
        get { return s_serverRestartScript; }
        set { s_serverRestartScript = value; }
    }
	
	public static string serverRegistrationScript
    {
        get { return s_serverRegistrationScript; }
        set { s_serverRegistrationScript = value; }
    }


	/**
	 * The web-server's ip.
	 **/
	public static string ipAddress
    {
        get { return MasterServer.ipAddress; }
        set { MasterServer.ipAddress = value; }
    }
	
	/**
	 * The port the service raknet is on, on the server.
	 */
	public static int port
    {
        get { return MasterServer.port; }
        set { MasterServer.port = value; }
    }
	
	/**
	 * The rate at which the server list should be refreshed
	 **/
	public static int updateRate
    {
        get { return MasterServer.updateRate; }
        set { MasterServer.updateRate = value; }
    }
	
	/**
	 Does a connection from the localhost count as a player connection?
	 */
	public static bool dedicatedServer
    {
        get { return MasterServer.dedicatedServer; }
        set { MasterServer.dedicatedServer = value; }
    }

	/**
	 The shared instance
	 */
	public static BTSMasterServer sharedInstance
    {
        get { return BTSMasterServer.SharedInstance(); }
    }
	


	//Methods//
	/**
	 * Fetch the most up to date host list from the server
	 **/
	public static void RequestHostList( string gameTypeName)
	{
		MasterServer.RequestHostList(gameTypeName);
	}
	
	/**
	 * Ask for the last retrived host data
	 **/
	public static HostData[] PollHostList()
	{
		return MasterServer.PollHostList();
	}
	
	public static BTSMasterServer SharedInstance()
	{
		
		if (null == g_sharedInstance)
		{
			string l_className = typeof(BTSMasterServer).Name;
			GameObject l_go = new GameObject(l_className +  " (" + Time.time + ")");
			g_sharedInstance = l_go.AddComponent(l_className) as BTSMasterServer;

			// Parent this object into the BTSGroup
			l_go.transform.parent = BTSManager.sharedInstance.transform;

			DontDestroyOnLoad(g_sharedInstance);
		}

		return g_sharedInstance;
	}
		
	static void RegisterHost(string gameTypeName, string gameName)
	{
		RegisterHost(gameTypeName,gameName,"");
	}
	
	public static void UnregisterHost()
	{

		MasterServer.UnregisterHost();
	}
	
	public static void ClearHostList()
	{
		MasterServer.ClearHostList();
	}
	
	public static void RegisterHost(string gameTypeName, string gameName, string comment)
	{
		MasterServer.RegisterHost( gameTypeName, gameName, comment);
	}
	
	// Begin Extended Functionality //
	
	/** 
	 * This method invokes a PHP script on the ipAddress and that
	 * PHP script, in turn, starts a copy of the MasterServer and
	 * the Facilitator on that server.  Then we have these two 
	 * services available on the Internet.
	 */
	
	public static void StartMasterServer()
	{
		
		/*
		 * Use the MasterServer shared instance to run the Coroutine that posts the WWW form
		 * to start the MasterServer at our IP address
		 */
		
		sharedInstance.StartCoroutine(sharedInstance.StartMasterServerUsingInstance());
		
	}


	/**
	 * The following code works to start the master server using an instance
	 */
	
	public IEnumerator StartMasterServerUsingInstance()
	{

		/*
		 * The first time the Iterator is called the WWW request is returned
		 */
		
		WWW www = new WWW (ipAddress+serverRestartScript);
		yield return www;
		
	}
	
	
	/**
	 * The following code works to restart the master server using a static class variable
	 * It depends on the ability to create an gameObject on which to run the coroutine and
	 * while it could be any dummy gameObject we use an instance of the BTSMasterServer class
	 * to keep things related
	 */

	public static void RestartMasterServer(string a_gametype, string a_gamename, string a_gamecomment)
	{
		
		/*
		 * Use the MasterServer shared instance to run the Coroutine that posts the WWW form
		 * and registers this host
		 */
		
		sharedInstance.StartCoroutine(sharedInstance.RestartMasterServerUsingInstance(a_gametype, a_gamename , a_gamecomment));
		
	}


	/**
	 * The following code works to restart the master server using an instance
	 */
	
	public IEnumerator RestartMasterServerUsingInstance(string a_gametype, string a_gamename, string a_gamecomment)
	{

		/*
		 * The first time the Iterator is called the WWW request is returned
		 */
		
		WWW www = new WWW (ipAddress+serverRestartScript);
		yield return www;
		
		/*
		 * The second time the Iterator is called the Masterserver is called
		 * to register the game.  It should have been restarted by the WWW request
		 */
		
		MasterServer.RegisterHost(a_gametype, a_gamename , a_gamecomment);
		
		/* 
		 * Probably this is not needed as the coroutine will end without it
		 */
		
		yield break; 

	}

	/**
	 End all connections on localhost and unlist from master
	 */
	static void UnregisterAndShutdownHost()
	{
		Network.Disconnect();
        UnregisterHost();
	}
	
		
	//Internal functionality//
	
	/**
	 * @todo implement
	 **/
	private static HostData[] ParseHosts ( string wwwText )
	{
		return null;
	}

}

