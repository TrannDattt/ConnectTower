using System.Collections.Generic;
using Assets._Scripts.Datas;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Tools.UI
{
    public class PillarDisplay : MonoBehaviour
    {
        [SerializeField] private Pillar _pillarPrefab;
        [SerializeField] private Transform _pillarParent;
        [SerializeField] private Button _addPillarButton;

        public int MaxPillars = 4;
        private int _lastPillarId = -1;

        public void OnAddPillarClicked()
        {
            if (_pillarPrefab != null && _pillarParent != null)
            {
                var newPillar = Instantiate(_pillarPrefab, _pillarParent);
                newPillar.SetPillarId(_lastPillarId + 1);
                _lastPillarId++;

                newPillar.OnPillarRemoved.AddListener(CheckFullPillars);
                LevelEditor.AddPillar(new PillarData 
                { 
                    Id = newPillar.PillarId,
                    BlockIds = new () { -1, -2, -3, -4 }
                });

                _addPillarButton.transform.SetAsLastSibling();
                CheckFullPillars();
            }
            else
            {
                Debug.LogWarning("Pillar prefab or parent is not assigned.");
            }
        }

        private void CheckFullPillars()
        {
            var pillarCount = _pillarParent.GetComponentsInChildren<Pillar>().Length;
            _addPillarButton.gameObject.SetActive(pillarCount < MaxPillars);
        }

        private void RemoveAllPillars()
        {
            var pillars = _pillarParent.GetComponentsInChildren<Pillar>();
            foreach (var pillar in pillars)
            {
                pillar.gameObject.SetActive(false);
                Destroy(pillar.gameObject);
            }
            _lastPillarId = -1;
            CheckFullPillars();
        }

        private void AddPillarsFromLevel(LevelJSON levelJSON)
        {
            foreach (var pillarData in levelJSON.PillarDatas)
            {
                var newPillar = Instantiate(_pillarPrefab, _pillarParent);
                newPillar.SetPillarId(pillarData.Id);
                newPillar.SetBlockIds(pillarData.BlockIds);

                newPillar.OnPillarRemoved.AddListener(CheckFullPillars);
            }
            _lastPillarId = levelJSON.PillarDatas.Count > 0 ? levelJSON.PillarDatas[^1].Id : -1;
            _addPillarButton.transform.SetAsLastSibling();
            CheckFullPillars();
        }

        void Start()
        {
            var pillars = _pillarParent.GetComponentsInChildren<Pillar>();
            foreach (var pillar in pillars)
            {
                Destroy(pillar.gameObject);
            }

            if (_addPillarButton != null)
            {
                _addPillarButton.onClick.AddListener(OnAddPillarClicked);
            }
            else
            {
                Debug.LogWarning("Add Pillar Button is not assigned.");
            }

            LevelEditor.OnLevelCleared.AddListener(RemoveAllPillars);
            LevelEditor.OnLevelLoaded.AddListener(AddPillarsFromLevel);
        }
    }
}