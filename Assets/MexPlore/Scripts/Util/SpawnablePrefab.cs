using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnablePrefab : MonoBehaviour
{
    public string PrefabName;

	private void Start()
	{
		//name = PrefabName + "_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
	}
}
