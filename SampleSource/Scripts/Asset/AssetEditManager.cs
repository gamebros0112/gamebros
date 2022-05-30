using UnityEngine.EventSystems;
using UnityEngine;
using RTG;
using Photon.Pun;
using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using SuperPivot;

public class AssetEditManager : MonoBehaviour
{
    // 에디트 모드 기즈모 상태 
    private enum TransformGizmoId
    {
        Move = 1,
        Rotate,
        Scale,
        Universal
    }

    /// <summary>
    /// The following 4 variables are references to the ObjectTransformGizmo behaviours
    /// that will be used to move, rotate and scale our objects.
    /// 기본 3가지 및 모두 표시 기즈모 
    /// </summary>
    private ObjectTransformGizmo _objectMoveGizmo;
    private ObjectTransformGizmo _objectRotationGizmo;
    private ObjectTransformGizmo _objectScaleGizmo;
    private ObjectTransformGizmo _objectUniversalGizmo;

    /// <summary>
    /// The current work gizmo id. The work gizmo is the gizmo which is currently used
    /// to transform objects. The W,E,R,T keys can be used to change the work gizmo as
    /// needed.
    /// 현재 기즈모 상태  
    /// </summary>
    private TransformGizmoId _workGizmoId;
    /// <summary>
    /// A reference to the current work gizmo. If the work gizmo id is GizmoId.Move, then
    /// this will point to '_objectMoveGizmo'. For GizmoId.Rotate, it will point to 
    /// '_objectRotationGizmo' and so on.
    /// 현재 표시 기즈모 
    /// </summary>
    private ObjectTransformGizmo _workGizmo;
    /// <summary>
    /// A reference to the target object. This is the object that will be manipulated by
    /// the gizmos and it will always be picked from the scene via a mouse click. This will
    /// be set to null when the user clicks in thin air.
    /// </summary>
    private GameObject _targetObject;
    private GameObject _targetBalloon;
    private Transform assetTrans;
    private PhotonView _pview;
    
    private Action<int> MoveAction;
    private ConstEventString.EditAxisType currentAxisType = ConstEventString.EditAxisType.X;
    /// <summary>
    /// 에디트 종료시 수정했던 에셋 전부 파이어베이스에 저장
    /// 키값과 트랜스폼 저장소
    /// </summary>
    private List<GameObject> keepEditedAsset = new List<GameObject>();

    /// select indicator와 에셋 사이 여백 크기 
    private float indicatorPadding = 1.2f;

    public LayerMask RayTargetLayer;

    private EditManager _editManager;
    private EditSales _editSalse;



    //[SerializeField] private GameObject _indicator;

