using UnityEngine;
using System.Collections;

public class BTSNetworkChat : MonoBehaviour 
{
	/** 
	 * These are the Class variables
	 * 
	 * Since the Class variables  are
	 * all accessed as properties, look for the details
	 * in that section.
	 */
	
	private static bool c_displayChat = false;
	private static BTSNetworkChat c_sharedInstance = null;
	private static GUISkin c_guiSkin = null;
	private static bool c_enterChat = false;

	private static float c_screenWidth = 480;
	private static float c_screenHeight = 320;
	private static Matrix4x4 c_TMatrix;
	
	private static string c_textToSend = "";
	
	private static string c_handle = "";

	private static ArrayList c_chatStack;
	
	private static int c_displayLines = 5;
	
	private static float c_displaySeconds = 30f;
	
	
	/** 
	 * The displayChat property is used to turn chat display on (true) or
	 * off (false).
	 */
	
	public static bool displayChat
    {
        get { return c_displayChat; }
		set { c_displayChat = value; }
	}

	/**
	 * The shared instance property
	 *
	 * This method returns the shared instance if it exists.  If it
	 * does not exist then a new gameObject is created and the 
	 * component is added to it.  This new gameObject is assigned to 
	 * the sharedInstance and flagged as DontDestroyOnLoad.
	 * 
	 * This gameObject is used for sending RPCs so you must be
	 * an initialized server or connected client prior to accessing
	 * the shared instance
	 * 
	 */

	public static BTSNetworkChat sharedInstance
    {
        get 
		{ 
			// Make sure you don't access the instance until you are either
			// an initialized server or a connected client
			
			if ((Network.peerType != NetworkPeerType.Server) &&
				(Network.peerType != NetworkPeerType.Client))
			{
				Debug.LogWarning("BTSNetworkChat only works on network connected machines!");
				return null;
			}
			
			// The gameObject is a singleton.  So its only initialized the very first time
			// it is accessed.  Afterwards its just returned
			
			if (null == c_sharedInstance)
			{
				// Get the name of this class
				string l_className = typeof(BTSNetworkChat).Name;
				
				// Create a gameObject and name it for the class - add a timestamp for debugging in the editor
				GameObject l_go = new GameObject(l_className +  " (" + Time.time + ")");
				
				// Add the behaviour (script) to the gameObject
				c_sharedInstance = l_go.AddComponent(l_className) as BTSNetworkChat;
				
				// Make it a singleton
				DontDestroyOnLoad(c_sharedInstance);

				// Parent this object into the BTSManager
				l_go.transform.parent = BTSManager.sharedInstance.transform;
				
				// If this instance was created on the Server
				// Ask the BTSManager to allocate a networkID for use by the class.
				// The BTSManager object will also and send an RPC to all clients 
				// and when they receive the RPC, they will create their own shared
				// instance and assign the allocated networkID
				//
				// The result will be all instances will share the networkID that was
				// allocated by, and therefore owned by, the server
				//
				// This is done by the manager, because it is the only object that is
				// setup, with a scene network view, to communicate across the network
				// due to it being created from the prefab
				//
				// As you can see for all of this to work, the machine that creates the
				// first instance of a BTSNetworkChat object MUST be an initialized 
				// server.  If its not, then chat will not work
				
				if (Network.peerType == NetworkPeerType.Server)
				{
					BTSManager.AllocateNetworkViewID(l_className);
				}
				
			}
			
			// Return the shared instance
			return c_sharedInstance;
		}
    }
	
	/** 
	 * The guiSkin property is used to assign a different GUISkin
	 */

	public static GUISkin guiSkin
    {
        get { return c_guiSkin; }
		set { c_guiSkin = value; }
	}
	
	/** 
	 * The transform matrix uses the screenWidth property
	 * to scale the GUI.
	 */
	
	public static float screenWidth
    {
        get { return c_screenWidth; }
		set { c_screenWidth = value; }
	}

	/** 
	 * The transform matrix uses the screenHeight property
	 * to scale the GUI.
	 */
	
	public static float screenHeight
    {
        get { return c_screenHeight; }
		set { c_screenHeight = value; }
	}

	/** 
	 * The TMatrix property is used to scale the GUI so that it
	 * fill the display regardless of its resolution
	 */
	
	private static Matrix4x4 TMatrix
    {
        get { return c_TMatrix; }
		set { c_TMatrix = value; }
	}

	/** 
	 * The textToSend property is a single line of entered text
	 * that will be sent to all the other players
	 */
	
	private static string textToSend
    {
        get { return c_textToSend; }
		set { c_textToSend = value; }
	}

	/** 
	 * The handle property is prepended to every chat message
	 * so that all the players know who said what
	 */

