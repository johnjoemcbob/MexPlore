using System;
using System.Collections.Generic;
using UnityEngine;

public class AllSpawnablePrefabs : MonoBehaviour
{
	public static AllSpawnablePrefabs Instance;

    [Serializable]
    public struct Spawnable
	{
		public string Name;
		public GameObject Prefab;
	}

	public Spawnable[] All;

	private void Awake()
	{
		Instance = this;
	}

	public static GameObject GetPrefab( string name )
	{
		foreach ( var spawnable in Instance.All )
		{
			if ( spawnable.Name == name )
			{
				return spawnable.Prefab;
			}
		}
		return null;
	}
}
