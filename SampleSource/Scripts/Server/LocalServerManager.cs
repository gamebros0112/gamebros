using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.JSON.LitJson;
using Newtonsoft.Json;
using EasyUI.Dialogs;


/// <summary>
/// 룸 속성
/// </summary>
[Serializable]
public class RoomPropertiesInfo
{
    public string no = "";
    public string ownerNo = "";
    public string types = "N";
    public string shape = "A";
    public string title = "";
    public string desc = "";
    public string hashTags = "#1";
    public bool passwordRequired = false;
    //public string private = "Y";
    public string openType = "E";
    public string spawnPoint = "0,2,0,0,180,0,1,1,1";
    public string spectatorMode = "Y";
    public int numberOfLikes = 0;
    public int numberOfActors = 0;
    public int numberOfSpectators = 0;
    public int cumlativeActors = 0;
    public string createDatetime = "";
    public string screenshotUri = "";
}

/// <summary>
/// 참여자 정보 
/// </summary>
[System.Serializable]
public class ActorInfo
{
    public string userNo;
    public string nickname;
    public string faces;
    public string trace;
}

public class LocalServerManager : BaseServerManager
{
    private WebSocketManager websocket;
    private WebSocketEventSender websocketSender;
    private InventoryItem invenItem;
    private Action<Item> inquiryAssetCallback;
    public Action<long,long,uint> OnAssetQtyLeft;
    public Action<long, long, string> OnRoomAssetUpdateTransform;
    public Action<long, long, string,uint> OnRoomAssetUpdate;

