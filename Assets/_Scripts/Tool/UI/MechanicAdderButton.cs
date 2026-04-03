using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Tools.UI
{
    public abstract class MechanicAdderButton : MonoBehaviour
    {
        [SerializeField] protected TMP_InputField _idInput;
        [SerializeField] protected Button _addIdButton;
        [SerializeField] protected Transform _idContainer;
        [SerializeField] protected IdButton _idDisplayPrefab;

        protected EMechanic _mechanicType;
        protected MechanicRuntimeData MechanicData
        {
            get
            {
                if (TryGetMechanicData(out MechanicRuntimeData data))
                {
                    return data;
                }
                return null;
            }
        }

        protected List<int> _ids = new();

        protected virtual void ResetInputs()
        {
            _idInput.text = "";
        }

        protected int GetInputId()
        {
            string idString = _idInput.text.Trim();
            if (string.IsNullOrEmpty(idString)) return -1;
            if (!int.TryParse(idString, out int id))
            {
                Debug.Log($"Invalid id format: {idString}");
                return -1;
            }

            if (_mechanicType == EMechanic.CoveredPillar)
            {
                if (!LevelEditor.PillarDatas.Any(pillar => pillar.Id == id))
                {
                    Debug.Log($"Cant find pillar with id {idString}");
                    return -1;
                }
                else
                {
                    return id;
                }
            }

            if(!LevelEditor.BlockDatas.Any(block => block.Id == id))
            {
                Debug.Log($"Cant find block with id {idString} for mechanic {_mechanicType}");
                return -1;
            }
            return id;
        }

        protected abstract bool TryGetMechanicData(out MechanicRuntimeData data);

        protected virtual void AddIdToLevel()
        {
            int id = GetInputId();
            if (id == -1) return;

            if (MechanicData == null) return;
            if(LevelEditor.TryAddMechanic(id, MechanicData))
            {
                var newIdButton = Instantiate(_idDisplayPrefab, _idContainer);
                newIdButton.SetId(id);
                newIdButton.OnRemoveClicked.AddListener(RemoveId);
            }
            else
            {
                Debug.Log($"Id {id} already has mechanic {_mechanicType}");
            }

            ResetInputs();
        }

        protected virtual void AddIdFromLevel()
        {
            int id = GetInputId();
            if (id == -1) return;

            var newIdButton = Instantiate(_idDisplayPrefab, _idContainer);
            newIdButton.SetId(id);
            newIdButton.OnRemoveClicked.AddListener(RemoveId);

            ResetInputs();
        }

        private void RemoveId(int id)
        {
            LevelEditor.RemoveMechanic(id, _mechanicType);
        }

        private void ClearAllMechnics()
        {
            var idButtons = _idContainer.GetComponentsInChildren<IdButton>();
            foreach (var idButton in idButtons)
            {
                Destroy(idButton.gameObject);
            }

            ResetInputs();
        }

        protected abstract void AddMechanicIds(LevelJSON levelJSON);

        protected virtual void Start()
        {
            var idButtons = _idContainer.GetComponentsInChildren<IdButton>();
            foreach (var idButton in idButtons)
            {
                Destroy(idButton.gameObject);
            }

            _addIdButton.onClick.AddListener(AddIdToLevel);
            LevelEditor.OnLevelCleared.AddListener(ClearAllMechnics);
            LevelEditor.OnLevelLoaded.AddListener(AddMechanicIds);
        }
    }
}