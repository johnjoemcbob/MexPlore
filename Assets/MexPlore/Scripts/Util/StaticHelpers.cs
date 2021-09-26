using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StaticHelpers
{
	public const int POOL_AUDIO_MAX = 128;
	public const int POOL_PREFAB_MAX = 24;

	#region ==Variables
	public static Dictionary<string, AudioClip> AudioClips = new Dictionary<string, AudioClip>();

	public static List<AudioSource> AudioPool = new List<AudioSource>();
	public static Dictionary<string, List<GameObject>> PrefabPools = new Dictionary<string, List<GameObject>>();
	#endregion

	#region Statics
	// Cache resource.loads?
	// Cache a pool of each type
	// Audio sources need some priority for player? or just lots
	// Stop GameObject.Destroy - change number of parameters to track down code
	public static GameObject GetOrCreateCachedPrefab( string path, Vector3 pos, Quaternion rot, Vector3 scale, float timeout = 1 )
	{
		GameObject prefab = null;
		{
			if ( PrefabPools.ContainsKey( path ) )
			{
				// Search prefabs for any disabled which are usable
				foreach ( var cached in PrefabPools[path] )
				{
					if ( cached != null && !cached.activeSelf )
					{
						prefab = cached;
						prefab.SetActive( true );
						break;
					}
				}
				//PrefabPools[path].Remove( null );
			}
			else
			{
				PrefabPools.Add( path, new List<GameObject>() );
			}

			// Couldn't find any, but still have space in the pool
			if ( prefab == null && PrefabPools[path].Count < POOL_PREFAB_MAX )
			{
				prefab = SpawnPrefab( path );

				PrefabPools[path].Add( prefab );
			}

			// Update prefab if found
			if ( prefab != null )
			{
				prefab.transform.position = pos;
				prefab.transform.rotation = rot;
				prefab.transform.localScale = scale;

				if ( timeout != 0 )
				{
					Game.Instance.StartCoroutine( DisableAfterTimeout( prefab, timeout ) );
				}
			}
		}
		return prefab;
	}

	public static IEnumerator DisableAfterTimeout( GameObject obj, float timeout )
	{
		yield return new WaitForSeconds( timeout );

		if ( obj != null )
		{
			obj.transform.position = Vector3.one * 1000; // In case any objects with colliders start getting events (I don't know)
			obj.SetActive( false );
		}
	}

	public static GameObject EmitParticleImpact( Vector3 point )
	{
		return GetOrCreateCachedPrefab( "Particle Effect", point, Quaternion.identity, Vector3.one, 0.5f );
	}

	public static GameObject EmitParticleDust( Vector3 point )
	{
		return GetOrCreateCachedPrefab( "Particle Dust", point, Quaternion.Euler( -90, 0, 0 ), new Vector3( 1, 1, 0.5f ) * 0.1f, 2 );
	}

	public static GameObject SpawnPrefab( string name )
	{
		return SpawnResource( "Prefabs/" + name );
	}
	public static GameObject SpawnResource( string name )
	{
		GameObject prefab = GameObject.Instantiate( Resources.Load( name ) as GameObject, Game.RuntimeParent );
		return prefab;
	}

	public static AudioSource SpawnResourceAudioSource( string clipname )
	{
		return SpawnAudioSource( GetOrLoadAudioClip( clipname ) );
	}

	public static AudioSource SpawnAudioSource( AudioClip clip )
	{
		GameObject source = GameObject.Instantiate( Resources.Load( "Prefabs/Audio Source" ), Game.RuntimeParent ) as GameObject;
		return source.GetComponent<AudioSource>();
	}

	public static AudioSource GetOrCreateCachedAudioSource( string clipname, bool spatial, float pitch = 1, float volume = 1, float delay = 0 )
	{
		var src = GetOrCreateCachedAudioSource( clipname, Camera.main.transform.position, pitch, volume, delay );
		src.spatialBlend = 0;
		return src;
	}
	public static AudioSource GetOrCreateCachedAudioSource( string clipname, Vector3 pos, float pitch = 1, float volume = 1, float delay = 0 )
	{
		return GetOrCreateCachedAudioSource( GetOrLoadAudioClip( clipname ), pos, pitch, volume, delay );
	}
	public static AudioSource GetOrCreateCachedAudioSource( AudioClip clip, Vector3 pos, float pitch = 1, float volume = 1, float delay = 0 )
	{
		AudioSource source = null;
		{
			// Search prefabs for any disabled which are usable
			foreach ( var cached in AudioPool )
			{
				if ( cached != null && !cached.gameObject.activeSelf )
				{
					source = cached;
					source.gameObject.SetActive( true );
					break;
				}
			}

			// Couldn't find any, but still have space in the pool
			if ( source == null && AudioPool.Count < POOL_PREFAB_MAX )
			{
				source = SpawnAudioSource( clip );

				AudioPool.Add( source );
			}

			// Update prefab if found
			if ( source != null )
			{
				source.transform.position = pos;

				source.clip = clip;
				source.pitch = pitch;
				source.volume = volume;
				source.spatialBlend = 1;
				source.PlayDelayed( delay );

				if ( Game.Instance != null )
				{
					Game.Instance.StartCoroutine( DisableAfterTimeout( source.gameObject, delay + clip.length + 0.1f ) );
				}
			}
		}
		return source;
	}

	public static AudioClip GetOrLoadAudioClip( string clipname )
	{
		AudioClip clip = null;
		{
			if ( !AudioClips.ContainsKey( clipname ) )
			{
				AudioClips.Add( clipname, Resources.Load( "Audio/" + clipname ) as AudioClip );
			}
			clip = AudioClips[clipname];
		}
		return clip;
	}

	public static void Reset()
	{
		// Reset static pools
		AudioPool = new List<AudioSource>();
		PrefabPools = new Dictionary<string, List<GameObject>>();
	}
	#endregion
}
