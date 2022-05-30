using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoViewController : AssetViewController, IPunObservable
{
    private PhotonView pview;
    private VideoPlayer vplayer;
    private double playTime;
    private bool isPlaying;

    [SerializeField] GameObject viewQuad;
    [SerializeField] Button playBtn;
    [SerializeField] Button stopBtn;

    private void Start()
    {
        pview = GetComponent<PhotonView>();

        playBtn.interactable = RoomData.instance.IsMyRoom;
        stopBtn.interactable = RoomData.instance.IsMyRoom;

        
    }
    public void SetVideo(string _url)
    {
        vplayer = GetComponent<VideoPlayer>();
        //CreateRenderTexture(); //video prepare 이후에 동작하도록 코루틴 처리
        vplayer.url = _url;
        
        //코루틴 이후에 CreateRenderTexture
        StartCoroutine("prepareVideo");
        vplayer.Stop();
        
    }
    public void SetVideo(VideoClip _vc)
    {
        vplayer = GetComponent<VideoPlayer>();
        //CreateRenderTexture(); //video prepare 이후에 동작하도록 코루틴 처리
        vplayer.clip = _vc;

        //코루틴 이후에 CreateRenderTexture
        StartCoroutine("prepareVideo");
        vplayer.Stop();

    }
    //
    IEnumerator prepareVideo()
    {
        vplayer.Prepare();
        yield return new WaitUntil(() => vplayer.isPrepared);
        CreateRenderTexture((int)vplayer.width, (int)vplayer.height);
    }
    /// <summary>
    /// video 가 prepare 되기까지 기다렸다가 w, h 값
    /// </summary>
    /// <param name="w">width</param>
    /// <param name="h">height</param>
    private void CreateRenderTexture(int w, int h)
    {
        //이부분 가변처리하고 화질 개선 효과 있을듯?
        RenderTexture rt = new RenderTexture(w,h,24);
        
        rt.Create();
        rt.name = "VideoRenderTexture";
        vplayer.targetTexture = rt;

        Shader shader = viewQuad.GetComponent<Renderer>().material.shader;
        Material myNewMaterial = new Material(shader);
        myNewMaterial.SetTexture("_BaseMap", rt);
        
        Vector3 c = viewQuad.transform.localScale;
        // 영상 비율 적용
        viewQuad.transform.localScale = new Vector3(c.x, c.y*h/w,c.z);
        viewQuad.GetComponent<Renderer>().material = myNewMaterial;

    }
    /// <summary>
    /// 비디오 재생  
    /// </summary>
    public void OnPlayVideo()
    {
        // 내 에셋이 아니면 리턴 
        if (!pview.IsMine) return;

        // 재생 동기화 
        if (PhotonNetwork.IsConnected) pview.RPC("PlayRPC", RpcTarget.All);
        else {
            // 재생 상태가 아니면 
            if(!vplayer.isPlaying) vplayer.Play();
        }
    }
    /// <summary>
    /// 비디오 정지 
    /// </summary>
    public void OnStopVideo()
    {
        // 내 에셋이 아니면 리턴 
        if (!pview.IsMine) return;

        // 정지 동기화 
        if (PhotonNetwork.IsConnected) pview.RPC("StopRPC", RpcTarget.All);
        else
        {
            // 재생 상태라면  
            if (vplayer.isPlaying) vplayer.Stop();
        }
    }
    /// <summary>
    /// 비디오 상태 동기화 재생 시간, 플레이 상태 계속 체크 
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(vplayer.time);
            stream.SendNext(vplayer.isPlaying);
        }
        else
        {
            if (stream.PeekNext() is Vector3 || stream.PeekNext() is Quaternion) return;
            playTime = (double)stream.ReceiveNext();
            isPlaying = (bool)stream.ReceiveNext();
            SerializePlay();
        }
    }

    private void SerializePlay()
    {
        if (isPlaying && !vplayer.isPlaying)
        {
            vplayer.time = playTime;
            vplayer.Play();
        }
    }
    /// <summary>
    /// 재생 rpc 전달 받으면 실행 
    /// </summary>
    public override void OnPlayRPC()
    {
        if (!vplayer.isPlaying) vplayer.Play();
    }
    /// <summary>
    /// 정지 rpc 전달 받으면 실행 
    /// </summary>
    public override void OnStopRPC()
    {
        if (vplayer.isPlaying) vplayer.Stop();
    }
    
}
