using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace DnD
{
    public class SoundManager : MonoBehaviour, IInitializer
    {
        public static SoundManager Instance { get; private set; }

        [SerializeField, Header("Other")]
        private AudioSource source;
        [SerializeField]
        private List<AudioClip> clicks;
        [SerializeField]
        private List<AudioClip> negative;
        [SerializeField]
        private List<AudioClip> switches;

        public void InitializeSelf()
        {
            Instance = this;
        }

        public void InitializeAfter()
        {
        }

        public void PlayClick()
        {
            source.PlayOneShot(clicks.Random());
        }

        public void PlayNegative()
        {
            source.PlayOneShot(negative.Random());
        }

        public void PlaySwitch()
        {
            source.PlayOneShot(switches.Random());
        }

        public void Play(AudioClip clip, float volume = 1f)
        {
            source.PlayOneShot(clip, volume);
        }
    }
}