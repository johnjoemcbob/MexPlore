using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechVoice : MonoBehaviour
{
    [Header( "Variables" )]
    public Vector2Int VoiceLength;
    public Vector2 BetweenNotesRange;
    public Vector2 PitchRange;

    [Header( "Assets" )]
    public AudioClip SoundNote;

    private Coroutine Coroutine;

    void Start()
    {
    }

    void Update()
    {
        var body = GetComponentInParent<MechBody>();
        if ( body != null && body.IsMainController )
		{
            if ( Input.GetButtonDown( MexPlore.GetControl( MexPlore.CONTROL.BUTTON_SPEAK ) ) && Coroutine == null )
            {
                Coroutine = StartCoroutine( Co_Chirp() );
            }
		}
    }

    IEnumerator Co_Chirp()
	{
		for ( int i = 0; i < Random.Range( VoiceLength.x, VoiceLength.y ); i++ )
        {
            PlayNote();
            yield return new WaitForSeconds( Random.Range( BetweenNotesRange.x, BetweenNotesRange.y ) );
        }

        Coroutine = null;
        yield break;
	}

    void PlayNote()
	{
        StaticHelpers.GetOrCreateCachedAudioSource( SoundNote, transform.position, Random.Range( PitchRange.x, PitchRange.y ) );
        StaticHelpers.GetOrCreateCachedPrefab( "Particle Noise", transform.position, transform.rotation, transform.lossyScale );
    }
}