    private void Awake()
    {
        RoomManager.GetInstance._baseServerManager = this;
        
        websocketSender = WebSocketEventSender.GetInstance;
        websocket = websocketSender.GetComponent<WebSocketManager>();
    }
    protected override void Start()
    {
        base.Start();
        server = ServerType.LocalServer;
       // StartWebSocket();
    }
    public void StartWebSocket()
    {
        websocket.OnConnectedToServer();
    }
    /// <summary>
    /// 웹소켓 api 연결 
    /// </summary>
    private void OnEnable()
    {
        websocketSender.OnLoginEvent += OnResponseLogin;
        // room
        websocketSender.OnRoomAssetListEvent += OnResponseRoomAssetList;
        websocketSender.OnRoomEnterEvent += OnResponseRoomEnter;
        websocketSender.OnRoomInquiryEvent += OnResponseRoomInquiry;
        // room asset
        websocketSender.OnRoomAssetPutEvent += OnResponseRoomAssetPut;
        websocketSender.OnRoomAssetRetrieveEvent += OnResponseRoomAssetRetireve;
        websocketSender.OnRoomAssetBuyEvent += OnResponseRoomAssetBuy;
        websocketSender.OnRoomAssetInquiryEvent += OnResponseRoomAssetInquiry;
        // inventory
        websocketSender.OnInvenAssetCreateEvent += OnResponseInvenAssetCreate;
        websocketSender.OnInvenAssetInquiryEvent += OnResponseInvenInquiry;
        websocketSender.OnInvenListEvent += OnResponseInvenList;
        websocketSender.OnInvenAssetDestroyEvent += OnInvenAssetDestroy;

        // push event
        websocketSender.OnRoomEnterPushEvent += OnRoomEnterPush;
        websocketSender.OnRoomLeavePushEvent += OnRoomLeavePush;
        websocketSender.OnRoomLeaveForcePushEvent += OnRoomLeaveForcePush;
        websocketSender.OnRoomDestroyPushEvent += OnRoomDestroyPush;
        websocketSender.OnRoomUpdatePushEvent += OnRoomUpdatePush;
        websocketSender.OnRoomAssetBuyPushEvent += OnRoomAssetBuyPush;
        websocketSender.OnRoomAssetRetrievePushEvent += OnRoomAssetRetrievePush;
        websocketSender.OnRoomAssetUpdatePushEvent += OnRoomAssetUpdatePush;
        websocketSender.OnRoomAssetPutPushEvent += OnRoomAssetPutPush;
        websocketSender.OnRoomAssetTransformPushEvent += OnRoomAssetTransformPush;
        websocketSender.OnPointIncomeEvent += OnPointIncomePush;
        websocketSender.OnUserMovePushEvent += OnUserMovePush;
        websocketSender.OnRoomUpdateSpawnpointPushEvent += OnRoomUpdateSpawnpointPush;

        //facepixelEditorevent 20220213 yongsik => FacePixelEditorManager 로 이동
        // websocketSender.OnCharFaceLoad += OnCharFaceLoad;
        // websocketSender.OnCharFaceUpdate += OnCharFaceUpdate;
    }
    /// <summary>
    /// 웹소켓 api 해제  
    /// </summary>
    private void OnDisable()
    {
        websocketSender.OnLoginEvent -= OnResponseLogin;
        // room
        websocketSender.OnRoomAssetListEvent -= OnResponseRoomAssetList;
        websocketSender.OnRoomEnterEvent -= OnResponseRoomEnter;
        websocketSender.OnRoomInquiryEvent -= OnResponseRoomInquiry;
        // room asset
        websocketSender.OnRoomAssetPutEvent -= OnResponseRoomAssetPut;
        websocketSender.OnRoomAssetRetrieveEvent -= OnResponseRoomAssetRetireve;
        websocketSender.OnRoomAssetBuyEvent -= OnResponseRoomAssetBuy;
        websocketSender.OnRoomAssetInquiryEvent -= OnResponseRoomAssetInquiry;
        // inventory
        websocketSender.OnInvenAssetCreateEvent -= OnResponseInvenAssetCreate;
        websocketSender.OnInvenAssetInquiryEvent -= OnResponseInvenInquiry;
        websocketSender.OnInvenListEvent -= OnResponseInvenList;
        websocketSender.OnInvenAssetDestroyEvent -= OnInvenAssetDestroy;
        // push event
        websocketSender.OnRoomEnterPushEvent -= OnRoomEnterPush;
        websocketSender.OnRoomLeavePushEvent -= OnRoomLeavePush;
        websocketSender.OnRoomLeaveForcePushEvent -= OnRoomLeaveForcePush;
        websocketSender.OnRoomDestroyPushEvent -= OnRoomDestroyPush;
        websocketSender.OnRoomUpdatePushEvent -= OnRoomUpdatePush;
        websocketSender.OnRoomAssetBuyPushEvent -= OnRoomAssetBuyPush;
        websocketSender.OnRoomAssetRetrievePushEvent -= OnRoomAssetRetrievePush;
        websocketSender.OnRoomAssetUpdatePushEvent -= OnRoomAssetUpdatePush;
        websocketSender.OnRoomAssetPutPushEvent -= OnRoomAssetPutPush;
        websocketSender.OnRoomAssetTransformPushEvent -= OnRoomAssetTransformPush;
        websocketSender.OnPointIncomeEvent -= OnPointIncomePush;
        websocketSender.OnUserMovePushEvent -= OnUserMovePush;
        websocketSender.OnRoomUpdateSpawnpointPushEvent -= OnRoomUpdateSpawnpointPush;

        //facepixelEditorevent 20220213 yongsik => FacePixelEditorManager 로 이동
        // websocketSender.OnCharFaceLoad -= OnCharFaceLoad;
        // websocketSender.OnCharFaceUpdate -= OnCharFaceUpdate;
    }




