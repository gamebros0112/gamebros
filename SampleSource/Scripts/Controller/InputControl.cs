using System;
using CinemachineCustom;
using UnityEngine;
using UnityEngine.InputSystem;

// [Serializable]
// public class MoveInputEvent : UnityEvent<float, float> { }

public class InputControl : Singleton<InputControl>
{
    // [SerializeField] private Camera Orthocam;
    [SerializeField] private OrthocamManager orthocamManager;   
    private InputMaster _inputMaster;

    public delegate void MoveInputEvent(float x, float y);
    public delegate void MoveDeltaEvent(float f);
    public delegate void ChangeViewEvent(bool b);
    public event MoveInputEvent OnMoveInputEvent, OnLookInputEvent, OnChangeLook;
    public event MoveDeltaEvent OnScrollMouse;
    public event ChangeViewEvent OnChangeView;
    
    // 이벤트 액션 딜리게이트
    //public Action OnMouseScroll;
    public Action OnMouseDownAction;
    public Action OnMouseUpAction;
    public Action OnJumpKeyAction;

    // 마우스 화면 스크롤 기능 on/off
    public bool isLookRotate = true;
    // 마우스로 뷰 이동 상태
    public bool isChangeLooking;
    
    private void Awake()
    {
        _inputMaster = new InputMaster();
    }

    /// <summary>
    /// 외부에서 Inputmaster 전체를 켜고 끔.
    /// </summary>
    public void Activate(bool b)
    {
        if (b)
            _inputMaster.Enable();
        else
            _inputMaster.Disable();
    }
    
    private void OnEnable() => _inputMaster.Enable();
    private void OnDisable() => _inputMaster.Disable();
    
    
    private void Start()
    {
        //InputMaster.Player.Shoot.performed += ShootPlayer;
        _inputMaster.Player.Movement.performed += MovePlayer;
        _inputMaster.Player.Movement.canceled += MovePlayer;
        _inputMaster.Player.Shoot.performed += ShootPlayer;
        _inputMaster.Player.Shoot.canceled += ShootPlayerCanceled;
        _inputMaster.Player.RightButton.performed += RightPlayer;
        _inputMaster.Player.RightButton.canceled += RightPlayer;
        _inputMaster.Player.Jump.performed += JumpPlayer;
        // _inputMaster.Player.Look.performed += LookPlayer;

        
        _inputMaster.Player.ChangeView.performed += ChangeView; //토글스위치이므로 performed로만 동작. 
        
        
        _inputMaster.Player.WheelView.performed += ScrollView;
        _inputMaster.Player.WheelView.canceled += ScrollView;

        _inputMaster.Player.Look.performed += ChangeLook;

    }

    /// <summary>
    /// 마우스 클릭시에 Camera Look Roatation 스위치 온!
    /// </summary>
    /// <param name="obj">Input ACtions 이벤트 콜백 객체</param>
    private void ShootPlayer(InputAction.CallbackContext obj) => OnMouseDownAction?.Invoke();
    private void ShootPlayerCanceled(InputAction.CallbackContext obj) => OnMouseUpAction?.Invoke();
    private void RightPlayer(InputAction.CallbackContext obj) => OnChangeView?.Invoke(obj.performed);
    private void LookPlayer(InputAction.CallbackContext obj)
    {
        
        Vector2 lookVector = obj.ReadValue<Vector2>();
        //Debug.Log($"look:{lookVector.x}, {lookVector.y}");
        OnLookInputEvent?.Invoke(lookVector.x, lookVector.y);
    }
    private void JumpPlayer(InputAction.CallbackContext obj)
    {
        OnJumpKeyAction?.Invoke();
    }
    private void MovePlayer(InputAction.CallbackContext obj)
    {
        Vector2 moveVector = obj.ReadValue<Vector2>();
       // Debug.Log(moveVector);
        OnMoveInputEvent?.Invoke(moveVector.x, moveVector.y);
    }

    private void ScrollView(InputAction.CallbackContext obj)
    {
        
        Vector2 moveVector = obj.ReadValue<Vector2>();
        OnScrollMouse?.Invoke(moveVector.y);
    }
    /// <summary>
    /// 2021.11.03 yongsik
    /// ChangeLook 이벤트 발생시켜 벽면 active 조정할 목적으로 
    /// </summary>
    /// <param name="obj"></param>
    private void ChangeLook(InputAction.CallbackContext obj)
    {
        Vector2 moveVector = obj.ReadValue<Vector2>();
        OnChangeLook?.Invoke(moveVector.x, moveVector.y);
    }
    /// <summary>
    /// 카메라의 퍼스펙티브 변경(싸이월드모드) 토글
    /// </summary>
    /// <param name="obj"></param>
    private void ChangeView(InputAction.CallbackContext obj)
    {
        switch (obj.control.displayName.ToLower())
        {
            case "c":
                orthocamManager.ToggleCMode();
                break;
            case "q":
                //ChattingEvent.GetInstance.CallChatUI();
                break;
        }
        //Orthocam.gameObject.SetActive(obj.performed);//press:실행 release:복귀 
        
        //Orthocam.gameObject.SetActive(!Orthocam.gameObject.activeInHierarchy); //toggle
    }
    
    

    
}
