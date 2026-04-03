using System.Collections.Generic;
using Assets._Scripts.Datas;
using Assets._Scripts.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Tools.UI
{
    public class GroupDropdownSelector : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Dropdown _dropdown;
        public UnityEvent<string> OnGroupSelected = new();

        public void Show()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void Start()
        {
            List<string> groupNames = BlockGroupMapper.GetAllGroups();
            _dropdown.ClearOptions();
            _dropdown.AddOptions(groupNames);

            _dropdown.onValueChanged.AddListener((index) =>
            {
                if (index >= 0 && index < groupNames.Count)
                {
                    OnGroupSelected.Invoke(groupNames[index]);
                }
                Hide();
            });
        }
    }
}