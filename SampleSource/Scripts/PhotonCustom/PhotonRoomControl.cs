using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;
using System.IO;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Newtonsoft.Json;
using EasyUI.Dialogs;
using System;

public class PhotonRoomControl : MonoBehaviourPunCallbacks
{

	[Tooltip("The prefab to use for representing the player")]
	[SerializeField] private GameObject playerPrefab;
	[SerializeField] private GameObject actorDummyPrefab;
	[SerializeField] private string gameVersion = "1";
	[SerializeField] private byte maxPlayersPerRoom = 15;
	[SerializeField] private JSGate jsgate;
	[SerializeField] DefaultPopupSO disconnectPopup;
	[SerializeField] Vector3 playerOffsetPos;
	[SerializeField] Quaternion playerOffsetRot;

	private string roomName;
	private Coroutine playerMoveCoroutine;


	public Action OnCompleteJoinedRoom;

    public byte MaxPlayersPerRoom { get => maxPlayersPerRoom; }

    void Awake()
    {
		PhotonNetwork.AutomaticallySyncScene = true;
	}

    public override void OnEnable()
    {
		base.OnEnable();
		RoomManager.GetInstance.OnLoadedPlayer += OnLoadedPlayerReceiver;
		WebSocketEventSender.GetInstance.OnCharFaceNotiPushEvent += OnFaceNoti;
    }
	public override void OnDisable()
	{
		base.OnDisable();
		RoomManager.GetInstance.OnLoadedPlayer -= OnLoadedPlayerReceiver;
		WebSocketEventSender.GetInstance.OnCharFaceNotiPushEvent -= OnFaceNoti;
	}

    private void OnFaceNoti(string _userNo, string _face)
    {
		Debug.Log("onFaceNoti");
		if (PlayerData.instance.IsActor)
		{
			Player[] players = PhotonNetwork.PlayerList;
			foreach (Player p in players)
			{
				string uniqueNo = p.CustomProperties["playerUniqueNo"].ToString();
				Debug.Log("uniqueNo : "+ uniqueNo+ " / _userNo : " + _userNo);
				if (uniqueNo == _userNo)
				{
					Debug.Log("onFaceNoti ok.....");
					((GameObject)p.TagObject).GetComponent<PhotonPlayerControl>().OnFaceNoti(Utility.GetFaceNotiTexture(_face));
					break;
				}

			}
		}
		else
		{
			RoomData.instance.ActorDummys[_userNo].GetComponent<ActorDummyControl>().OnFaceNoti(Utility.GetFaceNotiTexture(_face));
		}
	}

    /// <summary>
    /// 내방에 들어왔을때 내가 방장인지 체크해서 아니면 방장 권한 획득 
    /// </summary>
    /// <param name="obj"></param>
    private void OnLoadedPlayerReceiver(GameObject obj)
    {
        if (RoomData.instance.IsMyRoom)
        {
			if(!PhotonNetwork.IsMasterClient) PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
		}
    }

    /// <summary>
    /// 파이어베이스 데이터로드 완료후 포톤 연결을 위해 호출
    /// </summary>
    public void StartPhotonRoom()
    {
		Debug.Log("StartPhotonRoom  PhotonNetwork.IsConnected : "+ PhotonNetwork.IsConnected);

		roomName = RoomData.instance.EnterRoomNo;

		// 비로그인 유저인 경우 
        if (string.IsNullOrEmpty(PlayerData.instance.PlayerUniqueNo))
        {
			PhotonNetwork.OfflineMode = true;
		}
		// 로그인 유저인 경우 
        else
        {
			if (PlayerData.instance.IsActor)
			{
				Debug.Log("Photon Actor start");
				PhotonNetwork.ConnectUsingSettings();
			}
			else
			{
				Debug.Log("Spectator start ");
				PhotonNetwork.OfflineMode = true;
			}
		}
		
		
	}

