using UnityEngine;

/// <summary>
/// 각 화면에 필요한 로컬라이즈 팝업을
/// ScriptableObject로 만들어 필요한거 할당해서 사용 
/// </summary>
public class BasePopupSO : ScriptableObject
{
    // 인스펙터 설명 내용 
	[TextArea] public string description;
    
}
