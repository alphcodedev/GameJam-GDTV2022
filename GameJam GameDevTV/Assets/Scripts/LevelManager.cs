using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [SerializeField] private int levelNumber;

    public int LevelNumber => levelNumber;

    public Volume postProcessVolume;
    public VolumeProfile aliveProfile;
    public VolumeProfile ghostProfile;
    
    [Space]

    public GameObject pauseMenu;
    public SceneFader fader;
    public Transform respawnPoint;
    public PlayerController player;
    public ExitDoor exitDoor;
    public int keysNeeded = 0;
    public bool gamePaused;

    public GameObject[] reviveHearts;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        AudioManager.instance.StopSound("MainMenuTheme");
        AudioManager.instance.PlaySound("GhostTheme");
        
        if(!player.isGhost)
            AudioManager.instance.PlaySound("AliveTheme");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            PauseGame(!gamePaused);
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            fader.FadeTo(SceneManager.GetActiveScene().name);
        }
    }
    
    public void PauseGame(bool value)
    {
        pauseMenu.SetActive(value);
        gamePaused = value;
        if (gamePaused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void Respawn()
    {
        player.transform.position = respawnPoint.position;
        player.isGhost = true;
        
        AudioManager.instance.PauseSound("AliveTheme");
        postProcessVolume.profile = ghostProfile;
        
        EnableReviveHearts();
    }

   public void Revive()
    {
        AudioManager.instance.PlaySound("AliveTheme");
        postProcessVolume.profile = aliveProfile;
    }

   private void EnableReviveHearts()
    {
        foreach (var reviveHeart in reviveHearts)
        {
            reviveHeart.SetActive(true);
        }
    }

    public void PickUpKey()
    {
        if (keysNeeded != 0)
        {
            keysNeeded--;
            AudioManager.instance.PlaySound("PickUpKey");
        }

        if (keysNeeded == 0)
        {
            exitDoor.OpenDoor();
        }
    }

    public void WinLevel()
    {
        int nextLevel = levelNumber + 1;
        if (levelNumber < 10)
        {
            if(PlayerPrefs.GetInt("levelReached") < nextLevel)
                PlayerPrefs.SetInt("levelReached", nextLevel);
            
            fader.FadeTo("Level" + nextLevel);
        }
        else
        {
            AudioManager.instance.PlaySound("DeathSound");
            fader.FadeTo("MainMenu");
        }

        Debug.Log("Level Passed");
    }
}