using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Assets._Scripts.Managers
{
    public class GameSceneManager : Singleton<GameSceneManager>
    {
        [Serializable]
        public class GameScene
        {
            public EGameScene Key;
            public GameObject Scene;
        }

        [SerializeField] private List<GameScene> _scenes = new();

        private Dictionary<EGameScene, GameScene> _sceneDict = new();
        private GameScene _activeScene;

        public EGameScene GetActiveScene()
        {
            if (_activeScene != null) return _activeScene.Key;
            _activeScene = _scenes.FirstOrDefault(gs => gs.Scene != null && gs.Scene.activeInHierarchy);
            return _activeScene?.Key ?? EGameScene.None;
        }

        public void ChangeScene(EGameScene scene,  UnityAction onUnload = null, UnityAction onLoad = null)
        {
            EBgm bgm = scene switch
            {
                EGameScene.Menu => EBgm.MenuMusic,
                EGameScene.Ingame => EBgm.IngameMusic,
                _ => EBgm.None
            };
            onLoad += () => SoundManager.Instance.PlayBGM(bgm);
            StartCoroutine(DoChangeScene(scene, onUnload, onLoad));
        }

        private IEnumerator DoChangeScene(EGameScene scene, UnityAction onUnload = null, UnityAction onLoad = null)
        {
            if (!_sceneDict.TryGetValue(scene, out var toLoad))
            {
                Debug.Log($"Scene {scene} does not exist");
                yield break;
            }

            if (_activeScene == toLoad) yield break;

            yield return PopupManager.Instance.ShowPopup(EPopup.Loading);

            TryUnload(_activeScene);
            onUnload?.Invoke();
            TryLoad(toLoad);
            _activeScene = toLoad;

            yield return new WaitForSeconds(2f);
            yield return PopupManager.Instance.HidePopup(EPopup.Loading);
            onLoad?.Invoke();
        }

        private void UnloadAll()
        {
            foreach(var scene in _scenes) TryUnload(scene);
        }

        private bool TryUnload(GameScene scene)
        {
            if (scene == null || scene.Scene == null) return false;
            scene.Scene.SetActive(false);
            return true;
        }

        private bool TryLoad(GameScene scene)
        {
            if (scene == null || scene.Scene == null) return false;
            scene.Scene.SetActive(true);
            return true;
        }

        protected override void Awake()
        {
            base.Awake();

            _scenes.ForEach(gs => _sceneDict[gs.Key] = gs);
            Debug.Log($"Found {_scenes.Count} scenes");
            // UnloadAll();
        }
        
        void Start()
        {
            if(_activeScene == null) GetActiveScene();
        }
    }
}