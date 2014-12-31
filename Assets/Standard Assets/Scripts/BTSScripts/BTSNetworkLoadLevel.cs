using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class BTSNetworkLoadLevel : MonoBehaviour 
{
		
	private static BTSNetworkLoadLevel g_sharedInstance = null;

	/** Keep track of the last level prefix (increment each time a new level loads) */
	private static int g_lastLevelPrefix = 0;
	private static int g_levelPrefix = 0;
	
	
	/**
	 * The shared instance property
	 *
	 * This method returns the shared instance if it exists.  If it
	 * does not exist then a new gameObject is created and the 
	 * component is added to it.  This new gameObject is assigned to 
	 * the sharedInstance and flagged as DontDestroyOnLoad.
	 */

	public static BTSNetworkLoadLevel sharedInstance
    {
        get 
		{ 
			if (null == g_sharedInstance)
			{
				string l_className = typeof(BTSNetworkLoadLevel).Name;
				GameObject l_go = new GameObject(l_className +  " (" + Time.time + ")");
				
				g_sharedInstance = l_go.AddComponent(l_className) as BTSNetworkLoadLevel;
				
				NetworkView l_networkView = l_go.AddComponent("NetworkView") as NetworkView;
				l_networkView.stateSynchronization = NetworkStateSynchronization.Off;
				
				// Parent this object into the BTSGroup
				l_go.transform.parent = BTSManager.sharedInstance.transform;
				
				DontDestroyOnLoad(g_sharedInstance);
			}

			return g_sharedInstance;
		}
    }
	
	
	/** The BTSUDPServer lastLevelPrefix
	 * 
	 * This value is incremented each time a level 
	 * is loaded
	 * 
	 */

	public static int levelPrefix
    {
        get { return g_levelPrefix; }
        set { g_levelPrefix = value; }
    }
		
	public static int lastLevelPrefix
    {
        get { return g_lastLevelPrefix; }
        set { g_lastLevelPrefix = value; }
    }
		

	public static void LoadLevel(string a_levelName)
	{
		BTSNetworkLoadLevel l_shareInstance = BTSNetworkLoadLevel.sharedInstance;
		
		Debug.Log("public static void LoadLevel(" +  a_levelName + ")");
		
		levelPrefix++;
		lastLevelPrefix = levelPrefix;
		l_shareInstance.networkView.RPC("LoadLevelRPC",RPCMode.AllBuffered, a_levelName, levelPrefix);

		
		
	}
	
	public static void LoadLevelAdditiveAsync(string a_levelName)
	{
		BTSNetworkLoadLevel l_shareInstance = BTSNetworkLoadLevel.sharedInstance;
		
		Debug.Log("public static void LoadLevelAdditiveAsync(" +  a_levelName + ")");
		
		levelPrefix++;
		lastLevelPrefix = levelPrefix;
		l_shareInstance.networkView.RPC("LoadLevelAdditiveAsyncRPC",RPCMode.AllBuffered, a_levelName, levelPrefix);

		
		
	}
	
	[RPC]
	void LoadLevelRPC(string a_levelName, int a_levelPrefix)
	{
		Network.SetLevelPrefix(a_levelPrefix);
		Application.LoadLevel(a_levelName);
	}
	
	[RPC]
	void LoadLevelAdditiveAsyncRPC(string a_levelName, int a_levelPrefix)
	{
		Network.SetLevelPrefix(a_levelPrefix);
		Application.LoadLevelAdditiveAsync(a_levelName);
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
