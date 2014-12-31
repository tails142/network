using UnityEngine;
using System.Collections;

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Globalization;
using System.Reflection;


public class BTSTinySerializer : MonoBehaviour 
{
	static public bool LoadPlistFromString(string txt, Hashtable plist) 
	{
		
   	 	// Unless plist has already been initiated, it can't be passed by reference, which it has to be
    	if (null == plist) 
		{ 
			Debug.LogError("Cannot pass null plist value by reference to LoadPlistFromFile."); 
			return false; 
		}
   
    	// Load the string into an XML data object
    	var xml = new XmlDocument();
    	xml.LoadXml(txt);
    
    	// Find the root plist object.  If it doesn't exist or isn't a plist node, state so and return null.
    	var plistNode = xml.LastChild;
    	if (plistNode.Name != "tiny") 
		{ 
			Debug.LogError("Invalid plist data in string"); 
			return false; 
		}
    
    	// Get the root plist dict object.  This is the root object that contains all the data in the plist file.  This object will be the hashtable.
    	var dictNode = plistNode.FirstChild;
    	if (dictNode.Name != "D") 
		{ 
			Debug.LogError("Missing root dict from plist string"); 
			return false; 
		}
    
    	// Using the root dict node, load the plist into a hashtable and return the result.
    	// If successful, this will return true, and the plist object will be populated with all the appropriate information.
    	return LoadDictFromPlistNode((XmlNode) dictNode, ref plist);
	}
	

	private static bool LoadDictFromPlistNode(XmlNode node, ref Hashtable dict) 
	{
		if (node == null) 
		{
			Debug.LogError("Attempted to load a null plist dict node.");
			return false;
		}
		
		if (!node.Name.Equals("D")) 
		{
			Debug.LogError("Attempted to load an dict from a non-array node type: " + node + ", " + node.Name);
			return false;
		}
		
		if (dict == null) 
		{
			dict = new Hashtable();
		}
 
		int cnodeCount = node.ChildNodes.Count;
		for (int i = 0; i+1 < cnodeCount; i+=2) 
		{
			// Select the key and value child nodes
			XmlNode keynode = node.ChildNodes.Item(i);
			XmlNode valuenode = node.ChildNodes.Item(i+1);
 
			// If this node isn't a 'key'
			if (keynode.Name.Equals("K")) 
			{
				// Establish our variables to hold the key and value.
				string key = keynode.InnerText;
				ValueObject value = new ValueObject();
 
				// Load the value node.
				// If the value node loaded successfully, add the key/value pair to the dict hashtable.
				if (LoadValueFromPlistNode(valuenode, ref value)) {
					// This could be one of several different possible data types, including another dict.
					// AddKeyValueToDict() handles this by replacing existing key values that overlap, and doing so recursively for dict values.
					// If this not successful, post a message stating so and return false.
					if (!AddKeyValueToDict(ref dict, key, value)) 
					{
						Debug.LogError("Failed to add key value to dict when loading plist from dict");
						return false;
					}
				} 
				else 
				{
					Debug.LogError("Did not load plist value correctly for key in node: " + key + ", " + node);
					return false;
				}
			} 
			else 
			{
				Debug.LogError("The plist being loaded may be corrupt.");
				return false;
			}
 
		} //end for
 
		return true;
	}

	private static bool AddKeyValueToDict(ref Hashtable dict, string key, ValueObject value) 
	{
		// Make sure that we have values that we can work with.
		if (dict == null || key == null || key.Length < 1 || value == null) 
		{
			Debug.LogError("Attempted to AddKeyValueToDict() with null objects.");
			return false;
		}
		
		// If the hashtable doesn't already contain the key, they we can just go ahead and add it.
		if (!dict.ContainsKey(key)) 
		{
			dict.Add(key, value.val);
			return true;
		}
		
		// At this point, the dict contains already contains the key we're trying to add.
		// If the value for this key is of a different type between the dict and the new value, then we have a type mismatch.
		// Post an error stating so, but go ahead and overwrite the existing key value.
		if (value.val.GetType() != dict[key].GetType()) 
		{
			Debug.LogWarning("Value type mismatch for overlapping key (will replace old value with new one): " + value.val + ", " + dict[key] + ", " + key);
			dict[key] = value.val;
		}
		// If the value for this key is a hashtable, then we need to recursively add the key values of each hashtable.
		else if (value.val.GetType() == typeof(Hashtable)) 
		{
			// Itterate through the elements of the value's hashtable.
			Hashtable htTmp = (Hashtable)value.val;
			foreach (object element in htTmp) {
				// Recursively attempt to add/repalce the elements of the value hashtable to the dict's value hashtable.
				// If this fails, post a message stating so and return false.
				Hashtable htRef = (Hashtable)dict[key];
				if (!AddKeyValueToDict(ref htRef, (string)element, new ValueObject(htTmp[element]))) 
				{
					Debug.LogError("Failed to add key value to dict: " + element + ", " + htTmp[element] + ", " + dict[key]);
					return false;
				}
			}
		}
		// If the value is an array, then there's really no way we can tell which elements to overwrite, because this is done based on the congruent keys.
		// Thus, we'll just add the elements of the array to the existing array.
		else if (value.val.GetType() == typeof(ArrayList)) 
		{
			ArrayList alTmp = (ArrayList)value.val;
			ArrayList alAddTmp = (ArrayList)dict[key];
			foreach (object element in alTmp) 
			{
				alAddTmp.Add(element);
			}
		}
		// If the key value is not an array or a hashtable, then it's a primitive value that we can easily write over.
		else 
		{
			dict[key] = value.val;
		}
 
		return true;
	}