    //////////////////////////////////////////////////////////   push event callback
    /// <summary>
    /// 에셋 구매후 룸내 모든 유저에게 전달
    /// </summary>
    /// <param name="_assetNo">에셋 번호</param>
    /// <param name="_assetSubNo">에셋 서브 번호</param>
    /// <param name="_quantityLeft">남은 갯수</param>
    private void OnRoomAssetBuyPush(long _assetNo, long _assetSubNo, uint _qtyLeft)
    {
        OnAssetQtyLeft?.Invoke(_assetNo, _assetSubNo, _qtyLeft);
    }
    /// <summary>
    /// 에셋 판매후 
    /// </summary>
    private void OnRoomAssetRetrievePush(long _assetNo, long _assetSubNo, uint _qtyLeft)
    {
        OnAssetQtyLeft?.Invoke(_assetNo, _assetSubNo, _qtyLeft);
    }
    /// <summary>
    /// 룸에셋 위치 업데이트 됐을때 
    /// </summary>
    private void OnRoomAssetTransformPush(long _assetNo, long _assetSubNo, string _transform)
    {
        OnRoomAssetUpdateTransform?.Invoke(_assetNo, _assetSubNo, _transform);
    }
    /// <summary>
    /// 포인트 들어왔을때 
    /// </summary>
    private void OnPointIncomePush(uint _point, uint _totalPoint)
    {
        JSGate.GetInstance.SendPointStatus((int)_totalPoint);
    }
    /// <summary>
    /// 방 입장시 관전자에게 전달
    /// </summary>
    /// <param name="_userNo">유저 고유번호</param>
    /// <param name="_nickname">입장한 유저 닉</param>
    /// <param name="_faces">입장한 유저 얼굴 정보</param>
    private void OnRoomEnterPush(string _userNo, string _nickname, bool _actor,string _faces, string _trace)
    {
        if (!_actor)
        {
            List<ActorInfo> actorInfos = new List<ActorInfo>();
            ActorInfo ainfo = new ActorInfo();
            ainfo.userNo = _userNo;
            ainfo.nickname = _nickname;
            ainfo.faces = _faces;
            ainfo.trace = _trace;
            actorInfos.Add(ainfo);
            RoomManager.GetInstance.pRoomCtl.SpawnActors(actorInfos);
        }
    }
    /// <summary>
    /// 참여자 방나갔을때 
    /// </summary>
    private void OnRoomLeavePush(string _userNo, string _nickname)
    {
        RoomManager.GetInstance.DestroyActorDummy(_userNo);
    }
    /// <summary>
    /// 관전자 강퇴시 
    /// </summary>
    private void OnRoomDestroyPush()
    {
        RoomManager.GetInstance.OnRoomLeaveForce(LocalizeScriptableInfo.GetInstance.destroyRoom);
    }
    private void OnRoomLeaveForcePush(bool _passRequired, string _specMode)
    {
        // 관전모드 종료
        if (_passRequired)
        {
            RoomManager.GetInstance.OnRoomLeaveForce(LocalizeScriptableInfo.GetInstance.privateRoomOn);
        }
        // 비번방으로 변경
        else if(_specMode == "N")
        {
            RoomManager.GetInstance.OnRoomLeaveForce(LocalizeScriptableInfo.GetInstance.spectatorModeOff);
        }


    }
    private void OnRoomUpdatePush(JsonData _arguments)
    {
        /*RoomPropertiesInfo tempItem = JsonConvert.DeserializeObject<RoomPropertiesInfo>(_arguments.ToJson());
        if (tempItem.spectatorMode == "N")
        {
            DialogUI.Instance
              .SetData(LocalizeScriptableInfo.GetInstance.spectatorModeOff)
              .OnClose(() => JSGate.GetInstance.SendLeavRoom())
              .Show();
        }
        else if (tempItem.passwordRequired)
        {
            DialogUI.Instance
              .SetData(LocalizeScriptableInfo.GetInstance.privateRoomOn)
              .OnClose(() => JSGate.GetInstance.SendLeavRoom())
              .Show();
        }*/
      
        // if(DialogUI.Instance.IsActive)DialogUI.Instance.Hide();
    }
    private void OnRoomUpdateSpawnpointPush(string _spawnPoint)
    {
        RoomData.instance.RoomInfo.spawnPoint = _spawnPoint;
    }
    /// <summary>
    /// 룸소유자 판매 정보 업데이트후 관전자에게 전달 
    /// </summary>
    /// <param name="_assetNo">에셋번호</param>
    /// <param name="_assetSubNo">에셋서브번호</param>
    /// <param name="_sellable">판매여부</param>
    /// <param name="_salePoint">판매가격</param>
    private void OnRoomAssetUpdatePush(long _assetNo, long _assetSubNo, string _sellable, uint _salePoint)
    {
        OnRoomAssetUpdate?.Invoke(_assetNo, _assetSubNo, _sellable, _salePoint);
    }
    /// <summary>
    /// 방 소유자가 방에 에셋 꺼내놓으면 관전자에게 보내지는 이벤트
    /// </summary>
    /// <param name="obj"></param>
    private void OnRoomAssetPutPush(JsonData _arguments)
    {
        var tempItem = JsonConvert.DeserializeObject<Item>(_arguments.ToJson());
       
        string propertiesStr = _arguments["propertiesInRoom"].ToJson();

        PropertiesInRoom pData = JsonConvert.DeserializeObject<PropertiesInRoom>(propertiesStr);
        tempItem.PropertiesData = pData;

        LoadFromInventory?.Invoke(tempItem);

    }
    /// <summary>
    /// 참여자들 이동 데이터(트랜스폼)
    /// </summary>
    /// <param name="_arguments"></param>
    private void OnUserMovePush(JsonData _arguments)
    {
        var actorsInfoList = JsonConvert.DeserializeObject<List<ActorsMoveInfo>>(_arguments.ToJson());
        RoomManager.GetInstance.MoveActorsDummy(actorsInfoList);

    }


