using UnityEngine;
using System.IO;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class BTSPreferences : MonoBehaviour 
{
		
	private static string g_preferencesFileName = "com.burningthumb.zombieminigolf.plist";
	
	private static string preferencesFolderPath = "Library" + Path.DirectorySeparatorChar + "Preferences";


	/* 
	 * The preferences are stored on disk but cached in a Hashtable
	 */
	
	private static Hashtable g_preferencesHashtable = null;
		
	/* Some things, like sending events, need to be done on an instance
	 * of an object.  The single instance reference to the BTSPreferences object is
	 * kept here.
	 */
	
	private static BTSPreferences g_sharedInstance = null;
	
	private static string plistFile
	{
		get
		{
    		// The full path to the plist file
			return Path.Combine(BTSPreferences.PathToSpecialFolder(), g_preferencesFileName);
		}
	}
	
	/**
	 * The shared instance property
	 *
	 * This method returns the shared instance if it exists.  If it
	 * does not exist then a new gameObject is created and the 
	 * component is added to it.  This new gameObject is assigned to 
	 * the sharedInstance and flagged as DontDestroyOnLoad.
	 */

	public static BTSPreferences sharedInstance
    {
        get 
		{ 
			if (null == g_sharedInstance)
			{
				string l_className = typeof(BTSPreferences).Name;
				GameObject l_go = new GameObject(l_className +  " (" + Time.time + ")");
				g_sharedInstance = l_go.AddComponent(l_className) as BTSPreferences;
				
				// Parent this object into the BTSGroup
				l_go.transform.parent = BTSManager.sharedInstance.transform;

				DontDestroyOnLoad(g_sharedInstance);
			}

			return g_sharedInstance;
		}
    }
	
	
	

		
	/** The hashTable that caches the preferences
	 * 
	 * The hashTable is only accessable by the BTSPreferences
	 * class which provides accessor methods for other
	 * classes
	 * 
	 */

	private static Hashtable preferencesHashtable
	{
		get 
		{ 
			if (null == g_preferencesHashtable)
			{
				LoadPlayerPreferences();
			}
			return g_preferencesHashtable; 
		}
		
		set { g_preferencesHashtable = value; }
	}
		

	/** HasKey (a_key : string)
	 * 
	 * This method returns true if the key exists and 
	 * false if the key does not exist
	 * 
	 */
	
	public static bool HasKey(string a_key)
	{
		return (BTSPreferences.preferencesHashtable.Contains(a_key));
		
	}

	/** string GetStringForKey(string a_key)
	 * 
	 * This method either returns an empty string, if the string does not
	 * exist, or it returns the string
	 * 
	 */
	
	public static string GetStringForKey(string a_key)
	{
		var l_result = "";
		
		try
		{
			if (BTSPreferences.preferencesHashtable.Contains(a_key))
			{
				l_result = ("" + BTSPreferences.preferencesHashtable[a_key]);
			}

		}
		catch (System.Exception ex)
		{
			Debug.Log("GetStringForKey (" + a_key + "):" + ex);
			l_result = "";
		}
		
		return l_result;
		
	}
	
	/** void SetStringForKey(string a_string, string a_key)
	 * 
	 * This method sets the string for the key
	 * 
	 */
	
	public static void SetStringForKey(string a_string, string a_key)
	{		
		try
		{
			if (BTSPreferences.preferencesHashtable.Contains(a_key))
			{
				BTSPreferences.preferencesHashtable[a_key] = a_string;
			}
			else
			{
				BTSPreferences.preferencesHashtable.Add(a_key, a_string);
			}

		}
		catch (System.Exception ex)
		{
			Debug.Log("SetStringForKey (" + a_key + " /" + a_string + "):" + ex);
		}
				
	}

	/** void SetBoolForKey(bool a_bool, string a_key)
	 * 
	 * This method sets the bool for the key
	 * 
	 */
	
	public static void SetBoolForKey(bool a_bool, string a_key)
	{		
		try
		{
			if (BTSPreferences.preferencesHashtable.Contains(a_key))
			{
				BTSPreferences.preferencesHashtable[a_key] = a_bool;
			}
			else
			{
				BTSPreferences.preferencesHashtable.Add(a_key, a_bool);
			}

		}
		catch (System.Exception ex)
		{
			Debug.Log("SetBoolForKey (" + a_key + " /" + a_bool + "):" + ex);
		}
				
	}


	/** bool GetBoolForKey(string a_key)
	 * 
	 * This method sets the bool for the key
	 * 
	 */
	
	public static bool GetBoolForKey(string a_key)
	{		
		bool l_result = false;
		
		try
		{
			if (BTSPreferences.preferencesHashtable.Contains(a_key))
			{
				l_result = System.Convert.ToBoolean(BTSPreferences.preferencesHashtable[a_key]);
			}

		}
		catch (System.Exception ex)
		{
			Debug.Log("GetBoolForKey (" + a_key + "):" + ex);
			l_result = false;
		}
		
		return l_result;
		
	}


	public static void Synchronize()
	{
		BTSLocalMessenger.SendNotification(BTSNotifications.PreferencesDidChange);
		
		// Make sure the path to the plist file exits
		FileInfo l_fi = new FileInfo(BTSPreferences.plistFile);
  		if (!l_fi.Directory.Exists) 
  		{ 
    		System.IO.Directory.CreateDirectory(l_fi.DirectoryName); 
  		} 

		// Write the hashtable out to the file
		PListManager.SavePlistToFile(BTSPreferences.plistFile, BTSPreferences.preferencesHashtable);
	}
	
	
	// This functoon will load the
	// player preferences and if none exist
	// it will return an empty hashTable

	private static void LoadPlayerPreferences()
	{
		Hashtable l_plist = new Hashtable();
		
		if (File.Exists(BTSPreferences.plistFile))
		{
        	// If the game options file
        	// exists, load it into the new
        	// hashtable
			PListManager.ParsePListFile(BTSPreferences.plistFile, ref l_plist);
		}
		
		BTSPreferences.preferencesHashtable = l_plist;
	}

	
	/** PathToSpecialFolder ()
	 * 
	 * This used to be in Globals but really it seems it belongs in the 
	 * BTSPreferences class so that is where it has been moved.
	 * 
	 * This method returns the path to the location where preferences files
	 * should be accessed in a platform indepentant manner for Windows, Mac OS, 
	 * and iOS.
	 * 
	 * In fact if you are on some other platform, the result for iOS will be 
	 * returned ... you have been warned.
	 * 
	 */
	
	public static string PathToSpecialFolder ()
	{	
   	 	// The string to return
		string l_path; 
    
    	// On Windows return the 
    	// LocalApplicationData folder
   		if (Application.platform == 
 			 RuntimePlatform.WindowsPlayer ||
          Application.platform == 
 			 RuntimePlatform.WindowsEditor) 
   		{ 
 			l_path = System.Environment.GetFolderPath(
 			System.Environment.SpecialFolder.LocalApplicationData); 
   		}
		
   		// On iOS and Mac OS X return the
   		// ~/Library/Preferences folder
   		else 
   		{ 
         	l_path = Path.Combine(System.Environment.GetFolderPath(
         		System.Environment.SpecialFolder.Personal), 
 				preferencesFolderPath); 
   		} 

   		return l_path;
   
	}


	
}


