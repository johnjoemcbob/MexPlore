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
	public float TargetTargetLerpSpeed = 1;

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
	public bool IncrementalRaycasts = true;
	public bool PhysicsEnabled = true;
	public bool handMatchesTargetRotation = true;

	[Space(20)]
	public bool DebugDraw = true;
	#endregion

	#region Variables - Public
	[HideInInspector]
	public float ArmLength;

	public Coroutine WalkCoroutine;
	#endregion

	#region Variables - Private
	float angle;
	float targetDistance;
	float adyacent;

	private Vector3 InitialTargetTargetPos;
	#endregion

	#region MonoBehaviour
	private void Start()
	{
		if ( TargetTarget != null )
		{
			InitialTargetTargetPos = TargetTarget.localPosition;
		}
	}

	void LateUpdate()
	{
		// Extend arms to correct length
		UpdateArmLength();

		// Move target and decide when to walk
		UpdateTarget();

		// Do the walking
		UpdateLegWalkState( CurrentLegWalkState );

		// Move the arm IKs
		UpdateIK();
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
	#endregion

	#region Update
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
		forearm.localPosition = new Vector3( 0, 0, 1 ) * UpperArmLength;
		hand.localPosition = new Vector3( 0, 0, 1 ) * LowerArmLength;
	}

	void UpdateTarget()
	{
		// If there is a TargetTarget, then the normal target gets moved to this point at variable distance interval
		if ( TargetTarget != null && PhysicsEnabled )
		{
			var walker = GetComponentInParent<WalkController>();
			// If actual desired pos is out of range, try closer
			Vector3 pos = TargetTarget.position;
			if ( walker != null )
			{
				pos += walker.GetOvershoot();
				float dist;
				if ( IncrementalRaycasts && Application.isPlaying )
				{
					bool success = false;
					int increments = 5;
					// Try from the desired pos to directly below the mech in incremental steps
					for ( int i = 0; i < increments; i++ )
					{
						pos = InitialTargetTargetPos * ( 1 - ( (float) i / increments ) );
						TargetTarget.localPosition = pos;
						pos = transform.TransformPoint( pos ) + walker.GetOvershoot();
						{
							// TargetTarget transform retains its y, but need to raycast downwards to find the ACTUAL target pos for this movement
							pos = MexPlore.RaycastToGroundSphere( pos );
						}
						dist = ( upperArm.position - pos ).sqrMagnitude;
						float max = ( ArmLength * ArmLength );
						if ( dist <= max )
						{
							success = true;
							break;
						}
					}
					// Otherwise try further away
					if ( !success )
					{
						for ( int i = 0; i < increments; i++ )
						{
							pos = InitialTargetTargetPos * ( 1 + ( (float) i / increments ) );
							TargetTarget.localPosition = pos;
							pos = transform.TransformPoint( pos ) + walker.GetOvershoot();
							{
								// TargetTarget transform retains its y, but need to raycast downwards to find the ACTUAL target pos for this movement
								pos = MexPlore.RaycastToGroundSphere( pos );
							}
							dist = ( upperArm.position - pos ).sqrMagnitude;
							float max = ( ArmLength * ArmLength );
							if ( dist <= max )
							{
								success = true;
								break;
							}
						}
					}
				}

				dist = ( target.position - pos ).sqrMagnitude;
				float currentfootdist = ( hand.position - target.position ).sqrMagnitude;
				float maxdist = ArmLength;
				bool force = currentfootdist > maxdist;
				if ( ( dist > TargetMaxDistance || force ) )
				{
					walker.TryMoveLeg( target, pos, force );
				}
			}
			else
			{
				target.position = Vector3.Lerp( target.position, pos, Time.deltaTime * TargetTargetLerpSpeed );
			}
		}
	}

	void UpdateIK()
	{
		// IK
		if ( upperArm != null && forearm != null && hand != null && elbow != null && target != null )
		{
			upperArm.LookAt( target, elbow.position - upperArm.position );
			upperArm.Rotate( uppperArm_OffsetRotation );

			Vector3 cross = Vector3.Cross (elbow.position - upperArm.position, forearm.position - upperArm.position);

			ArmLength = UpperArmLength + LowerArmLength;
			targetDistance = Vector3.Distance( upperArm.position, target.position );
			targetDistance = Mathf.Min( targetDistance, ArmLength - ArmLength * 0.001f );

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
	#endregion

	#region Leg Walk
	public enum LegWalkState
	{
		Idle,
		Raise,
		Lower,
	}
	public LegWalkState CurrentLegWalkState;

	private float CurrentLegWalkTime = 0;
	private Vector3 LegWalkStartPos;
	private Vector3 LegWalkTargetPos;

	public void StartWalking( Vector3 target )
	{
		LegWalkTargetPos = target;
		if ( CurrentLegWalkState == LegWalkState.Idle )
		{
			SwitchLegWalkState( LegWalkState.Raise );
		}
	}

	public bool IsWalking()
	{
		return CurrentLegWalkState != LegWalkState.Idle;
	}

	public void SwitchLegWalkState( LegWalkState state )
	{
		FinishLegWalkState( CurrentLegWalkState );
		SetLegWalkState( state );
	}

	void SetLegWalkState( LegWalkState state )
	{
		CurrentLegWalkState = state;
		StartLegWalkState( CurrentLegWalkState );
	}

	void StartLegWalkState( LegWalkState state  )
	{
		var walker = GetComponentInParent<WalkController>();

		switch ( state )
		{
			case LegWalkState.Idle:
				break;
			case LegWalkState.Raise:
				// Start moving
				CurrentLegWalkTime = 0;
				LegWalkStartPos = target.position;

				// Effects
				walker.TryPlaySound( walker.SoundBankLegRaise, this, MexPlore.SOUND.MECH_LEG_RAISE );

				break;
			case LegWalkState.Lower:
				// Start lowering

				// Effects
				walker.TryPlaySound( walker.SoundBankLegLower, this, MexPlore.SOUND.MECH_LEG_LOWER );
				foreach ( var particle in GetComponentsInChildren<ParticleSystem>() )
				{
					particle.Play();
				}

				break;
			default:
				break;
		}
	}

	void UpdateLegWalkState( LegWalkState state )
	{
		var walker = GetComponentInParent<WalkController>();
		if ( walker == null ) return;

		CurrentLegWalkTime += Time.deltaTime;

		Vector3 targetpos = Vector3.zero;
		float progress = CurrentLegWalkTime / walker.LegLerpDuration;

		switch ( state )
		{
			case LegWalkState.Idle:
				CurrentLegWalkTime = 0;
				break;
			case LegWalkState.Raise:
				targetpos = LegWalkStartPos + ( LegWalkTargetPos - LegWalkStartPos ) / 2 + Vector3.up * 1;
				if ( progress > 0.5f )
				{
					SwitchLegWalkState( LegWalkState.Lower );
				}
				break;
			case LegWalkState.Lower:
				targetpos = LegWalkTargetPos;
				if ( progress >= 1 )
				{
					SwitchLegWalkState( LegWalkState.Idle );
				}
				progress -= 0.5f;
				break;
			default:
				break;
		}

		// Move to target
		if ( targetpos != Vector3.zero )
		{
			progress *= 2;
			Vector3 lerpedpos = Vector3.Lerp( LegWalkStartPos, targetpos, progress );
			target.position = lerpedpos;
			walker.StoreNewLegPos();
		}
	}

	void FinishLegWalkState( LegWalkState state )
	{
		var walker = GetComponentInParent<WalkController>();

		switch ( state )
		{
			case LegWalkState.Idle:
				break;
			case LegWalkState.Raise:
				break;
			case LegWalkState.Lower:
				// Play footstep sound
				walker.TryPlaySound( walker.SoundBankFootstep, this, MexPlore.SOUND.MECH_FOOTSTEP );

				// Play particle effect
				StaticHelpers.EmitParticleDust( target.position );

				break;
			default:
				break;
		}
	}
	#endregion
}
