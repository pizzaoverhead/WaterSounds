using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WaterSounds
{
    public static class Utils
    {
        public static AudioSource InitAudioSource(string path, GameObject gameObject, float volume, bool loop, bool randomiseStart)
        {
            if (!GameDatabase.Instance.ExistsAudioClip(path))
            {
                Debug.LogError("[WaterSounds] InitialiseFXGroup: Audio file not found: " + path);
                return null;
            }
            else
            {
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                if (audioSource == null)
                {
                    Debug.LogError("[WaterSounds] InitialiseFXGroup: Unable to create AudioSource for file: " + path);
                    return null;
                }
                audioSource.clip = GameDatabase.Instance.GetAudioClip(path);
                if (audioSource.clip == null)
                {
                    Debug.LogError("[WaterSounds] InitialiseFXGroup: Unable to find file: " + path);
                    return null;
                }

                audioSource.loop = loop;
                audioSource.volume = volume;
                audioSource.dopplerLevel = 0f;
                audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
                audioSource.minDistance = 0.5f;
                audioSource.maxDistance = 1f;
                if (randomiseStart)
                    audioSource.time = UnityEngine.Random.Range(0, audioSource.clip.length);

                return audioSource;
            }
        }
    }
}
