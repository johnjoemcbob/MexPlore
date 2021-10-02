using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechCockpitDock : MonoBehaviour
{
	public static MechCockpitDock CurrentInUse;

	public bool CanDock = true;
	public bool CanUnDock = true;
	public float DockCooldown = 0.25f;

	private HeliCockpit Cockpit;
	private float CurrentDockCooldown = 0;

	private void Update()
	{
		if ( Input.GetButton( MexPlore.GetControl( MexPlore.CONTROL.BUTTON_HELI_UNDOCK ) ) )
		{
			if ( Cockpit != null && LocalPlayer.Instance.Player == Cockpit.GetComponent<Player>() )
			{
				if ( CanUnDock )
				{
					UnDock();
				}
			}
		}
	}

	private void OnTriggerEnter( Collider other )
	{
		var cockpit = other.GetComponentInParent<HeliCockpit>();
		if ( cockpit != null && cockpit.enabled && CanDock && LocalPlayer.Instance.Player == cockpit.GetComponent<Player>() )
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
		CanDock = false;
		Cockpit.enabled = false;
		if ( Cockpit.GetComponent<Rigidbody>() != null )
		{
			Cockpit.GetComponent<Rigidbody>().isKinematic = true;
		}
		Cockpit.transform.SetParent( transform );
		Cockpit.transform.localPosition = Vector3.zero;
		Cockpit.transform.localEulerAngles = Vector3.zero;

		// Callbacks
		Cockpit.GetComponent<HeliCockpit>().OnDock();
		GetComponentInParent<MechBody>().OnDock();
		foreach ( var engine in GetComponentInParent<MechBody>().GetComponentsInChildren<Engine>() )
		{
			engine.OnDock();
		}

		// Network
		Cockpit.GetComponent<Player>().Dock( this );

		// Remove rigidbody
		if ( Cockpit.GetComponent<Rigidbody>() != null )
		{
			Destroy( Cockpit.GetComponent<Rigidbody>() );
		}

		// Visuals
		GetComponentInChildren<MeshRenderer>().enabled = false;

		// Enable mech controls
		if ( LocalPlayer.Instance.Player == Cockpit.GetComponent<Player>() )
		{
			GetComponentInParent<MechBody>().IsMainController = true;
			foreach ( var controller in GetComponentInParent<MechBody>().GetComponentsInChildren<BaseController>() )
			{
				controller.IsMainController = true;
			}
		}
	}

	public void UnDock( bool destroy = false )
	{
		if ( Cockpit == null ) return; // Might be null if joined late and error?

		// Undock + start the heli
		if ( !destroy )
		{
			Cockpit.transform.SetParent( null );
			Cockpit.enabled = true;
		}
		CanDock = true;
		CurrentDockCooldown = Time.time + DockCooldown;

		// Add rigidbody
		if ( !destroy )
		{
			Cockpit.AddRigidbody();
		}

		// Callbacks
		if ( !destroy )
		{
			Cockpit.GetComponent<HeliCockpit>().OnUnDock();
		}
		GetComponentInParent<MechBody>().OnUnDock();
		foreach ( var engine in GetComponentInParent<MechBody>().GetComponentsInChildren<Engine>() )
		{
			engine.OnUnDock();
		}

		// Network
		if ( !destroy )
		{
			Cockpit.GetComponent<Player>().UnDock( this );
		}

		// Visuals
		GetComponentInChildren<MeshRenderer>().enabled = true;

		// Disable mech controls
		GetComponentInParent<MechBody>().IsMainController = false;
		foreach ( var controller in GetComponentInParent<MechBody>().GetComponentsInChildren<BaseController>() )
		{
			controller.IsMainController = false;
		}

		Cockpit = null;
	}
}
