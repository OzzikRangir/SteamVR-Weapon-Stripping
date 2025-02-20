//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;
using System.Collections;
using Valve.VR;
using cakeslice;
using System.Collections.Generic;
using System.Linq;

public class SteamVR_LeftController : MonoBehaviour
{
	public Transform weapon;

	private SteamVR_TrackedObject trackedObj;
	private SteamVR_Controller.Device Controller{
		get { return SteamVR_Controller.Input ((int)trackedObj.index); }
	}


    Transform previousContact = null;



	// Use this for initialization
	void Start ()
	{
		trackedObj = GetComponent<SteamVR_TrackedObject> ();
       
	}
	void Update ()
    {
		if (Controller.GetTouch(EVRButtonId.k_EButton_SteamVR_Touchpad))
		{
			Vector3 position = weapon.transform.position;
			var axis = Controller.GetAxis();

			weapon.Rotate(Vector3.up, axis.x);
			position.y += axis.y*0.001f;


			weapon.transform.position = position;


			}

		}
		
}
