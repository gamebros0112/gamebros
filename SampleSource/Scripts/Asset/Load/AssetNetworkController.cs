using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using EasyUI.Dialogs;
using Newtonsoft.Json;
using Photon.Pun;
using SuperPivot;
using TriLibCore;
using UnityEngine;
using UnityEngine.Networking;

public class AssetNetworkController : MonoBehaviourPunCallbacks,IPunObservable,IPunInstantiateMagicCallback
{
    [SerializeField] private float PositionSerializeSpeed;
    [SerializeField] private float RotationSerializeSpeed;
    [SerializeField] private float ScaleSerializeSpeed;
    [SerializeField] private Transform serializeTarget;

    private PhotonView pView;
    private bool isEdit;
    private LocalServerManager lsm;
    private Sequence seq;
    private Item assetData;

    public Item AssetData { get => assetData; set => assetData = value; }

    void Awake()
    {
        lsm = (LocalServerManager)RoomManager.GetInstance._baseServerManager;
    }
    void Start()
    {
        // rpc call 전송 빈도
        PhotonNetwork.SendRate = 20;
        //OnPhotonSerializeView 호출 빈도 
        PhotonNetwork.SerializationRate = 10;
        pView = GetComponent<PhotonView>();
        seq = DOTween.Sequence();


    }
    /// <summary>
    /// 이벤트 연결 
    /// </summary>
    public override void OnEnable()
    {
        base.OnEnable();
        lsm.OnAssetQtyLeft += OnRefreshQty;
        lsm.OnRoomAssetUpdateTransform += OnUpdateTransform;
        lsm.OnRoomAssetUpdate += OnRoomAssetUpdateSpecator;
    }
    /// <summary>
    /// 이벤트 해제 
    /// </summary>
    public override void OnDisable()
    {
        base.OnDisable();
        lsm.OnAssetQtyLeft -= OnRefreshQty;
        lsm.OnRoomAssetUpdateTransform -= OnUpdateTransform;
        lsm.OnRoomAssetUpdate -= OnRoomAssetUpdateSpecator;
    }
    /// <summary>
    /// 거래후 남은 에셋 
    /// </summary>
    private void OnRefreshQty(long _assetNo, long _assetSubNo, uint _qtyLeft)
    {
        if(assetData.no == _assetNo && assetData.PropertiesData.assetSubNo == _assetSubNo)
        {
            assetData.PropertiesData.quantity = _qtyLeft;
            // 남은 에셋 수량이 0과 같거나 작으면 삭제
            if (_qtyLeft <= 0)
            {
                // 관전자일때
                if(PhotonNetwork.OfflineMode) Destroy(gameObject);
                // 방장일때
                else if(PhotonNetwork.IsMasterClient)PhotonNetwork.Destroy(gameObject);

            }
        }
       
    }
    /// <summary>
    /// 룸에셋 위치 업데이트
    /// 푸쉬 이벤트 받으면 호출 
    /// </summary>
    private void OnUpdateTransform(long _assetNo, long _assetSubNo, string _transform)
    {
        if (assetData.no == _assetNo && assetData.PropertiesData.assetSubNo == _assetSubNo)
        {
            ObjectTransform _objTrans = Utility.GetParsingObjectTransform(_transform);
            seq.Append(serializeTarget.DOMove(_objTrans.position, 1).SetSpeedBased());
            seq.Join(serializeTarget.DORotate(_objTrans.rotation, 1f).SetEase(Ease.Linear));
            seq.Join(serializeTarget.DOScale(_objTrans.scale, 1f));
        }
    }
    /// <summary>
    /// 판매정보 업데이트
    /// 푸쉬 이벤트 푸쉬 이벤트 받으면 호출 
    /// </summary>
    private void OnRoomAssetUpdateSpecator(long _assetNo, long _assetSubNo, string _sellable, uint _salePoint)
    {
        if (assetData.no == _assetNo && assetData.PropertiesData.assetSubNo == _assetSubNo)
        {
            GetComponent<AssetControl>().SetActiveSalesBalloon(_sellable);
            assetData.PropertiesData.sellable = _sellable;
            assetData.PropertiesData.salePoint = _salePoint;
        }
    }
    /// <summary>
    /// 포톤 동기화 함수 
    /// </summary>
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!isEdit) return;
        if (stream.IsWriting)
        {
            stream.SendNext(serializeTarget.position);
            stream.SendNext(serializeTarget.rotation);
            stream.SendNext(serializeTarget.localScale);
        }
        else
        {
            if (stream.PeekNext() is double || stream.PeekNext() is bool) return;
            serializeTarget.SetPositionAndRotation((Vector3)stream.ReceiveNext(), (Quaternion)stream.ReceiveNext());
            serializeTarget.localScale = (Vector3)stream.ReceiveNext();
        }

    }
    /// <summary>
    /// photon instantiate 사용한 경우 이 함수 호출됨 
    /// </summary>
    /// <param name="info"></param>
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        pView = GetComponent<PhotonView>();
        // 자신이 생성한 객체면 동기화 필요없음 
        if (pView.IsMine || PhotonNetwork.OfflineMode) return;
        // 생성시 위에서 떨어지도록 위치 이동
        gameObject.transform.position = new Vector3(0, 10, 0);

        //입장시 룸에 배치되어 있던 에셋 데이터가 맞는지 GetBaseAssetData 이용해서 확인 
        //baData가 null 인 경우 유저가 입장한 상태에서 방장이 새로 에셋 추가로 판단.
        //InquiryRoomAsset 에셋 번호, 에셋 서브 번호로 서버에서 데이터 받아와서 에셋 생성 
        long assetNo = (long)pView.InstantiationData[0];
        long assetSubNo = (long)pView.InstantiationData[1];
        BaseAssetData baData = RoomManager.GetInstance.GetBaseAssetData(assetNo, assetSubNo);

        if (baData == null)
        {
            ((LocalServerManager)RoomManager.GetInstance._baseServerManager).InquiryRoomAsset(assetNo, assetSubNo, 
                (Item _item)=> {
                    assetData = _item;
                    SwitchAssetType(null);
                });
        }
        else
        {
            assetData = baData.item;
            SwitchAssetType(baData);
        }
    }

    private void SwitchAssetType(BaseAssetData _baData)
    {
        // 룸 입장시 룸에셋 리스트에서 데이터 가져오는 경우 or 방장이 실시간으로 올린 에셋 데이터 가져오는 경우 분기
        // 룸 에셋 데이터에서 가져올때 
        if (_baData != null)
        {
            // 에셋 타입에 따른 분기 
            switch (assetData.types)
            {
                case "I":
                    LoadPictureFromAssetQueue(_baData);
                    break;
                case "A":
                    LoadAudio();
                    break;
                case "V":
                    LoadVideo();
                    break;
                case "D":
                    gameObject.SetActive(true);
                    LoadPDFFromAssetQueue(_baData);
                    break;
                case "T":
                    LoadModelFromModelObj(_baData);
                    break;

            }
        }
        // 실시간 에셋 추가할때 
        else
        {
            BaseAssetLoadManager balm = BaseAssetLoadManager.GetInstance;

            // 어드레서블 or S3에 올린 일반 데이터인지 분기
            // 일반 에셋 데이터인 경우 
            if (assetData.division == "N")
            {
                switch (assetData.types)
                {
                    case "I":
                        StartCoroutine(balm.DownloadTexture(assetData, (Texture2D _tex2d) => {
                            LoadPicture(_tex2d);
                        }));
                        break;
                    case "A":
                        LoadAudio(null);
                        break;
                    case "V":
                        LoadVideo();
                        break;
                    case "D":
                        gameObject.SetActive(true);
                        StartCoroutine(balm.DownloadPDF(assetData,default,default,false, (byte[] _pdf) => {
                            LoadPDF(_pdf);
                        }));
                        break;
                    case "T":
                        LoadModelFromURL();
                        break;

                }
            }
            // 어드레서블 에셋 데이터인 경우 
            else
            {
                switch (assetData.types)
                {
                    case "I":
                        StartCoroutine(balm.LoadAddressableImage(assetData, (Texture2D _tex2d) => {
                            LoadPicture(_tex2d);
                        }));
                        break;
                    case "A":
                        StartCoroutine(balm.LoadAddressableAudio(assetData, (AudioClip ac) => {
                            LoadAudio(ac);
                        }));
                        break;
                    case "V":
                        LoadVideo();
                        break;
                    case "D":
                        gameObject.SetActive(true);
                        StartCoroutine(balm.LoadAddressablePDF(assetData, (byte[] _pdf) => {
                            LoadPDF( _pdf);
                        }));
                        break;
                    case "T":
                        StartCoroutine(balm.InstantiateAddressableModel(assetData.imageUri, (GameObject go) => {
                            go.transform.parent = transform.Find("Asset");
                            InitModel(go,true);
                        }));
                        break;

                }
            }

                
        }

        //assetData.SetObjectTransform(transform);
        CustomFeel.GetInstance.AssetEnqueue(gameObject);
        CustomFeel.GetInstance.ReserveDoTween();
    }

    /// <summary>
    /// picture load
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private void LoadPicture(Texture2D myTexture)
    {
        myTexture.Compress(true);
        Material[] mat = gameObject.transform.Find("Asset/Cube_FramePictureSide").GetComponent<Renderer>().materials;
        mat[1].SetTexture("_BaseMap", myTexture);

    }

    private void LoadPictureFromAssetQueue(BaseAssetData _baData)
    {
        Texture2D myTexture = (Texture2D)_baData.imgData;
        myTexture.Compress(true);
        Material[] mat = gameObject.transform.Find("Asset/Cube_FramePictureSide").GetComponent<Renderer>().materials;
        mat[1].SetTexture("_BaseMap", myTexture);
    }

    /// <summary>
    /// 3d glb model load
    /// </summary>
    /// <param name="url"></param>
    /// <param name="parentObj"></param>
    private void LoadModelFromURL()
    {
        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        assetLoaderOptions.GenerateColliders = true;
        var webRequest = AssetDownloader.CreateWebRequest(assetData.imageUri);
        string urlLoadFileExtension = "glb";
        GameObject wrapObj = transform.Find("Asset").gameObject;
        AssetDownloader.LoadModelFromUri(webRequest, OnLoadModel, null, null, null, wrapObj, assetLoaderOptions, null, urlLoadFileExtension);
    }
    private void OnLoadModel(AssetLoaderContext assetLoaderContext)
    {
        GameObject go = assetLoaderContext.RootGameObject;
        InitModel(go,false);
    }
    private void InitModel(GameObject go, bool isAddressable)
    {
        // 버텍스 컬러 관련 추가
        ObjectUtils.CustomMaterial(go, RoomManager.GetInstance._assetLoadManager.vertexColorShader);

        // 센터 피봇 설정
        if (!isAddressable)
        {
            Bounds totalBounds = ObjectUtils.GetChildRendererBounds(go);
            Vector3 centerBoundPos = new Vector3(totalBounds.center.x, totalBounds.center.y, totalBounds.center.z);
            API.SetPivot(go.transform, centerBoundPos, API.Space.Global);
        }
        

        // 로컬 제로 포지션 설정
        go.transform.localPosition = Vector3.zero;

        // 레이어 설정 
        ObjectUtils.ChangeLayersRecursively(go.transform, "EditableObject");
    }
    private void LoadModelFromModelObj(BaseAssetData _baData)
    {
        GameObject go = _baData.modelData;
        go.transform.parent = transform.Find("Asset");

        if (_baData.item.division == "N")
        {
            Bounds totalBounds = ObjectUtils.GetChildRendererBounds(go);
            Vector3 centerBoundPos = new Vector3(totalBounds.center.x, totalBounds.center.y, totalBounds.center.z);
            API.SetPivot(go.transform, centerBoundPos, API.Space.Global);
        }
            

        go.transform.localPosition = Vector3.zero;
        ObjectUtils.ChangeLayersRecursively(go.transform, "EditableObject");
        go.SetActive(true);
        
    }
    /// <summary>
    /// load Video
    /// </summary>
    /// <param name="url"></param>
    public void LoadVideo()
    {
        gameObject.SetActive(true);
        gameObject.GetComponent<VideoViewController>().SetVideo(assetData.imageUri);
    }
    /// <summary>
    /// load audio
    /// </summary>
    /// <param name="url"></param>
    public void LoadAudio(AudioClip _ac = null)
    {
        // s3 서버에서 audio file download 
        bl_APAudioWeb audioWeb = new bl_APAudioWeb
        {
            AudioTitle = assetData.title,
            URL = assetData.imageUri,
            m_AudioType = AudioType.MPEG
        };

        gameObject.SetActive(true);
        
        if (_ac == null)
        {
            bl_DownloadAudio downloadAudioWeb = gameObject.GetComponent<bl_DownloadAudio>();
            downloadAudioWeb.AudioURLs.Add(audioWeb);
            downloadAudioWeb.StartDownload();
        }
        else
        {
            AudioSource ac = gameObject.GetComponent<AudioSource>();
            ac.clip = _ac;
        }

    }
    private void LoadPDF(byte[] _pdf )
    {
       gameObject.GetComponent<PDFLoader>().InitPDF(_pdf);
    }

    private void LoadPDFFromAssetQueue(BaseAssetData _baData)
    {
        gameObject.GetComponent<PDFLoader>().InitPDF(_baData.docData);
    }

    /// <summary>
    /// 포톤뷰 동기화 시작
    /// </summary>
    public void OnInitSerialize()
    {
        isEdit = true;
        pView.Synchronization = ViewSynchronization.UnreliableOnChange;
    }
    /// <summary>
    /// 에디트 활성 / 비활성 
    /// </summary>
    /// <param name="_isEdit"></param>
    [PunRPC]
    public void OnAssetEdit(bool _isEdit)
    {
       // Debug.Log("PunRPC OnAssetEdit  _isEdit : " + _isEdit);
        isEdit = _isEdit;
    }
    /// <summary>
    /// 판매풍선 위치 동기화 
    /// </summary>
    [PunRPC]
    public void OnSetPositionSalesBalloon()
    {
        GetComponent<AssetControl>().SetPositionSalesBalloon();
    }
    /// <summary>
    /// 판매 정보 동기화 
    /// </summary>
    [PunRPC]
    public void OnSetSalesInfo(string _sellAble, int _salaPoint)
    {
        GetComponent<AssetControl>().SetActiveSalesBalloon(_sellAble);
        assetData.PropertiesData.sellable = _sellAble;
        assetData.PropertiesData.salePoint = (uint)_salaPoint;
    }
}
