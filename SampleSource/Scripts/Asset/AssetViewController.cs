using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// 공통 함수 인터페이스 
/// </summary>
public interface IAssetRPCController
{
    void OnPlayRPC();
    void OnStopRPC();
}

/// <summary>
/// 오디오, 비디오 에셋 컨트롤을 위한 부모 클래스 
/// </summary>
public class AssetViewController : MonoBehaviour, IAssetRPCController
{
    // 재생  
    public virtual void OnPlayRPC()
    {
        Debug.Log("OnPlayRPC");
    }
    // 정지 
    public virtual void OnStopRPC()
    {
        Debug.Log("OnStopRPC");
    }

    // 포톤 동기화 
    [PunRPC]
    public void PlayRPC()
    {
        Debug.Log("PlayRPC ");
        OnPlayRPC();
    }

    [PunRPC]
    public void StopRPC()
    {
        Debug.Log("StopRPC " );
        OnStopRPC();
    }



    
}
