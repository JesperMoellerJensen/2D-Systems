using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBodyRotation : MonoBehaviour {

	public float Smoothness = 10f;

	Quaternion startRot;
	public int PlayerMoveDir;
	public float runningAngleMultiply = 5f;

	private void Awake()
	{
		startRot = transform.rotation;
	}

	void LateUpdate()
	{
		Quaternion offset = Quaternion.Euler(0, 0, -PlayerMoveDir*runningAngleMultiply);
		Quaternion actual = transform.parent.rotation;
		startRot = Quaternion.Lerp(startRot, actual * offset, Time.deltaTime * Smoothness);

		transform.rotation = startRot;

		Debug.Log(startRot + " " + actual);
	}}
