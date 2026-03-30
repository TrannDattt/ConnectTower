using System;
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
        //----------------------
        //TODO: Get scene from Resource
        [Serializable]
        public class GameScene
        {
            public EGameScene Key;
            public GameObject Scene;
        }

        [SerializeField] private List<GameScene> _scenes = new();
        //-----------------------

        private Dictionary<EGameScene, GameScene> _sceneDict = new();
        private GameScene _activeScene;

        public EGameScene GetActiveScene()
        {
            if (_activeScene != null) return _activeScene.Key;
            var activeScene = _scenes.First(gs => gs.Scene.activeInHierarchy);
            if (activeScene == null) return EGameScene.None;
            return activeScene.Key;
        }

        public void ChangeScene(EGameScene scene,  UnityAction onUnload = null, UnityAction onLoad = null)
        // public void ChangeScene(EGameScene scene,  UnityAction<Scene> onUnload = null, UnityAction<Scene> onLoad = null)
        {
            if (!_sceneDict.TryGetValue(scene, out var toLoad))
            {
                Debug.Log($"Scene {scene} does not exist");
                return;
            }

            if (_activeScene == toLoad) return;

            TryUnload(_activeScene);
            onUnload?.Invoke();
            // await SceneManager.LoadSceneAsync(toLoad.SceneName);
            TryLoad(toLoad);
            _activeScene = toLoad;
            onLoad?.Invoke();
            // SceneManager.sceneUnloaded += onUnload;
            // SceneManager.sceneLoaded += onLoad;
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
            UnloadAll();
        }

        //----------------------
        //TODO: Init first
        void Start()
        {
            // ChangeScene(EGameScene.Menu);
        }
        //-----------------------
    }
}