    private void Awake()
    {
        _editManager = UIButtonEventGate.GetInstance.GetComponent<EditManager>();
    }
    /// <summary>
    /// Performs all necessary initializations.
    /// </summary>
    private void Start()
    {
        // Create the 4 gizmos
        _objectMoveGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
        _objectRotationGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
        _objectScaleGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
        _objectUniversalGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();

        // Call the 'SetEnabled' function on the parent gizmo to make sure
        // the gizmos are initially hidden in the scene. We want the gizmo
        // to show only when we have a target object available.
        _objectMoveGizmo.Gizmo.SetEnabled(false);
        _objectRotationGizmo.Gizmo.SetEnabled(false);
        _objectScaleGizmo.Gizmo.SetEnabled(false);
        _objectUniversalGizmo.Gizmo.SetEnabled(false);

        // We initialize the work gizmo to the move gizmo by default. This means
        // that the first time an object is clicked, the move gizmo will appear.
        // You can change the default gizmo, by simply changing these 2 lines of
        // code. For example, if you wanted the scale gizmo to be the default work
        // gizmo, replace '_objectMoveGizmo' with '_objectScaleGizmo' and GizmoId.Move
        // with GizmoId.Scale.

        // 디폴트 기즈모는 Move 
        _workGizmo = _objectMoveGizmo;
        _workGizmoId = TransformGizmoId.Move;
        
        _workGizmo.Gizmo.PreHandlePicked += OnPreHandlePicked;
        _workGizmo.Gizmo.PreDragEnd += OnDragEnd;
        

        // edit mode start default rotation 
        MoveAction = MovePositionAction;

    }
    // UI 이벤트들 연결 
    private void OnEnable()
    {
        InputControl.GetInstance.OnMouseDownAction += OnMouseDown;
        RoomManager.GetInstance.OnEditAsset += OnStartEdit;

        _editManager.OnSelectTransform += OnSelectTransformReceive;
        _editManager.OnSelectAxis += OnSelectAxisReceive;
        _editManager.OnMove += OnMoveReceive;
        _editManager.OnSelectDone += OnSelectDoneReceive;
        _editManager.OnSelectRemove += OnSelectRemoveReceive;
        _editManager.OnSelectClose += OnSelectCloseReceive;
        _editManager.OnSelectSales += OnSelectSalesReceive;
    }
    // UI 이벤트들 해제 
    private void OnDisable()
    {
        InputControl.GetInstance.OnMouseDownAction -= OnMouseDown;
        RoomManager.GetInstance.OnEditAsset -= OnStartEdit;

        _editManager.OnSelectTransform -= OnSelectTransformReceive;
        _editManager.OnSelectAxis -= OnSelectAxisReceive;
        _editManager.OnMove -= OnMoveReceive;
        _editManager.OnSelectDone -= OnSelectDoneReceive;
        _editManager.OnSelectRemove -= OnSelectRemoveReceive;
        _editManager.OnSelectClose -= OnSelectCloseReceive;
        _editManager.OnSelectSales -= OnSelectSalesReceive;
    }
    /// <summary>
    /// 에디트 모드 시작시 디폴트 탭화면 표시
    /// </summary>
    private void OnStartEdit()
    {
        _editManager.SetDefaultTab();
    }
    private void OnChangeSalesReceive(bool isSale)
    {
        if (_targetObject == null) return;
        Item item = _targetObject.GetComponent<AssetNetworkController>().AssetData;
        if (isSale) item.TempSellable = "Y";
        else item.TempSellable = "N";
    }
    private void OnChangePriceReceive(uint _price)
    {
        if (_targetObject == null) return;
        Item item = _targetObject.GetComponent<AssetNetworkController>().AssetData;
        item.TempSalePoint = _price;
    }
    /// <summary>
    /// 에디트 모드 창 판매 관련 정보 업데이트시 
    /// </summary>
    /// <param name="editSales"></param>
    private void OnSelectSalesReceive(EditSales editSales)
    {
        _editSalse = editSales;

        // 타겟 에셋이 지정 안되어있으면 
        if (_targetObject == null) 
        {
            // 디폴트값 할당 
            _editSalse.Price = 0;
            _editSalse.Status = false;
        }
        else
        {
            // 에셋 판매 유무, 판매 가격 업데이트 창 닫으면 서버에 저장 
            Item item = _targetObject.GetComponent<AssetNetworkController>().AssetData;
            // 판매창 정보 업데이트 
            _editSalse.Price = item.PropertiesData.salePoint;
            _editSalse.Status = (item.PropertiesData.sellable == "Y") ? true : false;

            // 값 변경시 이벤트 콜백 연결 
            _editSalse.OnChangeSales += OnChangeSalesReceive;
            _editSalse.OnChagnePrice += OnChangePriceReceive;
        }
        
        
    }
   
