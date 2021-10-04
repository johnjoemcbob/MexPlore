using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechVoice : MonoBehaviour
{
    public struct VoiceInfo
	{
        public float[] Pitches;
        public float[] Delays;
	}

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
            if ( LocalPlayer.CanInput() && Input.GetButtonDown( MexPlore.GetControl( MexPlore.CONTROL.BUTTON_SPEAK ) ) && Coroutine == null )
            {
                var info = GetVoiceInfo();
                PlayVoice( info );
                LocalPlayer.Instance.Player.Voice( info );
            }
		}
    }

    public void PlayVoice( VoiceInfo info )
    {
        Coroutine = StartCoroutine( Co_Chirp( info ) );
    }

    IEnumerator Co_Chirp( VoiceInfo info )
	{
		for ( int i = 0; i < info.Pitches.Length; i++ )
        {
            PlayNote( info.Pitches[i] );
            yield return new WaitForSeconds( info.Delays[i] );
        }

        Coroutine = null;
        yield break;
	}

    VoiceInfo GetVoiceInfo()
	{
        VoiceInfo info = new VoiceInfo();
		{
            int length = Random.Range( VoiceLength.x, VoiceLength.y );
            info.Pitches = new float[length];
            info.Delays = new float[length];

            for ( int i = 0; i < length; i++ )
			{
                info.Pitches[i] = Random.Range( PitchRange.x, PitchRange.y );
                info.Delays[i] = Random.Range( BetweenNotesRange.x, BetweenNotesRange.y );
            }
        }
        return info;
	}

    void PlayNote( float pitch )
	{
        StaticHelpers.GetOrCreateCachedAudioSource( SoundNote, transform.position, pitch );
        StaticHelpers.GetOrCreateCachedPrefab( "Particle Noise", transform.position, transform.rotation, transform.lossyScale );
    }
}
