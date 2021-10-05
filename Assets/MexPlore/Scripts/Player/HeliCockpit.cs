using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeliCockpit : MonoBehaviour
{
    [Header( "Variables" )]
    public float RotorLerpSpeed = 5;
    public float RotorLookMultiplier = 0.5f;
    public float RotorLookY = -0.5f;
    public float RotorVelocityLookMultiplier = 0.2f;
    public float BladeLerpSpeed = 1;
    public float BladeMaxSpeed = 1;
    public float BladeVelocitySpeedMultiplier = 0.5f;
    public float BladeVisualSpeedMultiplier = 1;
    public float BladeVisualOffsetMultiplier = 1;
    public float BladeMaxForce = 5;
    public float BladeUpwardForce = 5;
    public float RigidDrag = 1.5f;
    public float RigidAngDrag = 1;

    [Header( "References" )]
    public Transform Rotor;
    public Transform[] Blades;

    [Header( "Assets" )]
    public AudioClip SoundDock;
    public AudioClip SoundUnDock;
    public Material DefaultHighlight;

    private Vector3 LastDirection = Vector3.zero;
    private float CurrentBladeSpeed = 0;
    private float CurrentVisualBladeSpeed = 0;
    private ParticleSystem Particles;

	private void Awake()
    {
        Particles = GetComponentInChildren<ParticleSystem>();
    }

	private void Start()
	{
        AddRigidbody();
    }

	void FixedUpdate()
    {
        bool islocal = LocalPlayer.Instance.Player == GetComponent<Player>();

        // Get input
        bool thrust = false;
        Vector3 dir = Vector3.zero;
            if ( islocal )
            {
                thrust = LocalPlayer.CanInput() && Input.GetButton( MexPlore.GetControl( MexPlore.CONTROL.BUTTON_HELI_THRUST ) );
                dir = MexPlore.GetCameraDirectionalInput().normalized;
            }
        Vector3 vel = Vector3.zero;
            if ( GetComponent<Rigidbody>() != null )
		    {
                vel = GetComponent<Rigidbody>().velocity;
            }
        if ( dir != Vector3.zero )
        {
            //dir = LastDirection;
            LastDirection = dir;
            dir = dir * RotorLookMultiplier + new Vector3( 0, RotorLookY, 0 );
        }
        else
        {
            dir = LastDirection * RotorLookMultiplier;
            dir.y = vel.y * RotorVelocityLookMultiplier;
        }
 
        // Turn rotor
        Quaternion target = Quaternion.identity;
        if ( dir != Vector3.zero )
		{
            target = Quaternion.LookRotation( dir, Vector3.up );
        }
        Rotor.rotation = Quaternion.Lerp( Rotor.rotation, target, Time.deltaTime * RotorLerpSpeed );

        // Add blade spin speed with input
        float targetspeed = 0;
        float visualtargetspeed = 0;
        if ( thrust )
		{
            targetspeed = BladeMaxSpeed;
            visualtargetspeed = BladeMaxSpeed;
        }
        else
		{
            visualtargetspeed = vel.magnitude * BladeVelocitySpeedMultiplier;
        }
        CurrentBladeSpeed = Mathf.Lerp( CurrentBladeSpeed, targetspeed, Time.deltaTime * BladeLerpSpeed );
        CurrentVisualBladeSpeed = Mathf.Lerp( CurrentVisualBladeSpeed, visualtargetspeed, Time.deltaTime * BladeLerpSpeed );

        // Blades audio
        float maxvol = MexPlore.GetVolume( MexPlore.SOUND.HELI_BLADES );
        GetComponent<AudioSource>().volume = Mathf.Clamp( CurrentVisualBladeSpeed / BladeMaxSpeed * maxvol, 0, maxvol );

        // Spin blades by speed
        int i = 0;
		foreach ( var blade in Blades )
		{
            blade.localEulerAngles += new Vector3( 0, 0, 1 ) * Time.deltaTime * CurrentVisualBladeSpeed * BladeVisualSpeedMultiplier * ( ( i + 1 ) * BladeVisualOffsetMultiplier );
            i++;
		}

        // Apply force towards rotor direction * space bar
        if ( islocal )
        {
            GetComponent<Rigidbody>().AddForce( Rotor.up * CurrentBladeSpeed * BladeMaxForce + Vector3.up * CurrentBladeSpeed * BladeUpwardForce, ForceMode.Acceleration );
        }

        // Update particles
        var part = Particles;
        {
            // Pos
            RaycastHit hit = MexPlore.RaycastToGroundHit( transform.position );
            float maxdist = 3;
            float dist = maxdist;
            if ( hit.collider != null )
            {
                dist = Vector3.Distance( hit.point, transform.position );
                part.transform.position = hit.point;
                part.transform.rotation = Quaternion.LookRotation( hit.normal );
            }

            // Scale
            float scale = Mathf.Clamp( ( CurrentVisualBladeSpeed / BladeMaxSpeed ) * ( 1 - ( dist / maxdist ) ), 0, 1 );
            float min = 1.6f;
            float max = 2.4f;
            if ( scale < 0.05f )
            {
                var emission = part.emission;
                {
                    emission.enabled = false;
                }
            }
            else
            {
                var main = part.main;
                {
                    main.startSize = new ParticleSystem.MinMaxCurve( min * scale, max * scale );
                }
                var emission = part.emission;
                {
                    emission.enabled = true;
                }
            }
        }
    }

    public void AddRigidbody()
	{
        var body = gameObject.GetComponent<Rigidbody>();
        {
            if ( body == null )
			{
                body = gameObject.AddComponent<Rigidbody>();
            }
            body.mass = 1;
            body.drag = RigidDrag;
            body.angularDrag = RigidAngDrag;
            body.useGravity = true;
            body.isKinematic = false;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    public void OnDock()
	{
        Rotor.transform.localEulerAngles = new Vector3( 0, -90, 0 );
        GetComponent<AudioSource>().volume = 0;
        Particles.gameObject.SetActive( false );

        StaticHelpers.GetOrCreateCachedAudioSource( SoundDock, transform.position, 1, MexPlore.GetVolume( MexPlore.SOUND.HELI_DOCK ) );
    }

    public void OnUnDock()
    {
        Particles.gameObject.SetActive( true );

		foreach ( var renderer in GetComponentsInChildren<Renderer>() )
		{
			for ( int i = 0; i < renderer.materials.Length; i++ )
			{
                if ( renderer.materials[i].name.Contains( "Highlight" ) )
				{
                    var mats = renderer.materials;
                    mats[i] = DefaultHighlight;
                    renderer.materials = mats;
                }
			}
		}

        StaticHelpers.GetOrCreateCachedAudioSource( SoundUnDock, transform.position, 1, MexPlore.GetVolume( MexPlore.SOUND.HELI_UNDOCK ) );

        transform.localEulerAngles = new Vector3( 0, transform.localEulerAngles.y, 0 );
    }
}