	public static string handle
    {
        get { 
				if ("" == c_handle)
				{
					return Network.player.ipAddress;
				}
				else
				{
					return c_handle;
				}
		}
		set { c_handle = value; }
	}

	
	/** 
	 * The displayLines property limits the number of chat lines
	 * that appear on the players screen
	 */

	public static int displayLines
    {
        get { return c_displayLines; }
		set { c_displayLines = value; }
	}
	
	/** 
	 * The displaySeconds property limits how long a chat line
	 * will appear on the players screen.  Its alpha will fade 
	 * from 1.0 to 0.0 in this amount of time
	 */

	public static float displaySeconds
    {
        get { return c_displaySeconds; }
		set { c_displaySeconds = value; }
	}
	
	/** 
	 * The chatStack property contains all the lines of chat that have
	 * been received.  Lines are removed when they are no longer
	 * displayed
	 */
	
	private static ArrayList chatStack
    {
        get 
		{ 	if (null == c_chatStack)
			{
				c_chatStack = new ArrayList();
			}
				
			return c_chatStack; 
		}
		set { c_chatStack = value; }
	}





	
	/** 
	 * Add a Network View and assign the server generated viewID
	 * 
	 * The SetNetworkViewOnBTSNetworkChat method is called by the BTSManager sharedInstance
	 * to add a NetworkView and assign it the viewID allocated by the server.  It is called
	 * on each client when the BTSManager sharedInatance receives an RPC from the server.
	 * 
	 * @param a_networkViewID is the networkViewID allocated by the server
	 * 
	 * @return void
	 */
	
	void SetNetworkViewOnBTSNetworkChat(NetworkViewID a_networkViewID)
	{
		Debug.Log("SetNetworkViewOnBTSNetworkChat");
		
		NetworkView l_networkView = gameObject.AddComponent("NetworkView") as NetworkView;
		l_networkView.stateSynchronization = NetworkStateSynchronization.Off;
		l_networkView.observed = null;
		l_networkView.viewID = a_networkViewID;
	}
	
	/** 
	 * Display the GUI if the displayChat property is true
	 * 
	 * The OnGUI method is called by the Unity game engine to display the GUI
	 * 
	 * @return void
	 */
	
