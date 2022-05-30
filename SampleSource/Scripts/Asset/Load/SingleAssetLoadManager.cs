using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Photon.Pun;
using Newtonsoft.Json;
using TriLibCore;
using Unity.Mathematics;
using SuperPivot;
using System;
using UnityEngine.Video;

public class SingleAssetLoadManager : MonoBehaviour
{
    const string RayTargetLayer = "EditableObject";
    
    [SerializeField] private Vector3 LoadAssetOffset;

    private void OnEnable()
    {
        // 인벤토리에서 이동시 호출 
        RoomManager.GetInstance._baseServerManager.LoadFromInventory += OnLoadFromInventory;
        // 룸에셋인 경우 호출 
        RoomManager.GetInstance._baseServerManager.OnCompleteMoveTo += OnLoadFromInventory;
    }
    private void OnDisable()
    {
        RoomManager.GetInstance._baseServerManager.LoadFromInventory -= OnLoadFromInventory;
        RoomManager.GetInstance._baseServerManager.OnCompleteMoveTo -= OnLoadFromInventory;
    }
    private void OnLoadFromInventory(Item _item)
    {
        OnLoadAssetDataWithItem(_item);
    }

    /// <summary>
    /// 프리뷰에서 에셋 로드하려고 호출
    /// </summary>
    /// <param name="_item">Item</param>
    public void LoadFromPreview(Item _item) => OnLoadAssetDataWithItem(_item);

    /// <summary>
    /// type에 따라 분류 
    /// </summary>
    private void OnLoadAssetDataWithItem(Item _item)
    {
        BaseAssetLoadManager balm = BaseAssetLoadManager.GetInstance;
        Item tempItem = _item;

        // 일반 / 어드레서블 분기 
        if (_item.division == "N")
        {
            switch (_item.types)
            {
                case "I":
                    StartCoroutine(balm.DownloadTexture(_item, (Texture2D _tex2d) => {
                        LoadPicture(tempItem, _tex2d);
                    }));
                    break;
                case "A":
                    LoadAudio(_item,null);
                    break;
                case "V":
                    LoadVideo(_item,null);
                    break;
                case "D":
                    StartCoroutine(balm.DownloadPDF(_item, default, default,false, (byte[] _pdf) => {
                        LoadPDF(tempItem, _pdf);
                    }));
                    break;
                case "T":
                    GameObject parentObj = LoadModelContainer(_item);
                    // 모델 로드 패키지 api 호출 
                    LoadFromURL(_item.imageUri, parentObj.transform.Find("Asset").gameObject);
                    break;

            }
        }
        else
        {
            switch (_item.types)
            {
                case "I":
                    StartCoroutine(balm.LoadAddressableImage(_item, (Texture2D _tex2d) => {
                        LoadPicture(tempItem, _tex2d);
                    }));
                    break;
                case "A":
                    StartCoroutine(balm.LoadAddressableAudio(_item, (AudioClip ac) => {
                        LoadAudio(tempItem, ac);
                    }));
                    break;
                case "V":
                    StartCoroutine(balm.LoadAddressableVideo(_item, (VideoClip vc) => {
                        LoadVideo(tempItem, vc);
                    }));
                    break;
                case "D":
                    StartCoroutine(balm.LoadAddressablePDF(_item, (byte[] _pdf) => {
                        LoadPDF(tempItem, _pdf);
                    }));
                    break;
                case "T":
                    GameObject parentObj = LoadModelContainer(_item);
                    // 어드레서블 로드후 콜백으로 초기화 함수 호출 
                    StartCoroutine(balm.InstantiateAddressableModel(_item.imageUri, (GameObject go) => {
                        go.transform.parent = parentObj.transform.Find("Asset");
                        InitModel(go,true);
                    }));
                    
                    break;

            }
        }




            

    }

    /// <summary>
    /// 이미지 로드 
    /// </summary>
    private void LoadPicture(Item _item, Texture2D myTexture)
    {
        // 로드한 텍스처 압축 
        myTexture.Compress(true);
        // 이미지 에셋 프리팹 생성 
        string prefabPath = "Prefabs/AssetContainer/PictureAssetContainer";
        GameObject go = PhotonNetwork.InstantiateRoomObject(prefabPath, LoadAssetOffset, quaternion.identity, 0, new object[] { _item.no, _item.PropertiesData.assetSubNo });
        go.name = _item.PropertiesData.assetSubNo.ToString();

        // 트랜스폼 업데이트 
        ObjectTransform objTransform = Utility.GetParsingObjectTransform(_item.PropertiesData.transform);
        go.transform.position = objTransform.position;
        go.transform.eulerAngles = objTransform.rotation;
        go.transform.localScale = objTransform.scale;

        // 텍스처 업데이트 
        Material[] mat = go.transform.Find("Asset/Cube_FramePictureSide").GetComponent<Renderer>().materials;
        mat[1].SetTexture("_BaseMap", myTexture);

        // 데이터 할당 
        go.GetComponent<AssetNetworkController>().AssetData = _item;

        // 커스텀필 큐에 등록 
        //CustomFeel.GetInstance.AssetEnqueue(go);
        // 커스텀필 트윈 시작 
        //CustomFeel.GetInstance.ReserveDoTween();
    }
    /// <summary>
    /// pdf 로드 
    /// </summary>
    private void LoadPDF(Item _item, byte[] _pdf)
    {
        string prefabPath = "Prefabs/AssetContainer/PDFAssetContainer";
        GameObject go = PhotonNetwork.InstantiateRoomObject(prefabPath, LoadAssetOffset, quaternion.identity, 0, new object[] { _item.no, _item.PropertiesData.assetSubNo });
        go.name = _item.title;
        ObjectTransform objTransform = Utility.GetParsingObjectTransform(_item.PropertiesData.transform);
        go.transform.position = objTransform.position;
        go.transform.eulerAngles = objTransform.rotation;
        go.transform.localScale = objTransform.scale;

        // 초기화 
        go.GetComponent<PDFLoader>().InitPDF(_pdf);
        go.GetComponent<AssetNetworkController>().AssetData = _item;
        
    }
    /// <summary>
    /// 비디오 로드 
    /// </summary>
    private void LoadVideo(Item _item, VideoClip _vc = null)
    {
        string prefabPath = "Prefabs/AssetContainer/VideoAssetContainer";
        GameObject go = PhotonNetwork.InstantiateRoomObject(prefabPath, LoadAssetOffset, quaternion.identity, 0, new object[] { _item.no, _item.PropertiesData.assetSubNo });
        go.name = _item.title;

        ObjectTransform objTransform = Utility.GetParsingObjectTransform(_item.PropertiesData.transform);
        go.transform.position = objTransform.position;
        go.transform.eulerAngles = objTransform.rotation;
        go.transform.localScale = objTransform.scale;

        go.GetComponent<AssetNetworkController>().AssetData = _item;

        if(_vc==null)
            go.GetComponent<VideoViewController>().SetVideo(_item.imageUri);
        else
            go.GetComponent<VideoViewController>().SetVideo(_vc);

    }

