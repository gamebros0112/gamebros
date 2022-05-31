using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "OneBtnPopup", menuName = "Popup/OneBtnPopup")]
public class OneBtnPopupSO : DefaultPopupSO
{
    [SerializeField] private LocalizedString _oneBtnLocalize = default;
    public LocalizedString OneBtnLocalize  => _oneBtnLocalize;

    private void Awake()
    {
        PopupType = ConstEventString.ModalType.OneBtn;
    }
}
