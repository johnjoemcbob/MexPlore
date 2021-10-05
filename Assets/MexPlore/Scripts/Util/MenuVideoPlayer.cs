using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class MenuVideoPlayer : MonoBehaviour
{
    void Start()
    {
        GetComponent<VideoPlayer>().url = Application.streamingAssetsPath + "/" + "menuvid.mp4";
        GetComponent<VideoPlayer>().Play();
        GetComponent<VideoPlayer>().time = 5;
    }

	private void Update()
	{
        var player = GetComponent<VideoPlayer>();
        if ( !player.isPlaying )
		{
            player.Play();
		}
    }
}
