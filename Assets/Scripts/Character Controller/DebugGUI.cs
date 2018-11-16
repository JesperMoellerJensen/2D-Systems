using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGUI : MonoBehaviour {

	public SeniPhysicsController controller;

	void OnGUI()
	{
		// Make a background box
		GUI.Box(new Rect(10, 10, 130, 200), "Debug");

		GUI.TextField(new Rect(20, 40, 110, 20),  "Grounded	: " + controller.Debug_Ground.ToString());

		GUI.TextField(new Rect(20, 70, 110, 20),  "Sliding	: " + controller.Debug_Sliding.ToString());
	
		GUI.TextField(new Rect(20, 100, 110, 20), "Blocked	: " + controller.Debug_Blocked.ToString());

		GUI.TextField(new Rect(20, 130, 110, 20), "Slope	: " + controller.Debug_Slope.ToString());

		GUI.TextField(new Rect(20, 160, 110, 20), "Velocity	: " + controller.Debug_Velocity.ToString());

	}
}
