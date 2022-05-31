using System;
using UnityEngine;
using Photon.Pun;
using Unity.Mathematics;

public class PhotonPlayerControl : MonoBehaviour, IPlayerControl
{
    [Header("Camera Movement Speed by Key Control")]
    [SerializeField] private float movementSpeed;
    [Header("Camera Rotation Speed by Mouse Control")]
    [SerializeField] private float rotationSpeed;
    [Header("Jump Speed by Key Control")]
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float gravity = 20f;
    [SerializeField] private Texture2D faceBaseTex;

    public MeshRenderer faceRenderer;
    private float horizontal = 0f;
    private float vertical = 0f;

    private CharacterController controller;
    private Vector3 moveDirection;
    private Vector3 projCamForward;

    private bool isJumped;
    private bool isPaused;

    private Vector2 deltaLook;
    private Camera mainCamera;
    private PhotonView pView;

    void Awake()
    {
        pView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        /// my character check
        if (pView.IsMine)
        {
            controller = GetComponent<CharacterController>();
            mainCamera = Camera.main;
        }
    }
    void OnEnable()
    {
        if (pView.IsMine)
        {
            InputControl.GetInstance.OnMoveInputEvent += OnMoveInput;
            InputControl.GetInstance.OnJumpKeyAction += OnJumpInput;
            ChattingEvent.GetInstance.OnEnterText += OnEnterTextReceive;
            FacePixelEditorManager.GetInstance.OnSelectFace += OnFaceNoti;
        }
    }

    void OnDisable()
    {
        if (pView.IsMine)
        {
            try
            {
                InputControl.GetInstance.OnMoveInputEvent -= OnMoveInput;
                InputControl.GetInstance.OnJumpKeyAction -= OnJumpInput;
                ChattingEvent.GetInstance.OnEnterText -= OnEnterTextReceive;
                FacePixelEditorManager.GetInstance.OnSelectFace -= OnFaceNoti;
            }
            catch (NullReferenceException ex)
            {
                Debug.Log("InputControl is null");
            }
        }
    }
    
    public void SetChatEvent(bool _bool)
    {
        //todo Chat Event 가 꺼져도 채팅은 업데이트 되어야 하는거 아닌가?
        Debug.Log("SetChatEvent");
        if (_bool)
        {
            // ChatTextContainer.GetInstance.OnEnterText += OnEnterTextReceive;
            ChattingEvent.GetInstance.OnEnterText += OnEnterTextReceive;
        }
        else
        {
            // ChatTextContainer.GetInstance.OnEnterText -= OnEnterTextReceive;
            ChattingEvent.GetInstance.OnEnterText -= OnEnterTextReceive;
        }
    }
    private void OnEnterTextReceive(string str)
    {
        Debug.Log($"OnEnterTextReceive movement:{str}, {pView.Owner.NickName}");
        pView.RPC("OnChatMessage",RpcTarget.All, str,pView.Owner.NickName,PhotonNetwork.LocalPlayer.ActorNumber);
    }
    [PunRPC]
    public void OnChatMessage(string msg, string sender, int actorNum)
    {
        // ChatTextContainer.GetInstance.AddTextList(sender+" : "+msg);
        int isMine = 1;
        if (actorNum != PhotonNetwork.LocalPlayer.ActorNumber) isMine = 0;
        ChattingEvent.GetInstance.RecieveMessage(sender+" : "+msg, isMine);
    }
    public void OnMoveInput(float x, float y)
    {
        /// my character check
        if (pView.IsMine)
        {
           // Debug.Log($"movement:{x}, {y}");
            horizontal = x;
            vertical = y;
        }
    }
    public void OnJumpInput()
    {
        //Debug.Log($"isGrounded:{controller.isGrounded}");
        if (controller.isGrounded) isJumped = true;
    }
    public void OnPaused(bool status)
    {
        isPaused = status;
    }
    public void MovementPlayer()
    {
        if (controller.isGrounded)
        {
            moveDirection = Vector3.forward * vertical + Vector3.right * horizontal;
            projCamForward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up);
            Quaternion rotationToCam = Quaternion.LookRotation(projCamForward, Vector3.up);
            moveDirection = rotationToCam * moveDirection;

            transform.rotation = Quaternion.LookRotation(projCamForward, Vector3.up);
            // transform.position += moveDirection * movementSpeed * Time.deltaTime;

            // Move 스피드 적용
            moveDirection *= movementSpeed;

            // 캐릭터 점프
            if (isJumped)
            {
                moveDirection.y = jumpSpeed;
                isJumped = false;
            }


        }

        // 캐릭터에 중력 적용.
        moveDirection.y -= gravity * Time.deltaTime;
        // 점프 초기화 
        //if (moveDirection.y<= 0) isJumped = false;

        // 캐릭터 떨어지는 경우
        if (moveDirection.y <= -30f)
        {
            // Debug.Log("  moveDirection.y  : " + moveDirection.y + " / transform.position.y : " + transform.position.y);
            //moveDirection.y = 50f;
            transform.position = new Vector3(0, 1f, 2f);
            moveDirection = Vector3.zero;
        }
        else
        {
            // 캐릭터 움직임.
            controller.Move(moveDirection * Time.deltaTime);
        }
    }

    private void Update()
    {
        /// my character check
        if (pView.IsMine)
        {
            if (isPaused || controller==null) return;
            MovementPlayer();
        }
    }

    public void OnFaceNoti(Texture2D tex)
    {
        faceRenderer.material.SetTexture("_BaseMap", tex);
        //faceRenderer.enabled = true;
        Invoke("DisableFaceRenderer", 5f);

    }
    private void DisableFaceRenderer()
    {
        //faceRenderer.enabled = false;
        faceRenderer.material.SetTexture("_BaseMap", faceBaseTex);
    }
}
