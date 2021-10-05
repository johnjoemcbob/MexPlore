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
	public float OfflineGroundHeight = 1;
	public float PosLerpSpeed = 5;
	public bool ChangePhysicsLayersOnDock = true;

	[Header( "Base - References" )]
	public Transform Torso;

	[Header( "Base - Assets" )]
	public AudioSource RotationSource;

	private Vector3 TargetPos;
	private Vector3 TargetDirection;
	private Vector3 InitialPos;
	private Quaternion TorsoRotation;
	private Dictionary<string, int> OldLayers = new Dictionary<string, int>();

	public virtual void Awake()
	{
		if ( TorsoLag )
		{
			TorsoRotation = Torso.rotation;
		}
		TargetPos = transform.position;
		TargetDirection = transform.forward;
		InitialPos = transform.position;
	}

	public virtual void Update()
	{
		if ( IsMainController )
		{
			if ( RotateTowardsTarget )
			{
				Quaternion target = Quaternion.identity;
				if ( TargetDirection != Vector3.zero )
				{
					target = Quaternion.LookRotation( TargetDirection, Vector3.up );
				}
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

		// Always lerp to target pos
		if ( GetComponent<Rigidbody>().isKinematic && TargetPos != Vector3.zero )
		{
			//float dist = Mathf.Max( 1, Vector3.Distance( transform.position, TargetPos ) );
			//transform.position = Vector3.Lerp( transform.position, TargetPos, Time.deltaTime * PosLerpSpeed * dist );
			transform.localPosition = TargetPos;
			if ( Vector3.Distance( transform.localPosition, TargetPos ) < 0.1f )
			{
				TargetPos = Vector3.zero;
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

	public void Reset()
	{
		SetParent( null );
		transform.position = InitialPos;
		TargetPos = InitialPos;
	}

	public void SetTargetPos( Vector3 pos )
	{
		if ( transform.parent != null )
		{
			TargetPos = transform.parent.InverseTransformPoint( pos );
		}
		else
		{
			TargetPos = pos;
		}
	}

	public void SetTargetDirection( Vector3 dir )
	{
		TargetDirection = dir;
	}

	public void SetParent( Transform parent )
	{
		if ( transform.parent != parent ) //&& !parent.name.Contains( "Terrain" ) )
		{
			Vector3 pos = transform.position;
			transform.SetParent( parent );
			transform.position = pos;
			SetTargetPos( pos );
		}
	}

	public void OnDock()
	{
		if ( ChangePhysicsLayersOnDock )
		{
			OldLayers.Clear();
			foreach ( var collider in GetComponentsInChildren<Collider>() )
			{
				if ( !OldLayers.ContainsKey( collider.name ) )
				{
					OldLayers.Add( collider.name, collider.gameObject.layer );
				}
				collider.gameObject.layer = LayerMask.NameToLayer( "Self" );
			}
		}
	}

	public void OnUnDock()
	{
		//Vector3 ground = MexPlore.RaycastToGround( transform.position );
		//TargetPos = ground + Vector3.up * OfflineGroundHeight;

		if ( ChangePhysicsLayersOnDock )
		{
			foreach ( var collider in GetComponentsInChildren<Collider>() )
			{
				collider.gameObject.layer = OldLayers[collider.name];
			}
			OldLayers.Clear();
		}
	}
}