    //////////////////////////////////////////////////////////////////////////  Request api
    /// <summary>
    /// 방 입장전 로그인
    /// </summary>
    private void LogIn()
    {
        // 웹에서 전달받은 id 있는 경우 또는 없는 경우 
        // 비로그인
        if (string.IsNullOrEmpty(PlayerData.instance.PlayerID))
        {
            InquiryRoom();
        }
        // 로그인
        else
        {
            websocket.OnSend("{\"direction\":\"request\",\"path\": \"/login\",\"arguments\":{\"x-auth-id\":\""+PlayerData.instance.PlayerID+ "\",\"x-auth-token\":\"" + PlayerData.instance.PlayerToken + "\"}}");
        }
    }


    /////////////////////////////////////////////////////////////////////////  방관련 
    /// <summary>
    /// 방 정보 업데이트 
    /// </summary>
    public void UpdateRoom(string _title, string _desc,string _hashtags,string _spectator,string _password, string _screenshotUri)
    {
        // websocket.OnSend("{\"direction\":\"request\",\"path\":\"/room/update/properties\",\"arguments\":" + jsonStr + "}");
        websocket.OnSend("{\"direction\":\"request\",\"path\":\"/room/update/properties\",\"arguments\":{\"shape\":\"A\",\"hashTags\":\""+_hashtags+ "\",\"title\":\"" + _title + "\",\"desc\":\"" + _desc + "\",\"password\":\"" + _password + "\",\"openType\":\"E\",\"spectatorMode\":\"" + _spectator + "\",\"screenshotUri\":\"" + _screenshotUri + "\"}}");

    }
    /// <summary>
    /// 스폰포인트 업데이트 
    /// </summary>
    /// <param name="_spnPnt"></param>
    public void UpdateSpawnPoint(string _spnPnt)
    {
        websocket.OnSend("{\"direction\":\"request\",\"path\": \"/room/update/spawnpoint\",\"arguments\":{\"spawnPoint\":\"" + _spnPnt + "\"}}");
    }
    /// <summary>
    /// 방 삭제
    /// </summary>
    public void DeleteRoom()
    {
        websocket.OnSend("{\"direction\":\"request\",\"path\": \"/room/destroy\"}");
    }
    /// <summary>
    /// 방 입장 
    /// 방에 입장해야 에셋 데이터 불러올수 있음 
    /// </summary>
    public void EnterRoom()
    {
        // 비번 확인을 통해 스타터씬에서 /room/enter reponse 받아서 참여자로 들어온 경우
        if (PlayerData.instance.IsActor)
        {
            DownloadAsset();
            Invoke("InvenList", 1f);
        }
        else
        {
            websocket.OnSend("{\"direction\":\"request\",\"path\":\"/room/enter\",\"arguments\":{\"no\":\"" + RoomData.instance.EnterRoomNo + "\",\"password\":\"12345\"}}");
        }
    }
    /// <summary>
    /// 개별 방 속성 조회
    /// </summary>
    private void InquiryRoom()
    {
        websocket.OnSend("{\"direction\":\"request\",\"path\": \"/room/inquiry\",\"arguments\":{\"no\":\"" + RoomData.instance.EnterRoomNo + "\"}}");
    }
    /// <summary>
    /// 방나갔을때 호출
    /// </summary>
    public override void RoomLeave()
    {
        websocket.OnSend("{\"direction\":\"request\",\"path\": \"/room/leave\"}");
    }
    /// <summary>
    /// 룸좋아요
    /// </summary>
    /// <param name="_action">"A" or "D"</param>
    public override void UpdateRoomLike(string _action)
    {
        websocket.OnSend("{\"direction\":\"request\",\"path\": \"/room/update/like\",\"arguments\":{\"action\":\"" + _action + "\"}}");
    }
    /// <summary>
    /// 인벤 에셋 리스트 가져오기
    /// </summary>
    public void InvenList()
    {
        inventoryItems = new List<InventoryItem>(); // inven list initialize
        websocket.OnSend("{\"direction\":\"request\",\"path\": \"/inven/list\"}");
    }
    /// <summary>
    /// 인벤 에셋 조회
    /// </summary>
    /// <param name="assetNo"></param>
    private void InquiryInvenAsset(long _assetNo)
    {
        websocket.OnSend("{\"direction\":\"request\",\"path\": \"/inven/asset/inquiry\",\"arguments\":{\"assetNo\":" + _assetNo + "}}");
    }
    /// <summary>
    /// 인벤 에셋 삭제 
    /// </summary>
    /// <param name="_assetNo"></param>
    public override void DeleteInvenAsset(long _assetNo)
    {
        websocket.OnSend("{\"direction\":\"request\",\"path\": \"/inven/asset/destroy\",\"arguments\":{\"assetNo\":" + _assetNo + "}}");
    }

