// This variable tells the game object to
// presist across level loads
var isSingleton : boolean = false;

// This variable tells the dialog to
// automatically show itself when the
// script starts

var showOnStart : boolean = false;

// These two inspector variables are set to
// the screen resolution that our GUI is
// designed to fit without any scaling

var screenWidth  : float = 480.0;
var screenHeight : float = 320.0;

// This is the game object responsible
// for rendering the dialog content

var contentGO : GameObject;

// This is the messages sent
// to the content rendering game object
// The rendercontent method must exist
// in a script on that object

var contentRenderMessage : String = "rendercontent";

// The box size, is the size of the dialogs
// bounding box specified as a fraction of
// the screen size so that it is
// resolution independent.  It will be
// converted to the correct number of
// pixels by the script

var boxSize : Vector2;

// The dialog box size needs to be
// converted to the correct number
// of pixels.  The number of pixels
// is stored in this private variable

private var boxSizePixels : Vector2;

// The dialog needs to start in one
// place and end in another so that
// we can make it slide.  Its important
// to note these positons can be off
// the screen so that dialogs can
// slide on and off the screen.  Similar
// to the box size, the dialog positions
// are specified in fractions of the 
// screen size and not in absolute pixels

var startPos: Vector3; 
var endPos: Vector3;

// The dialog box positions need to be
// converted to the correct number
// of pixels.  The number of pixels
// is stored in these private variable

private var startPosPixels: Vector3; 
private var endPosPixels: Vector3;

// The dialog needs to slide for some
// amount of time.  Some dialogs may
// slide in quickly while others may
// slide in slowly.  The amount of time
// depends on the dialog and the game
var slideTime: float = 3.0; 

// Normally when a dialog is in the start
// position, we want to disable it so
// that it does not impact performance.
// There are special cases where we want
// the dialog to remain active even if 
// it is in the start position
var disableWhenInStartPos : boolean = true;

// This is a private helper variable that
// we set when we wnat the update function
// to disable the game object
private var disableThis : boolean = false;

// The dialog can optionally fade in 
// as it slides in and fade out as it
// slides out.  The fade option is
// controlled by the fade flag
var fade : boolean = true;

// Sometimes we want a dialog to be
// opaque
var maxAlpha : float = 1.0;

// This is an optional skin.  If null
// the default skin will be used.
var _Skin : GUISkin;

// This is an optional depth.  If you
// don't understand what GUI.depth
// is then use the default of -2
var _Depth : int = -2;

// When a dialog is displayed the
// game time scale is adjusted.
// Typically set it to 0.01 to stop
// the game and 1.0 to let the game
// continue running
var dialogTimeScale : float = 1.0;

// This private flag is used so that
// the time is frozen only one time
private var timeIsFrozen : boolean;

// Set two icons for the buttons
// The first icon is displayed
// when the button is not selected
// and the second icon is displayed
// when the button is selected

var okIcon : Texture2D[];
var cancelIcon : Texture2D[];

// Index of which icon texture
// to display
private var gOKIconIndex : int;
private var gCancelIconIndex : int;

// The game object that should be
// activated when this one disables
// itself
var callingDialogGO : GameObject;

// Set any messages to be sent
// when the OK or Cancel 
// buttons are pressed

// An array of messages to be sent
var okMessage : String[];

// An array that defines the kind
// of message to send
var okMessageType : String[];

// An array that defines the delay
// to wait between sending of messages
var okSendDelay : float[];

// An array of game objects that
// specifies the game object
// to which the message will be
// sent
var okMessageGO : GameObject[];

// An array of messages to be sent
var cancelMessage : String[];

// An array that defines the kind
// of message to send
var cancelMessageType : String[];

// An array that defines the delay
// to wait between sending of messages
var cancelSendDelay : float[];

// An array of game objects that
// specifies the game object
// to which the message will be
// sent
var cancelMessageGO : GameObject[];

// The delta time adjusted for the 
// time scale
private var gTime : float;

// Some variables to keep track
// of the messages that need to
// be sent when a button is 
// pressed
private var gMessageIndex : int;
private var message : String[];
private var messageType : String[];
private var sendDelay : float[];
private var messageGO : GameObject[];

