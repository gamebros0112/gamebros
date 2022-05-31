using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun.Demo.Asteroids;
using Photon.Pun;
public class PhotonRoomSetting : MonoBehaviourPunCallbacks
{
    [Header("Login Panel")]
    public GameObject LoginPanel;

    public InputField PlayerNameInput;

    [Header("Selection Panel")]
    public GameObject SelectionPanel;

    [Header("Create Room Panel")]
    public GameObject CreateRoomPanel;

    public InputField RoomNameInputField;
    public InputField MaxPlayersInputField;

    [Header("Join Random Room Panel")]
    public GameObject JoinRandomRoomPanel;

    [Header("Room List Panel")]
    public GameObject RoomListPanel;

    public GameObject RoomListContent;
    public GameObject RoomListEntryPrefab;

    [Header("Inside Room Panel")]
    public GameObject InsideRoomPanel;

    public Button StartGameButton;
    public GameObject PlayerListEntryPrefab;
    
    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;
    private Dictionary<int, GameObject> playerListEntries;

    private string roomName;
    [SerializeField]private FirebaseManagerLobby _firebaseLobby;

    // debug.log active/deactive
    public bool IsDebugLog;

    #region UNITY

    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
       
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();

        Debug.unityLogger.logEnabled = IsDebugLog;
    }

    private void Start()
    {
        if(PhotonRoomData.PhotonNickName=="")
            PlayerNameInput.text = "UID" + Random.Range(0, 120) + "@gesta.com";
        else
        {
            PhotonNetwork.LocalPlayer.NickName = PhotonRoomData.PhotonNickName;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        _firebaseLobby.OnCompleteRoomList += OnGetRoomListComplete;
    }

    public override void  OnDisable()
    {
        base.OnDisable();
        _firebaseLobby.OnCompleteRoomList -= OnGetRoomListComplete;
    }
    #endregion

    #region PUN CALLBACKS

    public override void OnConnectedToMaster()
    {
        this.SetActivePanel(SelectionPanel.name);
    }
    /*
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }
*/
    public override void OnJoinedLobby()
    {
        // whenever this joins a new lobby, clear any previous room lists
        cachedRoomList.Clear();
        ClearRoomListView();
    }

    // note: when a client joins / creates a room, OnLeftLobby does not get called, even if the client was in a lobby before
    public override void OnLeftLobby()
    {
        Debug.Log("OnLeftLobby OnLeftLobby OnLeftLobby");
        cachedRoomList.Clear();
        ClearRoomListView();
       
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        //LogFeedback("<Color=Red>OnDisconnected</Color> " + cause);
        Debug.Log("photon room setting PUN Disconnected");
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }
    }


    #endregion

    #region UI CALLBACKS

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        SetActivePanel(SelectionPanel.name);
    }

    public void OnCreateRoomButtonClicked()
    {
        cachedRoomList.Clear();
        PhotonNetwork.LoadLevel("MainRoomControl");


        /*
        roomName = "Room " + Random.Range(1000, 10000);

        // RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers, PlayerTtl = 10000 };
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)8;
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        roomOptions.CustomRoomProperties.Add(RoomDataManager.OwnerEmail, PhotonNetwork.LocalPlayer.NickName);
        roomOptions.CustomRoomProperties.Add(RoomDataManager.RoomTitle, roomName);
        roomOptions.CustomRoomProperties.Add(RoomDataManager.RoomType, "Room8X12X4");
        roomOptions.CustomRoomProperties.Add(RoomDataManager.PlayerList, new string[0]);
        roomOptions.CustomRoomProperties.Add(RoomDataManager.ImageGroup, new string[0]);
        roomOptions.CustomRoomProperties.Add(RoomDataManager.MusicGroup, new string[0]);
        roomOptions.CustomRoomProperties.Add(RoomDataManager.ModelGroup, new string[0]);
        roomOptions.CustomRoomProperties.Add(RoomDataManager.VideoGroup, new string[0]);
        roomOptions.CustomRoomProperties.Add(RoomDataManager.DocGroup, new string[0]);
        roomOptions.CustomRoomPropertiesForLobby = new string[] { roomName,"TestLobbyProperties" };
     
        PhotonNetwork.CreateRoom(roomName, roomOptions, null);
       */
    }

    
    public void OnJoinRandomRoomButtonClicked()
    {
        SetActivePanel(JoinRandomRoomPanel.name);

        PhotonNetwork.JoinRandomRoom();
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnConnectButtonClicked()
    {
        string playerName = PlayerNameInput.text;

        if (!playerName.Equals(""))
        {
            PhotonRoomData.PhotonNickName = playerName;
            PhotonNetwork.LocalPlayer.NickName = playerName;
            // PhotonNetwork.ConnectUsingSettings();
            this.SetActivePanel(SelectionPanel.name);
        }
        else
        {
            Debug.LogError("Player Name is invalid.");
        }
    }

    public void OnRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        SetActivePanel(RoomListPanel.name);
        _firebaseLobby.UpdateRoomList();
    }

    public void OnStartGameButtonClicked()
    {
        /*
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LoadLevel("MainRoomControl");
        */
    }

    #endregion

    private bool CheckPlayersReady()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object isPlayerReady;
            if (p.CustomProperties.TryGetValue(AsteroidsGame.PLAYER_READY, out isPlayerReady))
            {
                if (!(bool)isPlayerReady)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        roomListEntries.Clear();
    }

    public void LocalPlayerPropertiesUpdated()
    {
        StartGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    public void SetActivePanel(string activePanel)
    {
        LoginPanel.SetActive(activePanel.Equals(LoginPanel.name));
        SelectionPanel.SetActive(activePanel.Equals(SelectionPanel.name));
        CreateRoomPanel.SetActive(activePanel.Equals(CreateRoomPanel.name));
        JoinRandomRoomPanel.SetActive(activePanel.Equals(JoinRandomRoomPanel.name));
        RoomListPanel.SetActive(activePanel.Equals(RoomListPanel.name));    // UI should call OnRoomListButtonClicked() to activate this
        InsideRoomPanel.SetActive(activePanel.Equals(InsideRoomPanel.name));
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }

                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // Add new room info to cache
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    private void UpdateRoomListView()
    {
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject entry = Instantiate(RoomListEntryPrefab);
            entry.transform.SetParent(RoomListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<RoomListEntry>().Initialize(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

            roomListEntries.Add(info.Name, entry);
        }
    }

    private void OnGetRoomListComplete(List<string> _roomlist)
    {
        ClearRoomListView();

        //UpdateCachedRoomList(roomList);
        //UpdateRoomListView();
        
       
        foreach (var room in _roomlist)
        {
            GameObject entry = Instantiate(RoomListEntryPrefab);
            entry.transform.SetParent(RoomListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<RoomListEntry>().Initialize(room, (byte)1, 15);

            roomListEntries.Add(room, entry);
        }
    }
}
