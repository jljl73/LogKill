using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LogKill.Core
{
    public enum EBGM
    {

    }

    public enum ESFX
    { 
        Walking, Death
    }


    public class SoundManager : MonoSingleton<SoundManager>
    {
        private const string _bgmLabel = "BGM";
        private const string _sfxLabel = "SFX";

        [Header("BGM Setting")]
        [SerializeField] private AudioSource _bgmPlayer;
        [Range(0f, 1f)]
        [SerializeField] private float _bgmVolume = 1f;

        [Header("SFX Setting")]
        [SerializeField] private AudioSource _sfxPlayer;
        [Range(0f, 1f)]
        [SerializeField] private float _sfxVolume = 1f;

        private Dictionary<string, AudioClip> _bgmClips = new();
        private Dictionary<string, AudioClip> _sfxClips = new();


        public async UniTask InitializeAsync()
        {
            // BGM Initialize
            _bgmPlayer.playOnAwake = false;
            _bgmPlayer.loop = true;
            _bgmPlayer.volume = _bgmVolume;

            // SFX Initialize
            _sfxPlayer.playOnAwake = false;
            _sfxPlayer.loop = false;
            _sfxPlayer.volume = _sfxVolume;

            await LoadAllClipAsync(_bgmLabel, _bgmClips);
            await LoadAllClipAsync(_sfxLabel, _sfxClips);
        }

        public void PlayBGM(EBGM bgm)
        {
            if (_bgmClips.TryGetValue(bgm.ToString(), out AudioClip clip))
            {
                _bgmPlayer.clip = clip;
                _bgmPlayer.Play();
            }
            else
            {
                Debug.LogWarning($"[BGM] - {bgm} 을 찾을 수 없습니다.");
            }
        }

        public void StopBGM()
        {
            _bgmPlayer.Stop();
        }

        public void PlaySFX(ESFX sfx)
        {
            if (_sfxClips.TryGetValue(sfx.ToString(), out AudioClip clip))
            {
                _sfxPlayer.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"[SFX] - {sfx} 을 찾을 수 없습니다.");
            }
        }

        private async UniTask LoadAllClipAsync(string label, Dictionary<string, AudioClip> targetDict)
        {
            var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
            await locationsHandle.Task;

            if (locationsHandle.Status != AsyncOperationStatus.Succeeded || locationsHandle.Result.Count == 0)
            {
                Debug.LogWarning($"[SoundManager] '{label}' 라벨에 해당하는 오디오 리소스가 존재하지 않습니다.");
                return;
            }

            var handle = Addressables.LoadAssetsAsync<AudioClip>(label, clip =>
            {
                if (!targetDict.ContainsKey(clip.name))
                    targetDict.Add(clip.name, clip);
            });

            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"[{label}] 오디오 클립 로드 성공");
            }
            else
            {
                Debug.LogError($"[{label}] 오디오 클립 로드 실패");
            }
        }
    }
}