    /// <summary>
    /// 에디트창 탭 변경시
    /// </summary>
    private void OnSelectTransformReceive(ConstEventString.EditTransformType type)
    {
        if (_targetObject == null) return;
        switch (type)
        {
            case  ConstEventString.EditTransformType.MOVE :
                SetWorkGizmoId(TransformGizmoId.Move);
                MoveAction = MovePositionAction;
                break;
            case  ConstEventString.EditTransformType.ROTATATION :
                SetWorkGizmoId(TransformGizmoId.Rotate);
                MoveAction = MoveRotationAction;
                break;
            case  ConstEventString.EditTransformType.SCALE :
                SetWorkGizmoId(TransformGizmoId.Scale);
                MoveAction = MoveScaleAction;
                break;
        }
    }
    /// <summary>
    /// 방향값 선택시 
    /// </summary>
    private void OnSelectAxisReceive(ConstEventString.EditAxisType type)
    {
        if (_targetObject == null) return;
        switch (type)
        {
            case  ConstEventString.EditAxisType.X :
                currentAxisType = ConstEventString.EditAxisType.X;
                break;
            case  ConstEventString.EditAxisType.Y :
                currentAxisType = ConstEventString.EditAxisType.Y;
                break;
            case  ConstEventString.EditAxisType.Z :
                currentAxisType = ConstEventString.EditAxisType.Z;
                break;
        }
    }