	private void CreateRoom()
    {
		if (PhotonNetwork.InRoom)
		{
			// 방안에 있으면 조인 상태로 넘김 
			//OnJoinedRoom();
			
		}
		else
		{
			// 방에 들어오기 전이면 방생성 

			/*if (string.IsNullOrEmpty(PhotonRoomData.PhotonRoomName)) {
				roomName = "Room " + UnityEngine.Random.Range(1000, 10000);
			}
			else
			{
				roomName = PhotonRoomData.PhotonRoomName;
			}*/

			// 비로그인 유저인 경우 랜덤 닉네임 지정 Guest1234
			if (string.IsNullOrEmpty(PlayerData.instance.PlayerNickName)) PlayerData.instance.PlayerNickName = "Guest_" + UnityEngine.Random.Range(1000, 10000);
			PhotonNetwork.LocalPlayer.NickName = PlayerData.instance.PlayerNickName;
			// RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers, PlayerTtl = 10000 };
			RoomOptions roomOptions = new RoomOptions();
			// 플레이어 퇴장시 오브젝트 제거 옵션 
			//roomOptions.CleanupCacheOnLeave = false;  
			roomOptions.MaxPlayers = MaxPlayersPerRoom;
			/*roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
			roomOptions.CustomRoomProperties.Add(RoomDataManager.OwnerEmail, PhotonNetwork.LocalPlayer.NickName);
			roomOptions.CustomRoomProperties.Add(RoomDataManager.RoomTitle, roomName);
			roomOptions.CustomRoomProperties.Add(RoomDataManager.RoomType, "Room8X12X4");
			roomOptions.CustomRoomProperties.Add(RoomDataManager.PlayerList, new string[0]);
			roomOptions.CustomRoomProperties.Add(RoomDataManager.ImageGroup, new string[0]);
			roomOptions.CustomRoomProperties.Add(RoomDataManager.MusicGroup, new string[0]);
			roomOptions.CustomRoomProperties.Add(RoomDataManager.ModelGroup, new string[0]);
			roomOptions.CustomRoomProperties.Add(RoomDataManager.VideoGroup, new string[0]);
			roomOptions.CustomRoomProperties.Add(RoomDataManager.DocGroup, new string[0]);
			roomOptions.CustomRoomPropertiesForLobby = new string[] { roomName, "TestLobbyProperties" };*/

			PhotonNetwork.JoinOrCreateRoom(roomName,roomOptions,null);
		}
	}
	public bool IsInRoom()
    {
		return PhotonNetwork.InRoom;
    }
	public override void OnConnectedToMaster()
	{
		Debug.Log("OnConnectedToMaster");
		CreateRoom();
	}
    public override void OnCreatedRoom()
    {
		Debug.Log("OnCreatedRoom PhotonNetwork.OfflineMode: " + PhotonNetwork.OfflineMode);
		base.OnCreatedRoom();
		if(!PhotonNetwork.OfflineMode )RoomManager.GetInstance._baseServerManager.UpdateAsset("/room/update/manager", "");
	}
    public override void OnJoinedRoom()
	{
		Debug.Log("OnJoinedRoom room name : " + PhotonNetwork.CurrentRoom.Name);

		//플레이어 프리팹 로드 
		//SpawnPlayer();

		//방장인지 체크하고 아니면 에디트 버튼 숨기기
		CheckRoomMaster();
		
		//방에 입장한 플레이어 정보 웹에 전달
		SendPlayerList();

		// 방장이면 참여자 위치정보 서버로 보내기
		if (PhotonNetwork.IsMasterClient && !PhotonNetwork.OfflineMode)
			playerMoveCoroutine = StartCoroutine(SendActorsTransform()); 
	}

    private IEnumerator SendActorsTransform()
    {
		while (true)
        {
			if (!PhotonNetwork.IsMasterClient) yield break;
			yield return new WaitForSecondsRealtime(10.0f);

			Player[] players = PhotonNetwork.PlayerList;
			List<ActorsMoveInfo> playerList = new List<ActorsMoveInfo>();
			foreach (Player p in players)
			{
				ActorsMoveInfo ami = new ActorsMoveInfo();
				// 플레이어 오브젝트 이름 가져옴 ( 이름 == 플레이어 고유번호)
				GameObject playerObj = p.TagObject as GameObject;
                if (playerObj != null)
                {
					ami.userNo = p.CustomProperties["playerUniqueNo"].ToString();
					ami.trace = Utility.GetDecimalTransformNoneScale(playerObj.transform);
					playerList.Add(ami);
				}
				
			}
			string playerJsonList = JsonConvert.SerializeObject(playerList);
			RoomManager.GetInstance._baseServerManager.UpdateAsset("/room/user/move", playerJsonList);
		}
    }