    /// <summary>
    /// 에셋 회수
    /// </summary>
    private void RetrieveRoomAsset(long _assetNo,long _assetSubNo,int _quantity)
    {
        Debug.Log("_assetNo :  " + _assetNo + " / _assetSubNo : " + _assetSubNo + " / _quantity : " + _quantity);
        websocket.OnSend("{\"direction\":\"request\",\"path\": \"/room/asset/retrieve\",\"arguments\":{\"assetNo\":"+ _assetNo + ",\"assetSubNo\":"+ _assetSubNo + ",\"quantity\":"+ _quantity + "}}");
    }
    /// <summary>
    /// 개별 에셋 조회
    /// </summary>
    public void InquiryRoomAsset(long _assetNo, long _assetSubNo, Action<Item> _callback)
    {
        inquiryAssetCallback = _callback;
        websocket.OnSend("{\"direction\":\"request\",\"path\": \"/room/asset/inquiry\",\"arguments\":{\"assetNo\":" + _assetNo + ",\"assetSubNo\":" + _assetSubNo + "}}");
    }



    //////////////////////////////////////////////////////////////////////// Callback
    /// <summary>
    /// 로그인 콜백
    /// </summary>
    /// <param name="code">0 성공/ 실패</param>
    /// <param name="no">소유자 고유 번호</param>
    /// <param name="roomNo">소유자 방번호</param>
    private void OnResponseLogin(int code, string no, string roomNo, uint point, uint incomePoint)
    {
        
    }
    /// <summary>
    /// 방 입장 할때
    /// </summary>
    /// <param name="code"></param>
    private void OnResponseRoomEnter(int code, string _nickname, bool _actor, bool _likeRoom, JsonData _actors=null)
    {
        if (code == 0)
        {
            PlayerData.instance.PlayerNickName = _nickname;
            PlayerData.instance.IsActor = _actor;
            RoomData.instance.IsLikeRoom = _likeRoom;

            DownloadAsset();
            Invoke("InvenList", 1f);

            if (!_actor && _actors != null)
            {
                List<ActorInfo> actorInfos = JsonConvert.DeserializeObject<List<ActorInfo>>(_actors.ToJson());
                RoomManager.GetInstance.pRoomCtl.SpawnActors(actorInfos);

            }
        }
    }
    /// <summary>
    /// 룸 조회
    /// </summary>
    private void OnResponseRoomInquiry(int code, JsonData _roomInfo)
    {
        RoomPropertiesInfo roomInfo = JsonConvert.DeserializeObject<RoomPropertiesInfo>(_roomInfo.ToJson());
        Debug.Log("roomInfo.numberOfActor : " + roomInfo.numberOfActors);
    }

