using System.Collections.Generic;
using Assets._Scripts.Datas;
using Assets._Scripts.Managers;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class LevelHolderVisual : MonoBehaviour
    {
        [SerializeField] private Transform _levelContainer;
        [SerializeField] private LevelButtonVisual _levelButtonPrefabs;

        private List<LevelButtonVisual> _buttons = new();

        //TODO: Use pooling to manage level buttons
        //TODO: Add behaviors to button: Auto focus, scale when scroll, button change color,...

        public void InitVisual(List<LevelRuntimeData> levelDatas)
        {
            for (int i = _buttons.Count; i < levelDatas.Count; i++)
            {
                _buttons.Add(Instantiate(_levelButtonPrefabs, _levelContainer));
            }
            UpdateVisual(levelDatas);
        }

        public void UpdateVisual(List<LevelRuntimeData> levelDatas)
        {
            for(int i = 0; i < levelDatas.Count; i++)
            {
                _buttons[i].UpdateVisual(levelDatas[i]);
            }
        }

        public void ClearAllButtons()
        {
            var buttons = _levelContainer.GetComponentsInChildren<LevelButtonVisual>();
            foreach (var button in buttons)
            {
                Destroy(button.gameObject);
            }
        }

        // void Start()
        // {
        //     ClearAllButtons();

        //     foreach(var data in LevelManager.Instance.GetAllLevels())
        //     {
        //         var newButton = Instantiate(_levelButtonPrefabs, _levelContainer);
        //         newButton.InitVisual(data);
        //     }
        // }
    }
}