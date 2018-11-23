using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CreateRoomController : MonoBehaviourPunCallbacks {

	public GameObject createRoomPanel;		
	public GameObject roomLoadingLabel;		
	public Text roomName;					
	public Text roomNameHint;				
	//public GameObject maxPlayerToggle;		

    private List<RoomInfo> roomList;

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        this.roomList = roomList;
    }
	public void ClickConfirmCreateRoomButton(){
		RoomOptions roomOptions=new RoomOptions();
        roomOptions.MaxPlayers = 4;

		//RectTransform toggleRectTransform = maxPlayerToggle.GetComponent<RectTransform> ();
		//int childCount = toggleRectTransform.childCount;
		//for (int i = 0; i < childCount; i++) {
		//	if (toggleRectTransform.GetChild (i).GetComponent<Toggle> ().isOn == true) {
		//		roomOptions.MaxPlayers = 2;
		//		break;
		//	}
		//}

		bool isRoomNameRepeat = false;

	    if (this.roomList != null)
	    {
	        foreach (RoomInfo info in this.roomList)
	        {
	            if (roomName.text == info.Name)
	            {
	                isRoomNameRepeat = true;
	                break;
	            }
	        }
        }

		if (isRoomNameRepeat) {
			roomNameHint.text = "Repeated Room Name!";
		}
		else {
			PhotonNetwork.CreateRoom (roomName.text, roomOptions, TypedLobby.Default);	

		}
	}
    public override void OnCreatedRoom()
    {
        roomNameHint.text = "";
        createRoomPanel.SetActive(false);
        roomLoadingLabel.SetActive(true);

        
    }
    
    public void ClickCancelCreateRoomButton(){
		createRoomPanel.SetActive (false);		
		roomNameHint.text = "";		
	}
}
