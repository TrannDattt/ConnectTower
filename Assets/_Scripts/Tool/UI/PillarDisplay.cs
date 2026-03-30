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
            if (pillarCount >= MaxPillars)
            {
                _addPillarButton.gameObject.SetActive(false);
            }
            else
            {
                _addPillarButton.gameObject.SetActive(true);
            }
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
        }
    }
}