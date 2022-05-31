using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using BestHTTP.JSON.LitJson;
using BestHTTP;
using System;
using UnityEngine.UI;
using EasyUI.Dialogs;
using System.Collections.Generic;
using Newtonsoft.Json;
using Photon.Pun;

public class FirebaseManager : BaseServerManager
{
    

    private void Awake()
    {
        RoomManager.GetInstance._baseServerManager = this;
    }
    protected override void Start()
    {
        base.Start();
        server = ServerType.Firebase;

        Debug.Log("FirebaseManager Start");
        if (string.IsNullOrEmpty(PhotonRoomData.PhotonRoomName))
        {
            if (PhotonRoomData.isActive) LoadRoomItems?.Invoke(null);
            else
            {
                //PhotonRoomData.PhotonRoomName = RoomManager.GetInstance.RoomName;
                PhotonRoomData.PhotonRoomName = RoomData.instance.EnterRoomNo;
                DownloadAsset();
            }

        }
        else
        {
            DownloadAsset();
        }
       
    }
  
    private void MovetoRoomPopup(InventoryItem data)
    {
        // 에셋 이동 팝업 호출
        DialogUI.Instance
        .SetData(LocalizeScriptableInfo.GetInstance.assetToRoom)
        .SetButtonColor(DialogButtonColor.Red)
        .OnClose(() => Debug.Log("Closed"))
        .OnFirstBtn(() => {
            //UpdateAsset(data.OBJECT_UNIQUEID, "INVENTORY_ASSET", "N");
            //LoadFromInventory?.Invoke(data);
        })
        .Show();
      
    }
    /// <summary>
    /// 프리뷰 판넬에서 호출하는 함수, 파이어베이스에 업로드 
    /// </summary>
    /// <param name="data"></param>
    public override void UploadAsset(InventoryItem data)
    {
        var request = new HTTPRequest(new Uri($"https://gestagallery-default-rtdb.firebaseio.com/rooms/item.json"), HTTPMethods.Post, (HTTPRequest req, HTTPResponse resp) =>
        {
            if (req.State == HTTPRequestStates.Finished)
            {
                if (resp.IsSuccess)
                {
                    string jsonStr = System.Text.Encoding.UTF8.GetString(resp.Data);
                    JsonData Objects = JsonMapper.ToObject(jsonStr);
                   // data.OBJECT_UNIQUEID = Objects["name"].ToString();

                  //  UpdateAsset(data.OBJECT_UNIQUEID, "OBJECT_UNIQUEID", data.OBJECT_UNIQUEID);
                    // update test
                    //StartCoroutine(UpdateDataPatch(OBJECT_ID));

                    OnCompleteUpload?.Invoke(true);

                    //MovetoRoomPopup(data);
                }
                else
                {
                    Debug.Log("upload error 1");
                    // 에러 팝업 호출 
                    DialogUI.Instance
                    .SetData(LocalizeScriptableInfo.GetInstance.uploadError)
                    .SetButtonColor(DialogButtonColor.Blue)
                    .OnClose(() => Debug.Log("Upload Error Close"))
                    .Show();

                    OnCompleteUpload?.Invoke(false);
                }
            }
            else
            {
                Debug.Log("upload error 2" );
                // 에러 팝업 호출 
                DialogUI.Instance
                   .SetData(LocalizeScriptableInfo.GetInstance.uploadError)
                   .SetButtonColor(DialogButtonColor.Blue)
                   .OnClose(() => Debug.Log("Upload Error Close"))
                   .Show();

                OnCompleteUpload?.Invoke(false);
            }
            


        });
        request.SetHeader("Content-Type", "application/json");
        request.RawData = System.Text.Encoding.UTF8.GetBytes(JsonMapper.ToJson(data));
        request.Send();
    }
    public override void DownloadAsset()
    {
        StartCoroutine(GetItemsFromFirebase());
    }

