using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using FrostweepGames.Plugins.Native;
using FrostweepGames.WebGLPUNVoice;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;
using BuildTimestampDisplay;
using EasyUI.Dialogs;

/// <summary>
/// 컨텐츠 전체 총괄하는 메인 클래스 
/// </summary>
public class RoomManager : Singleton<RoomManager>
{
    [SerializeField] private InputControl iControl;
    [SerializeField] private CinemachineVirtualCameraBase vcam;
    [SerializeField] private WelcomePopupManager welcomePopMgr;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private TMP_Text roomNameUI;
    [SerializeField] private SetupManager setupManager;
    [SerializeField] private Vector3 spawnPointOffsetPos;
    [SerializeField] private Vector3 spawnPointOffsetRot;
    [SerializeField] private BuildTimestamp bts;

    [HideInInspector] public bool _isEditMode;
    [HideInInspector] public PhotonRoomControl pRoomCtl;
    [HideInInspector] public List<BaseAssetData> assetDataList;
    [HideInInspector] public GameObject player;

    /// <summary>
    /// 어드레서블 완료된후에 룸 에셋 생성하기 위해 
    /// </summary>
    [HideInInspector] public bool isLoadCompleteAddressables;


    private UISystem _uiSystem;
    
    public BaseServerManager _baseServerManager;
    public AssetLoadManager _assetLoadManager;

    public Action OnSaveEditAsset;
    public Action OnEditAsset;
    public Action OnLoadAsset;
    public Action<GameObject> OnLoadedPlayer;
    public Text debugTxt;
    public Recorder recorder;
    public Listener listener;

    

    private void Awake()
    {
        pRoomCtl = GetComponent<PhotonRoomControl>();
        JSGate.GetInstance.Init(iControl.GetComponent<FocusControl>());
    }
    void Start()
    {
       // SetEnterRoomData("c@b.c", "12345", "484A755CB286802");
        //20211211 yongsik UISystem(singleton)정의
        _uiSystem = UISystem.GetInstance;
        
        SetEnterRoom();
    }
    private void OnEnable()
    {
        // 웰컴 팝업
        welcomePopMgr.OnReloadButtonClick += OnReloadMicDropList;
        welcomePopMgr.OnChangeMicStatus += OnMuteMicrophone;
        // 셋업 UI 버튼들  
        setupManager.OnApply += OnSetupApply;
        setupManager.OnDeleteRoom += OnSetupDeleteRoom;
    }
    private void OnDisable()
    {
        welcomePopMgr.OnReloadButtonClick -= OnReloadMicDropList;
        welcomePopMgr.OnChangeMicStatus -= OnMuteMicrophone;
        setupManager.OnApply -= OnSetupApply;
        setupManager.OnDeleteRoom -= OnSetupDeleteRoom;
    }
    /// <summary>
    /// 방입장시 받게 되는 데이터 ( 웹으로부터 받아오기 )
    /// </summary>
    /// <param name="_id">로그인 아이디</param>
    /// <param name="_token">로그인 토큰</param>
    /// <param name="_roomNo">입장할 방번호</param>
    public void SetEnterRoom()
    {
        roomNameUI.text = RoomData.instance.EnterRoomNo;
        roomNameUI.text += "\n " + bts.ToString();

        if (string.IsNullOrEmpty(PlayerData.instance.PlayerUniqueNo))
        {
            ((LocalServerManager)_baseServerManager).DownloadAsset();
        }
        else
        {
            SetSetupData();
            ((LocalServerManager)_baseServerManager).EnterRoom();
        }
    }
    /// <summary>
    /// 포톤 연결시 싱글캐릭터에서 포튼캐릭터로 변경 by 영수 2021.11.15
    /// </summary>
    /// <param name="_player"></param>
    public void SetPlayer(GameObject _player=null)
    {
        player = _player;
        
        vcam.Follow = _player.transform;
        vcam.LookAt = _player.transform.Find("LookTarget");

        OnLoadedPlayer?.Invoke(_player);
        //CameraFrustums cfrustums = Camera.main.GetComponent<CameraFrustums>();
        //cfrustums.targetCharactor = _player;
        //StartWithMicSetup Bool 값에 따라 mic 설정 
        if (ApplicationSetup.GetInstance.StartWithMicSetup)
        {
            Invoke("SetWelcomePopup" , 1f);    
        }
    }
    /// <summary>
    /// 관리자 화면에서 보이는 다른 참여자들 위치 동기화 
    /// </summary>
    /// <param name="_userMoveList"></param>
    public void MoveActorsDummy(List<ActorsMoveInfo> _userMoveList)
    {
        Dictionary<string, GameObject> actorDummys = RoomData.instance.ActorDummys;
        foreach (var i in _userMoveList)
        {
            ObjectTransform otrans = Utility.GetParsingObjectTransform(i.trace);
            actorDummys[i.userNo].GetComponent<ActorDummyControl>().MoveActor(otrans);
        }
    }
    /// <summary>
    /// 관리자 화면에서 다른 참여자 제거 
    /// </summary>
    /// <param name="_userNo"></param>
    public void DestroyActorDummy(string _userNo)
    {
        if (!PhotonNetwork.OfflineMode) return;
        Dictionary<string, GameObject> actorDummys = RoomData.instance.ActorDummys;
        GameObject adummy = actorDummys[_userNo];
        actorDummys.Remove(_userNo);
        Destroy(adummy);
        
    }
    /// <summary>
    /// 방입장시 웰컴 팝업
    /// </summary>
    private void SetWelcomePopup()
    {
        welcomePopMgr.Active();
        welcomePopMgr.SetTitle("TitleTEST");
        welcomePopMgr.SetName("내꺼");
        welcomePopMgr.SetStatus("15/15 가득참");
        OnReloadMicDropList();
    }
    /// <summary>
    /// 플레이어 머리위에 메세지 띄우기
    /// </summary>
    /// <param name="str"></param>
    public void SendMsg(string str)
    {
        player.transform.Find("Msg").GetComponent<TextMesh>().text = str;
    }