	private static bool LoadValueFromPlistNode(XmlNode node, ref ValueObject value) 
	{
		if (node == null) 
		{
			Debug.LogError("Attempted to load a null plist value node.");
			return false;
		}
		
		if (node.Name.Equals("S")) 
		{ 
			value.val = node.InnerText; 
		}
		else if (node.Name.Equals("I")) 
		{ 
			value.val = int.Parse(node.InnerText); 
		}
		else if (node.Name.Equals("R")) 
		{ 
			value.val = float.Parse(node.InnerText); 
		}
		else if (node.Name.Equals("date")) 
		{ 
			value.val = DateTime.Parse(node.InnerText, null, DateTimeStyles.None); 
		} // Date objects are in ISO 8601 format
		else if (node.Name.Equals("data")) 
		{ 
			value.val = node.InnerText; 
		} // Data objects are just loaded as a string
		else if (node.Name.Equals("true")) 
		{ 
			value.val = true; 
		} // Boollean values are empty objects, simply identified with a name being "true" or "false"
		else if (node.Name.Equals("false")) 
		{ 
			value.val = false; 
		}
		// The value can be an array or dict type.  In this case, we need to recursively call the appropriate loader functions for dict and arrays.
		// These functions will in turn return a boolean value for their success, so we can just return that.
		// The val value also has to be instantiated, since it's being passed by reference.
		else if (node.Name.Equals("dict")) 
		{
			value.val = new Hashtable();
			Hashtable htRef = (Hashtable)value.val;
			return LoadDictFromPlistNode(node, ref htRef);
		}
		else if (node.Name.Equals("array")) 
		{
			value.val = new ArrayList();
			ArrayList alRef = (ArrayList)value.val;
			return LoadArrayFromPlistNode(node, ref alRef);
		} 
		else 
		{
			Debug.LogError("Attempted to load a value from a non value type node: " + node + ", " + node.Name);
			return false;
		}
 
		return true;
	}
 
	private static bool LoadArrayFromPlistNode(XmlNode node, ref ArrayList array ) 
	{
		// If we were passed a null node object, then post an error stating so and return false
		if (node == null) 
		{
			Debug.LogError("Attempted to load a null plist array node.");
			return false;
		}
		
		// If we were passed a non array node, then post an error stating so and return false
		if (!node.Name.Equals("array")) 
		{
			Debug.LogError("Attempted to load an array from a non-array node type: " + node + ", " + node.Name);
			return false;
		}
 
		// We can be passed an empty array object.  If so, initialize it
		if (array == null) 
		{ 
			array = new ArrayList(); 
		}
 
		// Itterate through the child nodes for this array object
		int nodeCount = node.ChildNodes.Count;
		for (int i = 0; i < nodeCount; i++) 
		{
			// Establish variables to hold the child node of the array, and it's value
			XmlNode cnode = node.ChildNodes.Item(i);
			ValueObject element = new ValueObject();
			// Attempt to load the value from the current array node.
			// If successful, add it as an element of the array.  If not, post and error stating so and return false.
			if (LoadValueFromPlistNode(cnode, ref element)) 
			{
				array.Add(element.val);
			} else 
			{
				return false;
			}
		}
 
		// If we made it through the array without errors, return true
		return true;
	}
	
	public static string PlistToString (Hashtable plist ) 
	{
		
		// If the hashtable is null, then there's apparently an issue; fail out.
    	if (null == plist) 
		{ 
			Debug.LogError("Passed a null plist hashtable to SavePlistToFile."); 
			return(null); 
		}
    
    	// Create the base xml document that we will use to write the data
    	var xml = new XmlDocument();
    
   	 	var plistNode = xml.CreateNode(XmlNodeType.Element, "tiny", null);
    	xml.AppendChild(plistNode);
   
    	// Now that we've created the base for the XML file, we can add all of our information to it.
    	// Pass the plist data and the root dict node to SaveDictToPlistNode, which will write the plist data to the dict node.
    	// This function will itterate through the hashtable hierarchy and call itself recursively for child hashtables.
    	if (!SaveDictToPlistNode(plistNode, plist)) 
		{
        	// If for some reason we failed, post an error and return false.
        	Debug.LogError("Failed to save plist data to root dict node: " + plist);
        	return (null);
    	}
		
		// We were successful
		return (xml.OuterXml); 
	}
	
