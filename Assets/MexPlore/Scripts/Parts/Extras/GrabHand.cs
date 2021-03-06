using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabHand : MonoBehaviour
{
	GameObject Holding = null;

	private void OnTriggerEnter( Collider other )
	{
		if ( other.GetComponent<FuelSource>() )
		{
			TryPickup( other.gameObject );
		}
		if ( other.tag == "Building" )
		{
			TryPunch( other.GetComponentInParent<Building>() );
		}
	}

	private void OnTriggerStay( Collider other )
	{
		if ( other.tag == "Building" )
		{
			TryPunch( other.GetComponentInParent<Building>() );
		}
	}

	#region Hold
	public void TryPickup( GameObject obj )
	{
		if ( Holding == null )
		{
			Pickup( obj );
		}
	}

	void Pickup( GameObject obj )
	{
		Holding = obj;

		// Parent, 0 pos
		Holding.transform.SetParent( transform );
		Holding.transform.localPosition = Vector3.zero;
		Holding.transform.localEulerAngles = Vector3.zero;
		foreach ( var collider in Holding.GetComponentsInChildren<Collider>() )
		{
			collider.enabled = false;
		}

		GetComponentInParent<Arm>().OnPickup( obj );
	}

	public void TryDrop()
	{
		if ( Holding != null )
		{
			Drop();
		}
	}

	void Drop()
	{
		GameObject obj = Holding;
		Holding = null;

		// Unparent
		obj.transform.parent = null;
		foreach ( var collider in obj.GetComponentsInChildren<Collider>( true ) )
		{
			collider.enabled = true;
		}

		// Make gravity affected
		var rigid = obj.GetComponent<Rigidbody>();
		if ( rigid == null )
		{
			rigid = obj.AddComponent<Rigidbody>();
		}
		rigid.isKinematic = false;
		rigid.useGravity = true;

		GetComponentInParent<Arm>().OnDrop( obj );
	}
	#endregion

	#region Punch
	void TryPunch( Building building )
	{
		if ( GetComponentInParent<Arm>().Extended && !GetComponentInParent<Arm>().JustPunched )
		{
			building.TakeDamage( MexPlore.DAMAGE_PUNCH, GetComponentInParent<MechBody>().transform.position );
			StaticHelpers.GetOrCreateCachedPrefab( "Particle Effect", transform.position, transform.rotation, transform.lossyScale );
			GetComponentInParent<Arm>().JustPunched = true;
		}
	}
	#endregion
}
