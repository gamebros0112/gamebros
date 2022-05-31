using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

/// <summary>
/// 기본 팝업 정보 
/// </summary>
[CreateAssetMenu(fileName = "DefaultPopup", menuName = "Popup/DefaultPopup")]
public class DefaultPopupSO : BasePopupSO
{
	[Space]
	[Header("Popup Data Setting")]
	[SerializeField] private LocalizedString _titleLocalize = default;
	[SerializeField] private LocalizedString _contentsLocalize = default;
	
	//팝업 타이틀 로컬라이즈 스트링
	//팝업 내용 로컬라이즈 스트링 
	public LocalizedString TitleLocalize => _titleLocalize;
	public LocalizedString ContentsLocalize => _contentsLocalize;

	// 닫기 버튼 이벤트 
    public UnityAction OnCloseEventRaised;
	// 팝업 지정 
	private ConstEventString.ModalType _popupType = ConstEventString.ModalType.NoBtn;
	public ConstEventString.ModalType PopupType { get => _popupType; set => _popupType = value; }

	
	//public void PopupCloseEvent()
	//{
	//	OnCloseEventRaised?.Invoke();
	//}


}