	private static bool SaveDictToPlistNode(XmlNode node, Hashtable dict)
	{
		
   	 	// If we were passed a null object, return false
    	if (null == node) 
		{ 
			Debug.LogError("Attempted to save a null plist dict node.");
			return false; 
		}
    
    	var dictNode = node.OwnerDocument.CreateNode(XmlNodeType.Element, "D", null);
    	node.AppendChild(dictNode);
    
    	// We could be passed an null hashtable.  This isn't necessarily an error.
    	if (null == dict) 
		{ 
			Debug.LogWarning("Attemped to save a null dict: " + dict); 
			return true; 
		}
    
		// Iterate through the keys in the hashtable
	    //for (var key in dict.Keys) {
		foreach (object key in dict.Keys) 
		{
	        // Since plists are key value pairs, save the key to the plist as a new XML element
	        XmlElement keyNode = node.OwnerDocument.CreateElement("K");
	        keyNode.InnerText = (string)key;
	        dictNode.AppendChild(keyNode);
 
	        // The name of the value element is based on the datatype of the value.  We need to serialize it accordingly.  Pass the XML node and the hash value to SaveValueToPlistNode to handle this.
	        if (!SaveValueToPlistNode(dictNode, dict[key])) {
	            // If SaveValueToPlistNode() returns false, that means there was an error.  Return false to indicate this up the line.
	            Debug.LogError("Failed to save value to plist node: " + key);
	            return false;
	        }
	    }
    
    	// If we got this far then all is well.  Return true to indicate success.
    	return true;
		
	}
	
	private static bool SaveValueToPlistNode(XmlNode node, object value) {
	    // The node passed will be the parent node to the new value node.
	    XmlNode valNode;
	    System.Type type = value.GetType();
	    // Identify the data type for the value and serialize it accordingly
	    if (type == typeof(String)) 
		{ 
			valNode = node.OwnerDocument.CreateElement("S"); 
		}
	    else if (type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64)) 
		{ 
			valNode = node.OwnerDocument.CreateElement("I"); 
		}
	    else if (type == typeof(Single) ||  type == typeof(Double) || type == typeof(Decimal)) 
		{ 
//			valNode = node.OwnerDocument.CreateElement("R"); 
    		// For compactness, floating point values are limited to 3 decimal place
    		// This should be enough for accurate locations in Unity3D
    		double l_double  = Convert.ToDouble(value);
    		l_double = Mathf.Floor((float)l_double * 1000) / 1000;
			valNode = node.OwnerDocument.CreateElement("R"); 
     		valNode.InnerText = "" + l_double; // Convert to string
    		node.AppendChild(valNode);
        	return true;
		}
	    else if (type == typeof(DateTime)) 
		{
	        // Dates need to be stored in ISO 8601 format
	        valNode = node.OwnerDocument.CreateElement("date");
			DateTime dt = (DateTime)value;
	        valNode.InnerText = dt.ToUniversalTime().ToString("o");
	        node.AppendChild(valNode);
	        return true;
	    }
	    else if (type == typeof(bool)) {
	        // Boolean values are empty elements, simply being stored as an elemement with a name of true or false
	        if ((bool)value == true) { valNode = node.OwnerDocument.CreateElement("true"); }
	        else { valNode = node.OwnerDocument.CreateElement("false"); }
	        node.AppendChild(valNode);
	        return true;
	    }
	    // Hashtables and arrays require special functions to save their values in an itterative and recursive manner.
	    // The functions will return true/false to indicate success/failure, so pass those on.
	    else if (type == typeof(Hashtable))    { 
			return SaveDictToPlistNode(node, (Hashtable)value); 
		}
	    else if (type == typeof(ArrayList)) { return SaveArrayToPlistNode(node, (ArrayList)value); }
	    // Anything that doesn't fit the defined data types will just be stored as "data", which is effectively a string.
	    else { 
			valNode = node.OwnerDocument.CreateElement("data");
		}
 
	    // Some of the values (strings, numbers, data) basically get stored as a string.  The rest will store their values in their special format and return true for success.  If we made it this far, then the value in valNode must be stored as a string.
	    if (valNode != null) valNode.InnerText = value.ToString();
	    node.AppendChild(valNode);
 
	    // We're done.  Return true for success.
	    return true;
	}
 
	private static bool SaveArrayToPlistNode (XmlNode node, ArrayList array) {
	    // Create the value node as an "array" element.
	    XmlElement arrayNode = node.OwnerDocument.CreateElement("array");
	    node.AppendChild(arrayNode);
 
	    // Each element in the array can be any data type.  Itterate through the array and send each element to SaveValueToPlistNode(), where it can be stored accordingly based on its data type.
	    foreach (object element in array) {
	        // If SaveValueToPlistNode() returns false, then there was a problem.  Return false in that case.
	        if (!SaveValueToPlistNode(arrayNode, element)) { return false; }
	    }
    	return true;
	}
}