    /// <summary>
    /// 방입장시 내방인지 먼저 체크
    /// @@ 타인방인 경우
    /// 에셋 생성, 편집 버튼 감추기
    /// 룸에셋이 있는지 체크 없으면 바로 캐릭터 생성
    /// 룸에셋이 있으면 관전모드인지 체크해서 관전이면 바로 룸에셋 생성
    /// 관전이 아니면 방장인 경우만 룸에셋 생성
    /// 
    /// @@ 내방인 경우
    /// 에셋 생성, 편집 버튼 보이게
    /// 룸에셋이 있는지 체크 없으면 바로 캐릭터 생성
    /// 룸에셋이 있으면 방장인 경우만 룸에셋 생성 
    /// (내방에는 언제나 마음껏 들어올수 있으므로 내방 들어올때는 관전모드 발생하지 않음)
    /// </summary>
    private void CheckRoomMaster()
	{
		RoomData rdata = RoomData.instance;
		
		if (PlayerData.instance.PlayerRoomNo != roomName)
		{
			rdata.IsMyRoom = false;
			UISystem.GetInstance.ToggleRightBottomAlignGroup(false);
			//에셋 없으면 바로 캐릭터 생성
			if (RoomManager.GetInstance.assetDataList == null)
				SpawnPlayer();
            else
            {
				if (PhotonNetwork.OfflineMode) 
				{
					OnCompleteJoinedRoom?.Invoke();
				}
                else
                {
					if(PhotonNetwork.IsMasterClient) OnCompleteJoinedRoom?.Invoke();
				}
			}
		}
        else
        {
			AddressableManager addressMgr = UIButtonEventGate.GetInstance.GetComponent<AddressableManager>();

			rdata.IsMyRoom = true;
			UISystem.GetInstance.ToggleRightBottomAlignGroup(true);

            if (addressMgr.IsActiveAddressableBtn)
            {
				UISystem.GetInstance.ToggleAddressableBtn(true);
			}
            else
            {
				// 오피셜 계정인 경우만 어드레서블 입력 버튼 활성화
				if(rdata.RoomInfo.types!="N") UISystem.GetInstance.ToggleAddressableBtn(true);
			}

			//에셋 없으면 바로 캐릭터 생성
			if (RoomManager.GetInstance.assetDataList==null)
				SpawnPlayer();
            else
            {
				if (PhotonNetwork.IsMasterClient) OnCompleteJoinedRoom?.Invoke();
			}
		}
		
	}


	#region Photon Callbacks

	/// <summary>
	/// Called when a Photon Player got connected. We need to then load a bigger scene.
	/// 참여자 입장시 호출
	/// </summary>
	/// <param name="other">Other.</param>
	public override void OnPlayerEnteredRoom(Player other)
	{
		Debug.Log("OnPlayerEnteredRoom " + other.NickName);
		SendPlayerList();
		
		/*if (PhotonNetwork.IsMasterClient)
		{
			Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
		}*/
	}

	/// <summary>
	/// Called when a Photon Player got disconnected. We need to load a smaller scene.
	/// 참여자 퇴장시 호출
	/// </summary>
	/// <param name="other">Other.</param>
	public override void OnPlayerLeftRoom(Player other)
	{
		SendPlayerList();
		Debug.Log("OnPlayerLeftRoom() " + other.NickName); // seen when other disconnects
		
		//PhotonNetwork.Destroy(other);
		/*if (PhotonNetwork.IsMasterClient)
		{
			Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
		}*/
	}

