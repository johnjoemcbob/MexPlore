using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InverseKinematics : MonoBehaviour
{
	#region Variables - Inspector
	[Header( "Variables" )]
	public float TargetMaxDistance = 10;
	public float UpperArmLength = 2.5f;
	public float LowerArmLength = 2.5f;

	[Header( "Offsets" )]
	public Vector3 uppperArm_OffsetRotation;
	public Vector3 forearm_OffsetRotation;
	public Vector3 hand_OffsetRotation;

	[Header( "References" )]
	public Transform VisualUpperArm;
	public Transform VisualLowerArm;
	public Transform upperArm;
	public Transform forearm;
	public Transform hand;
	public Transform elbow;
	public Transform target;
	public Transform TargetTarget;

	[Space(20)]
	public bool PhysicsEnabled = true;
	public bool handMatchesTargetRotation = true;

	[Space(20)]
	public bool DebugDraw = true;
	#endregion

	#region Variables - Private
	float angle;
	float arm_Length;
	float targetDistance;
	float adyacent;
	#endregion

	void LateUpdate()
	{
		// Extend arms to correct length
		UpdateArmLength();

		// If there is a TargetTarget, then the normal target gets moved to this point at variable distance interval
		if ( TargetTarget != null && PhysicsEnabled )
		{
			float dist = ( target.position - TargetTarget.position ).sqrMagnitude;
			if ( dist > TargetMaxDistance )
			{
				GetComponentInParent<WalkController>().TryMoveLeg( target, TargetTarget.position );
			}
		}

		// IK
		if ( upperArm != null && forearm != null && hand != null && elbow != null && target != null )
		{
			upperArm.LookAt( target, elbow.position - upperArm.position );
			upperArm.Rotate( uppperArm_OffsetRotation );

			Vector3 cross = Vector3.Cross (elbow.position - upperArm.position, forearm.position - upperArm.position);

			arm_Length = UpperArmLength + LowerArmLength;
			targetDistance = Vector3.Distance( upperArm.position, target.position );
			targetDistance = Mathf.Min( targetDistance, arm_Length - arm_Length * 0.001f );

			adyacent = ( ( UpperArmLength * UpperArmLength ) - ( LowerArmLength * LowerArmLength ) + ( targetDistance * targetDistance ) ) / ( 2 * targetDistance );

			angle = Mathf.Acos( adyacent / UpperArmLength ) * Mathf.Rad2Deg;

			// Guard against NaN (Incredimech, fold in on self)
			if ( float.IsNaN( angle ) )
			{
				angle = 0;
			}
			upperArm.RotateAround( upperArm.position, cross, -angle );

			forearm.LookAt( target, cross );
			forearm.Rotate( forearm_OffsetRotation );

			if ( handMatchesTargetRotation )
			{
				hand.rotation = target.rotation;
				hand.Rotate( hand_OffsetRotation );
			}

			if ( DebugDraw )
			{
				if ( forearm != null && elbow != null )
				{
					Debug.DrawLine( forearm.position, elbow.position, Color.blue );
				}

				if ( upperArm != null && target != null )
				{
					Debug.DrawLine( upperArm.position, target.position, Color.red );
				}
			}
		}
	}

	void OnDrawGizmos()
	{
		if ( DebugDraw )
		{
			if ( upperArm != null && elbow != null && hand != null && target != null && elbow != null )
			{
				Gizmos.color = Color.gray;
				Gizmos.DrawLine( upperArm.position, forearm.position );
				Gizmos.DrawLine( forearm.position, hand.position );
				Gizmos.color = Color.red;
				Gizmos.DrawLine( upperArm.position, target.position );
				Gizmos.color = Color.blue;
				Gizmos.DrawLine( forearm.position, elbow.position );
			}
		}
	}

	void UpdateArmLength()
	{
		if ( VisualUpperArm != null )
		{
			Vector3 scale = VisualUpperArm.localScale;
			{
				scale.z = UpperArmLength;
			}
			VisualUpperArm.localScale = scale;
			Vector3 pos = VisualUpperArm.localPosition;
			{
				pos.z = UpperArmLength / 2;
			}
			VisualUpperArm.localPosition = pos;
		}
		//else
		//{
		//	VisualUpperArm = transform.Find( "ShoulderJoint/UpperArm" );
		//}
		if ( VisualLowerArm != null )
		{
			Vector3 scale = VisualLowerArm.localScale;
			{
				scale.z = LowerArmLength;
			}
			VisualLowerArm.localScale = scale;
			Vector3 pos = VisualLowerArm.localPosition;
			{
				pos.z = LowerArmLength / 2;
			}
			VisualLowerArm.localPosition = pos;
		}
		//else
		//{
		//	VisualLowerArm = transform.Find( "ShoulderJoint/ElbowJoint/ForeArm" );
		//}
		forearm.localPosition = new Vector3( 0, 0, 1 ) * UpperArmLength;
		hand.localPosition = new Vector3( 0, 0, 1 ) * LowerArmLength;
	}
}
