using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using Photon.Pun;
using TriLibCore;
using Unity.Mathematics;
using SuperPivot;
using UnityEngine.SceneManagement;
using BrainFailProductions.PolyFewRuntime;
using System;
using UnityEngine.Video;


/// <summary>
/// 룸 에셋 관리하는 클래스 
/// </summary>
public class AssetLoadManager : MonoBehaviour
{
    // Addressable Lable name
    const string ADDRESSABLE_LABEL_PROPS = "props";
    // 에디트 가능 레이어
    const string RayTargetLayer = "EditableObject";
    // 로드한 각각의 에셋 데이터 BaseAssetData 오브젝트에 저장후 리스트로 관리 
    private List<BaseAssetData> assetDataList = new List<BaseAssetData>();
    // 메세지 팝업 
    private MessageManager _message;
    // 에셋 초기 좌표 
    [SerializeField] private Vector3 LoadAssetOffset;
    // 다이얼로그 팝업에 표시할 로컬라이즈 텍스트 정보
    [SerializeField] DefaultPopupSO assetInstantiatePopupData;
    // 버텍스 컬러 쉐이더 (버텍스 컬러 사용 에셋인 여기에 할당된 쉐이더로 교체 )
    public Shader vertexColorShader;

    /// <summary>
    /// 서버에 저장된 어드레서블 에셋 가져오기 
    /// </summary>
    private void Start()
    {
        _message = MessageManager.GetInstance;
        //var catalog = Addressables.LoadContentCatalogAsync("https://gestagalleykr.s3.ap-northeast-2.amazonaws.com/Addressables/catalog_gesta_v1.0.json");
    }
    /// <summary>
    /// 로드 에셋 버튼 콜백에 연결 
    /// </summary>
    private void OnEnable()
    {
        //디버그 에셋 로드 버튼 클릭시 (테스트에만 사용)
        //RoomManager.GetInstance.OnLoadAsset += OnLoadAssetData;
        // 포톤 룸 입장 완료시 
        RoomManager.GetInstance.pRoomCtl.OnCompleteJoinedRoom += OnInstantiateAssets;
        // 서버로 부터 룸에셋 리스트 다운로드 완료시 호출 
        RoomManager.GetInstance._baseServerManager.LoadRoomItems += OnLoadRoomItem;
    }
    private void OnDisable()
    {
        //RoomManager.GetInstance.OnLoadAsset -= OnLoadAssetData;
        RoomManager.GetInstance.pRoomCtl.OnCompleteJoinedRoom -= OnInstantiateAssets;
        RoomManager.GetInstance._baseServerManager.LoadRoomItems -= OnLoadRoomItem;
    }
    private void OnInstantiateAssets()
    {
        StartCoroutine(CoroutineInstantiateAssets());
    }

    /// <summary>
    /// 타입에 따라 에셋 instantiate
    /// </summary>
    /// <returns></returns>
    IEnumerator CoroutineInstantiateAssets()
    {
        int cnt = assetDataList.Count;
        for (int i = 0; i < cnt; i++)
        {
            switch (assetDataList[i].item.types)
            {
                case "I":
                    yield return StartCoroutine(PhotonInstantiatePicture(assetDataList[i]));
                    break;
                case "A":
                    yield return StartCoroutine(PhotonInstantiateAudio(assetDataList[i]));
                    break;
                case "V":
                    yield return StartCoroutine(PhotonInstantiateVideo(assetDataList[i]));
                    break;
                case "D":
                    yield return StartCoroutine(PhotonInstantiatePDF(assetDataList[i]));
                    break;
                case "T":
                    yield return StartCoroutine( PhotonInstantiateModel(assetDataList[i]));
                    break;
            }

        }

        // 메세지 창 켜져있으면 꺼주기 
        if (_message.IsActiveMessage())_message.CloseMessage();
        // Instantiate 룸에셋이 한개 이상이면 CustomFeel에서 이동 시작 
        if (cnt>0) CustomFeel.GetInstance.ReserveDoTween();

        yield return null;

    }

