using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBodyRotation : MonoBehaviour {

	public float Smoothness = 10f;

	Quaternion _startRot;
	public int PlayerMoveDir;
	public float RunningAngleMultiply = 5f;

	private void Awake()
	{
		_startRot = transform.rotation;
	}

	void LateUpdate()
	{
		Quaternion offset = Quaternion.Euler(0, 0, -PlayerMoveDir*RunningAngleMultiply);
		Quaternion actual = transform.parent.rotation;
		_startRot = Quaternion.Lerp(_startRot, actual * offset, Time.deltaTime * Smoothness);

		transform.rotation = _startRot;
	}}
