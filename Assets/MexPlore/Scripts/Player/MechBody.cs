using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechBody : MonoBehaviour
{
	[Header( "Variables" )]
	public float RotateSpeed = 5;
	public float RotateEpsilon = 0.5f;
	public float RotateVolumeMultiplier = 0.5f;
	public float RotateVolumeMax = 0.5f;

	[Header( "Assets" )]
	public AudioSource RotationSource;

	private Vector3 TargetDirection;

	private void Update()
	{
		Quaternion target = Quaternion.LookRotation( TargetDirection, Vector3.up );
		transform.rotation = Quaternion.Lerp( transform.rotation, target, Time.deltaTime * RotateSpeed );

		float ang = Quaternion.Angle( transform.rotation, target );
		if ( ang > RotateEpsilon )
		{
			if ( !RotationSource.isPlaying )
			{
				RotationSource.Play();
			}
			RotationSource.volume = Mathf.Min( ang * RotateVolumeMultiplier, RotateVolumeMax );
		}
		else
		{
			RotationSource.Pause();
		}
	}

	private void OnCollisionEnter( Collision collision )
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
