using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGUI : MonoBehaviour {

	public SeniPhysicsController controller;

	void OnGUI()
	{
		// Make a background box
		GUI.Box(new Rect(10, 10, 130, 120), "Debug");

		GUI.TextField(new Rect(20, 40, 110, 20),  "Grounded	: " + controller._isGrounded.ToString());

		GUI.TextField(new Rect(20, 70, 110, 20),  "Sliding	: " + controller._isSliding.ToString());
	
		GUI.TextField(new Rect(20, 100, 110, 20), "Blocked	: " + controller._MovementBlocked.ToString());
	}
}
