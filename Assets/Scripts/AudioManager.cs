using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles audio for the game
/// </summary>
public class AudioManager : MonoBehaviour
{
    [SerializeField] private Sound[] sounds;

    /// <summary>
    /// Finds and plays sound from list of sounds
    /// </summary>
    /// <param name="name">name of sound</param>
    /// <returns>length of sound clip</returns>
    public float PlaySound(string name) {
        foreach (Sound s in sounds) {
            if (s.GetName() == name) {
                s.PlaySound();
                return s.GetSource().clip.length;
            }
        }

        Debug.LogWarning($"couldn't find sound {name}");
        return 0;
    }
}

[Serializable]
public class Sound
{
    [SerializeField] private AudioSource source;
    [SerializeField] private string name;
    
    public Sound(AudioSource source, string name) {
        this.source = source;
        this.name = name;
    }
    
    public string GetName() => name;
    public AudioSource GetSource() => source;
    public void PlaySound() => source.Play();
}