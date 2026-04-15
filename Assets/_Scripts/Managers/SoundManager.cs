using System;
using System.Collections;
using System.Collections.Generic;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using UnityEngine;

namespace Assets._Scripts.Managers
{
    public class SoundManager : Singleton<SoundManager>
    {
        [Serializable]
        private struct GameSFX
        {
            public ESfx Key;
            public List<AudioClip> Clips;
        }

        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _sfxSource;
        
        [Header("BGM")]
        [SerializeField] private AudioClip _menuBGM;
        [SerializeField] private AudioClip _ingameBGM;

        [Header("SFX")]
        [SerializeField] private List<GameSFX> _gameSFX;

        private Dictionary<ESfx, List<AudioClip>> _sfxDict = new();

        public bool IsEnable {get; private set;} = true;

        public void SetEnable(bool state) => IsEnable = state;

        private AudioClip GetBGM(EBgm key) => key switch
        {
            EBgm.MenuMusic => _menuBGM,
            EBgm.IngameMusic => _ingameBGM,
            _ => null
        };
        
        private List<AudioClip> GetSFXs(ESfx key) => _sfxDict.TryGetValue(key, out var sounds) ? sounds : null;

        public void PlayBGM(EBgm key)
        {
            _bgmSource.Stop();
            if (!IsEnable) return;
            var toPlay = GetBGM(key);
            if (toPlay == null)
            {
                Debug.Log($"Play BGM {key}");
                return;
            }

            _bgmSource.clip = toPlay;
            _bgmSource.Play();
        }

        public void PlayRandomSFX(ESfx key)
        {
            if (!IsEnable) return;
            var sounds = GetSFXs(key);
            if (sounds == null || sounds.Count == 0)
            {
                Debug.Log($"Play random SFX of type {key}");
                return;
            }

            var toPlay = sounds[UnityEngine.Random.Range(0, sounds.Count)];
            _sfxSource.PlayOneShot(toPlay);
        }

        public void PlayChainedSFXs(ESfx key, int chainCount)
        // public void PlayChainedSFXs(ESfx key, int chainCount, float timeOffset)
        {
            if (!IsEnable) return;
            var sounds = GetSFXs(key);
            _sfxSource.PlayOneShot(sounds[chainCount - 1]);
            // if (sounds == null)
            // {
            //     Debug.Log($"Play chain {chainCount} SFXs of type {key}");
            //     return;
            // }

            // StartCoroutine(PlayChainedSFXs(sounds, chainCount, timeOffset));
        }

        private IEnumerator PlayChainedSFXs(List<AudioClip> clips, int chainCount, float timeOffset)
        {
            var delay = new WaitForSeconds(timeOffset);

            for (int i = 0; i < chainCount; i++)
            {
                _sfxSource.PlayOneShot(clips[i]);
                yield return delay;
            }
        }

        protected override void Awake()
        {
            foreach(var sfx in _gameSFX)
            {
                _sfxDict[sfx.Key] = sfx.Clips;
            }
        }
    }
}