	void  OnGUI()
	{
		// Do nothing if the displayChat property is false
		if (false == BTSNetworkChat.displayChat)
		{
			return;
		}
		
   		// Apply the transformation matrix to scale the GUI
 		// to the actual device screen size
		GUI.matrix = BTSNetworkChat.TMatrix;
		
		// Apply an optional GUISkin
		if (null != c_guiSkin)
		{
			GUI.skin = c_guiSkin;
		}
		
		// Calculate some values that will be used to display the GUI
		float l_padding = 2;
		float l_buttonWidth = BTSNetworkChat.screenWidth / 6;
		float l_buttonHeight = BTSNetworkChat.screenHeight / 8;
		float l_textHeight = BTSNetworkChat.screenHeight / 16;
		float l_chatWidth = BTSNetworkChat.screenWidth / 8 * 5;
		
		// Calculate some rectangles that will be used to display the GUI
		Rect l_chatButtonRect = new Rect(BTSNetworkChat.screenWidth / 2 - l_buttonWidth / 2 , BTSNetworkChat.screenHeight - l_buttonHeight - l_padding, l_buttonWidth, l_buttonHeight);
		Rect l_cancelButtonRect = new Rect(BTSNetworkChat.screenWidth / 2 - l_buttonWidth - l_padding , BTSNetworkChat.screenHeight - l_buttonHeight - l_padding, l_buttonWidth, l_buttonHeight);
		Rect l_sendButtonRect = new Rect(BTSNetworkChat.screenWidth / 2 + l_padding , BTSNetworkChat.screenHeight - l_buttonHeight - l_padding, l_buttonWidth, l_buttonHeight);
		Rect l_textRect = new Rect(BTSNetworkChat.screenWidth / 2 - l_chatWidth / 2  , BTSNetworkChat.screenHeight - 2 * l_buttonHeight, l_chatWidth, l_textHeight);
		
		// Create some references that will be used to display the GUI
		Rect l_chatStringsRect;
		Hashtable l_chatHash;
		Color l_color = GUI.contentColor;
		System.DateTime l_chatTime;
		System.TimeSpan l_delta;
		float l_alpha;
		
		// Iterate over all the lines in the chatStack
		for (int i = 0; i < BTSNetworkChat.chatStack.Count; i ++)
		{
			// Calculate the rectangle where the chat line will be displayed in the GUI
			l_chatStringsRect = new Rect(l_padding  , i *  l_textHeight, l_chatWidth, l_textHeight );
			
			// Notice the lines are selected from the newest (end of the stack) to the oldest
			// (top of the stack) which is essentially in reverse order
			l_chatHash = BTSNetworkChat.chatStack[BTSNetworkChat.chatStack.Count - i - 1] as Hashtable;
			
			// Get the time for this chat line, and calculate how old it is as l_delta
			l_chatTime = (System.DateTime)l_chatHash["time"];
			l_delta = System.DateTime.Now - l_chatTime;
			
			// Calculate the alpha based on the age of the chat line in milliseconds
			l_alpha = (float)(((displaySeconds * 1000)  - l_delta.TotalMilliseconds) / (displaySeconds * 1000));
			
			// If the alpha is greater than zero, display the line
			if (l_alpha > 0)
			{
				GUI.contentColor = new Color(1f,1f,1f,l_alpha);
				GUI.Label(l_chatStringsRect, l_chatHash["chat"] as string,"chat");
			}
			// Otherwise remove this line and all the lines that may follow it
			else
			{
				BTSNetworkChat.chatStack.RemoveRange(0, BTSNetworkChat.chatStack.Count - i);
				break;
			}
			
			// Stop displaying if the maximum number of lines to display has been reached
			if (i == (BTSNetworkChat.displayLines - 1))
			{
				break;
			}
		}
		
		// Restsore the original color (saved above)
		GUI.contentColor = l_color;
		
		// If the player clicked the chat button
		if (c_enterChat)
		{
			// Display a text field to accept characters.  
			// TODO: Notice the BAD magic number of 60 that limits the input length.  That needs to be changed, at least into a literal
			BTSNetworkChat.textToSend = GUI.TextField(l_textRect, BTSNetworkChat.textToSend, 60);
			
			// If the player changes his mind, stop displaying the text field
			// TODO: Notice the BAD magic string of "Cancel".  That needs to be changed, at least into a literal
			if (GUI.Button(l_cancelButtonRect, "Cancel"))
			{
				c_enterChat = false;
			}
			
			// If the player changes clicks the send button, send the chat to all the other players
			// TODO: Notice the BAD magic string of "Send".  That needs to be changed, at least into a literal
			if (GUI.Button(l_sendButtonRect, "Send"))
			{
				// If, for some reason, the connection to the server was lost, just do a local display
				if ((Network.peerType == NetworkPeerType.Disconnected) || (Network.peerType == NetworkPeerType.Connecting))
				{
					NewChatMessage(BTSNetworkChat.handle +": " + BTSNetworkChat.textToSend);
				}
				// Otherwise, send the message to all the other players.
				// Notice the RPC is *not* buffered.  Chat is not important.
				else
				{
					networkView.RPC("NewChatMessage", RPCMode.All, BTSNetworkChat.handle +": " + BTSNetworkChat.textToSend);
				}
				
				// Clear the chat line and wait for more input
				
				BTSNetworkChat.textToSend = "";
			}
		}
		// Otherwise, display the chat button
		// TODO: Notice the BAD magic string of "Chat".  That needs to be changed, at least into a literal
		else
		{
			c_enterChat = GUI.Button(l_chatButtonRect, "Chat");
		}
		
	}

	/** 
	 * Whenever the dialog is re-enabled we check to see if the device has been rotated
	 * 
	 * The OnEnable method is called by the Unity game engine whenever the gameObject
	 * is enabled.  Its a good place to check for device rotation
	 * 
	 * @return void
	 */
	
	void OnEnable()
	{
	
		RotateDevice();
	
	}

	/** 
	 * TODO: Somebody needs to call this -- HELLO !!!
	 * 
	 * This is currently not being called.  It needs to be called if the
	 * GUI is to rotate with the device.
	 */
	
	// Whenever the device is rotated
// the matrix needs to be
// recalculated
	void RotateDevice()
	{

 	// Calculate the transformation matrix
 	// for the actual device screen size
	BTSNetworkChat.TMatrix = Matrix4x4.TRS(Vector3.zero,  Quaternion.identity, new Vector3(Screen.width / BTSNetworkChat.screenWidth,  Screen.height / BTSNetworkChat.screenHeight, 1.0f));
	}
	
	/**
	 * Receive the Chat messages from all the other players
	 * 
	 * When a chat message is sent to the other players its timestamp is
	 * calculated on the receiving end, not the sending end, and it is
	 * added to the chatStack.
	 */

	[RPC]
	void NewChatMessage(string a_newChat)
	{
		Hashtable l_entry = new Hashtable();
		l_entry.Add("chat", a_newChat);
		l_entry.Add("time", System.DateTime.Now);
		BTSNetworkChat.chatStack.Add(l_entry);
	}

}

