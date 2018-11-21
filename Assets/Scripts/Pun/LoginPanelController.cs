using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LoginPanelController : MonoBehaviourPunCallbacks {

	public GameObject loginPanel;		
	public GameObject userMessage;		
	public Button backButton;			
	public GameObject lobbyPanel;		
	public GameObject roomPanel;		
	public Text username;
	public Text connectionState;

    void Start() {
        if (!(PhotonNetwork.IsConnected)) {
			SetLoginPanelActive ();	
			username.text = PlayerPrefs.GetString ("Username");	
		} 
		else
			SetLobbyPanelActive ();
		connectionState.text = "";
        if (PhotonNetwork.InRoom)
        {
            lobbyPanel.SetActive(false);
            roomPanel.SetActive(true);
        }
	}



//#if(UNITY_EDITOR)	
	void Update(){		
        connectionState.text = PhotonNetwork.NetworkClientState.ToString ();
	}
//#endif

	public void SetLoginPanelActive(){
		loginPanel.SetActive (true);			
		userMessage.SetActive (false);				
		backButton.gameObject.SetActive (false);	
		lobbyPanel.SetActive (false);				
		if(roomPanel!=null)
			roomPanel.SetActive (false);			
	}
	public void SetLobbyPanelActive(){				
		loginPanel.SetActive (false);			
		userMessage.SetActive (true);				
		backButton.gameObject.SetActive (true);		
		lobbyPanel.SetActive (true);				
	}

	public void ClickLogInButton(){							
		SetLobbyPanelActive ();         
                                       
        PhotonNetwork.GameVersion = "1.0";
        if (!PhotonNetwork.IsConnected)						
            PhotonNetwork.ConnectUsingSettings ();		
		if (username.text == "")							
			username.text = "Ghost" + Random.Range (1, 9999);
        PhotonNetwork.LocalPlayer.NickName = username.text;			
		PlayerPrefs.SetString ("Username", username.text);	
	}
	
	public void ClickExitGameButton(){
		Application.Quit ();			
	}


    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }


    public override void OnJoinedLobby()
    {
        userMessage.GetComponentInChildren<Text>().text
                   = "Welcome，" + PhotonNetwork.LocalPlayer.NickName;
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        SetLoginPanelActive();
    }
}