    public void DebugWrite(string str)
    {
        debugTxt.text += "\n " + str;
    }

    /// <summary>
    /// 에디트후 저장 버튼 클릭시
    /// </summary>
    public void SaveEditAsset()
    {
        _isEditMode = false;
        
        //20211211 yongsik
        //UISystem으로 넘김.
        _uiSystem.TurnOffEditAsset();
        
        // debugCanvas.gameObject.SetActive(true);
        // editCanvas.gameObject.SetActive(false);

        player.SetActive(true);

        string spnt = Utility.GetDecimalTransformNoneScale(spawnPoint.transform);
        if (RoomData.instance.RoomInfo.spawnPoint != spnt)
        {
            UpdateSpawnPoint(spnt);
        }
        spawnPoint.SetActive(false);


        // 스폰포인트 트랜스폼 기존값과 비교해서 변경됐으면 서버에 업데이트 보내게 작업 필요
       // _baseServerManager.UpdateRoom();
        OnSaveEditAsset?.Invoke();
        
    }
    /// <summary>
    /// 어드레서블 데이터 추가하기 
    /// </summary>
    public void AddAddressAsset()
    {

    }
    
    /// <summary>
    /// AddAsset 버튼 실행
    /// </summary>
    public void AddAsset() => JSGate.GetInstance.AddAsset(); //addAsset 버튼 동작
    
    /// <summary>
    /// 에셋 에디트 버튼 클릭시 
    /// </summary>
    public void EditAsset()
    {
        _isEditMode = true;

        //20211211 yongsik
        //UISystem으로 넘김.
        _uiSystem.TurnOnEditAsset();
        // debugCanvas.gameObject.SetActive(false);
        // editCanvas.gameObject.SetActive(true);

        player.SetActive(false);
        spawnPoint.SetActive(true);
        OnEditAsset?.Invoke();
    }
    /// <summary>
    /// 체인지 룸 버튼 클릭시 
    /// </summary>
    public void ChangeRoom()
    {
       
    }
    /// <summary>
    /// Load Asset 버튼 클릭시 
    /// </summary>
    public void LoadAsset()
    {
        OnLoadAsset?.Invoke();
    }
    /// <summary>
    /// 에셋데이터 리스트에서 유니크 아이디에 해당하는 에셋데이터 반환
    /// 방장 화면에 로드된 에셋과 일치되는 에셋데이터 가져오기 위해 
    /// </summary>
    /// <param name="_uniqueId"></param>
    /// <returns></returns>
    public BaseAssetData GetBaseAssetData(long _no, long _subNo )
    {
        foreach (BaseAssetData i in assetDataList)
        {
            if (i.item.no == _no && i.item.PropertiesData.assetSubNo == _subNo)
            {
                Debug.Log("find success ");
                BaseAssetData baData = i;
                assetDataList.Remove(i);
                return baData;
            }
        }

        return null;
    }