    ////////////////////////////////////////////////////////////////////////  인벤토리
    /// <summary>
    /// 에셋 처음에 생성하고 서버에 업로드하면
    /// </summary>
    /// <param name="code"></param>
    private void OnResponseInvenAssetCreate(int code, long assetNo, int sequence)
    {
        if (code == 0)
        {
            Debug.Log("inven asset create success ");
            OnCompleteUpload?.Invoke(true);
            SelectToRoomPopup(assetNo);
           // websocket.OnSend("{\"direction\":\"request\",\"path\": \"/inven/list\"}");
        }
    }
    /// <summary>
    /// 인벤 리스트 가져오기
    /// </summary>
    /// <param name="code">0:성공</param>
    /// <param name="last">true:마지막 배열 데이터</param>
    /// <param name="assets">에셋 배열</param>
    private void OnResponseInvenList(int code, bool last, JsonData assets)
    {
        if (code == 0)
        {
            var tempItems = JsonConvert.DeserializeObject<List<InventoryItem>>(assets.ToJson());
            int cnt = tempItems.Count;

            for(int i = 0; i < cnt; i++)
            {
                inventoryItems.Add(tempItems[i]);
            }

            if (last)
            {
                // yongsik 1개남은 남지막 인벤토리 아이템이 룸으로 넘어갈때 count가 0이고 last:true로 넘어옴. 
                // if (inventoryItems.Count > 0)
                // {
                    Debug.Log("call inven refresh");
                    OnCompleteInventoryItems?.Invoke(inventoryItems);
                // }
            }
            

            /*for (int i = 0; i < cnt; i++)
            {
                long _assetNo = (long)inventoryItems[i].no;
                Debug.Log("asset no : " + _assetNo);
                DeleteInvenAsset(_assetNo);
            }*/

        }
    }
    /// <summary>
    /// 인벤 에셋 조회 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="assets"></param>
    private void OnResponseInvenInquiry(int code, JsonData assets)
    {
        if (code == 0)
        {
            // 현재는 개별 인벤아이템 조회시 바로 룸으로 이동 되게 적용. 추후 기획에 따라 변경
            invenItem = JsonConvert.DeserializeObject<InventoryItem>(assets.ToJson());
            SellInfoManager.GetInstance.SetSellInfoToRoom(invenItem, (int _quantity) =>
            {
                MoveToRoom(invenItem, _quantity);
            });

        }
    }
    /// <summary>
    /// 인벤토리 에셋 삭제 
    /// </summary>
    /// <param name="code"></param>
    private void OnInvenAssetDestroy(int code)
    {
        if (code == 0)
        {
            // 인벤토리 갱신 
            InvenList();
        }
    }

    ////////////////////////////////////////////////////////////////////// 룸에셋
    /// <summary>
    /// 룸 에셋 리스트 요청 콜백
    /// </summary>
    /// <param name="code">0:성공</param>
    /// <param name="last">true:마지막 데이터</param>
    /// <param name="assets">에셋 배열</param>
    private void OnResponseRoomAssetList(int code, bool last, JsonData assets)
    {
        if (code == 0)
        {
            var tempItems = JsonConvert.DeserializeObject<List<Item>>(assets.ToJson());
            int cnt = tempItems.Count;
            for (int i = 0; i < cnt; i++)
            {
                string propertiesStr = assets[i]["propertiesInRoom"].ToJson();

                PropertiesInRoom pData = JsonConvert.DeserializeObject<PropertiesInRoom>(propertiesStr);
                tempItems[i].PropertiesData = pData;

                roomItems.Add(tempItems[i]);
            }

            if (last)
            {
                Debug.Log("roomAssets.Count :  " + roomItems.Count);
                if (roomItems == null || roomItems.Count <= 0)
                {
                    LoadRoomItems?.Invoke(null);
                    return;
                }

                LoadRoomItems?.Invoke(roomItems);
            }
        }
    }
    /// <summary>
    /// 룸에셋 번호로 개별 조회
    /// </summary>
    /// <param name="code"></param>
    /// <param name="assets"></param>
    private void OnResponseRoomAssetInquiry(int code, JsonData assets)
    {
        if (code == 0)
        {
            Item tempItem = JsonConvert.DeserializeObject<Item>(assets.ToJson());
            string propertiesStr = assets["propertiesInRoom"].ToJson();
            PropertiesInRoom pData = JsonConvert.DeserializeObject<PropertiesInRoom>(propertiesStr);
            tempItem.PropertiesData = pData;
            inquiryAssetCallback(tempItem);
        }
    }
    /// <summary>
    /// 인벤에셋을 방에 놓을때
    /// </summary>
    /// <param name="code"></param>
    /// <param name="assets">판매정보</param>
    private void OnResponseRoomAssetPut(int code, JsonData assets)
    {
        if (code == 0)
        {
            InvenList(); // 인벤에셋을 방으로 옮기면서 인벤토리 리프레시 요청

            Item _item = new Item();
            _item.no = invenItem.no;
            _item.creatorNo = invenItem.creatorNo;
            _item.creatorNickname = invenItem.creatorNickname;
            _item.division = invenItem.division;
            _item.types = invenItem.types;
            _item.title = invenItem.title;
            _item.hashTags = invenItem.hashTags;
            _item.desc = invenItem.desc;
            _item.imageUri = invenItem.imageUri;
            _item.thumnailUri = invenItem.thumnailUri;
            _item.meta = invenItem.meta;
            _item.transform = invenItem.transform;
            _item.createDatetime = invenItem.createDatetime;
            _item.PropertiesData = JsonConvert.DeserializeObject<PropertiesInRoom>(assets.ToJson());
            LoadFromInventory?.Invoke(_item);
        }
    }
    private void OnResponseRoomAssetRetireve(int code, int _sequence)
    {
        if (code == 0)
        {
            InvenList();
        }
    }
    private void OnResponseRoomAssetBuy(int code, uint point)
    {
        if (code == 0)
        {
            PlayerData.instance.PlayerPoint = point;
            JSGate.GetInstance.SendPointStatus((int)point);
            InvenList();
        }
    }


