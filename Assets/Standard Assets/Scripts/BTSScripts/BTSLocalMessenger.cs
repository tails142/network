using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class BTSLocalMessenger : MonoBehaviour 
{
	
	private static BTSLocalMessenger g_sharedInstance;
	
	/**
	 * The shared instance property
	 *
	 * This method returns the shared instance if it exists.  If it
	 * does not exist then a new gameObject is created and the 
	 * component is added to it.  This new gameObject is assigned to 
	 * the sharedInstance and flagged as DontDestroyOnLoad.
	 */

	public static BTSLocalMessenger sharedInstance
    {
        get 
		{ 
			if (null == g_sharedInstance)
			{
				string l_className = typeof(BTSLocalMessenger).Name;
				GameObject l_go = new GameObject(l_className +  " (" + Time.time + ")");
				g_sharedInstance = l_go.AddComponent(l_className) as BTSLocalMessenger;
				DontDestroyOnLoad(g_sharedInstance);
			}

			return g_sharedInstance;
		}
    }
	

	public static void SendNotification(string a_notification)
	{
		// TODO: Implement this method
	}
	
}


