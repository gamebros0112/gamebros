using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
public class AudioViewController : AssetViewController, IPunObservable
{
    private PhotonView pview;
    private bl_AudioPlayer audioPlayer;
    private float playTime;
    private bool isPlaying;

    [SerializeField] Button playBtn;
    [SerializeField] Slider progressBtn;

    private void Awake()
    {
        audioPlayer = GetComponent<bl_AudioPlayer>();
    }
    private void Start()
    {
        pview = GetComponent<PhotonView>();

        // 내방인 경우 버튼 활성화 
        playBtn.interactable = RoomData.instance.IsMyRoom;
        progressBtn.interactable = RoomData.instance.IsMyRoom;

    }
    /// <summary>
    /// 오디오 재생  
    /// </summary>
    public void OnPlayPauseAudio()
    {
        // 내 에셋이 아니면 리턴 
        if (!pview.IsMine) return;

        // 포톤 연결 중이면 재생 동기화 
        if (PhotonNetwork.IsConnected) pview.RPC("PlayRPC", RpcTarget.All);
        else
        {
            // 오디오 재생 또는 정지 
            audioPlayer.PlayPause();
        }
    }
    /// <summary>
    /// 반복 재생 
    /// </summary>    
    public void OnRepeatAudio()
    {
        if (!pview.IsMine) return;
        audioPlayer.ChangeRepeat();
    }
    /// <summary>
    /// 볼륨 끄기 
    /// </summary>
    public void OnVolumeMute()
    {
       // if (!pview.IsMine) return;
        audioPlayer.Mute();
    }
    /// <summary>
    /// 볼륨 조절 
    /// </summary>
    public void OnVolumeSlider(float vol)
    {
        //if (!pview.IsMine) return;
        audioPlayer.onChangeVolumen(vol);
    }
    /// <summary>
    /// 구간 이동 
    /// </summary>
    public void OnProgressSlider(bool b)
    {
        if (!pview.IsMine) return;
        audioPlayer.OnSelectProgress(b);
    }
    /// <summary>
    /// 포톤 동기화 재생 시간, 플레이 상태 계속 체크 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(audioPlayer.m_Source.time);
            stream.SendNext(audioPlayer.m_Source.isPlaying);
        }
        else
        {
            playTime = (float)stream.ReceiveNext();
            isPlaying = (bool)stream.ReceiveNext();
            SerializePlay();
        }
    }

    private void SerializePlay()
    {
        if (isPlaying && !audioPlayer.m_Source.isPlaying)
        {
            audioPlayer.m_Source.time = playTime;
            audioPlayer.Play();
        }
    }

    /// <summary>
    /// 재생 rpc 전달 받으면 실행 
    /// </summary>
    public override void OnPlayRPC()
    {
        Debug.Log("OnPlayRPC");
        audioPlayer.PlayPause();
    }
    /// <summary>
    /// 정지 rpc 전달 받으면 실행 
    /// </summary>
    public override void OnStopRPC()
    {
        Debug.Log("OnStopRPC");
    }
}