    /// <summary>
    /// 파이어베이스 아이템 값 가져오기 
    /// </summary>
    /// <returns></returns>
    IEnumerator GetItemsFromFirebase()
    {
        Debug.Log("GetItemsFromFirebase " );
        UnityWebRequest www = UnityWebRequest.Get($"https://gestagallery-default-rtdb.firebaseio.com/rooms/{PhotonRoomData.PhotonRoomName}/item.json");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            var textData = www.downloadHandler.text;
            Debug.Log("textData : " + textData.ToString());
            Dictionary<string,Item> items = JsonConvert.DeserializeObject<Dictionary<string, Item>>(textData);
            Debug.Log("items.Keys : " + items.Keys);
            if (items == null || items.Count <= 0) 
            {
                LoadRoomItems?.Invoke(null);
                yield break;
            }
            List<Item> allItems = new List<Item>(items.Values);
            
            Debug.Log("allItems.Count: " + allItems.Count);
            for (int i=0; i < allItems.Count; i++)
            {
               // if (allItems[i].INVENTORY_ASSET == "Y") inventoryItems.Add(allItems[i]);
               // else roomItems.Add(allItems[i]);
            }
            /*
            Debug.Log("Item OBJECT_TRANSFORM : " + allItems[0].OBJECT_TRANSFORM);
            ObjectTransform testObj = JsonConvert.DeserializeObject<ObjectTransform>(allItems[0].OBJECT_TRANSFORM);
            Debug.Log("Item OBJECT_TRANSFORM position : " + testObj.position);
            Debug.Log("Item OBJECT_TRANSFORM position : " + testObj.position.x);
            Debug.Log("Item OBJECT_TRANSFORM scale : " + testObj.scale.y);
            Debug.Log("Item OBJECT_TRANSFORM rotation : " + testObj.rotation.x);
            */

            LoadRoomItems?.Invoke(roomItems);
           // OnCompleteInventoryItems?.Invoke(inventoryItems);
        }
    }


    /// <summary>
    /// Firebase 유니크 키값으로 접근해서 속성값 수정
    /// </summary>
    IEnumerator UpdateDataPatch(string _uniqueId, string _key, string _value)
    {
        //Debug.Log("_uniqueId : " + _uniqueId + "_key : " + _key + "_value : " + _value);
        Dictionary<string, string> UserData = new Dictionary<string, string>();
        // UserData["CREATOR_EMAIL"] = "gamebros@gmail.com";
        UserData[_key] = _value;
        string json = JsonMapper.ToJson(UserData);

        UnityWebRequest uwrequest = UnityWebRequest.Put($"https://gestagallery-default-rtdb.firebaseio.com/rooms/item/" + _uniqueId + ".json", json);
        uwrequest.method = "PATCH";

        yield return uwrequest.SendWebRequest();
        Debug.Log("UpdateDataPatch end ");
    }
    /// <summary>
    /// Firebase 유니크 키값으로 접근해서 속성값 수정
    /// edit mode remove
    /// </summary>
    ///
    IEnumerator UpdateDataRemove(Item _item, string prop)
    {
        Dictionary<string, string> UserData = new Dictionary<string, string>();
        // UserData["INVENTORY_ASSET"] = "Y";
        UserData["INVENTORY_ASSET"] = prop;
        string json = JsonMapper.ToJson(UserData);
        // UnityWebRequest uwrequest = UnityWebRequest.Put($"https://gestagallery-default-rtdb.firebaseio.com/rooms/item/" + _item.OBJECT_UNIQUEID + ".json", json);
        UnityWebRequest uwrequest = UnityWebRequest.Put($"https://gestagallery-default-rtdb.firebaseio.com/rooms/item/"  + ".json", json);
        uwrequest.method = "PATCH";
        yield return uwrequest.SendWebRequest();
        if (uwrequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(uwrequest.error);
        }
        else
        {
            if (prop == "Y")
            {
                // 인벤토리 업데이트를 위한 이벤트 전달 
               // _item.SetObjectTransform();
               // OnCompleteRemove?.Invoke(_item);
                Debug.Log("Stage TO INVENTORY DATA CALLBACK OK");    
            }
            else
            {
                // 스테이지 업데이트를 위한 이벤트 전달 
                OnCompleteMoveTo?.Invoke(_item);
                Debug.Log("INVENTORY TO Stage DATA CALLBACK OK");
            }
            
        }
    }
    public override void UpdateAsset(string _uniqueId, string _key, string _value)
    {
        StartCoroutine(UpdateDataPatch(_uniqueId, _key, _value));
    }

    /// <summary>
    /// 스테이지에서 Inventory 로 이동
    /// </summary>
    /// <param name="_item"></param>
    public override void RemoveAsset(Item _item)
    {
        StartCoroutine(UpdateDataRemove(_item, "Y"));
    }
    /// <summary>
    /// Inventory에서  스테이지로 이동
    /// </summary>
    /// <param name="_item"></param>
    /*public override void MoveToRoom(Item _item)
    {
        StartCoroutine(UpdateDataRemove(_item, "N"));
    }*/
}