    /// <summary>
    /// 이동 버튼 이벤트 콜백 
    /// </summary>
    private void OnMoveReceive(int val)
    {
        if (_targetObject == null) return;
        MoveAction(val);
        _workGizmo.SetTargetObject(assetTrans.gameObject);
        //_indicator.transform.position = _targetObject.transform.position;
    }
    /// <summary>
    /// 에셋 포지션 이동
    /// val 0 이면 원래 포지션값으로 원복 
    /// </summary>
    private void MovePositionAction(int val)
    {
        if (val == 0)
        {
            Item item = _targetObject.GetComponent<AssetNetworkController>().AssetData;
            ObjectTransform objTransform = Utility.GetParsingObjectTransform(item.PropertiesData.transform);
            assetTrans.position = objTransform.position;
            return;
        }

        float DecimalVal = val * 0.1f;
        switch (currentAxisType)
        {
            case  ConstEventString.EditAxisType.X :
                assetTrans.position = new Vector3(assetTrans.position.x + -DecimalVal,
                    assetTrans.position.y, assetTrans.position.z);
                break;
            case  ConstEventString.EditAxisType.Y :
                assetTrans.position = new Vector3(assetTrans.position.x,
                    assetTrans.position.y + DecimalVal, assetTrans.position.z);
                break;
            case  ConstEventString.EditAxisType.Z :
                assetTrans.position = new Vector3(assetTrans.position.x,
                    assetTrans.position.y, assetTrans.position.z + DecimalVal);
                break;
        }

    }
    /// <summary>
    /// 에셋 로테이션 회전
    /// val 0 이면 원래 회전값으로 원복 
    /// </summary>
    /// <param name="val"></param>
    private void MoveRotationAction(int val)
    {
        if (val == 0)
        {
            Item item = _targetObject.GetComponent<AssetNetworkController>().AssetData;
            ObjectTransform objTransform = Utility.GetParsingObjectTransform(item.PropertiesData.transform);
            assetTrans.eulerAngles = objTransform.rotation;
            return;
        }

        float DecimalVal = val * 10f;
       
        switch (currentAxisType)
        {
            case  ConstEventString.EditAxisType.X :
                assetTrans.Rotate(DecimalVal,0,0);
                break;
            case  ConstEventString.EditAxisType.Y :
                assetTrans.Rotate(0, DecimalVal,0);
                break;
            case  ConstEventString.EditAxisType.Z :
                assetTrans.Rotate(0,0, DecimalVal);
                break;
        }
    }
    /// <summary>
    /// 에셋 스케일 변경
    /// val 0 이면 원래 스케일로 원복 
    /// </summary>
    /// <param name="val"></param>
    private void MoveScaleAction(int val)
    {
        if (val == 0)
        {
            Item item = _targetObject.GetComponent<AssetNetworkController>().AssetData;
            ObjectTransform objTransform = Utility.GetParsingObjectTransform(item.PropertiesData.transform);
            assetTrans.localScale = objTransform.scale;
            return;
        }

        float DecimalVal= val * 0.1f;
        switch (currentAxisType)
        {
            case  ConstEventString.EditAxisType.X :
                assetTrans.localScale = new Vector3(assetTrans.localScale.x + DecimalVal,
                    assetTrans.localScale.y, assetTrans.localScale.z);
                break;
            case  ConstEventString.EditAxisType.Y :
                assetTrans.localScale = new Vector3(assetTrans.localScale.x,
                    assetTrans.localScale.y + DecimalVal, assetTrans.localScale.z);
                break;
            case  ConstEventString.EditAxisType.Z :
                assetTrans.localScale = new Vector3(assetTrans.localScale.x,
                    assetTrans.localScale.y, assetTrans.localScale.z + DecimalVal);
                break;
        }

    }
    /// <summary>
    /// 에디트 모드 종료 저장 
    /// </summary>
    private void OnSelectDoneReceive()
    {
        OnSaveAssetData();
        RoomManager.GetInstance.SaveEditAsset();
    }
    /// <summary>
    /// 룸 에셋 삭제
    /// 인벤토리로 이동 
    /// </summary>
    private void OnSelectRemoveReceive()
    {
        if (_targetObject == null||_targetObject.CompareTag("SpawnPoint")) return;

        CloseEditMode();

        Item item = _targetObject.GetComponent<AssetNetworkController>().AssetData;
        if (item.PropertiesData.quantity == 1)
        {
            RoomManager.GetInstance._baseServerManager.MoveToInventory(item,1);
            if (PhotonNetwork.IsConnected) PhotonNetwork.Destroy(_targetObject);
            else Destroy(_targetObject);
        }
        else
        {
            //RoomManager.GetInstance._baseServerManager.SetSellInfoPopup(item);
            SellInfoManager.GetInstance.SetSellInfoToInven(item, (int _quantity) =>
            {
                RoomManager.GetInstance._baseServerManager.MoveToInventory(item, _quantity);
                /*if(_quantity >= item.PropertiesData.quantity)
                {
                    Debug.Log("if left _quantity check " + _quantity + " / total quantity : "+ item.PropertiesData.quantity);
                    if (PhotonNetwork.IsConnected) PhotonNetwork.Destroy(_targetObject);
                    else Destroy(_targetObject);
                }
                else
                {
                    Debug.Log("else left _quantity check " + _quantity + " / total quantity : " + item.PropertiesData.quantity);
                }*/

            });
        }
    }
    /// <summary>
    /// 에디트 모드 종료 저장 
    /// </summary>
    private void OnSelectCloseReceive()
    {
        OnSaveAssetData();
        RoomManager.GetInstance.SaveEditAsset();
    }
    /// <summary>
    /// 마우스 다운 이벤트
    /// 에디트 모드인 경우만 체크 
    /// </summary>
    void OnMouseDown()
    {
        if (!RoomManager.GetInstance._isEditMode) return;
        if (RTGizmosEngine.Get.HoveredGizmo == null)
        {
            // 유저가 선택한 게임오브젝트 가져오기 (레이캐스트 사용)
            GameObject pickedObject = PickGameObject();
            // 선택한 객체가 없으면 리턴 
            if (pickedObject == null) return;


            GameObject pickObjectRoot = pickedObject.transform.root.gameObject;

            // SpawnPoint 선택시 
            if (pickObjectRoot.CompareTag("SpawnPoint"))
            {
                assetTrans = pickObjectRoot.transform;
                OnTargetObjectChanged(pickObjectRoot);

                // 디폴트 탭으로 전환 (MOVE 탭)
                _editManager.SetDefaultTab();
                // 스케일, 판매 탭 비활성 
                _editManager.SetSpawnPointActive(false);
                
                //_editManager.SelectTransform(ConstEventString.EditTransformType.MOVE);
                _editManager.SelectAxis(ConstEventString.EditAxisType.X);

                SetWorkGizmoId(TransformGizmoId.Move);
                return;
            }
            // 일반 에셋 선택시 
            else
            {
                // 디폴트 탭으로 전환 (MOVE 탭)
                _editManager.SetDefaultTab();
                // 스케일, 판매 탭 활성 
                _editManager.SetSpawnPointActive(true);
                
            }


            assetTrans = pickObjectRoot.GetComponent<AssetControl>().AssetTransform;
            // 선택한 객체의 최상위 부모객체 연결 
            //GameObject pickObjectAsset = pickedObject.transform.root.Find("Asset").gameObject;

            // 포톤뷰 가져오기 
            _pview = pickObjectRoot.GetComponent<PhotonView>();
            // 포톤에 연결안됐거나 남의것이면 리턴
            if (PhotonNetwork.IsConnected && !_pview.IsMine) return;

            // 선택한 객체 저장(나중에 세이브하기 위해서)
            KeepAssetObject(pickObjectRoot);
            SetTempSellData(pickObjectRoot);

            // 기존 타겟오브젝트 새로 가져온 오브젝트로 갱신 
            OnTargetObjectChanged(pickObjectRoot);
        }
    }
    /// <summary>
    /// 판매정보 수정시 가격, 판매여부, 임시 저장 
    /// </summary>
    /// <param name="_target"></param>
    private void SetTempSellData(GameObject _target)
    {
        Item item = _target.GetComponent<AssetNetworkController>().AssetData;
        item.TempSalePoint = item.PropertiesData.salePoint;
        item.TempSellable = item.PropertiesData.sellable;
    }
    /// <summary>
    /// 기즈모 핸들 선택했을때
    /// </summary>
    void OnPreHandlePicked(Gizmo gizmo, int handleId)
    {
        // InputControl.GetInstance.isLookRotate = false;
        // 인디케이터 비활성 
        SetActiveIndicator(false);
    }
    /// <summary>
    /// 에셋 드래그 끝나면 
    /// </summary>
    void OnDragEnd(Gizmo gizmo, int handleId)
    {
        if (_targetObject == null) return;
        SetActiveIndicator(true);
    }


