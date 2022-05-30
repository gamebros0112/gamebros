using UnityEngine;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Video;
using Paroxe.PdfRenderer;

public class BaseAssetLoadManager : Singleton<BaseAssetLoadManager>
{
    /// *************************************************************  
    /// Set Progressbar
    /// *************************************************************

    [HideInInspector]
    public ProgressBarManager _progressBarManager;

    private void ExcuteProgressBar(int _totalCount, int _idx, float progress)
    {
        _progressBarManager.Ratio = (_idx + progress) / _totalCount;
    }
    public void InitProgress()
    {
        _progressBarManager = ProgressBarManager.GetInstance;
    }

    /// *************************************************************  
    /// Addressable Asset Download
    /// *************************************************************

    public IEnumerator LoadAddressableImage(Item _item, Action<Texture2D> callback)
    {
#if UNITY_EDITOR
        AsyncOperationHandle<Texture2D> assetHandle = Addressables.LoadAssetAsync<Texture2D>("DummyImage");
        yield return assetHandle;

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Texture2D myTexture = assetHandle.Result;
            callback(myTexture);
        }
#elif UNITY_WEBGL
        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(_item.imageUri);
        yield return locationHandle;
        AsyncOperationHandle<Texture2D> assetHandle = Addressables.LoadAssetAsync<Texture2D>(locationHandle.Result[0]);
        yield return assetHandle;

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Texture2D myTexture = assetHandle.Result;
            callback(myTexture);

            Addressables.Release(locationHandle);
        }
#endif
    }

    public IEnumerator LoadAddressableImageWithProgress(Item _item, int _totalCount, int _idx, Action<Texture2D> callback)
    {
#if UNITY_EDITOR
        AsyncOperationHandle<Texture2D> assetHandle = Addressables.LoadAssetAsync<Texture2D>("DummyImage");
        //yield return assetHandle;
        while (!assetHandle.IsDone)
        {
            ExcuteProgressBar(_totalCount, _idx, assetHandle.PercentComplete);
            yield return null;
        }

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Texture2D myTexture = assetHandle.Result;
            callback(myTexture);
        }
#elif UNITY_WEBGL
        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(_item.imageUri);
        yield return locationHandle;

        AsyncOperationHandle<Texture2D> assetHandle = Addressables.LoadAssetAsync<Texture2D>(locationHandle.Result[0]);
        //yield return assetHandle;
        while (!assetHandle.IsDone)
        {
            ExcuteProgressBar(_totalCount, _idx, assetHandle.PercentComplete);
            yield return null;
        }

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Texture2D myTexture = assetHandle.Result;
            callback(myTexture);
            Addressables.Release(locationHandle);
        }
#endif
    }

    public IEnumerator LoadAddressablePDF(Item _item, Action<byte[]> callback)
    {
#if UNITY_EDITOR
        AsyncOperationHandle<PDFAsset> assetHandle = Addressables.LoadAssetAsync<PDFAsset>("DummyPdf");
        yield return assetHandle;

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            callback(assetHandle.Result.m_FileContent);
        }
#elif UNITY_WEBGL
        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(_item.imageUri);
        yield return locationHandle;
        AsyncOperationHandle<PDFAsset> assetHandle = Addressables.LoadAssetAsync<PDFAsset>(locationHandle.Result[0]);
        yield return assetHandle;

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            callback(assetHandle.Result.m_FileContent);
            Addressables.Release(locationHandle);
        }
#endif
    }

    public IEnumerator LoadAddressablePDFWithProgress(Item _item, int _totalCount, int _idx, Action<byte[]> callback)
    {
#if UNITY_EDITOR
        AsyncOperationHandle<PDFAsset> assetHandle = Addressables.LoadAssetAsync<PDFAsset>("DummyPdf");
        //yield return assetHandle;
        while (!assetHandle.IsDone)
        {
            ExcuteProgressBar(_totalCount, _idx, assetHandle.PercentComplete);
            yield return null;
        }

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            callback(assetHandle.Result.m_FileContent);
        }