// A variable to keep track of
// the time scale to restore
// when a dialog is closed
private var restoreTime : float;

// A flag to make the dialog
// slide in (true) or out (false)
private var slide : boolean;

// this sets our "virtual" screen for the GUI.matrix stuff
private var tMatrix : Matrix4x4;

/* 
	CLASS METHODS
*/

static var GUIs : Hashtable;


static function SetGuiForKey(a_key : String, a_guiGO : GameObject)
{

	if (null == GUIs)
	{
		GUIs = new Hashtable();
	}
	
	if (GUIs.Contains(a_key))
	{
		GUIs.Remove(a_key);
	}
	
	GUIs.Add(a_key, a_guiGO);

}

static function GetGuiForKey(a_key : String) : GameObject
{
	var l_guiGO : GameObject = null;

	if (null == GUIs)
	{
		GUIs = new Hashtable();
	}

	if (GUIs.Contains(a_key))
	{
		l_guiGO = GUIs[a_key];
	}

	return l_guiGO;
}

static function ActivateGUIForKey(a_key : String)
{

	var l_guiGO : GameObject = null;
	var l_guiSlideDialog : GUISlideDialogWrapper = null;

	if (null == GUIs)
	{
		GUIs = new Hashtable();
	}

	if (GUIs.Contains(a_key))
	{
		l_guiGO = GUIs[a_key];
		l_guiSlideDialog = l_guiGO.GetComponent(GUISlideDialogWrapper);
		l_guiGO.SetActive(true);
		l_guiSlideDialog.displaydialog();
	}


}

static function RotateAllGUIs ()
{

	for (var l_GO : GameObject in GUIs.Values)
	{
		Debug.Log(l_GO.name);
			if (true == l_GO.activeSelf)
		{
			l_GO.SendMessage("RotateDevice");
		}
	
	}

}
/*
	INSTANCE METHODS
*/
function displaydialog ()
{

	gTime = 0;
	gOKIconIndex = 0;
	gCancelIconIndex = 0;

	if (false == slide)
	{
		return;
	}
	
	gMessageIndex = 0;
	slide = false;
		
}

function ok ()
{

	// Restore the timescale 
	// to its original value
	if (timeIsFrozen)
	{
		timeIsFrozen = false;
		Time.timeScale = restoreTime;
	}

	slide = !slide;
	gTime = 0;
	
	if (callingDialogGO)
	{
		callingDialogGO.SetActive(true);
		callingDialogGO.GetComponent(GUISlideDialogWrapper).displaydialog();
	}
}

function cancel ()
{
	// Restore the timescale 
	// to its original value
	if (timeIsFrozen)
	{
		timeIsFrozen = false;
		Time.timeScale = restoreTime;
	}

	slide = !slide;
	gTime = 0;

	if (callingDialogGO)
	{
		callingDialogGO.SetActive(true);
		callingDialogGO.GetComponent(GUISlideDialogWrapper).displaydialog();
	}
}


function Awake ()
{

	if (isSingleton)
	{
		var l_name : String;
		var l_object :GameObject;
		var l_objectHash : Hashtable;
		
		if (GUISlideDialogWrapper.GetGuiForKey(gameObject.name))
		{
	   		   	Debug.Log(gameObject.name + " already exists.  Destroying myself.");
		 	  	Destroy(gameObject);
		 	  	return;
		}

		// This is The One
//		DontDestroyOnLoad(this);
//		GameObjectManager.Keep(gameObject, 0);
		
		// This is its content
//		DontDestroyOnLoad(contentGO);
//		GameObjectManager.Keep(contentGO, 0);
						
		GUISlideDialogWrapper.SetGuiForKey(gameObject.name, gameObject);
	}
		
	// Make sure the dialog time scale
	// is not < .0.01 and not > 1.0
	dialogTimeScale = Mathf.Max(0.01, dialogTimeScale);
	dialogTimeScale = Mathf.Min(1.0,  dialogTimeScale);
	
    // Convert the fractional represnetation
    // of the box size to absolute pixels
	boxSizePixels.x = boxSize.x * screenWidth;
	boxSizePixels.y = boxSize.y * screenHeight;

    // Convert the fractional represnetation
    // of the dialog positions to absolute pixels
	startPosPixels.x = startPos.x * screenWidth;
	startPosPixels.y = startPos.y * screenHeight;
	startPosPixels.x -= boxSizePixels.x / 2;
	startPosPixels.y -= boxSizePixels.y / 2;

	endPosPixels.x = endPos.x * screenWidth;
	endPosPixels.y = endPos.y * screenHeight;
	endPosPixels.x -=  boxSizePixels.x / 2;
	endPosPixels.y -=  boxSizePixels.y / 2;
	
	// The very first time we slide the
	// set these to put the dialog in the
	// out position
	gTime = slideTime;
	slide = true;

	// Setup to send the first message first
	gMessageIndex = 0;
	
	// Show the unselected icons for the
	// OK and Cancel buttons
	gOKIconIndex = 0;
	gCancelIconIndex = 0;

 	// The very first time the script 
 	// runs the matrix must be calculated
 	// and since that is done in RotateDevice()
 	// we call it here
	RotateDevice();
	
	if (showOnStart)
    {
        displaydialog();
    }

}

