using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    [Header( "Variables" )]
    public Vector3 ShakeMultiplier;

    [Header( "References" )]
    public Transform ShakePivot;

    [Header( "Assets" )]
    public AudioClip SoundEngineOn;
    public AudioClip SoundEngineLoop;
    public AudioClip SoundEngineOff;
    public AudioClip SoundFuelConsume;

    private AudioSource Source;

    private bool On = false;

    void Start()
    {
        // AudioSource attached is engine loop
        Source = GetComponent<AudioSource>();
        Source.clip = SoundEngineLoop;
        Source.volume = MexPlore.GetVolume( MexPlore.SOUND.ENGINE_LOOP );

        ToggleParticles( false );
    }

    void Update()
    {
        if ( On )
		{
            // Shake the engine
            ShakePivot.transform.localPosition = new Vector3( ShakeMultiplier.x * Random.Range( -1.0f, 1.0f ), ShakeMultiplier.y * Random.Range( -1.0f, 1.0f ), ShakeMultiplier.z * Random.Range( -1.0f, 1.0f ) );
        }
    }

    public void OnDock()
    {
        On = true;

        StaticHelpers.GetOrCreateCachedAudioSource( SoundEngineOn, transform.position, 1, MexPlore.GetVolume( MexPlore.SOUND.ENGINE_ON ) );

        // Engine loop start delayed with length of engineon * 0.7f
        Source.PlayDelayed( SoundEngineOn.length * 0.7f );

        ToggleParticles( true );
    }

    public void OnUnDock()
    {
        On = false;

        StaticHelpers.GetOrCreateCachedAudioSource( SoundEngineOff, transform.position, 1, MexPlore.GetVolume( MexPlore.SOUND.ENGINE_OFF ) );

        // Engine loop
        Source.Stop();

        ShakePivot.transform.localPosition = Vector3.zero;
        ToggleParticles( false );
    }

    public void OnFuelConsume()
	{
        StaticHelpers.GetOrCreateCachedAudioSource( SoundFuelConsume, transform.position, 1, MexPlore.GetVolume( MexPlore.SOUND.ENGINE_FUEL_CONSUME ) );
		foreach ( var particle in GetComponentsInChildren<ParticleSystem>() )
		{
            StaticHelpers.GetOrCreateCachedPrefab( "Particle Smoke OneShot", particle.transform.position, particle.transform.rotation, particle.transform.localScale );
        }
    }

    void ToggleParticles( bool toggle )
	{
		foreach ( var particle in GetComponentsInChildren<ParticleSystem>() )
		{
            var emission = particle.emission;
            emission.enabled = toggle;
		}
	}
}