	/// <summary>
	/// Called when the local player left the room. We need to load the launcher scene.
	/// </summary>
	public override void OnLeftRoom()
	{
		PhotonNetwork.Disconnect();
		
	}
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);

		if (PhotonNetwork.IsMasterClient)
		{
			// 서버에 내가 방장이라고 알림
			RoomManager.GetInstance._baseServerManager.UpdateAsset("/room/update/manager", "");
			// 참여자 트랜스폼 정보도 새로운 방장이 보냄
			playerMoveCoroutine = StartCoroutine(SendActorsTransform());
		}


		Debug.Log("OnMasterClientSwitched PhotonNetwork.IsMasterClient " + PhotonNetwork.IsMasterClient);
	}
    #endregion

    #region Public Methods
	/// <summary>
	/// Logs the feedback in the UI view for the player, as opposed to inside the Unity Editor for the developer.
	/// </summary>
	/// <param name="message">Message.</param>
	void LogFeedback(string message)
	{
		// we do not assume there is a feedbackText defined.
		//if (feedbackText == null)
		//{
		//	return;
		//}

		//// add new messages as a new line and at the bottom of the log.
		//feedbackText.text += System.Environment.NewLine + message;
	}

	
   
	/// <summary>
	/// Called after disconnecting from the Photon server.
	/// </summary>
	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log("PUN Disconnected");
		// 포톤 연결 종료후 관전 서버에 방나간거 알림
		RoomManager.GetInstance._baseServerManager.RoomLeave();
		// 관전 서버 로그 아웃후 랜딩페이지로
		JSGate.GetInstance.SendLeavRoom();


		/*DialogUI.Instance
					.SetData(disconnectPopup)
					.SetButtonColor(DialogButtonColor.Red)
					.OnClose(() => Debug.Log("Closed"))
					.Show();*/
	}


	public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		/*Debug.Log("OnRoomPropertiesUpdate :  " + propertiesThatChanged.ToStringFull());

		if (propertiesThatChanged.ContainsKey(RoomDataManager.ImageGroup))
		{
			string[] imagegroup = (string[])propertiesThatChanged[RoomDataManager.ImageGroup];
			Debug.Log("imagegroup : " + imagegroup.ToStringFull());
		}*/
	}


	#endregion


	/// <summary>
	/// 플레이어 프리팹 로드
	/// 포톤 Instantiate 형태라서 방 나가면 자동 삭제됨 
	/// </summary>
	public void SpawnPlayer()
    {
		if (playerPrefab == null || !PhotonNetwork.IsConnected)
		{ // #Tip Never assume public properties of Components are filled up properly, always check and inform the developer of it.

			Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
			Debug.LogError("<Color=Red><b>Not connected </b></Color> Please connect to Photon'", this);
		}
		else
		{
			Debug.Log("SpawnPlayer PlayerID : " + PlayerData.instance.PlayerID);
			
			if (!RoomData.instance.IsMyRoom && PlayerData.instance.IsActor)
            {
				Transform spawnPointTrans = RoomManager.GetInstance.GetSpawnPoint();
				playerOffsetPos = spawnPointTrans.position;
				playerOffsetRot = Quaternion.Euler(0, spawnPointTrans.rotation.y, 0);
			}
			
			string prefabPath = Path.Combine("Prefabs", playerPrefab.name);
			GameObject go = PhotonNetwork.Instantiate(prefabPath, playerOffsetPos, playerOffsetRot, 0,new object[] { PlayerData.instance.PlayerUniqueNo });
			// photon player custom properties 고유번호 할당
			/*Player myPlayer = PhotonNetwork.LocalPlayer;
			myPlayer.TagObject = go;*/
			//myPlayer.SetCustomProperties(new Hashtable { { "playerUniqueNo", PlayerData.instance.PlayerUniqueNo } });
			//Hashtable playerCP = PhotonNetwork.LocalPlayer.CustomProperties;

			RoomManager.GetInstance.SetPlayer(go);
		}
	}
	/// <summary>
	/// 관전자 모드로 들어오면 룸에 들어와있는 참여자 캐릭터 만들어주기 
	/// </summary>
	/// <param name="_actorInfos"></param>
	public void SpawnActors(List<ActorInfo> _actorInfos)
	{
		foreach(ActorInfo actor in _actorInfos)
        {
			GameObject go;
			// 참여자 이동 데이터 존재하지 않거나 공백이면
			if (string.IsNullOrEmpty(actor.trace))
            {
				// 룸의 스폰포인트 정보 불러오기
				string _spawnPoint = RoomData.instance.RoomInfo.spawnPoint;
				// 스폰포인트 정보도 없거나 공백이면
				if (string.IsNullOrEmpty(_spawnPoint))
                {
					// 가운데에 생성
					go = Instantiate(actorDummyPrefab, new Vector3(0f, 1f, 2f), Quaternion.Euler(0, 180, 0));
				}
				// 스폰 포인트 존재하는 경우
                else
                {
					ObjectTransform otranform = Utility.GetParsingObjectTransform(_spawnPoint);
					go = Instantiate(actorDummyPrefab, otranform.position,
				   Quaternion.Euler(otranform.rotation.x, otranform.rotation.y, otranform.rotation.z));
				}
				

			}
			// 참여자 이동데이터 있으면
			else
            {
				ObjectTransform otranform = Utility.GetParsingObjectTransform(actor.trace);
				 go = Instantiate(actorDummyPrefab, otranform.position,
					Quaternion.Euler(otranform.rotation.x, otranform.rotation.y, otranform.rotation.z));

			}
			// 참여자 더미 오브젝트 정보 룸데이터에 저장 추후에 업데이트 할때 유저번호로 찾아서 개별 포지션 업데이트 
			RoomData.instance.ActorDummys.Add(actor.userNo, go);
			
		}
	}
	
	/// <summary>
	/// room 안에 플레이어 정보 웹에 보냄 
	/// </summary>
	private void SendPlayerList()
	{

#if UNITY_WEBGL && !UNITY_EDITOR
		Player[] players = PhotonNetwork.PlayerList;
		List<PlayersInRoom> playerList = new List<PlayersInRoom>();
		
		foreach (var i in players)
        {
			PlayersInRoom pir = new PlayersInRoom();
			Debug.Log("player id : " + i.NickName);
			pir.name = i.NickName;
			playerList.Add(pir);
        }

		string playerJsonList = JsonConvert.SerializeObject(playerList);
		jsgate.UpdateMemberList(playerJsonList);
		Debug.Log("playerlist : " + playerJsonList);
#endif
	}

	/*IEnumerator DisconnectAndMoveScene()
	{
		PhotonNetwork.Disconnect();
		while(PhotonNetwork.IsConnected)
		yield return null;
		SceneManager.LoadScene("PhotonRoomMaker");
	}

	public void GoBackRoomList()
	{
		StartCoroutine(DisconnectAndMoveScene());
	}*/

}


[System.Serializable]
public class PlayersInRoom
{
	public string name;

}
[System.Serializable]
public class ActorsMoveInfo
{
	public string userNo;
	public string trace; 

}