    /// <summary>
    /// 오디오 로드 
    /// </summary>
    private void LoadAudio(Item _item, AudioClip _ac=null)
    {
        string prefabPath = "Prefabs/AssetContainer/AudioAssetContainer";
        GameObject go = PhotonNetwork.InstantiateRoomObject(prefabPath, LoadAssetOffset, quaternion.identity, 0, new object[] { _item.no, _item.PropertiesData.assetSubNo });
        go.name = _item.title;
        ObjectTransform objTransform = Utility.GetParsingObjectTransform(_item.PropertiesData.transform);
        go.transform.position = objTransform.position;
        go.transform.eulerAngles = objTransform.rotation;
        go.transform.localScale = objTransform.scale;

        // audio file download 
        bl_APAudioWeb audioWeb = new bl_APAudioWeb();
        audioWeb.AudioTitle = _item.title;
        audioWeb.URL = _item.imageUri;
        audioWeb.m_AudioType = AudioType.MPEG;

        // 오디오 클립 없으면 다운로드 
        if (_ac == null)
        {
            bl_DownloadAudio downloadAudioWeb = go.GetComponent<bl_DownloadAudio>();
            downloadAudioWeb.AudioURLs.Add(audioWeb);
            downloadAudioWeb.StartDownload();
        }
        else
        {
            AudioSource ac = go.GetComponent<AudioSource>();
            ac.clip = _ac;
        }
        

        go.GetComponent<AssetNetworkController>().AssetData = _item;

        
        

    }
    /// <summary>
    /// glb 로드 
    /// </summary>
    private GameObject LoadModelContainer(Item _item)
    {
        string prefabPath = "Prefabs/AssetContainer/ModelAssetContainer";
        GameObject go = PhotonNetwork.InstantiateRoomObject(prefabPath, LoadAssetOffset, quaternion.identity, 0, new object[] { _item.no, _item.PropertiesData.assetSubNo });
        go.name = _item.title;

        // 트랜스폼 데이터 파싱
        ObjectTransform objTransform = Utility.GetParsingObjectTransform(_item.PropertiesData.transform);
        go.transform.position = objTransform.position;
        go.transform.eulerAngles = objTransform.rotation;
        go.transform.localScale = objTransform.scale;

        go.GetComponent<AssetNetworkController>().AssetData = _item;

        return go;
    }
    /// <summary>
    /// 로드 모델 api 호출 
    /// </summary>
    public void LoadFromURL(string url, GameObject parentObj)
    {
        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        assetLoaderOptions.GenerateColliders = true;
        var webRequest = AssetDownloader.CreateWebRequest(url);
        string urlLoadFileExtension = "glb";
        AssetDownloader.LoadModelFromUri(webRequest, OnLoadModel, null, null, null, parentObj, assetLoaderOptions, null, urlLoadFileExtension);
    }
    /// <summary>
    /// 로드 모델 콜백 
    /// </summary>
    protected virtual void OnLoadModel(AssetLoaderContext assetLoaderContext)
    {
        GameObject go = assetLoaderContext.RootGameObject;
        InitModel(go,false);

       // Item _item = go.transform.root.GetComponent<AssetNetworkController>().AssetData;
       // ObjectTransform objTransform = JsonConvert.DeserializeObject<ObjectTransform>(_item.OBJECT_TRANSFORM);
       // GameObject rootObj = go.transform.root.gameObject;

    }
    /// <summary>
    /// 모델 에셋 초기화 
    /// </summary>
    private void InitModel(GameObject go, bool isAddressable)
    {
        // 포지션 초기화 
        go.transform.localPosition = Vector3.zero;
        // 버텍스 쉐이더 
        Shader vertexShader = RoomManager.GetInstance._assetLoadManager.vertexColorShader;

        // 어드레서블 / 일반 마테리얼 초기화 
        if (isAddressable) ObjectUtils.CustomMaterialWithMeshCollider(go, vertexShader);
        else
        {
            // 센터 피봇 설정 
            Bounds totalBounds = ObjectUtils.GetChildRendererBounds(go);
            Vector3 centerBoundPos = new Vector3(totalBounds.center.x, totalBounds.center.y, totalBounds.center.z);
            API.SetPivot(go.transform, centerBoundPos, API.Space.Global);
            ObjectUtils.CustomMaterial(go, vertexShader);
        }
        // 레이어 변경 
        ObjectUtils.ChangeLayersRecursively(go.transform, RayTargetLayer);
    }
    
}
