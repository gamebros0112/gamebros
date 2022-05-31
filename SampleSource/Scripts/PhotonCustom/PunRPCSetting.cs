using UnityEngine;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;

public class PunRPCSetting : MonoBehaviour
{
    [PunRPC]
    public void OnLoadPicture(string url, int viewID)
    {
       // GameObject go = PhotonNetwork.Instantiate(prefabPath, new Vector3(0f, 1f, 2f), Quaternion.Euler(0, 180, 0), 0);
       // go.GetComponent<PhotonView>().ViewID = viewID;
    }

   

}