    /// <summary>
    /// Called every frame to perform all necessary updates. In this tutorial,
    /// we listen to user input and take action. 
    /// </summary>
    private void Update()
    {
        // Switch between different gizmo types using the W,E,R,T keys.
        // Note: We use the 'SetWorkGizmoId' function to perform the switch.
       // if (Input.GetKeyDown(KeyCode.W)) SetWorkGizmoId(TransformGizmoId.Move);
       // else if (Input.GetKeyDown(KeyCode.E)) SetWorkGizmoId(TransformGizmoId.Rotate);
       // else if (Input.GetKeyDown(KeyCode.R)) SetWorkGizmoId(TransformGizmoId.Scale);
       // else if (Input.GetKeyDown(KeyCode.T)) SetWorkGizmoId(TransformGizmoId.Universal);
    }

    /// <summary>
    /// Uses the mouse position to pick a game object in the scene. Returns
    /// the picked game object or null if no object is picked.
    /// </summary>
    /// <remarks>
    /// Objects must have colliders attached.
    /// </remarks>
    private GameObject PickGameObject()
    {
        // 게임오브젝트가 아닌 UI 클릭시 null 리턴 
        if (EventSystem.current.IsPointerOverGameObject()) return null;
        // Build a ray using the current mouse cursor position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Check if the ray intersects a game object. If it does, return it
        RaycastHit rayHit;
        // 편집 가능 레이어만 찾기 
       // int layerMask = 1 << LayerMask.NameToLayer(RayTargetLayer);
        if (Physics.Raycast(ray, out rayHit, float.MaxValue, RayTargetLayer))
            return rayHit.collider.gameObject;

        // No object is intersected by the ray. Return null.
        return null;
    }