    /// <summary>
    /// 룸에셋 전체 로드 완료시 호출
    /// </summary>
    private void OnCompleteLoaderProgress()
    {
        // 로더 제거 
        SceneManager.UnloadSceneAsync("Loader");
        // 다른곳에서 에셋 데이터 조회할수 있게 싱글톤 매니저에 할당  
        RoomManager.GetInstance.assetDataList = assetDataList;
        // 포톤 연결 시작 
        RoomManager.GetInstance.pRoomCtl.StartPhotonRoom();

        // 방장인 경우 웰컴 메세지 띄우기 (현재는 비활성)
        if (PhotonNetwork.IsMasterClient)
        {
            string loadingText = LocalizeScriptableInfo.GetInstance.assetLoading.ContentsLocalize.GetLocalizedString();
            _message.SetMessage(loadingText);
            _message.TurnOnMessage(false);
        }
        
    }
    /// <summary>
    /// 서버로 부터 룸에셋 리스트 다운로드 완료시 호출됨 
    /// </summary>
    /// <param name="_items"></param>
    private void OnLoadRoomItem(List<Item> _items=null)
    {
        /// 룸에셋 리스트에 값이 없으면 룸에셋 없는 경우. 바로 포톤 연결. 
        if (_items == null)
        {
            RoomManager.GetInstance.pRoomCtl.StartPhotonRoom();
        }
        // 아닌 경우 룸에셋 로드하는 함수 호출 
        else
        {
            StartCoroutine(StartLoaderScene(_items));
        }
            
    }
    /// <summary>
    /// 에셋 로드 분기 
    /// </summary>
    /// <param name="_items">룸에셋 리스트</param>
    /// <returns></returns>
    IEnumerator StartLoaderScene(List<Item> _items)
    {
        // 로더 실행 
        AsyncOperation operation = SceneManager.LoadSceneAsync("Loader", LoadSceneMode.Additive);

        // 씬로딩 완료될때까지 대기 
        while (!operation.isDone) 
        {
            yield return null;
        }

        // 어들레서블 로드가 완료될때까지 대기 
        RoomManager rm = RoomManager.GetInstance;
        while (!rm.isLoadCompleteAddressables) 
        {
            yield return null;
        }

        // 에셋 로드를 위해 인스턴스 할당 
        BaseAssetLoadManager balm = BaseAssetLoadManager.GetInstance;
        // 프로그레스바 초기화
        balm.InitProgress();

        float totalProgress;

        for (var i = 0; i < _items.Count; i++)
        {
            Item tempItems = _items[i];

            // 어드레서블 or S3에 올린 일반 데이터인지 분기
            if (_items[i].division == "N")
            {
                // 에셋 타입에 따른 분기 
                switch (_items[i].types)
                {
                    case "I":   // 이미지 
                        yield return StartCoroutine(balm.DownloadTextureWithProgress(_items[i], _items.Count, i, (Texture2D _tex2d) => {
                            LoadPicture(tempItems, _tex2d);
                        }));
                        break;
                    case "A":   // 오디오 
                        LoadAudio(_items[i],null);
                        totalProgress = (i + 1) / _items.Count;
                        balm._progressBarManager.Ratio = totalProgress;

                        break;
                    case "V":   // 비디오 
                        LoadVideo(_items[i], null);
                        totalProgress = (i + 1) / _items.Count;
                        balm._progressBarManager.Ratio = totalProgress;
                        break;
                    case "D":   // 문서 
                        yield return StartCoroutine(balm.DownloadPDF(_items[i], _items.Count, i, true, (byte[] _pdf) => {
                            LoadPDF(tempItems, _pdf);
                        }));
                        break;
                    case "T":   // 모델 
                        yield return StartCoroutine(balm.DownloadModel(_items[i], _items.Count, i, true, (byte[] byteModel) => {
                            AddModelData(tempItems);
                            LoadFromStream(byteModel, tempItems.no, tempItems.PropertiesData.assetSubNo);
                        }));
                        
                        break;
                }

            }
            else
            {
                // 어드레서블 데이터 타입에 따라 로드하기 
                switch (_items[i].types)
                {
                    case "I":
                        yield return StartCoroutine(balm.LoadAddressableImageWithProgress(_items[i], _items.Count, i, (Texture2D _tex2d) => {
                            LoadPicture(tempItems, _tex2d);
                        }));
                        break;
                    case "A":
                        yield return StartCoroutine(balm.LoadAddressableAudioWithProgress(_items[i], _items.Count, i, (AudioClip ac) => {
                            LoadAudio(_items[i], ac);
                        }));
                        break;
                    case "V":
                        yield return StartCoroutine(balm.LoadAddressableVideoWithProgress(_items[i], _items.Count, i, (VideoClip vc) => {
                            LoadVideo(_items[i], vc);
                        }));
                        break;
                    case "D":
                        yield return StartCoroutine(balm.LoadAddressablePDFWithProgress(_items[i], _items.Count, i, (byte[] _pdf) => {
                            LoadPDF(tempItems, _pdf);
                        }));
                        break;
                    case "T":
                        yield return StartCoroutine(balm.LoadAddressableModel(_items[i], _items.Count, i, (GameObject go) => {
                            AddModelData(tempItems);
                            InitModel(go, tempItems.no, tempItems.PropertiesData.assetSubNo,true);
                        }));
                        break;

                }

            }
        }

        // 로더 제거  
        OnCompleteLoaderProgress();
    }
    /*
    IEnumerator LoadRoomAsset(List<Item> _items)
    {
        foreach(var i in _items)
        {
            OnLoadAssetDataWithItem(i);
            yield return new WaitForSeconds(1.5f);
        }
        
        yield return null;
    }
    */
    