#elif UNITY_WEBGL
        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(_item.imageUri);
        yield return locationHandle;

        AsyncOperationHandle<PDFAsset> assetHandle = Addressables.LoadAssetAsync<PDFAsset>(locationHandle.Result[0]);
        //yield return assetHandle;
        while (!assetHandle.IsDone)
        {
            ExcuteProgressBar(_totalCount, _idx, assetHandle.PercentComplete);
            yield return null;
        }

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            callback(assetHandle.Result.m_FileContent);
            Addressables.Release(locationHandle);
        }
#endif
    }

    public IEnumerator LoadAddressableModel(Item _item, int _totalCount, int _idx, Action<GameObject> callback)
    {

        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(_item.imageUri);
        yield return locationHandle;
        AsyncOperationHandle<GameObject> assetHandle = Addressables.InstantiateAsync(locationHandle.Result[0]);

        while (!assetHandle.IsDone)
        {
            ExcuteProgressBar(_totalCount, _idx, assetHandle.PercentComplete);
            yield return null;
        }

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            callback(assetHandle.Result);

            Addressables.Release(locationHandle);
        }
    }

    public IEnumerator InstantiateAddressableModel(string _url, Action<GameObject> callback)
    {

        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(_url);
        yield return locationHandle;
        Addressables.InstantiateAsync(locationHandle.Result[0]).Completed += (loadAsset) =>
        {
            GameObject go = loadAsset.Result;
            callback(go);

            Addressables.Release(locationHandle);
        };
    }

    public IEnumerator LoadAddressableAudio(Item _item, Action<AudioClip> callback)
    {
#if UNITY_EDITOR
        AsyncOperationHandle<AudioClip> assetHandle = Addressables.LoadAssetAsync<AudioClip>("DummyAudio");
        yield return assetHandle;

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            callback(assetHandle.Result);
        }
#elif UNITY_WEBGL
        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(_item.imageUri);
        yield return locationHandle;
        AsyncOperationHandle<AudioClip> assetHandle = Addressables.LoadAssetAsync<AudioClip>(locationHandle.Result[0]);
        yield return assetHandle;

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            callback(assetHandle.Result);

            Addressables.Release(locationHandle);
        }
#endif
    }

    public IEnumerator LoadAddressableAudioWithProgress(Item _item, int _totalCount, int _idx, Action<AudioClip> callback)
    {
#if UNITY_EDITOR
        AsyncOperationHandle<AudioClip> assetHandle = Addressables.LoadAssetAsync<AudioClip>("DummyAudio");
        while (!assetHandle.IsDone)
        {
            ExcuteProgressBar(_totalCount, _idx, assetHandle.PercentComplete);
            yield return null;
        }

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            callback(assetHandle.Result);
        }
#elif UNITY_WEBGL
        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(_item.imageUri);
        yield return locationHandle;
        AsyncOperationHandle<AudioClip> assetHandle = Addressables.LoadAssetAsync<AudioClip>(locationHandle.Result[0]);
        while (!assetHandle.IsDone)
        {
            ExcuteProgressBar(_totalCount, _idx, assetHandle.PercentComplete);
            yield return null;
        }

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            callback(assetHandle.Result);

            Addressables.Release(locationHandle);
        }
#endif
    }

    public IEnumerator LoadAddressableVideo(Item _item, Action<VideoClip> callback)
    {
#if UNITY_EDITOR
        AsyncOperationHandle<VideoClip> assetHandle = Addressables.LoadAssetAsync<VideoClip>("DummyVideo");
        yield return assetHandle;

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            callback(assetHandle.Result);
        }
