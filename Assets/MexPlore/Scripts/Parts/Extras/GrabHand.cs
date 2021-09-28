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
	}

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
}