function Start()
{

}

function setSlideOut(a_value : boolean)
{
	slide = a_value;
}

// Since we cannot disable a game object
// from withing OnGUI() (it causes an internal
// error in Unity3D to do so), we do it in
// Update() based on a flag set in OnGUI()
function Update()
{
	
	if (disableThis)
		{
			disableThis = false;
			gameObject.SetActive(false);	
		}

}

// Whenever the dialog is re-enabled
// we check to see if the device
// has been rotated
function OnEnable()
{
	
	RotateDevice();
	
}

// Whenever the device is rotated
// the matrix needs to be
// recalculated
function RotateDevice()
{

 	// Calculate the transformation matrix
 	// for the actual device screen size
	tMatrix = Matrix4x4.TRS(Vector3.zero,
 							Quaternion.identity,
 							Vector3(Screen.width / screenWidth,
 							Screen.height / screenHeight,
 							1.0));
}


// This is the main function
// for our GUI Dialog wrapper
function OnGUI() 
{ 
	
    // If the GUI is waiting to be
    // disabled, do nothing
	if (disableThis)
	{
		return;
	}

	// Set the GUI's skin if a custom
	// skin was specified in the editor
	if (_Skin)
	{
		GUI.skin = _Skin;
	}

	// Set the GUI Depth so that things
	// appear on the desired GUI layer
	GUI.depth = _Depth;

	// Calculate the delta time taking the 
	// time scale into account
	gTime += Time.deltaTime * (1 / Time.timeScale);
	
	// Calculater the lerp time based on
	// the slide time
	var t: float = gTime / slideTime; 
    
    // The corner of the dialog in 
    // screen pixels
	var corner : Vector3;
    
    // Default the alpha to 1 so that the
    // dialog is fully visible
	var l_a : float = 1.0;
	
    // This is the part that does the actual
    // sliding of the dialog
	if (slide)
	{
        // This is slide out
        // Linear interpolate from the END 
        // position to the START posiiton
	 	corner = Vector3.Lerp(endPosPixels, startPosPixels, t);
	 	
        // If fading, then fade out
        // The dialog will fade out as it slides out
	 	if (fade)
	 	{
			l_a = Mathf.Lerp(maxAlpha, 0.0, t*2);
	 	}
        
        // If the dialog auto disables
        // after sliding out, set the 
        // flag so that Update() will 
        // disable the game object
		if (disableWhenInStartPos)
		{
			if (
				(Mathf.FloorToInt(corner.x) ==  Mathf.FloorToInt(startPosPixels.x)) && 
				(Mathf.FloorToInt(corner.y) ==  Mathf.FloorToInt(startPosPixels.y)) &&
				(Mathf.FloorToInt(corner.z) ==  Mathf.FloorToInt(startPosPixels.z))
			   )
			{
				disableThis = true;
				return;
			}
		}
	}
	else
	{
        // This is slide in
        // Linear interpolate from the START 
        // position to the END posiiton
	 	corner = Vector3.Lerp(startPosPixels, endPosPixels, t);
        
        // If fading, then fade in
        // The dialog will fade in as it slides in
	 	if (fade)
	 	{
	 		l_a = Mathf.Lerp(0.0, maxAlpha, t/1.25);
	 	}
		
		if (
			(Mathf.FloorToInt(corner.x) ==  Mathf.FloorToInt(endPosPixels.x)) && 
			(Mathf.FloorToInt(corner.y) ==  Mathf.FloorToInt(endPosPixels.y)) &&
			(Mathf.FloorToInt(corner.z) ==  Mathf.FloorToInt(endPosPixels.z)) &&
			(Mathf.FloorToInt(l_a) == 1.0)
		   )
		{
			// Change the time scale for
			// the dialog
			if (!timeIsFrozen)
			{
				timeIsFrozen = true;
				restoreTime = Time.timeScale;
				Time.timeScale = dialogTimeScale;
			}
		}
	}
	
    // Set the transparency of the dialog
    GUI.color.a = l_a;

    // The pixel position of the 
    // dialog on the screen
    var positionRect : Rect = new Rect(corner.x, corner.y, boxSizePixels.x, boxSizePixels.y);
    
    // The pixel positon of the OK
    // button on the screen
    var okRect : Rect = Rect(boxSizePixels.x - 64, boxSizePixels.y - 64, 48, 48);
    
    // The pixel position of the Cancel
    // button on the screen
    var cancelRect : Rect = Rect(16, boxSizePixels.y - 64, 48, 48);
      
   	// Apply the transformation matrix
 	// for the actual device screen size
	GUI.matrix = tMatrix;
  
  	// Position the dialog
	GUI.BeginGroup(positionRect);
	
	// If there is an active game object
	// to display content send it the 
	// message to do so now
	if (true == contentGO.activeSelf)
	{
		contentGO.SendMessage (contentRenderMessage,
                               boxSizePixels,
                               SendMessageOptions.RequireReceiver);
	}
		
		// Draw the OK button
		// If two textures are provided the 
		// button will be drawn
		if (2 == okIcon.length)
		{
			if (GUI.Button(okRect, okIcon[gOKIconIndex]))
			{
				gMessageIndex = 0;
				gOKIconIndex = 1;
				message = okMessage;
				messageType = okMessageType;
				sendDelay = okSendDelay;
				messageGO = okMessageGO;
				Invoke("sendTheMessages", sendDelay[gMessageIndex]);
			}
		}
		
		// Draw the Cancel button
		// If two textures are provided the
		// button will be drawn
		if (2 == cancelIcon.length)
		{
			if (GUI.Button(cancelRect, cancelIcon[gCancelIconIndex]))
			{
				gMessageIndex = 0;
				gCancelIconIndex = 1;
				message = cancelMessage;
				messageType = cancelMessageType;
				sendDelay = cancelSendDelay;
				messageGO = cancelMessageGO;
				Invoke("sendTheMessages", sendDelay[gMessageIndex]);
			}
		}
		
	GUI.EndGroup();
	
}

