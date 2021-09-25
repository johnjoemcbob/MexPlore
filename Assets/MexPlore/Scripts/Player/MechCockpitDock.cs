using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechCockpitDock : MonoBehaviour
{
	public static MechCockpitDock CurrentInUse;

	public float DockCooldown = 0.25f;

	private HeliCockpit Cockpit;
	private float CurrentDockCooldown = 0;

	private void Update()
	{
		if ( Input.GetButton( "Fire2" ) )
		{
			UnDock();
		}
	}

	private void OnTriggerEnter( Collider other )
	{
		var cockpit = other.GetComponentInParent<HeliCockpit>();
		if ( cockpit != null && cockpit.enabled )
		{
			if ( CurrentDockCooldown < Time.time )
			{
				Dock( cockpit );
			}
			CurrentDockCooldown = Time.time + DockCooldown;
		}
	}

	private void OnTriggerExit( Collider other )
	{
		// Give a little extra buffer time to escape in case of bumping
		var cockpit = other.GetComponentInParent<HeliCockpit>();
		if ( cockpit != null )
		{
			CurrentDockCooldown = Time.time + DockCooldown;
		}
	}

	public void Dock( HeliCockpit cockpit )
	{
		Cockpit = cockpit;

		// Stop the heli + dock
		Cockpit.enabled = false;
		Cockpit.GetComponent<Rigidbody>().isKinematic = true;
		Cockpit.transform.SetParent( transform );
		Cockpit.transform.localPosition = Vector3.zero;
		Cockpit.transform.localEulerAngles = Vector3.zero;

		// Remove rigidbody
		Destroy( Cockpit.GetComponent<Rigidbody>() );

		// Visuals
		GetComponent<MeshRenderer>().enabled = false;
		// TODO fold the heli blades

		// Enable mech controls
		GetComponentInParent<MechBody>().IsMainController = true;
		foreach ( var controller in GetComponentInParent<MechBody>().GetComponentsInChildren<BaseController>() )
		{
			controller.IsMainController = true;
		}
	}

	public void UnDock()
	{
		if ( Cockpit != null )
		{
			// Undock + start the heli
			Cockpit.transform.SetParent( null );
			Cockpit.enabled = true;
			CurrentDockCooldown = Time.time + DockCooldown;

			// Add rigidbody
			Cockpit.AddRigidbody();

			// Visuals
			GetComponent<MeshRenderer>().enabled = true;
			// TODO unfold the heli blades

			// Disable mech controls
			GetComponentInParent<MechBody>().IsMainController = false;
			foreach ( var controller in GetComponentInParent<MechBody>().GetComponentsInChildren<BaseController>() )
			{
				controller.IsMainController = false;
			}

			Cockpit = null;
		}
	}
}
