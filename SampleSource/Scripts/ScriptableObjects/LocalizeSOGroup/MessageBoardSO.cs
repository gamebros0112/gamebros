using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

/// <summary>
/// 버튼 없는 메세지용 팝업 
/// </summary>
[CreateAssetMenu(fileName = "MessageBoard", menuName = "MessageBoard")]
public class MessageBoardSO : BasePopupSO
{
	[Space]
	[Header("MessageBoard Text Setting")]
	[SerializeField] private LocalizedString _contentsLocalize = default;
	public LocalizedString ContentsLocalize => _contentsLocalize;
}
