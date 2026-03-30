using System.Collections.Generic;
using Assets._Scripts.Datas;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class LevelHolderVisual : MonoBehaviour
    {
        [SerializeField] private Transform _levelContainer;
        [SerializeField] private LevelButtonVisual _levelButtonPrefabs;

        //TODO: Use pooling to manage level buttons

        public void InitVisual(List<LevelRuntimeData> levelDatas)
        {
            ClearAllButtons();

            foreach(var data in levelDatas)
            {
                var newButton = Instantiate(_levelButtonPrefabs, _levelContainer);
                newButton.InitVisual(data);
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
    }
}