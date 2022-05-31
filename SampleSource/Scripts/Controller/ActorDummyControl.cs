using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class ActorDummyControl : MonoBehaviour
{
    [SerializeField] private MeshRenderer faceRenderer;
    [SerializeField] private Texture2D faceBaseTex;
    public ActorInfo actorInfo;
    Sequence seq;
    // Start is called before the first frame update
    void Start()
    {
        seq = DOTween.Sequence();
    }
    public void SetData(ActorInfo _actorInfo)
    {
        actorInfo = _actorInfo;
    }
    public void MoveActor(ObjectTransform _objTrans)
    {
        seq.Append(transform.DOMove(_objTrans.position, 1).SetSpeedBased());
        seq.Join(transform.DORotate(_objTrans.rotation, 1f).SetEase(Ease.Linear));
    }
    public void OnFaceNoti(Texture2D tex)
    {
        faceRenderer.material.SetTexture("_BaseMap", tex);
        //faceRenderer.enabled = true;
        Invoke("DisableFaceRenderer", 5f);
    }
    private void DisableFaceRenderer()
    {
        faceRenderer.material.SetTexture("_BaseMap", faceBaseTex);
        //faceRenderer.enabled = false;
    }
}
