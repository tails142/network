#pragma strict
/*
var guiSkin : GUISkin;

function Awake()
{
	// On the server and clients, 
	
	// Assign the skin and display the chat interface
	BTSNetworkChat.guiSkin = guiSkin;
	BTSNetworkChat.displayChat = true;
	
	
	// Set the player handle based on the preferences or the IP address
	BTSNetworkChat.handle = BTSPreferences.GetStringForKey(Literals.kPlayerHandle);
	if (null == BTSNetworkChat.handle)
	{
		BTSNetworkChat.handle = Network.player.ipAddress;
	} 
		
	if ("" == BTSNetworkChat.handle)
	{
		BTSNetworkChat.handle = Network.player.ipAddress;
	} 
		
	// Only on the server, so this object must not be instantiated until after the 
	// server is initialized, create an instance of the BTSNetworkChat
	// object
	
	if (Network.peerType == NetworkPeerType.Server)
	{	
		if (null == BTSNetworkChat.sharedInstance)
		{
			Debug.LogError("NetworkChat: Could not create instance of BTSNetworkChat!");
		}
	}

}
*/