    /// <summary>
    /// This function is called to change the type of work gizmo. This is
    /// used in the 'Update' function in response to the user pressing the
    /// W,E,R,T keys to switch between different gizmo types.
    /// </summary>
    private void SetWorkGizmoId(TransformGizmoId gizmoId)
    {
        // If the specified gizmo id is the same as the current id, there is nothing left to do
        if (gizmoId == _workGizmoId) return;

        // Start with a clean slate and disable all gizmos
        _objectMoveGizmo.Gizmo.SetEnabled(false);
        _objectRotationGizmo.Gizmo.SetEnabled(false);
        _objectScaleGizmo.Gizmo.SetEnabled(false);
        _objectUniversalGizmo.Gizmo.SetEnabled(false);

        // At this point all gizmos are disabled. Now we need to check the gizmo id
        // and adjust the '_workGizmo' variable.
        _workGizmoId = gizmoId;
        if (gizmoId == TransformGizmoId.Move) _workGizmo = _objectMoveGizmo;
        else if (gizmoId == TransformGizmoId.Rotate) _workGizmo = _objectRotationGizmo;
        else if (gizmoId == TransformGizmoId.Scale) _workGizmo = _objectScaleGizmo;
        else if (gizmoId == TransformGizmoId.Universal) _workGizmo = _objectUniversalGizmo;

        // At this point, the work gizmo points to the correct gizmo based on the 
        // specified gizmo id. All that's left to do is to activate the gizmo. 
        // Note: We only activate the gizmo if we have a target object available.
        //       If no target object is available, we don't do anything because we
        //       only want to show a gizmo when a target is available for use.
        if (_targetObject != null)
        {
            _workGizmo.Gizmo.SetEnabled(true);
            _workGizmo.RefreshPositionAndRotation();
        }
    }
    /// <summary>
    /// 바운드 영역 계산해서 center pivot 설정 
    /// </summary>
    /// <param name="go"></param>
    private void SetPivotAndIndicator(GameObject go)
    {
        Bounds totalBounds = ObjectUtils.GetChildRendererBounds(go);
        Vector3 centerBoundPos = new Vector3(totalBounds.center.x, totalBounds.center.y, totalBounds.center.z);
        API.SetPivot(go.transform, centerBoundPos, API.Space.Global);

        //_workGizmo.SetTransformPivot(GizmoObjectTransformPivot.ObjectCenterPivot);
    }

    private void SetActiveIndicator(bool _visible)
    {
      //  _indicator.GetComponentInChildren<Renderer>().enabled = _visible;
    }
    /// <summary>
    /// Called from the 'Update' function when the user clicks on a game object
    /// that is different from the current target object. The function takes care
    /// of adjusting the gizmo states accordingly.
    /// </summary>
    private void OnTargetObjectChanged(GameObject newTargetObject)
    {
        // 동기화 종료 
        //if(_targetObject != null) _targetObject.GetComponent<PhotonView>().RPC("OnAssetEdit", RpcTarget.All, false);
        // Store the new target object
        _targetObject = newTargetObject;

        // 선택된 타겟 에셋이 있으면 기즈모 켜기 
        if (_targetObject != null)
        {
            //SetPivotAndIndicator(assetTrans.gameObject);
            _objectMoveGizmo.SetTargetObject(assetTrans.gameObject);
            _objectRotationGizmo.SetTargetObject(assetTrans.gameObject);
            _objectScaleGizmo.SetTargetObject(assetTrans.gameObject);
            _objectUniversalGizmo.SetTargetObject(assetTrans.gameObject);
            _workGizmo.Gizmo.SetEnabled(true);

        }
        else
        {
            // 선택된 타겟 에셋 없으면 인디케이터 끄기 
            // SetActiveIndicator(false);
            // 선택된 타겟 에셋 없으면 기즈모 끄기 
            _objectMoveGizmo.Gizmo.SetEnabled(false);
            _objectRotationGizmo.Gizmo.SetEnabled(false);
            _objectScaleGizmo.Gizmo.SetEnabled(false);
            _objectUniversalGizmo.Gizmo.SetEnabled(false);

            
           // if(PhotonNetwork.IsConnected && _pview) _pview.RPC("OnAssetEdit", RpcTarget.All, false);
        }
    }

    
    /// <summary>
    /// 기즈모 언비저블
    /// </summary>
    private void CloseEditMode()
    {
        _workGizmo.Gizmo.PreHandlePicked -= OnPreHandlePicked;
        _workGizmo.Gizmo.PreDragEnd -= OnDragEnd;

        _objectMoveGizmo.Gizmo.SetEnabled(false);
        _objectRotationGizmo.Gizmo.SetEnabled(false);
        _objectScaleGizmo.Gizmo.SetEnabled(false);
        _objectUniversalGizmo.Gizmo.SetEnabled(false);

        // 부모 오브젝트에서 BoundBox 컴포넌트 찾아서 가져오기
       // BoundBox bb = FindBoundBox(_targetObject);
       // bb.EnableLines(false, _targetObject.transform);

      

    }
    /// <summary>
    /// 에셋 데이터 저장 
    /// </summary>
    private void OnSaveAssetData()
    {
        // 에디트 모드 종료 
        CloseEditMode();

        // rpc 날려서 더이상 옵저브 안하도록 
        // if (PhotonNetwork.IsConnected) _pview.RPC("OnAssetEdit", RpcTarget.All, false);

        // 선택된 에셋이 없으면 리턴
        //if (_targetObject == null) return;  // 현재 선택된게 없어도 기존에 선택한게 있으면 저장해야함 

        // 기존에 선택했었던 오브젝트 있는지 체크해서 있으면 저장하도록 
        if (keepEditedAsset.Count > 0) 
        {
            StartCoroutine(OnSaveEditedAssets());
        }
        

    }

