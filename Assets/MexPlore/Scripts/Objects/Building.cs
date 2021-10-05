using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
	public float Health = 10;
	public float FallSpeed = 3;

	public AudioClip SoundDamage;
	public AudioClip SoundFall;
	public AudioClip SoundHitGround;

	[HideInInspector]
	public bool Fallen = false;
	[HideInInspector]
	public Vector3 FallPos;
	[HideInInspector]
	public bool Idle = false;

	void Start()
    {
		// Apply tags and layers to all children
		Recurse( transform );

		// Auto name it the same for every client, for syncing purposes
		var points = 2;
		var mult = points * 10;
		name += "_" + ( Mathf.Round( transform.position.x * mult ) / mult ) + "_" + ( Mathf.Round( transform.position.z * mult ) / mult );
    }

	void Recurse( Transform trans )
	{
		foreach ( Transform child in trans )
		{
			child.gameObject.tag = gameObject.tag;
			child.gameObject.layer = gameObject.layer;
			Recurse( child );
		}
	}

	public void TakeDamage( float damage, Vector3 dmgpos )
	{
		Health -= damage;
		if ( Health <= 0 && !Fallen )
		{
			StartCoroutine( FallDown( dmgpos ) );
		}

		// VFX
		GetComponentInChildren<Punchable>().Punch();

		// SFX
		StaticHelpers.GetOrCreateCachedAudioSource( SoundDamage, dmgpos, Random.Range( 0.9f, 1.1f ), MexPlore.GetVolume( MexPlore.SOUND.BUILDING_DAMAGE ) );
	}

	IEnumerator FallDown( Vector3 dmgpos, bool network = false, bool immediate = false )
	{
		Fallen = true;
		FallPos = dmgpos;

		// Turn all colliders to triggers
		foreach ( var collider in GetComponentsInChildren<Collider>() )
		{
			collider.isTrigger = true;
		}

		// Add kinematic rigidbody (for trigger enters)
		var body = gameObject.AddComponent<Rigidbody>();
		body.isKinematic = true;

		if ( !immediate )
		{
			// SFX
			StaticHelpers.GetOrCreateCachedAudioSource( SoundFall, dmgpos, Random.Range( 0.9f, 1.1f ), MexPlore.GetVolume( MexPlore.SOUND.BUILDING_FALL ) );
		}

		// Fall away from player
		Vector3 dir = ( dmgpos - transform.position ).normalized;
		Vector3 right = Vector3.Cross( dir.normalized, Vector3.up );
		Vector3 startang = transform.eulerAngles;
		Quaternion start = Quaternion.LookRotation( startang );
		Quaternion target = Quaternion.AngleAxis( 90, right );
		float progress = 0;
			if ( immediate )
			{
				progress = 1;
			}
		while ( progress <= 1 )
		{
			Quaternion current = Quaternion.Lerp( start, target, Mathf.Clamp( progress, 0, 1 ) );
			body.MoveRotation( current );
			progress += Time.deltaTime * FallSpeed;
			yield return new WaitForEndOfFrame();
		}

		if ( !immediate )
		{
			// VFX
			foreach ( var particle in GetComponentsInChildren<ParticleSystem>() )
			{
				particle.Play();
			}

			// SFX
			StaticHelpers.GetOrCreateCachedAudioSource( SoundHitGround, dmgpos, Random.Range( 0.9f, 1.1f ), MexPlore.GetVolume( MexPlore.SOUND.BUILDING_HITGROUND ) );
		}

		// Network
		if ( !network )
		{
			LocalPlayer.Instance.Player.KnockBuilding( name, dmgpos );
		}

		Idle = true;

		yield break;
	}

	private void OnTriggerEnter( Collider other )
	{
		if ( Idle ) return;

		if ( other.tag == "Building" )
		{
			//other.GetComponentInParent<Building>().TakeDamage( MexPlore.DAMAGE_BUILDING_FALL, transform.position );
			other.GetComponentInParent<Building>().TakeDamage( MexPlore.DAMAGE_BUILDING_FALL, FallPos );
		}
	}

	public void NetworkFall( Vector3 dmgpos, bool immediate = false )
	{
		if ( Fallen ) return;

		StartCoroutine( FallDown( dmgpos, true, immediate ) );
	}

	public static Building FindByName( string buildingname )
	{
		var obj = GameObject.Find( buildingname );
		if ( obj != null )
		{
			return obj.GetComponent<Building>();
		}
		return null;
	}
}
