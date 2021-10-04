using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Game : MonoBehaviour
{
    public static Game Instance;
    public static Transform RuntimeParent;

    private bool Loading = false;
    private string CrossoverMech = "";

    #region MonoBehaviour
    void Awake()
    {
        Instance = this;
        RuntimeParent = transform;

        MexPlore.InitVolumes();
        MexPlore.InitControls();

        DontDestroyOnLoad( gameObject );
        DontDestroyOnLoad( this );
    }

    void Start()
    {
        
    }

    void Update()
    {
        //if ( Application.isEditor )
		{
			for ( int i = 1; i <= 5; i++ )
			{
                if ( Input.GetKeyDown( KeyCode.Alpha0 + i ) )
				{
                    LoadLevel( i );
				}
			}
		}
    }
	#endregion

	#region Levels / Scenes
	public void LoadLevel( int level )
	{
        // Store current mech
        var mech = LocalPlayer.Instance.Player.GetComponentInParent<SpawnablePrefab>();
        if ( mech != null )
		{
            CrossoverMech = mech.PrefabName;
        }

        LoadScene( level );
	}

    public void LoadScene( int index )
	{
        if ( Loading ) return;

        StartCoroutine( Co_LoadScene( index ) );
	}

    IEnumerator Co_LoadScene( int index )
    {
        Loading = true;

        LocalPlayer.Instance.Player.LeaveRoom();
        Photon.Pun.PhotonNetwork.LeaveRoom();

        yield return SceneManager.LoadSceneAsync( index );

        Loading = false;
    }

    public void OnPlayerSpawnLoadCrossoverMech()
    {
        // Crossover mechs
        if ( CrossoverMech != "" )
        {
            GameObject mech = CreateCrossoverMech( CrossoverMech, SystemInfo.deviceUniqueIdentifier );
            var target = PhotonPlayerCreator.GetSpawn();
            mech.transform.position = target;
            mech.GetComponent<MechBody>().SetTargetPos( target );
            mech.GetComponentInChildren<MechCockpitDock>().Dock( LocalPlayer.Instance.Player.GetComponent<HeliCockpit>() );
            LocalPlayer.Instance.Player.CrossoverMech = CrossoverMech;

            // TODO could be moved to player function...
            LocalPlayer.Instance.Player.photonView.RPC( "SendCrossoverMech", RpcTarget.Others, CrossoverMech, SystemInfo.deviceUniqueIdentifier );
        }
        CrossoverMech = "";
    }
    #endregion

    public GameObject CreateCrossoverMech( string crossovermech, string id )
    {
        var mech = Instantiate( AllSpawnablePrefabs.GetPrefab( crossovermech ), FindObjectOfType<MechConnectJoin>().transform );
        {
            var newname = mech.name + "_" + id;
            var existing = GameObject.Find( newname );
            if ( existing != null )
            {
                Destroy( mech );
                mech = existing;
            }
            mech.name = newname;
        }
        return mech;
    }
}
