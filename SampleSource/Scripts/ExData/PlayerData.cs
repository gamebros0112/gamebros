using UnityEngine;
/// <summary>
/// 플레이어 관련 데이터 
/// </summary>
public class PlayerData {
	private static PlayerData s_instance = null;
	private string playerEmail;
	private string playerNickName;
	private Texture2D playerProfileImage;

	private bool isActor;
	private bool isBlink;

	private string playerID;
	private string playerToken;
	private string playerUniqueNo;
	private string playerRoomNo;

	private uint playerPoint;
	private uint playerIncomePoint;

	/// <summary>
    /// 인스턴스 생성 
    /// </summary>
	public static PlayerData instance
	{
		get
		{
			if (null == s_instance)
			{
				
				s_instance = new PlayerData();
				
				s_instance.init();
			}
			return s_instance;
		}
	}
	
	public void init(){
	}
	
    // 프로필 이미지 
	public Texture2D PlayerProfileImage { get => playerProfileImage; set => playerProfileImage = value; }

	// 닉네임 
	public string PlayerNickName { get => playerNickName; set => playerNickName = value; }

	// 이메일 
	public string PlayerEmail { get => playerEmail; set => playerEmail = value; }

	// 참여자인지 확인 
	public bool IsActor { get => isActor; set => isActor = value; }

	// 스폰후 반짝이는 상태인지 
	public bool IsBlink { get => isBlink; set => isBlink = value; }

	// 플레이어 아이디 ( 이메일주소 )
	public string PlayerID { get => playerID; set => playerID = value; }

	// 플레이어 로그인 토큰  
	public string PlayerToken { get => playerToken; set => playerToken = value; }

	// 서버에서 발행한 고유번호 
	public string PlayerUniqueNo { get => playerUniqueNo; set => playerUniqueNo = value; }

	// 내방 고유번호 
	public string PlayerRoomNo { get => playerRoomNo; set => playerRoomNo = value; }

	// 내 토탈 포인트 
	public uint PlayerPoint { get => playerPoint; set => playerPoint = value; }

	// 새로 입금된 포인트 
	public uint PlayerIncomePoint { get => playerIncomePoint; set => playerIncomePoint = value; }
}