    ///////////////////////////////////////////////////////////////////////////////  interface
    /// <summary>
    /// upload to server
    /// </summary>
    /// <param name="data"></param>
    public override void UploadAsset(InventoryItem data) {
        Debug.Log("asset create ");
        
        websocket.OnSend("{\"direction\":\"request\",\"path\": \"/inven/asset/create\",\"arguments\":{\"division\":\"" + data.division + "\",\"types\":\"" + data.types + "\",\"title\":\"" + data.title + "\",\"hashTags\":\"" + data.hashTags + "\",\"desc\":\"" + data.desc + "\",\"imageUri\":\"" + data.imageUri + "\",\"thumnailUri\":\"" + data.thumnailUri + "\",\"meta\":\"" + data.meta + "\",\"transform\":\"" + data.transform + "\",\"quantity\":" + data.quantity + "}}");
    }
    /// <summary>
    /// download from server
    /// </summary>
    public override void DownloadAsset() {
        roomItems = new List<Item>(); // room item initialize
        websocket.OnSend("{\"direction\":\"request\",\"path\": \"/room/asset/list\",\"arguments\":{\"no\":\"" + RoomData.instance.EnterRoomNo + "\"}}");

    }
    public override void UpdateAsset(string _path, string _argument) {
        if (string.IsNullOrEmpty(_argument)){
            websocket.OnSend("{\"direction\":\"request\",\"path\": \"" + _path + "\"}");
        }
        else
        {
            websocket.OnSend("{\"direction\":\"request\",\"path\": \"" + _path + "\",\"arguments\":" + _argument + "}");
        }
    }
   
    public override void MoveToRoom(InventoryItem _invenItem, int _quantity)
    {
        /* invenItem = new InventoryItem();
         invenItem.no = 343582899100000054;
         invenItem.transform = "0.0,2.0,0.0,0.0,0.0,0.0,1.0,1.0,1.0";
         invenItem.quantity = 1;
         Debug.Log("MoveToRoom assetNo  :" + invenItem.no + " /  _quantity :" + _quantity);*/

        invenItem = _invenItem;
        websocket.OnSend("{\"direction\":\"request\",\"path\": \"/room/asset/put\",\"arguments\":{\"assetNo\":" + _invenItem.no + ",\"transform\":\"" + _invenItem.transform + "\",\"quantity\":" + _quantity + ",\"sellable\":\"N\",\"salePoint\":0}}");
    }
    public override void MoveToInventory(Item _item, int _quantity) {
        websocket.OnSend("{\"direction\":\"request\",\"path\": \"/room/asset/retrieve\",\"arguments\":{\"assetNo\":" + _item.no + ",\"assetSubNo\":" + _item.PropertiesData.assetSubNo + ",\"quantity\":" + _quantity + "}}");
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_item"></param>
    public override void RemoveAsset(Item _item)
    {
     
    }
    /// <summary>
    /// 인벤-> 룸 에셋 이동 팝업
    /// </summary>
    private void SelectToRoomPopup(long assetNo)
    {
        Debug.Log("SelectToRoomPopup assetNo : "+ assetNo);
        DialogUI.Instance
       .SetData(LocalizeScriptableInfo.GetInstance.assetToRoom)
       .OnClose(() => Debug.Log("Closed"))
       .OnFirstBtn(() => {
           InquiryInvenAsset(assetNo);
           
       })
       .Show();
    }
    
    
}
