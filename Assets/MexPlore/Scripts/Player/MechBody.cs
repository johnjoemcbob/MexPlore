using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechBody : MonoBehaviour
{
	public bool IsMainController = false;
	public bool RotateTowardsTarget = true;

	[Header( "Base - Variables" )]
	public float RotateSpeed = 5;
	public float RotateEpsilon = 0.5f;
	public float RotateVolumeMultiplier = 0.5f;
	public bool TorsoLag = false;
	public float TorsoLagSpeed = 1;

	[Header( "Base - References" )]
	public Transform Torso;

	[Header( "Base - Assets" )]
	public AudioSource RotationSource;

	private Vector3 TargetDirection;
	private Quaternion TorsoRotation;

	public virtual void Awake()
	{
		if ( TorsoLag )
		{
			TorsoRotation = Torso.rotation;
		}
	}

	public virtual void Update()
	{
		if ( !IsMainController ) return;

		if ( RotateTowardsTarget )
		{
			Quaternion target = Quaternion.LookRotation( TargetDirection, Vector3.up );
			Quaternion start = transform.rotation;
			transform.rotation = Quaternion.Lerp( transform.rotation, target, Time.deltaTime * RotateSpeed );
			if ( TorsoLag )
			{
				TorsoRotation = Quaternion.Lerp( TorsoRotation, target, Time.deltaTime * TorsoLagSpeed );
				Torso.rotation = TorsoRotation;
			}

			float ang = Quaternion.Angle( transform.rotation, target );
			if ( ang > RotateEpsilon )
			{
				if ( !RotationSource.isPlaying )
				{
					RotationSource.Play();
				}
				RotationSource.volume = Mathf.Min( ang * RotateVolumeMultiplier, MexPlore.GetVolume( MexPlore.SOUND.MECH_TURN ) );
			}
			else
			{
				RotationSource.Pause();
			}
		}
	}

	public virtual void OnCollisionEnter( Collision collision )
	{
		foreach ( var controller in GetComponentsInChildren<BaseController>() )
		{
			controller.OnCollisionEnter( collision );
		}
	}

	public void SetTargetDirection( Vector3 dir )
	{
		TargetDirection = dir;
	}
}
