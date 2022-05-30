using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Newtonsoft.Json;
using UnityEngine;
using System;
using Photon.Pun;

public class CustomFeel : Singleton<CustomFeel>
{
    [SerializeField] private MMFeedbacks loadAssetFeel;
    [SerializeField] private MMFeedbacks makeAssetFeel;

    Queue<GameObject> enterRoomAssets = new Queue<GameObject>();
    public Action OnCompleteTween;
    Sequence seq;
    
    /// <summary>
    /// Feel 셋팅 (현재 사용안함)
    /// </summary>
    /// <param name="pos">좌표</param>
    /// <param name="targetObj">에셋</param>
    public void MakeFeelFeedback(Vector3 pos, GameObject targetObj)
    {
        List<MMFeedback> feedbacks = makeAssetFeel.Feedbacks;
        MMFeedbackPosition positionAsset = feedbacks[1] as MMFeedbackPosition;

        positionAsset.InitialPosition = new Vector3(0, 10, 0);
        positionAsset.DestinationPosition = pos;
        positionAsset.AnimatePositionTarget = targetObj;

        makeAssetFeel?.PlayFeedbacks();
    }
    /// <summary>
    /// 큐에 넣기 
    /// </summary>
    /// <param name="go">에셋</param>
    public void AssetEnqueue(GameObject go)
    {
        enterRoomAssets.Enqueue(go);
        //ReserveDoTween();
    }
    /// <summary>
    /// 큐에서 꺼내기 
    /// </summary>
    public void ReserveDoTween()
    {
        // 큐에 에셋 있으면 
        if (enterRoomAssets.Count > 0 )
        {
            // 시퀀스 실행 안됐으면 트윈 시작 
            if(seq==null || !seq.IsPlaying())
            StartDoTween();
        }
        else
        {
            // 에셋 트윈 끝났으니 캐릭터 생성 
            //OnCompleteTween?.Invoke();
            RoomManager roomMgr = RoomManager.GetInstance;
            if(roomMgr.player==null)roomMgr.pRoomCtl.SpawnPlayer();
        }
    }
    /// <summary>
    /// 트윈 시작 
    /// </summary>
    private void StartDoTween()
    {
        GameObject rootObj = enterRoomAssets.Dequeue();
        AssetNetworkController anc = rootObj.GetComponent<AssetNetworkController>();
        AssetControl assetControl = rootObj.GetComponent<AssetControl>();
        Transform assetTrans = assetControl.AssetTransform;

        // 트랜스폼 데이터 파싱  
        ObjectTransform objTransform = Utility.GetParsingObjectTransform(anc.AssetData.PropertiesData.transform);
        assetTrans.eulerAngles = objTransform.rotation;
        assetTrans.localScale = objTransform.scale;

        // 에셋 활성 
        rootObj.SetActive(true);
        // 시퀀스 할당 
        seq = DOTween.Sequence();
        // 시퀀스 추가 
        seq.Append(rootObj.transform.DOMove(objTransform.position, 1f).SetEase(Ease.InQuad));
        // 시퀀스 합류 
        seq.Join(assetTrans.DORotate(new Vector3(0,360,0), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).OnComplete(() =>
        {
            // 판매 풍선 위치 업데이트 
            assetControl.SetPositionSalesBalloon();
            // 판매 풍선 액티브 업데이트 
            assetControl.SetActiveSalesBalloon(anc.AssetData.PropertiesData.sellable);
            // 포톤뷰 동기화 시작 
            anc.OnInitSerialize();
            // 다음 에셋 트윈 시작   
            ReserveDoTween(); 
        }));
        
    }

    
}
