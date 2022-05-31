using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "ThreeBtnPopup", menuName = "Popup/ThreeBtnPopup")]
public class ThreeBtnPopupSO : DefaultPopupSO
{
    [SerializeField] private LocalizedString _leftBtnLocalize = default;
    [SerializeField] private LocalizedString _rightBtnLocalize = default;
    [SerializeField] private LocalizedString _centerBtnLocalize = default;

    public LocalizedString LeftBtnLocalize => _leftBtnLocalize;
    public LocalizedString RightBtnLocalize => _rightBtnLocalize;
    public LocalizedString CenterBtnLocalize => _centerBtnLocalize;

    private void Awake()
    {
        PopupType = ConstEventString.ModalType.ThreeBtn;
    }
}
