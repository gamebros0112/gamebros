using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AssetBuyManager : MonoBehaviour
{
    private BuyInfoManager buyInfoMgr;
    private Transform buyingAsset;

    public LayerMask AssetBalloonLayer;

    private void Awake()
    {
        buyInfoMgr = UIButtonEventGate.GetInstance.GetComponent<BuyInfoManager>();
    }
    void Start()
    {
        
    }
    private void OnEnable()
    {
        InputControl.GetInstance.OnMouseDownAction += OnMouseDown;
        // 구매창 구매 버튼 클릭시 
        buyInfoMgr.buyInfoContainer.OnBuyAsset += OnBuyAssetReceive;
    }
    private void OnDisable()
    {
        InputControl.GetInstance.OnMouseDownAction -= OnMouseDown;
        buyInfoMgr.buyInfoContainer.OnBuyAsset -= OnBuyAssetReceive;
    }
    /// <summary>
    /// 마우스 다운 이벤트
    /// 에디트 모드가 아니고 판매풍선 클릭했을때 구매창 호출 
    /// </summary>
    private void OnMouseDown()
    {
        // if edit mode
        if (RoomManager.GetInstance._isEditMode) return;
        else
        {
            // 비로그인 유저가 판매풍선 클릭시 랜딩페이지로 
            if (string.IsNullOrEmpty(PlayerData.instance.PlayerUniqueNo))
            {
                JSGate.GetInstance.SendLeavRoom();
            }
            else
            {
                // 판매 풍선 클릭했는지 체크 
                OnClickSalesBalloon();
            }
            
        }
        
    }
    private void OnClickSalesBalloon()
    {
        GameObject pickedObject = PickGameObject();
        // 판매 풍선 클릭이 아니면 리턴 
        if (pickedObject == null || !pickedObject.CompareTag("SalesBalloon")) return;

        // 판매 데이터 읽어오기 
        buyingAsset = pickedObject.transform.root;
        Item assetItem = buyingAsset.GetComponent<AssetNetworkController>().AssetData;
        // 판매창에 데이터 셋팅  
        buyInfoMgr.SetBuyInfoData(assetItem);
        // 판매창 활성 
        buyInfoMgr.Active(true);
    }
    /// <summary>
    /// 마우스 클릭시 레이를 사용해 오브젝트 판별 
    /// </summary>
    private GameObject PickGameObject()
    {
        // UI 클릭한 경우 리턴 
        if (EventSystem.current.IsPointerOverGameObject()) return null;

        // 레이 실행 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit;
        // 레이어 체크 
        if (Physics.Raycast(ray, out rayHit, float.MaxValue, AssetBalloonLayer))
        {
            GameObject go = rayHit.collider.gameObject;
            // 렌더러 비활성이면 null 리턴 
            if (!go.GetComponent<Renderer>().enabled) return null;
            return go;
        }
            
        // 조건에 맞지 않으면 최종 null 리턴 
        return null;
    }
    /// <summary>
    /// 서버에 에셋 구매 요청 
    /// </summary>
    private void OnBuyAssetReceive()
    {
        int pcs = buyInfoMgr.buyInfoContainer.PCS;
        Item assetItem = buyingAsset.GetComponent<AssetNetworkController>().AssetData;
        string jsonSellInfo = "{\"assetNo\":" + assetItem.no + ",\"assetSubNo\":" + assetItem.PropertiesData.assetSubNo + ",\"quantity\":" + pcs + "}";
        RoomManager.GetInstance._baseServerManager.UpdateAsset("/room/asset/buy", jsonSellInfo);

        buyingAsset = null;
        buyInfoMgr.Active(false);
        buyInfoMgr.Reset(); //거래 데이타 처리가 끝나고 나면 패널창을 리셋시킴. todo Active, Reset 을 묶어서 하나의 함수로 작성할 필요 있을까?
        
    }
}