    ///////////////////////////////////////////////////////////////  룸설정
    /// <summary>
    /// 마이크 관련 설정
    /// </summary>
    private void OnReloadMicDropList()
    {
        CustomMicrophone.RequestMicrophonePermission();
        welcomePopMgr.SetMicList(CustomMicrophone.devices.ToList());

    }
    /// <summary>
    /// 마이크 켜기/끄기
    /// </summary>
    /// <param name="b"></param>
    public void OnMuteMicrophone(bool b)
    {
        if (b)
        {
            recorder.StartRecord();
        }
        else
        {
            recorder.StopRecord();
        }
    }
    /// <summary>
    /// 마이크 켜기/끄기 (토글 온/오프)
    /// </summary>
    /// <param name="b"></param>
    public void OnMuteMicrophone(Toggle toggleMicMute)
    {
        //Debug.Log("toggleMicMute++++++++++++++++++++++++" + toggleMicMute.isOn);
        if (toggleMicMute.isOn)
        {
            recorder.StartRecord();
        }
        else
        {
            recorder.StopRecord();
        }
    }
    /// <summary>
    /// 마이크 에코 ( 테스트시 자기가 말한거 들을때)
    /// </summary>
    /// <param name="toggleMicEcho"></param>
    public void OnEchoMicrophone(Toggle toggleMicEcho)
    {
        //Debug.Log("toggleMicEcho++++++++++++++++++++++++" + toggleMicEcho.isOn);
        recorder.debugEcho = toggleMicEcho.isOn;
    }

    /////////////////////////////////////////////////////////////// 
    /// <summary>
    /// 방 관련 속성값 적용  
    /// </summary>
    /// <param name="_roomPptInfo"></param>
    public void SetSetupData()
    {
        RoomPropertiesInfo _roomPptInfo = RoomData.instance.RoomInfo;
        setupManager.RoomTitle = _roomPptInfo.title;
        setupManager.RoomDesc = _roomPptInfo.desc;
        setupManager.RoomHashtags = _roomPptInfo.hashTags;
        setupManager.RoomPrivate = _roomPptInfo.passwordRequired;
        setupManager.RoomSpectator = (_roomPptInfo.spectatorMode == "Y") ? true : false;
        setupManager.ScreenShotURL = _roomPptInfo.screenshotUri;
        setupManager.DoInit();

        SetSpawnPoint(_roomPptInfo.spawnPoint);
    }
    /// <summary>
    /// 방 속성 변경 적용
    /// </summary>
    private void OnSetupApply()
    {
        string spectatorStr = (setupManager.RoomSpectator) ? "Y" : "N";
        string hashtagStr = (string.IsNullOrEmpty(setupManager.RoomHashtags)) ? "" : setupManager.RoomHashtags;
        string screenShotURLStr = (string.IsNullOrEmpty(setupManager.ScreenShotURL)) ? "https://" : setupManager.ScreenShotURL;
        string passwordStr = (setupManager.RoomPrivate) ? setupManager.RoomPassword : "";
        ((LocalServerManager)_baseServerManager).UpdateRoom(setupManager.RoomTitle, setupManager.RoomDesc, hashtagStr, spectatorStr, passwordStr, screenShotURLStr);
    }
    
    /// <summary>
    /// 방 삭제시 호출
    /// </summary>
    private void OnSetupDeleteRoom()
    {
        ((LocalServerManager)_baseServerManager).DeleteRoom();
    }
    /// <summary>
    /// 룸속성 변경시 관전자 강퇴
    /// </summary>
    /// <param name="_popupText"></param>
    public void OnRoomLeaveForce(DefaultPopupSO _popupText)
    {
        DialogUI.Instance
              .SetData(_popupText)
              .OnClose(() => JSGate.GetInstance.SendLeavRoom())
              .Show();

        //  관전자 강퇴시 팝업 닫기 이벤트 없는 경우 자동종료
        Invoke("RoomLeaveForceCallback", 10f);
        
    }
    /// <summary>
    ///  관전자 강퇴시 팝업 자동종료 콜백
    /// </summary>
    private void RoomLeaveForceCallback()
    {
        if (DialogUI.Instance.IsActive) DialogUI.Instance.Hide();
        JSGate.GetInstance.SendLeavRoom();
    }
    /// <summary>
    /// spawn point ///////////////////////////
    /// </summary>
    /// <param name="_spawnPoint"></param>
    private void SetSpawnPoint(string _spawnPoint)
    {
        if (string.IsNullOrEmpty(_spawnPoint))
        {
            spawnPoint.transform.position = spawnPointOffsetPos;
            spawnPoint.transform.eulerAngles = spawnPointOffsetRot;
        }
        else
        {
            ObjectTransform objTra = Utility.GetParsingObjectTransform(_spawnPoint);
            spawnPoint.transform.position = objTra.position;
            spawnPoint.transform.eulerAngles = objTra.rotation;
        }
    }
    /// <summary>
    /// 스폰 위치 가져오기 
    /// </summary>
    /// <returns></returns>
    public Transform GetSpawnPoint()
    {
        return spawnPoint.transform;
    }
    /// <summary>
    /// 스폰 위치 업데이트 
    /// </summary>
    /// <param name="_spawnPoint"></param>
    private void UpdateSpawnPoint(string _spawnPoint)
    {
        ((LocalServerManager)_baseServerManager).UpdateSpawnPoint(_spawnPoint);
    }

}
