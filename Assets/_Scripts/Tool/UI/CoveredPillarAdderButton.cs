using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Tools.UI
{
    public class CoveredPillarAdderButton : MechanicAdderButton
    {
        [SerializeField] private Dropdown _tagDD;
        private string _selectedTag = "";

        protected override void ResetInputs()
        {
            base.ResetInputs();
            _tagDD.SetValueWithoutNotify(0);
        }

        protected override bool TryGetMechanicData(out MechanicRuntimeData data)
        {
            var tag = _selectedTag;
            if (string.IsNullOrEmpty(tag) || LevelEditor.GroupTags.All(groupTag => groupTag != tag))
            {
                Debug.Log("Tag cannot be empty for Covered Pillar mechanic.");
                data = null;
                return false;
            }

            data = new CoveredPillarMechanic(tag);
            return true;
        }

        protected override void AddMechanicIds(LevelJSON levelJSON)
        {
            foreach (var cpm in levelJSON.CoveredPillarDatas)
            {
                foreach (var id in cpm.PillarIds)
                {
                    _idInput.text = id.ToString();
                    _selectedTag = cpm.TagToOpen;
                    AddIdFromLevel();
                }
            }
        }

        protected override void Start()
        {
            _mechanicType = EMechanic.CoveredPillar;
            base.Start();
            _tagDD.SetValueWithoutNotify(0);
        }

        void Awake()
        {
            var groupNames = BlockGroupMapper.GetAllGroups().Where(s => !LevelEditor.GroupTags.Contains(s)).ToList();
            groupNames.Insert(0, "");
            _tagDD.ClearOptions();
            _tagDD.AddOptions(groupNames);

            _tagDD.onValueChanged.AddListener((val) =>
            {
                _selectedTag = _tagDD.options[val].text;
            });
        }
    }
}