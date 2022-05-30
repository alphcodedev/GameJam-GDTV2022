using System;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f,1f)]
    public float volume = 0.7f;
    [Range(.1f,3f)]
    public float pitch = 1f;

    public bool loop;
    
    private AudioSource source;
    
    public void SetSource(AudioSource source)
    {
        this.source = source;
        this.source.clip = clip;
        this.source.volume = volume;
        this.source.pitch = pitch;
        this.source.loop = loop;
    }

    public void Play()
    {
        source.Play();
    }
    
    public void Pause()
    {
        source.Pause();
    }
    
    public void Stop()
    {
        source.Stop();
    }
}


public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        
        foreach (var s in sounds)
        {
            GameObject _go = new GameObject("Sound_" + s.name);
            _go.transform.SetParent(this.transform);
            
            s.SetSource(_go.AddComponent<AudioSource>());
        }
    }
    
    private void Start()
    {
        // PlaySound("MainMenuTheme");
    }

    public void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.Play();
    }
    
    public void StopSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.Stop();
    }
    
    public void PauseSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.Pause();
    }
}