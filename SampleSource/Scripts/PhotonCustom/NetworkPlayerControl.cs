using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class NetworkPlayerControl : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback
{
	private PhotonPlayerControl ppc;
	private Vector3 correctCharacterPos;
	private Quaternion correctCharacterRot;
	private Vector3 correctCharacterScale;
	//private float correctCharacterSpeed = 0;
	//private Animator anim;
	private double lastNetworkDataReceiveTime;
	//private Vector3 characterMoveValue = Vector3.zero;
	//private bool correctCharacterJump = false;
	//private bool correctCharacterJumping = false;

	private double InitPhotonTime;
	// Use this for initialization
	void Awake()
	{
		PhotonNetwork.SendRate = 20;
		PhotonNetwork.SerializationRate = 10;

		ppc = GetComponent<PhotonPlayerControl>();
		//anim = GetComponent<Animator>();
	
		if (!photonView.IsMine)
		{
			correctCharacterPos = transform.position;
			correctCharacterRot = transform.rotation;
			correctCharacterScale = transform.localScale;
		}
	}
    void Update()
    {
        if (!photonView.IsMine)
        {
            UpdateNetworkedPosition();
        }

    }


    void UpdateNetworkedPosition()
	{
		transform.position = correctCharacterPos;
		transform.rotation = correctCharacterRot;
		transform.localScale = correctCharacterScale;

		//float pingInSeconds = (float)PhotonNetwork.GetPing() * 0.001f;
		//float timeSinceLastUpdate = (float)(PhotonNetwork.Time - lastNetworkDataReceiveTime);
		
		/*
		Vector3 exterpolatedTargetPosition = correctCharacterPos + characterMoveValue * totalTimePassed;
		Vector3 newPosition = Vector3.MoveTowards(transform.position, exterpolatedTargetPosition, correctCharacterSpeed);

		//if(Vector3.Distance(characterTr.position , exterpolatedTargetPosition) > 1f)
		//		{
		//			newPosition = exterpolatedTargetPosition;
		//		}
		//newPosition.y = characterTr.position.y;
		transform.position = newPosition;
		transform.rotation = Quaternion.Lerp(transform.rotation, correctCharacterRot, Time.deltaTime * 5);
		transform.localScale = Vector3.Lerp(transform.localScale, correctCharacterScale, Time.deltaTime * 5);
		*/
	}
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			//stream.SendNext(transform.localScale);
			//stream.SendNext(jpc.characterMoveValue);
			//stream.SendNext(jpc.characterSpeed);
			/*
			stream.SendNext((int)jpc._characterAniState);
			stream.SendNext(anim.speed);
			
			stream.SendNext(jpc.n_jump);
			stream.SendNext(jpc.n_Jumping);
			*/
			float totalTimePassed = (float)(PhotonNetwork.Time - InitPhotonTime);
			if (totalTimePassed > 2)
			{
				RoomManager.GetInstance.DebugWrite("send transform data : "+ transform.position);
				InitPhotonTime = PhotonNetwork.Time;
			}
			
			//RoomManager.GetInstance.DebugWrite("totalTimePassed : " + totalTimePassed);
		}
		else
		{
			correctCharacterPos = (Vector3)stream.ReceiveNext();
			correctCharacterRot = (Quaternion)stream.ReceiveNext();
			//correctCharacterScale = (Vector3)stream.ReceiveNext();
			//characterMoveValue = (Vector3)stream.ReceiveNext();
			//correctCharacterSpeed = (float)stream.ReceiveNext();

			/*
			this.correctCharacterAniState = (int)stream.ReceiveNext();
			this.correctCharacterAniSpeed = (float)stream.ReceiveNext();
			this.correctCharacterJump = (bool)stream.ReceiveNext();
			this.correctCharacterJumping = (bool)stream.ReceiveNext();
			*/
			lastNetworkDataReceiveTime = info.SentServerTime;
		}
	}
	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		RoomManager.GetInstance.DebugWrite("info.photonView.name :  " + info.photonView.name);
        if (info.Sender == PhotonNetwork.LocalPlayer)
		{
			InitPhotonTime = PhotonNetwork.Time;
		}
		info.Sender.TagObject = gameObject;
		info.Sender.SetCustomProperties(new Hashtable { { "playerUniqueNo", photonView.InstantiationData[0].ToString() } });


		/*
		string characterProfileName = null;
		string characterCountryCode = null;
		Texture characterCountryTexture = null;
		if (info.Sender == PhotonNetwork.LocalPlayer)
		{
			main.setTexture(gameObject, CharacterSkinSets.GetCharacterSkin(PlayerData.instance.PlayerSkinIndex));
			characterTr.name = "player";
			main.player = characterTr;

			//Debug.Log ("PlayerData.instance.PlayerSkinIndex = "+PlayerData.instance.PlayerSkinIndex);
			//Debug.Log ("PlayerData.instance.GetPlayerSkinType() = "+PlayerData.instance.GetPlayerSkinType());


			Texture2D myProfileImg = GameObject.Find("OpenSocialObject").GetComponent<OpenSocialServiceManager>().myProfileTexture;
			characterProfileName = PlayerData.instance.MyProfileName;
			characterCountryCode = PlayerData.instance.MyCountryCode;
			characterCountryTexture = PlayerData.instance.MyCountryImg;
		}
		else
		{
			//Debug.Log ("otherPlayers : "+PhotonNetwork.otherPlayers[0]);
			//this.name = "other";
			int skinIndex = (int)PhotonNetwork.otherPlayers[0].customProperties["skinIndex"];
			main.setTexture(gameObject, CharacterSkinSets.GetCharacterSkin(skinIndex));
			characterTr.name = "other";
			main.other = characterTr;

			//Debug.Log ("other skinIndex : "+ skinIndex);
			//Debug.Log (" other myTexture  : "+(byte[])PhotonNetwork.otherPlayers[0].customProperties["myTexture"]);
			byte[] byteImg = (byte[])PhotonNetwork.otherPlayers[0].customProperties["myTexture"];
			Texture2D tex = new Texture2D(256, 256);
			tex.LoadImage(byteImg);

			//otherTexture.texture = tex;
			GameData.instance.OpProfileImg = tex;
			GameData.instance.OpProfileName = (string)PhotonNetwork.otherPlayers[0].customProperties["myProfileName"];
			GameData.instance.OpCountryCode = (string)PhotonNetwork.otherPlayers[0].customProperties["myCountryCode"];
			GameData.instance.OpContryImg = Resources.Load<Texture>("FlagsRoundingImg/" + GameData.instance.OpCountryCode);
			characterProfileName = GameData.instance.OpProfileName;
			characterCountryCode = GameData.instance.OpCountryCode;
			characterCountryTexture = GameData.instance.OpContryImg;
			main.OnJoinClient();
		}
		
		GameObject nameTextPrefab = Resources.Load<GameObject>("Prefabs/character/nameText");
		GameObject nameText = GameObject.Instantiate(nameTextPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		nameText.transform.SetParent(characterTr);
		nameText.transform.localPosition = new Vector3(0.0f, 2.5f, 0.0f);
		nameText.transform.localRotation = Quaternion.identity;
		//nameText.transform.localScale = new Vector3 ( -1.0f , 1.0f , 1.0f);
		nameText.GetComponent<TextMesh>().text = characterProfileName;
		nameText.GetComponent<NameText>().SetImage(characterCountryTexture);
		*/
	}
}
