using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 룸 관련 데이터 
/// </summary>
public class RoomData 
{
	private Dictionary<string, GameObject> actorDummys = new Dictionary<string, GameObject>();
	private string enterRoomNo;
	private RoomPropertiesInfo roomInfo;
	private bool isLikeRoom;
	private bool isMyRoom;
	private static RoomData s_instance = null;

	// 인스턴스 가져오기 
	public static RoomData instance
	{
		get
		{
			if (null == s_instance)
			{

				s_instance = new RoomData();

				s_instance.init();
			}
			return s_instance;
		}
	}
	public void init()
	{

	}
	// 입장한방 고유번호 
	public string EnterRoomNo 
	{ 
		get => enterRoomNo; 
		set => enterRoomNo = value; 
	}
	// 현재 방정보 
	public RoomPropertiesInfo RoomInfo 
	{ 
		get => roomInfo; 
		set => roomInfo = value; 
	}
	// 더미 참여자 정보 
	public Dictionary<string, GameObject> ActorDummys 
	{ 
		get => actorDummys; 
		set => actorDummys = value; 
	}
	// 좋아요 클릭 유무
	public bool IsLikeRoom 
	{ 
		get => isLikeRoom; 
		set => isLikeRoom = value; 
	}
	// 내방인지 판별 
	public bool IsMyRoom
	{
		get => isMyRoom;
		set => isMyRoom = value;
	}
}