    /// <summary>
    /// 에디트 에셋 리스트에 임시 저장 
    /// </summary>
    /// <param name="go"></param>
    private void KeepAssetObject(GameObject go)
    {
        if(!keepEditedAsset.Contains(go))
        keepEditedAsset.Add(go);
       
    }
    /// <summary>
    /// 에셋 트랜스폼 변경 데이터 저장
    /// </summary>
    /// <returns></returns>
    IEnumerator OnSaveEditedAssets()
    {
        AssetNetworkController anc;
        Item assetData;
        string splitDecimalTransform;
        Transform targetTransform;
        AssetControl assetControl;

        // 에셋 수정 데이터 서버에 저장 
        foreach (GameObject go in keepEditedAsset)
        {
            if (go != null)
            {
                anc = go.GetComponent<AssetNetworkController>();
                assetData = anc.AssetData;
                assetControl = go.GetComponent<AssetControl>();
                targetTransform = assetControl.AssetTransform;

                //********************************************************  서버 에셋 트랜스폼 정보 업데이트
                splitDecimalTransform = Utility.GetDecimalTransform(targetTransform);
                if (assetData.PropertiesData.transform != splitDecimalTransform)
                {
                    string jsonTransform = "{\"assetNo\":" + assetData.no +",\"assetSubNo\":" + assetData.PropertiesData.assetSubNo + ",\"transform\":\"" + splitDecimalTransform + "\"}";
                    RoomManager.GetInstance._baseServerManager.UpdateAsset("/room/update/asset", jsonTransform);
                    assetData.PropertiesData.transform = splitDecimalTransform;
                    _pview.RPC("OnSetPositionSalesBalloon", RpcTarget.All);//  판매 풍선 위치 업데이트 (방안의 모든 포톤 유저)
                }





                //********************************************************  서버 에셋 판매 정보 업데이트
                yield return new WaitForSeconds(0.1f);

                if (assetData.TempSellable != assetData.PropertiesData.sellable || assetData.TempSalePoint != assetData.PropertiesData.salePoint)
                {
                    string jsonSellInfo = "{\"assetNo\":" + assetData.no + ",\"assetSubNo\":" + assetData.PropertiesData.assetSubNo + ",\"sellable\":\"" + assetData.TempSellable + "\",\"salePoint\":" + assetData.TempSalePoint + "}";
                    RoomManager.GetInstance._baseServerManager.UpdateAsset("/room/asset/update", jsonSellInfo);

                    //  판매 정보 업데이트 (방안의 모든 포톤 유저, 포톤 uint 지원안함 )
                    _pview.RPC("OnSetSalesInfo", RpcTarget.All, assetData.TempSellable, (int)assetData.TempSalePoint);
                }

               // assetControl.SetPositionSalesBalloon();
               // assetControl.SetActiveSalesBalloon(assetData.PropertiesData.sellable);
            }
           
        }

        yield return null;
    }
    
    

}
