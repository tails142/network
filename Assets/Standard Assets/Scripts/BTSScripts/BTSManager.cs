using UnityEngine;
using System.Collections;

public class BTSManager : MonoBehaviour 
{

	private static BTSManager g_sharedInstance = null;
	private static Hashtable g_networkViewIDs = new Hashtable();
	
	/**
	 * The shared instance property
	 *
	 * This method returns the shared instance if it exists.  If it
	 * does not exist then a new gameObject is created and the 
	 * component is added to it.  This new gameObject is assigned to 
	 * the sharedInstance and flagged as DontDestroyOnLoad.
	 */

	public static BTSManager sharedInstance
    {
        get 
		{ 
			string l_className = typeof(BTSManager).Name;

			if (null == g_sharedInstance)
			{
				Debug.LogError(l_className + ": Must be created in the scene from its prefab!");
			}

			return g_sharedInstance;
		}
    }
	
	public static void AllocateNetworkViewID (string a_className)
	{
		
		Debug.Log("AllocateNetworkViewID");
		
		string l_className = typeof(BTSManager).Name;
		
		if (null == BTSManager.sharedInstance)
		{
			return;
		}
		
		if (Network.peerType != NetworkPeerType.Server)
		{
			Debug.LogWarning (l_className + ": Only the server can allocate Network View IDs for BTS Managed classes");
			return;
		}
		
		NetworkViewID l_networkViewID = Network.AllocateViewID();
		
		g_networkViewIDs.Add(a_className, l_networkViewID);
		
		if (Network.peerType == NetworkPeerType.Server)
		{
			BTSManager.sharedInstance.networkView.RPC("AllocateNetworkViewIDRPC",RPCMode.All, a_className, l_networkViewID);
		}
	}
	

	 void OnPlayerConnected(NetworkPlayer a_player) 
	{
        Debug.Log("BTSManager: Player connected from " + a_player.ipAddress + ":" + a_player.port);
		
		NetworkViewID l_networkViewID;
		
		foreach (string l_key in g_networkViewIDs.Keys)
		{
			l_networkViewID = (NetworkViewID)g_networkViewIDs[l_key];
			BTSManager.sharedInstance.networkView.RPC("AllocateNetworkViewIDRPC",a_player, l_key, l_networkViewID);
		}
	}

/*
 	void OnServerInitialized() 
	{
        Debug.Log("Server Initialized");
		
		NetworkViewID l_networkViewID;
		
		foreach (string l_key in g_networkViewIDs.Keys)
		{
			l_networkViewID = (NetworkViewID)g_networkViewIDs[l_key];
			BTSManager.sharedInstance.networkView.RPC("AllocateNetworkViewIDRPC",RPCMode.Server, l_key, l_networkViewID);
		}
	}
*/	
	[RPC]
	void AllocateNetworkViewIDRPC(string a_className, NetworkViewID a_networkViewID )
	{
		Debug.Log("AllocateNetworkViewIDRPC");
		
		if (a_className == typeof(BTSNetworkChat).Name)
		{
			if (null == BTSNetworkChat.sharedInstance)
			{
				Debug.LogError("BTSManager could not create instance of BTSNetworkChat!");
				return;
			}
		}
		
		BTSManager.sharedInstance.BroadcastMessage("SetNetworkViewOn" + a_className, a_networkViewID, SendMessageOptions.RequireReceiver);
		
		// Clear any outstanding RPCs
		if (Network.peerType == NetworkPeerType.Server)
		{
			Network.RemoveRPCs(a_networkViewID);
		}
	}
		
	
	void Awake()
	{
		if (null == g_sharedInstance)
		{
			g_sharedInstance = this;
			DontDestroyOnLoad(g_sharedInstance);
		}
	}
		
}
