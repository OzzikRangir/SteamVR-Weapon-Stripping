//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;
using System.Collections;
using Valve.VR;
using cakeslice;
using System.Collections.Generic;
using System.Linq;

public struct PointerEventArgs
{
    public uint controllerIndex;
    public uint flags;
    public float distance;
    public Transform target;
}

public delegate void PointerEventHandler(object sender, PointerEventArgs e);


public class SteamVR_LaserPointer : MonoBehaviour
{
    public bool active = true;
    public Color color;
    public float thickness = 0.001f;
    public GameObject holder;
    public GameObject pointer;
    bool isActive = false;
    public bool addRigidBody = false;
    public Transform reference;
    public event PointerEventHandler PointerIn;
    public event PointerEventHandler PointerOut;
	public GameObject text;
	public Transform weapon;
	private VRControllerState_t controllerState;
	private List<GameObject> hidden = new List<GameObject> ();

	private SteamVR_TrackedObject trackedObj;
	private SteamVR_Controller.Device Controller{
		get { return SteamVR_Controller.Input ((int)trackedObj.index); }
	}


    Transform previousContact = null;

	EVRButtonId[] axisIds = new EVRButtonId[] {
		EVRButtonId.k_EButton_SteamVR_Touchpad,
		EVRButtonId.k_EButton_SteamVR_Trigger
	};

	// Use this for initialization
	void Start ()
    {
		trackedObj = GetComponent<SteamVR_TrackedObject> ();
        holder = new GameObject();
        holder.transform.parent = this.transform;
        holder.transform.localPosition = Vector3.zero;
		holder.transform.localRotation = Quaternion.Euler (70, 0, 0);

		pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointer.transform.parent = holder.transform;
        pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
        pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
		pointer.transform.localRotation = Quaternion.identity;
		BoxCollider collider = pointer.GetComponent<BoxCollider>();
        if (addRigidBody)
        {
            if (collider)
            {
                collider.isTrigger = true;
            }
            Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
        }
        else
        {
            if(collider)
            {
                Object.Destroy(collider);
            }
        }
        Material newMaterial = new Material(Shader.Find("Unlit/Color"));
        newMaterial.SetColor("_Color", color);
        pointer.GetComponent<MeshRenderer>().material = newMaterial;
	}

    public virtual void OnPointerIn(PointerEventArgs e)
    {
        if (PointerIn != null)
            PointerIn(this, e);
    }

    public virtual void OnPointerOut(PointerEventArgs e)
    {
        if (PointerOut != null)
            PointerOut(this, e);
    }


    // Update is called once per frame
	void Update ()
    {
		
		if (Controller.GetPressDown(EVRButtonId.k_EButton_Grip)) {
			GameObject item = (GameObject) hidden.Last();
			item.SetActive (true);
			hidden.Remove (item);

		}



        if (!isActive)
        {
            isActive = true;
            this.transform.GetChild(0).gameObject.SetActive(true);
        }

        float dist = 100f;

        SteamVR_TrackedController controller = GetComponent<SteamVR_TrackedController>();


			if (Controller.GetTouch(EVRButtonId.k_EButton_SteamVR_Touchpad))
			{
			Vector3 position = weapon.transform.position;
			var axis = Controller.GetAxis();
			position.x -= axis.x*0.001f;
			position.z -= axis.y*0.001f;
				

			weapon.transform.position = position;
			}

		Quaternion angle = Quaternion.AngleAxis (30, Vector3.up);
		Ray raycast = new Ray(transform.position,holder.transform.forward);
        RaycastHit hit;
        bool bHit = Physics.Raycast(raycast, out hit);

        if(previousContact && previousContact != hit.transform)
        {
            PointerEventArgs args = new PointerEventArgs();
            if (controller != null)
            {
                args.controllerIndex = controller.controllerIndex;
            }
            args.distance = 0f;
            args.flags = 0;
            args.target = previousContact;
            OnPointerOut(args);
			previousContact.gameObject.GetComponent<Outline>().enabled = false;
            previousContact = null;
        }
        if(bHit && previousContact != hit.transform)
        {
			if(previousContact)
				previousContact.gameObject.GetComponent<Outline>().enabled = false;
			//
            PointerEventArgs argsIn = new PointerEventArgs();
            if (controller != null)
            {
                argsIn.controllerIndex = controller.controllerIndex;
            }
            argsIn.distance = hit.distance;
            argsIn.flags = 0;
            argsIn.target = hit.transform;
            OnPointerIn(argsIn);
			previousContact = hit.transform;
			hit.collider.gameObject.GetComponent<Outline> ().enabled = true;
			text.GetComponent<TextMesh> ().text = hit.collider.gameObject.name;
			if (Controller.GetHairTriggerDown ()) {
				hidden.Add (hit.collider.gameObject);
				hit.collider.gameObject.SetActive(false);
			}
			//Debug.Log (hit.collider.gameObject.tag);
        }
        if(!bHit)
        {
			if(previousContact)
				previousContact.gameObject.GetComponent<Outline>().enabled = false;
			//GetComponent<Outline>().enabled = !GetComponent<Outline>().enabled;

			text.GetComponent<TextMesh> ().text = null;
            previousContact = null;
        }
        if (bHit && hit.distance < 100f)
        {
            dist = hit.distance;
        }

        if (controller != null && controller.triggerPressed)
        {
            pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, dist);
        }
        else
        {
            pointer.transform.localScale = new Vector3(thickness, thickness, dist);
			if (Controller.GetHairTriggerDown ()) {
				hidden.Add (hit.collider.gameObject);
				hit.collider.gameObject.SetActive(false);
			}
        }
        pointer.transform.localPosition = new Vector3(0f, 0f, dist/2f);
    }
}
