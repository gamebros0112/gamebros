using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetControl : MonoBehaviour
{
    [SerializeField] private Renderer balloonRenderer;
    [SerializeField] private Collider balloonCollider;
    [SerializeField] private Transform balloonTransform;
    [SerializeField] private Transform assetTransform;

    public Transform AssetTransform { get => assetTransform; }

    /// <summary>
    /// 판매 유무에 따라 판매풍선, 콜라이더 활성/비활성 
    /// </summary>
    /// <param name="_sellable"></param>
    public void SetActiveSalesBalloon(string _sellable)
    {
        if (_sellable == "Y")
        {
            balloonRenderer.enabled = true;
            balloonCollider.enabled = true;
        }
        else 
        {
            balloonRenderer.enabled = false;
            balloonCollider.enabled = false;
        }
        
    }
    /// <summary>
    /// 에셋 크기에 맞춰 판매풍선 위치 업데이트 
    /// </summary>
    public void SetPositionSalesBalloon()
    {
        Transform assetTrans = AssetTransform;
        Bounds assetBound = ObjectUtils.GetChildRendererBounds(assetTrans.gameObject);
        float balloonPosY = assetBound.size.y / 2f;
        balloonTransform.localPosition = new Vector3(AssetTransform.localPosition.x , AssetTransform.localPosition.y+balloonPosY, AssetTransform.localPosition.z);
    }


}
