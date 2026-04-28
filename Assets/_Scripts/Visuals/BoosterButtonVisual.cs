using System.Collections;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public partial class BoosterButtonVisual : GameButtonVisual
    {
        [SerializeField] private Image _lockImage;
        [SerializeField] private Image _lockBackground;
        [SerializeField] private GameObject _baseContent;
        [SerializeField] private Text _countText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _getMoreImage;
        [SerializeField] private BoosterButtonEffectVisual _effectVisual;

        public bool IsLocked {get; private set;}

        public void ChangeLockStatus(bool isLock)
        {
            if (isLock) Lock();
            else Unlock();
        }

        public void Lock()
        {
            IsLocked = true;
            SetEnable(false);
            _lockImage.gameObject.SetActive(true);
            _lockBackground.gameObject.SetActive(true);
            _baseContent.SetActive(false);
        }

        public void Unlock()
        {
            IsLocked = false;
            SetEnable(true);
            _lockImage.gameObject.SetActive(false);
            _lockBackground.gameObject.SetActive(false);
            _baseContent.SetActive(true);
        }

        public void SetCount(int count)
        {
            if (count > 0)
            {
                _countText.text = $"{count}";
                _countText.gameObject.SetActive(true);
                _getMoreImage.gameObject.SetActive(false);
            }
            else
            {
                _getMoreImage.gameObject.SetActive(true);
            }
        }

        public IEnumerator DoOnUseBoosterAnim(BoosterRuntimeData data, Vector3 gatherPoint) => _effectVisual?.DoOnUseBoosterAnim(data, gatherPoint);

        public void ShowPopupText()
        {
            PopupManager.Instance.ShowPopupText("Locked", GetCenterPosition());
        }
    }
}