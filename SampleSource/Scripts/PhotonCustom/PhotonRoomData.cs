using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonRoomData : MonoBehaviour
{
    public static string PhotonRoomName="";
    public static string PhotonNickName="";
    public static bool isActive = false;

    void Start()
    {
        isActive = true;
    }

}
