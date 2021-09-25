using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncrediMech : MechBody
{
	[Header( "Variables" )]
	public float RollForce = 5;
	public float RollUpForce = 10;
	public Vector3 ForceOffset = Vector3.up;

	[Header( "References" )]
	public Transform Head;

	private bool WalkerState = true;
	private float StartUpperArmLength;
	private float StartLowerArmLength;
	private Vector3 StartHeadPos;

	private void Start()
	{
		StartHeadPos = Head.localPosition;
	}

	public override void Update()
	{
		base.Update();

		if ( !IsMainController ) return;

		// Toggle button
		if ( Input.GetButtonDown( "Jump" ) )
		{
			ToggleState();
		}
	}

	private void FixedUpdate()
	{
		if ( !IsMainController ) return;

		// Rolling controls
		if ( !WalkerState )
		{
			Vector3 dir = MexPlore.GetCameraDirectionalInput();
			GetComponent<Rigidbody>().AddForceAtPosition( dir * RollForce + Vector3.up * RollUpForce, transform.position + ForceOffset, ForceMode.VelocityChange );
		}
	}

	void ToggleState()
	{
		WalkerState = !WalkerState;

		// Enable/disable physics
		var body = GetComponent<Rigidbody>();
		if ( WalkerState )
		{
			body.isKinematic = true;
			body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
			GetComponentInChildren<WalkController>().IsMainController = true;
			Head.localPosition = StartHeadPos;
			GetComponentInChildren<MechCockpitDock>().CanUnDock = true;
		}
		else
		{
			body.isKinematic = false;
			body.constraints = RigidbodyConstraints.None;
			GetComponentInChildren<WalkController>().IsMainController = false;
			Head.localPosition = Vector3.zero;
			GetComponentInChildren<MechCockpitDock>().CanUnDock = false;
		}
		RotateTowardsTarget = WalkerState;

		foreach ( var ik in GetComponentsInChildren<InverseKinematics>() )
		{
			//ik.enabled = WalkerState;
			ik.PhysicsEnabled = WalkerState;
			if ( !WalkerState )
			{
				StartUpperArmLength = ik.UpperArmLength;
				StartLowerArmLength = ik.LowerArmLength;
				ik.UpperArmLength = 0;
				ik.LowerArmLength = 0;
			}
			else
			{
				ik.UpperArmLength = StartUpperArmLength;
				ik.LowerArmLength = StartLowerArmLength;
			}
		}
	}
}
