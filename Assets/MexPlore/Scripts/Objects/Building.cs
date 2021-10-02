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

	private bool Fallen = false;

    void Start()
    {
		// Apply tags and layers to all children
		Recurse( transform );
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

	IEnumerator FallDown( Vector3 dmgpos )
	{
		Fallen = true;

		// Turn all colliders to triggers
		foreach ( var collider in GetComponentsInChildren<Collider>() )
		{
			collider.isTrigger = true;
		}

		// Add kinematic rigidbody (for trigger enters)
		var body = gameObject.AddComponent<Rigidbody>();
		body.isKinematic = true;

		// SFX
		StaticHelpers.GetOrCreateCachedAudioSource( SoundFall, dmgpos, Random.Range( 0.9f, 1.1f ), MexPlore.GetVolume( MexPlore.SOUND.BUILDING_FALL ) );

		// Fall away from player
		Vector3 dir = ( dmgpos - transform.position ).normalized;
		Vector3 right = Vector3.Cross( dir.normalized, Vector3.up );
		Vector3 startang = transform.eulerAngles;
		Quaternion start = Quaternion.LookRotation( startang );
		Quaternion target = Quaternion.AngleAxis( 90, right );
		float progress = 0;
		while ( progress <= 1 )
		{
			Quaternion current = Quaternion.Lerp( start, target, Mathf.Clamp( progress, 0, 1 ) );
			body.MoveRotation( current );
			progress += Time.deltaTime * FallSpeed;
			yield return new WaitForEndOfFrame();
		}

		// VFX
		foreach ( var particle in GetComponentsInChildren<ParticleSystem>() )
		{
			particle.Play();
		}

		// SFX
		StaticHelpers.GetOrCreateCachedAudioSource( SoundHitGround, dmgpos, Random.Range( 0.9f, 1.1f ), MexPlore.GetVolume( MexPlore.SOUND.BUILDING_HITGROUND ) );

		// Then fade into ground
		//while ( above ground )
		//{

		//}

		yield break;
	}

	private void OnTriggerEnter( Collider other )
	{
		if ( other.tag == "Building" )
		{
			other.GetComponentInParent<Building>().TakeDamage( MexPlore.DAMAGE_BUILDING_FALL, transform.position );
		}
	}
}