#elif UNITY_WEBGL
        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(_item.imageUri);
        yield return locationHandle;
        AsyncOperationHandle<VideoClip> assetHandle = Addressables.LoadAssetAsync<VideoClip>(locationHandle.Result[0]);
        yield return assetHandle;

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            callback(assetHandle.Result);

            Addressables.Release(locationHandle);
        }
#endif
    }

    public IEnumerator LoadAddressableVideoWithProgress(Item _item, int _totalCount, int _idx, Action<VideoClip> callback)
    {
#if UNITY_EDITOR
        AsyncOperationHandle<VideoClip> assetHandle = Addressables.LoadAssetAsync<VideoClip>("DummyVideo");
        while (!assetHandle.IsDone)
        {
            ExcuteProgressBar(_totalCount, _idx, assetHandle.PercentComplete);
            yield return null;
        }

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            callback(assetHandle.Result);
        }
#elif UNITY_WEBGL
        AsyncOperationHandle<IList<IResourceLocation>> locationHandle = Addressables.LoadResourceLocationsAsync(_item.imageUri);
        yield return locationHandle;
        AsyncOperationHandle<VideoClip> assetHandle = Addressables.LoadAssetAsync<VideoClip>(locationHandle.Result[0]);
        while (!assetHandle.IsDone)
        {
            ExcuteProgressBar(_totalCount, _idx, assetHandle.PercentComplete);
            yield return null;
        }

        if (assetHandle.Status == AsyncOperationStatus.Succeeded)
        {
            callback(assetHandle.Result);

            Addressables.Release(locationHandle);
        }
#endif
    }


    /// *************************************************************  
    /// S3 Asset Download
    /// *************************************************************

    /// <summary>
    /// 이미지 파일 가져오기 
    /// </summary>
    /// <param name="_item"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public IEnumerator DownloadTexture(Item _item, Action<Texture2D> callback)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(_item.imageUri);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            myTexture.Compress(true);
            callback(myTexture);
        }
    }
    /// <summary>
    /// progressbar 사용을 위해 파라미터 추가 
    /// </summary>
    /// <param name="_item"></param>
    /// <param name="_totalCount"></param>
    /// <param name="_idx"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public IEnumerator DownloadTextureWithProgress(Item _item, int _totalCount, int _idx, Action<Texture2D> callback)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(_item.imageUri);
        var async = www.SendWebRequest();
        while (true)
        {
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
                yield break;
            }

            ExcuteProgressBar(_totalCount, _idx, async.progress);

            if (async.isDone)
            {
                Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                myTexture.Compress(true);
                callback(myTexture);
                yield break;
            }

            yield return null;
        }
       
    }
    public IEnumerator DownloadPDF(Item _item, int _totalCount=default, int _idx= default, bool isProgress=default, Action<byte[]> callback=null)
    {
        UnityWebRequest www = UnityWebRequest.Get(_item.imageUri);
        var async = www.SendWebRequest();
        while (true)
        {
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
                yield break;
            }

            if(isProgress) ExcuteProgressBar(_totalCount, _idx, async.progress);

            if (async.isDone)
            {
                callback(www.downloadHandler.data);
                yield break;
            }

            yield return null;
        }

    }
    public IEnumerator DownloadModel(Item _item, int _totalCount, int _idx, bool isProgress, Action<byte[]> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get(_item.imageUri);
        var async = www.SendWebRequest();
        while (true)
        {
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
                yield break;
            }

            if(isProgress) ExcuteProgressBar(_totalCount, _idx, async.progress);

            if (async.isDone)
            {

                // progress 처리를 위해 LoadFromURL 대신 LoadFromStream 사용
                // 코루틴 상에서 진행 상황 체크 가능 (LoadFromStream 사용시 모듈 내부에서 처리하므로 progress 처리 불가)
                // LoadFromStream(www.downloadHandler.data, baseAssetData.item.no ,baseAssetData.item.PropertiesData.assetSubNo);

                callback(www.downloadHandler.data);
                yield break;
            }

            yield return null;
        }


    }

    
}