// This function sends all messages
// that need to be sent when
// the OK or Cancel button was
// pressed
function sendTheMessages()
{

	if ("SendMessage" == messageType[gMessageIndex])
	{
		if (null == messageGO[gMessageIndex])
		{
			SendMessage(message[gMessageIndex]);
		}
		else
		{
			messageGO[gMessageIndex].SendMessage(message[gMessageIndex]);
		}
	}
	else 	if ("SendMessageUpwards" == messageType[gMessageIndex])
	{
		if (null == messageGO[gMessageIndex])
		{
			SendMessageUpwards(message[gMessageIndex]);
		}
		else
		{
			messageGO[gMessageIndex].SendMessageUpwards(message[gMessageIndex]);
		}
	}
	else if ("BroadcastMessage" == messageType[gMessageIndex])
	{
		if (null == messageGO[gMessageIndex])
		{
		BroadcastMessage(message[gMessageIndex]);
		}
		else
		{
			messageGO[gMessageIndex].BroadcastMessage(message[gMessageIndex]);
		}
	}
	
	gMessageIndex++;
	
	// Invoke the next message if there is one
	if (gMessageIndex < message.length)
	{
		Invoke("sendTheMessages", sendDelay[gMessageIndex]);
	}
	else
	{
		gMessageIndex = 0;
	}
		
}




