using UnityEngine;
using UnityEngine.Localization;
/// <summary>
/// 버튼 두개 팝업 
/// </summary>
[CreateAssetMenu(fileName = "TwoBtnPopup", menuName = "Popup/TwoBtnPopup")]
public class TwoBtnPopupSO : DefaultPopupSO
{
    [SerializeField] private LocalizedString _leftBtnLocalize = default;
    [SerializeField] private LocalizedString _rightBtnLocalize = default;

    public LocalizedString LeftBtnLocalize  => _leftBtnLocalize; 
    public LocalizedString RightBtnLocalize  => _rightBtnLocalize;

    private void Awake()
    {
        PopupType = ConstEventString.ModalType.TwoBtn;
    }
}