    /// <summary>
    /// 어드레서블 레이블 마다 로드하기
    /// </summary>
    private void OnLoadAssetData()
    {
        // 어드레서블 가져오기 ( 메모리에 로드 안함 )
        Addressables.LoadResourceLocationsAsync(ADDRESSABLE_LABEL_PROPS, typeof(GameObject)).Completed += (AsyncOperationHandle<IList<IResourceLocation>> obj) =>
        {
            foreach (var resource in obj.Result)
            {
                // 메모리 로드되고 씬에 로드 
                Addressables.InstantiateAsync(resource).Completed += (loadAsset) =>
                {
                    GameObject go = loadAsset.Result;
                    // 에디트 가능한 레이어로 변경 
                    ObjectUtils.ChangeLayersRecursively(go.transform, RayTargetLayer);
                };
            }
        };
    }

    public void CheckTheDownloadFileSize()
    {
        //크기를 확인할 번들 또는 번들들에 포함된 레이블을 인자로 주면 됨.
        //long타입으로 반환되는게 특징임.
        Addressables.GetDownloadSizeAsync(ADDRESSABLE_LABEL_PROPS).Completed +=
            (AsyncOperationHandle<long> SizeHandle) =>
            {
                string sizeText = string.Concat(SizeHandle.Result, " byte");

                //메모리 해제.
                Addressables.Release(SizeHandle);
            };
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////

    /// 에셋 용량 체크
    /*
    private void GetAssetSize(UnityWebRequest _www)
    {
        ulong size = 0;
        var header = _www.GetResponseHeader("Content-Length");
        if (header != null)
        {
            ulong.TryParse(header, out size);
            ulong sizeInkb = size / 1024;

            Debug.Log("img file size : " + sizeInkb + " KB");
        }
    }
    */


    /// <summary>
    /// 이미지 파일 가져오기
    /// </summary>
    /// <param name="_item"></param>
    /// <returns></returns>
    private void LoadPicture(Item _item, Texture2D myTexture)
    {
        myTexture.Compress(true);
        BaseAssetData baseAssetData = new BaseAssetData
        {
            item = _item,
            imgData = myTexture
        };
        assetDataList.Add(baseAssetData);
    }
    IEnumerator PhotonInstantiatePicture(BaseAssetData _baseData)
    {
        string prefabPath = "Prefabs/AssetContainer/PictureAssetContainer";
        GameObject go = PhotonNetwork.InstantiateRoomObject(prefabPath, LoadAssetOffset, quaternion.identity, 0, new object[] { _baseData.item.no, _baseData.item.PropertiesData.assetSubNo });

        go.name = _baseData.item.PropertiesData.assetSubNo.ToString();
        go.SetActive(false);
        //ObjectTransform objTransform = JsonConvert.DeserializeObject<ObjectTransform>(_baseData.item.OBJECT_TRANSFORM);
        
        //go.transform.eulerAngles = objTransform.rotation;
        //go.transform.localScale = objTransform.scale;

        // s3 서버에서 가져온 이미지 적용 
        // go.transform.Find("Cube_FramePictureSide").GetComponent<Renderer>().material.SetTexture("_BaseMap", myTexture);
        Material[] mat = go.transform.Find("Asset/Cube_FramePictureSide").GetComponent<Renderer>().materials;
        mat[1].SetTexture("_BaseMap", _baseData.imgData);
        // 아이템 데이터 적용
        go.GetComponent<AssetNetworkController>().AssetData = _baseData.item;
        // Feel 적용
        CustomFeel.GetInstance.AssetEnqueue(go);

        yield return null;
    }
    /// <summary>
    /// pdf 파일 가져오기
    /// </summary>
    /// <param name="_url"></param>
    /// <returns></returns>
    private void LoadPDF(Item _item, byte[] _data)
    {
        BaseAssetData baseAssetData = new BaseAssetData
        {
            item = _item,
            docData = _data
        };
        assetDataList.Add(baseAssetData);
    }
    private void LoadAudio(Item _item, AudioClip _ac = null)
    {

        BaseAssetData baseAssetData = new BaseAssetData();
        baseAssetData.item = _item;
        if (_ac) baseAssetData.audioData = _ac;

        // baseAssetData.uniqueId = _item.OBJECT_UNIQUEID;
        assetDataList.Add(baseAssetData);
    }
    private void LoadVideo(Item _item, VideoClip _vc = null)
    {
        BaseAssetData baseAssetData = new BaseAssetData();
        baseAssetData.item = _item;
        if (_vc) baseAssetData.videoData = _vc;
        // baseAssetData.uniqueId = _item.OBJECT_UNIQUEID;
        assetDataList.Add(baseAssetData);
    }
    IEnumerator PhotonInstantiatePDF(BaseAssetData _baseData)
    {
        // 리소스 프리팹 로드 
        string prefabPath = "Prefabs/AssetContainer/PDFAssetContainer";
        // 방나가도 안없어지게 InstantiateRoomObject 생성 
        GameObject go = PhotonNetwork.InstantiateRoomObject(prefabPath, LoadAssetOffset, quaternion.identity, 0, new object[] { _baseData.item.no, _baseData.item.PropertiesData.assetSubNo });
        go.name = _baseData.item.PropertiesData.assetSubNo.ToString();
        // s3 서버에서 가져온 pdf 적용 
        go.GetComponent<PDFLoader>().InitPDF(_baseData.docData);
        // 아이템 데이터 적용
        go.GetComponent<AssetNetworkController>().AssetData = _baseData.item;

        // Feel 적용
        CustomFeel.GetInstance.AssetEnqueue(go);

        yield return null;

    }
    /// <summary>
    /// 비디오 파일 가져오기
    /// </summary>
    IEnumerator PhotonInstantiateVideo(BaseAssetData _baseData)
    {
        string prefabPath = "Prefabs/AssetContainer/VideoAssetContainer";
        GameObject go = PhotonNetwork.InstantiateRoomObject(prefabPath, LoadAssetOffset, quaternion.identity, 0, new object[] { _baseData.item.no, _baseData.item.PropertiesData.assetSubNo });
        go.name = _baseData.item.PropertiesData.assetSubNo.ToString();

        //ObjectTransform objTransform = JsonConvert.DeserializeObject<ObjectTransform>(_item.OBJECT_TRANSFORM);

        //go.transform.eulerAngles = objTransform.rotation;
        //go.transform.localScale = objTransform.scale;

        // 아이템 데이터 적용
        go.GetComponent<AssetNetworkController>().AssetData = _baseData.item;
        //VideoPlayer vplayer = go.GetComponent<VideoPlayer>();
        //vplayer.url = _item.OBJECT_LOCATION;
        go.GetComponent<VideoViewController>().SetVideo(_baseData.item.imageUri);

        // Feel 적용
        CustomFeel.GetInstance.AssetEnqueue(go);

        yield return null;
    }

    /// <summary>
    /// 오디오 파일 가져오기 
    /// </summary>
    /// <param name="_url"></param>
    /// <returns></returns>
    IEnumerator PhotonInstantiateAudio(BaseAssetData _baseData)
    {
        string prefabPath = "Prefabs/AssetContainer/AudioAssetContainer";
        GameObject go = PhotonNetwork.InstantiateRoomObject(prefabPath, LoadAssetOffset, quaternion.identity, 0, new object[] { _baseData.item.no, _baseData.item.PropertiesData.assetSubNo });
        go.name = _baseData.item.PropertiesData.assetSubNo.ToString();
        //ObjectTransform objTransform = Utility.GetParsingObjectTransform(_baseData.item.PropertiesData.transform);
        //go.transform.eulerAngles = objTransform.rotation;
        //go.transform.localScale = objTransform.scale;

        // 오디오 파일 설정 셋업 타이틀, url 
        bl_APAudioWeb audioWeb = new bl_APAudioWeb
        {
            AudioTitle = _baseData.item.no.ToString(),
            URL = _baseData.item.imageUri,
            m_AudioType = AudioType.MPEG
        };

        // 오디오 데이터 없으면 다운로드 ( 패키지 자체에서 다운로드 )
        if (_baseData.audioData==null)
        {
            bl_DownloadAudio downloadAudioWeb = go.GetComponent<bl_DownloadAudio>();
            downloadAudioWeb.AudioURLs.Add(audioWeb);
            downloadAudioWeb.StartDownload();
        }
        else
        {
            // 오디오 데이터 할당
            AudioSource ac = go.GetComponent<AudioSource>();
            ac.clip = _baseData.audioData;
        }
        


        // 아이템 데이터 적용
        go.GetComponent<AssetNetworkController>().AssetData = _baseData.item;

        // Feel 적용
        CustomFeel.GetInstance.AssetEnqueue(go);

        yield return null;
    }
    /// <summary>
    /// 모델 로더 패키지 api  호출 
    /// </summary>
    /// <param name="url">S3 주소</param>
    /// <param name="parentObj">부모 오브젝트</param>
    public void LoadFromURL(string url, GameObject parentObj)
    {
        // 옵션 설정 
        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        assetLoaderOptions.GenerateColliders = true;
        var webRequest = AssetDownloader.CreateWebRequest(url);
        string urlLoadFileExtension = "glb";
        AssetDownloader.LoadModelFromUri(webRequest, OnLoadModel, null, null, null, parentObj, assetLoaderOptions,null, urlLoadFileExtension);
    }
    /// <summary>
    /// 모델 데이터 저장 
    /// </summary>
    /// <param name="_item"></param>
    private void AddModelData(Item _item)
    {
        BaseAssetData baseAssetData = new BaseAssetData
        {
            item = _item
        };

        assetDataList.Add(baseAssetData);
    }
    /// <summary>
    /// 포톤 에셋 생성 
    /// </summary>
    /// <param name="_baseData"></param>
    /// <returns></returns>
    IEnumerator PhotonInstantiateModel(BaseAssetData _baseData)
    {
        
        string prefabPath = "Prefabs/AssetContainer/ModelAssetContainer";
        GameObject rootObj = PhotonNetwork.InstantiateRoomObject(prefabPath, LoadAssetOffset, quaternion.identity, 0, new object[] { _baseData.item.no,_baseData.item.PropertiesData.assetSubNo });
        AssetNetworkController anc = rootObj.GetComponent<AssetNetworkController>();
        anc.AssetData = _baseData.item;
        
        // 에셋 생성 부모 오브젝트 설정 
        _baseData.modelData.transform.parent = rootObj.transform.Find("Asset");
        _baseData.modelData.SetActive(true);

        rootObj.name = _baseData.item.no.ToString();
        // 인트로 트윈 전까지 비활성 
        rootObj.SetActive(false);

        // 피봇 센터로 정렬, 모델 파일만 컨테이너 자식으로 붙기 때문에 피봇 재설정 필요
        // 일반 glb 파일만 피봇 정렬 , 어드레서블은 제작시 이미 센터로 피봇 설정. 
        if (_baseData.item.division == "N")
        {
            Bounds totalBounds = ObjectUtils.GetChildRendererBounds(_baseData.modelData);
            Vector3 centerBoundPos = new Vector3(totalBounds.center.x, totalBounds.center.y, totalBounds.center.z);
            API.SetPivot(_baseData.modelData.transform, centerBoundPos, API.Space.Global);
        }

        _baseData.modelData.transform.localPosition = Vector3.zero;



        //ObjectUtils.ChangeLayersRecursively(rootObj.transform, RayTargetLayer);
        //ObjectTransform objTransform = JsonConvert.DeserializeObject<ObjectTransform>(_baseData.item.OBJECT_TRANSFORM);


        // Feel 적용
        CustomFeel.GetInstance.AssetEnqueue(rootObj);


        yield return null;
        //yield return new WaitForSeconds(1f);
    }
    /// <summary>
    /// 바이트 어레이 데이터로 생성 
    /// 로더로 이미 가져왔으므로 처음부터 다시 가져올 필요 없어서 
    /// </summary>
    /// <param name="modelByte"></param>
    /// <param name="_assetNo"></param>
    /// <param name="_assetSubNo"></param>
    public void LoadFromStream(byte[] modelByte,long _assetNo, long _assetSubNo)
    {
        Stream stream = new MemoryStream(modelByte);
        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        assetLoaderOptions.GenerateColliders = true;
        // assetLoaderOptions.TextureCompressionQuality = TriLibCore.General.TextureCompressionQuality.Best;
        string urlLoadFileExtension = "glb";
        AssetLoader.LoadModelFromStream(stream, null, urlLoadFileExtension, OnLoadModel, null, null, null, null, assetLoaderOptions, new long[] { _assetNo,_assetSubNo });
    }
    /// <summary>
    /// 로드 모델 콜백 
    /// </summary>
    /// <param name="assetLoaderContext"></param>
    private void OnLoadModel(AssetLoaderContext assetLoaderContext)
    {
        GameObject go = assetLoaderContext.RootGameObject;
        long[] customData = (long[])assetLoaderContext.CustomData;
        InitModel(go, customData[0], customData[1], false);
    }
    /// <summary>
    /// 모델 생성후 레이어, 마테리얼 초기화 
    /// </summary>
    private void InitModel(GameObject go, long assetNo, long assetSubNo, bool isAddressable)
    {
        FindBaseData(assetNo, assetSubNo).modelData = go;
        if(isAddressable) ObjectUtils.CustomMaterialWithMeshCollider(go, vertexColorShader);
        else ObjectUtils.CustomMaterial(go, vertexColorShader);

        go.SetActive(false);
        // layer 할당
        ObjectUtils.ChangeLayersRecursively(go.transform, RayTargetLayer);
        
    }
    /// <summary>
    /// 에셋 번호로 에셋데이터 리스트 조회 
    /// </summary>
    /// <param name="_assetNo"></param>
    /// <param name="_assetSubNo"></param>
    /// <returns></returns>
    private BaseAssetData FindBaseData(long _assetNo, long _assetSubNo)
    {
        foreach(BaseAssetData bdata in assetDataList)
        {
            if (bdata.item.no == _assetNo && bdata.item.PropertiesData.assetSubNo == _assetSubNo) return bdata;
        }
        return null;
    }
    /// <summary>
    /// 폴리퓨 에셋 사용해서 버텍스 최적화 진행   
    /// </summary>
    /// <param name="targetObject"></param>
    public void OnReductionChange(GameObject targetObject)
    {
        float[] PercentSet = new float[] { 20f, 10f, 100f };
       
        //int quality = RoomManager.GetInstance.SetupMgr.QualityLevel;
        int quality = 0;
        if (quality == 2) return;
        float simplifyPercent = PercentSet[quality];

        PolyfewRuntime.SimplificationOptions options = new PolyfewRuntime.SimplificationOptions
        {
            simplificationStrength = simplifyPercent,
            enableSmartlinking = true,
            preserveBorderEdges = true,
            preserveUVSeamEdges = true,
            preserveUVFoldoverEdges = true,
            recalculateNormals = true,
            regardCurvature = true
        };

        string result = PolyfewRuntime.SimplifyObjectDeep(PolyfewRuntime.GetObjectMeshPairs(targetObject, true), options, (GameObject go, PolyfewRuntime.MeshRendererPair mInfo) =>
        {
            // Debug.Log("Simplified mesh  " + mInfo.mesh.name + " on GameObject  " + go.name);
        }) + "";


    }
    

}
