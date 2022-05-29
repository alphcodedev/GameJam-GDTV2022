using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public SceneFader fader;
    public Transform respawnPoint;
    public Transform player;
    public Transform finishTrigger;

    private void Awake()
    {
        instance = this;
    }
    
    public void Respawn()
    {
        player.position = respawnPoint.position;
    }

    public void WinLevel()
    {
        // fader.FadeTo("");   
        Debug.Log("Win");
    }
}
