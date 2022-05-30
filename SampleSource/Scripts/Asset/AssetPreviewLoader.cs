using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TriLibCore;
using SuperPivot;
using Photon.Pun;
using TriLibCore.Utils;

public class AssetPreviewLoader : Singleton<AssetPreviewLoader>
{
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] private GameObject modelPrefab;
    [SerializeField] private GameObject docPrefab;
    [SerializeField] private GameObject audioPrefab;
    [SerializeField] private GameObject videoPrefab;
    [SerializeField] private Transform assetParent;
    [SerializeField] private LayerMask assetLayer;

    private SingleAssetLoadManager _singleAssetLoadManager;
    public Action<GameObject> OnCompletePreviewAsset;

    private void Start()
    {
        _singleAssetLoadManager = GetComponent<SingleAssetLoadManager>();
    }

    public void AddsetLoad()
    {
        Item item = new Item();
        item.no = 0;
        item.title = "creator_no1";
        item.thumnailUri =
            "https://gesta-s3-lambda-upload.s3.ap-northeast-2.amazonaws.com/icons/inventory/8704002_cube_dynamic_color_icon.png";
        item.imageUri = "https://gesta-s3-lambda-upload.s3.ap-northeast-2.amazonaws.com/b87b7dd0-62b9-4d64-bcc9-2bbc82d3068d.glb";
        
        _singleAssetLoadManager.LoadFromPreview(item);
        Debug.Log("addsetLoad");
    }
    /// <summary>
    /// 타입에 따라 로드 
    /// </summary>
    /// <param name="_type"></param>
    /// <param name="_url"></param>
    public void LoadPreviewAsset(ConstEventString.Object_DataType _type, string _url)
    {
        Debug.Log($"LoadPreviewAsset({_type} ,{_url}");
        switch (_type)
        {
            case ConstEventString.Object_DataType.I:
                StartCoroutine(LoadPicture(_url));
                break;
            case ConstEventString.Object_DataType.A:
                LoadAudio(_url);
                break;
            case ConstEventString.Object_DataType.V:
                LoadVideo(_url);
                break;
            case ConstEventString.Object_DataType.D:
                StartCoroutine(LoadPDF(_url));
                break;
            case ConstEventString.Object_DataType.T:
                LoadModel(_url);
                break;

        }

    }
    /// <summary>
    /// 이미지 로드 
    /// </summary>
    IEnumerator LoadPicture(string _url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(_url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            myTexture.Compress(true);
            GameObject go = Instantiate(imagePrefab, assetParent, false);
            RemoveComponent(go);
            go.transform.eulerAngles = new Vector3(0, 180, 0);
            go.name = "PreviewImage";
            // go.transform.parent = assetParent;

            // 텍스처 적용 
            Material[] mat = go.transform.Find("Asset/Cube_FramePictureSide").GetComponent<Renderer>().materials;
            mat[1].SetTexture("_BaseMap", myTexture);

            ObjectUtils.ChangeLayersRecursively(go.transform, ObjectUtils.layermask_to_layer(assetLayer));
            OnCompletePreviewAsset?.Invoke(go);
        }
    }
    /// <summary>
    /// pdf 로드 
    /// </summary>
    IEnumerator LoadPDF(string _url)
    {
        UnityWebRequest www = UnityWebRequest.Get(_url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {

            byte[] bytePDF = www.downloadHandler.data;

            GameObject go = Instantiate(docPrefab, assetParent, false);
            RemoveComponent(go);
            go.name = "PreviewDoc";

            // pdf 초기화 
            go.GetComponent<PDFLoader>().InitPDF(bytePDF);

            ObjectUtils.ChangeLayersRecursively(go.transform, ObjectUtils.layermask_to_layer(assetLayer));
            OnCompletePreviewAsset?.Invoke(go);
        }
    }
    /// <summary>
    /// video 로드 
    /// </summary>
    private void LoadVideo(string _url)
    {
        GameObject go = Instantiate(videoPrefab, assetParent, false);
        RemoveComponent(go);
        go.name = "PreviewVideo";

        // 비디오 셋팅 ( 주소에서 스트리밍 형태로 읽어옴 )
        go.GetComponent<VideoViewController>().SetVideo(_url);

        ObjectUtils.ChangeLayersRecursively(go.transform, ObjectUtils.layermask_to_layer(assetLayer));
        OnCompletePreviewAsset?.Invoke(go);

    }

    /// <summary>
    /// audio 로드 
    /// </summary>
    private void LoadAudio(string _url)
    {
        GameObject go = Instantiate(audioPrefab, assetParent, false);
        RemoveComponent(go);
        go.name = "PreviewAudio";

        // s3 audio file download 
        bl_APAudioWeb audioWeb = new bl_APAudioWeb();
        audioWeb.AudioTitle = "";
        audioWeb.URL = _url;
        audioWeb.m_AudioType = AudioType.MPEG;

        bl_DownloadAudio downloadAudioWeb = go.GetComponent<bl_DownloadAudio>();
        downloadAudioWeb.AudioURLs.Add(audioWeb);
        downloadAudioWeb.StartDownload();

        ObjectUtils.ChangeLayersRecursively(go.transform, ObjectUtils.layermask_to_layer(assetLayer));
        OnCompletePreviewAsset?.Invoke(go);

    }
    /// <summary>
    /// glb 로드  
    /// </summary>
    private void LoadModel(string _url)
    {
        GameObject go = Instantiate(modelPrefab, assetParent, false);
        RemoveComponent(go);
        //go.transform.eulerAngles = new Vector3(0, 180, 0);
        go.name = "PreviewModel";

        // url 로드 
        LoadFromURL(_url, go);
    }
    private void LoadFromURL(string url, GameObject parentObj)
    {
        // 모델 로드 옵션
        var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        assetLoaderOptions.GenerateColliders = true;
        var webRequest = AssetDownloader.CreateWebRequest(url);
        string urlLoadFileExtension = "glb";
        AssetDownloader.LoadModelFromUri(webRequest, OnLoadModel, null, null, null, parentObj, assetLoaderOptions, null, urlLoadFileExtension);
    }
    /// <summary>
    /// onload 콜백 
    /// </summary>
    private void OnLoadModel(AssetLoaderContext assetLoaderContext)
    {
        GameObject go = assetLoaderContext.RootGameObject;

        // 버텍스 컬러 사용하는 경우 쉐이더 교체 
        ObjectUtils.CustomMaterial(go, RoomManager.GetInstance._assetLoadManager.vertexColorShader);

        // pivot 센터로 맞추기 
        Bounds totalBounds = ObjectUtils.GetChildRendererBounds(go);
        Vector3 centerBoundPos = new Vector3(totalBounds.center.x, totalBounds.center.y, totalBounds.center.z);
        API.SetPivot(go.transform, centerBoundPos, API.Space.Global);

        go.transform.localPosition = Vector3.zero;

        // 레이어 지정 
        ObjectUtils.ChangeLayersRecursively(assetLoaderContext.WrapperGameObject.transform, ObjectUtils.layermask_to_layer(assetLayer));
        OnCompletePreviewAsset?.Invoke(assetLoaderContext.WrapperGameObject);

    }
    /// <summary>
    /// component 제거 (룸에셋 프리팹 사용하기때문에 프리뷰에서 필요없는 컴포넌트들 삭제)
    /// </summary>
    private void RemoveComponent(GameObject go)
    {
        Destroy(go.GetComponent<PhotonView>());
        Destroy(go.GetComponent<AssetNetworkController>());
        Destroy(go.GetComponent<AssetControl>());
    }

}
