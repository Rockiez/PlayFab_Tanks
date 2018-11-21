using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class RoomPanelController : MonoBehaviourPunCallbacks{

	public GameObject lobbyPanel;
	public GameObject roomPanel;
	public Button backButton;
	public Text roomName;				
	public GameObject[] Team1;		
	public GameObject[] Team2;		
	public Button readyButton;		
	public Text promptMessage;		

	PhotonView pView;
	int teamSize;
	Text[] texts;
	ExitGames.Client.Photon.Hashtable costomProperties;

    public override void OnEnable()
    {
        //PhotonNetwork.AutomaticallySyncScene = true;
        pView = GetComponent<PhotonView>();

        if (!(PhotonNetwork.IsConnected)) return;

        roomName.text = "Room" + PhotonNetwork.CurrentRoom.Name;
        promptMessage.text = "";

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(delegate ()
        {
            PhotonNetwork.LeaveRoom();
            lobbyPanel.SetActive(true);
            roomPanel.SetActive(false);
        });

        teamSize = PhotonNetwork.CurrentRoom.MaxPlayers / 2;

        teamSize = 1;
        DisableTeamPanel();
        UpdateTeamPanel(false);

        for (int i = 0; i < teamSize; i++)
        {
            if (!Team1[i].activeSelf)
            {
                Team1[i].SetActive(true);
                texts = Team1[i].GetComponentsInChildren<Text>();
                texts[0].text = PhotonNetwork.NickName;
                if (PhotonNetwork.IsMasterClient) texts[1].text = "Master";
                else texts[1].text = "UnReady";
                costomProperties = new ExitGames.Client.Photon.Hashtable() {
                    { "Team","Team1" },
                    { "TeamNum",i },
                    { "isReady",false },
                    { "Score",0 }
                };
                PhotonNetwork.LocalPlayer.SetCustomProperties(costomProperties);
                break;
            }
            else if (!Team2[i].activeSelf)
            {
                Team2[i].SetActive(true);
                texts = Team2[i].GetComponentsInChildren<Text>();
                if (PhotonNetwork.IsMasterClient) texts[1].text = "Master";
                else texts[1].text = "UnReady";
                costomProperties = new ExitGames.Client.Photon.Hashtable() {
                    { "Team","Team2" },
                    { "TeamNum",i },
                    { "isReady",false },
                    { "Score",0 }
                };
                PhotonNetwork.LocalPlayer.SetCustomProperties(costomProperties);
                break;
            }
        }
        ReadyButtonControl();

        base.OnEnable();
    }

    //public override void OnPlayerEnteredRoom(Player newPlayer)
    //{
    //    GameObject go;
    //    costomProperties = newPlayer.CustomProperties;
    //    if (costomProperties["Team"].Equals("Team1"))
    //    {
    //        go = Team1[(int)costomProperties["TeamNum"]];
    //        go.SetActive(true);
    //        texts = go.GetComponentsInChildren<Text>();
    //    }
    //    else
    //    {
    //        go = Team2[(int)costomProperties["TeamNum"]];
    //        go.SetActive(true);
    //        texts = go.GetComponentsInChildren<Text>();
    //    }
    //    texts[0].text = newPlayer.NickName;
    //    if ((bool)costomProperties["isReady"])
    //    {
    //        texts[1].text = "Ready";
    //    }
    //    else
    //        texts[1].text = "UnReady";

    //}



    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {

        DisableTeamPanel(); 
        UpdateTeamPanel(true);
    }


    public override void OnMasterClientSwitched (Player newMasterClient) {
		ReadyButtonControl ();
	}


    public override void OnPlayerLeftRoom(Player otherPlayer){
		DisableTeamPanel ();
		UpdateTeamPanel (true);	
	}


	void DisableTeamPanel(){
		for (int i = 0; i < Team1.Length; i++) {
			Team1 [i].SetActive (false);
		}
		for (int i = 0; i < Team2.Length; i++) {
			Team2 [i].SetActive (false);
		    

        }
	}


	void UpdateTeamPanel(bool isUpdateSelf){
		GameObject go;
        foreach (Player p in PhotonNetwork.PlayerList) {
            if (!isUpdateSelf && p.IsLocal) continue;

            costomProperties = p.CustomProperties;	
			if (costomProperties ["Team"].Equals ("Team1")) {	
				go = Team1 [(int)costomProperties ["TeamNum"]];	
				go.SetActive (true);	
				texts = go.GetComponentsInChildren<Text> ();
			} else {											
				go = Team2 [(int)costomProperties ["TeamNum"]];	
				go.SetActive (true);
				texts = go.GetComponentsInChildren<Text> ();
			}
            texts [0].text = p.NickName;	
			if(p.IsMasterClient)			
				texts[1].text="Master";	
			else if ((bool)costomProperties ["isReady"]) {
				texts [1].text = "Ready";	
			} else
				texts [1].text = "UnReady";	
		}
	}
    
	void ReadyButtonControl(){
		if (PhotonNetwork.IsMasterClient) {
			readyButton.GetComponentInChildren<Text> ().text = "Start Game";
			readyButton.onClick.RemoveAllListeners ();	
			readyButton.onClick.AddListener (delegate() {
				ClickStartGameButton ();		
			});
		} else {
            if((bool)PhotonNetwork.LocalPlayer.CustomProperties["isReady"])	
				readyButton.GetComponentInChildren<Text> ().text = "Cancel Ready";		
			else 
				readyButton.GetComponentInChildren<Text> ().text = "Ready";
			readyButton.onClick.RemoveAllListeners ();	
			readyButton.onClick.AddListener (delegate() {		
				ClickReadyButton ();							
			});
		}
	}
    
	public void ClickSwitchButton(){
        costomProperties = PhotonNetwork.LocalPlayer.CustomProperties;
		if ((bool)costomProperties ["isReady"]) {			
            promptMessage.text="Cannot switch teams in preparation state";		
			return;											
		}
		bool isSwitched = false;		
		if (costomProperties ["Team"].ToString ().Equals ("Team1")) {	
			for (int i = 0; i < teamSize; i++) {
				if (!Team2 [i].activeSelf) {	
					isSwitched = true;	
					Team1 [(int)costomProperties ["TeamNum"]].SetActive (false);
					texts = Team2 [i].GetComponentsInChildren<Text> ();	
                    texts [0].text = PhotonNetwork.NickName;	
					if(PhotonNetwork.IsMasterClient)texts[1].text="Master";		
					else texts [1].text = "UnReady";		
					Team2 [i].SetActive (true);	
					costomProperties = new ExitGames.Client.Photon.Hashtable ()	
					{ { "Team","Team2" }, { "TeamNum",i } };
                    PhotonNetwork.LocalPlayer.SetCustomProperties (costomProperties);	
					break;
				}
			}
		} else if (costomProperties ["Team"].ToString ().Equals ("Team2")) {
			for (int i = 0; i < teamSize; i++) {						
				if (!Team1 [i].activeSelf) {						
					isSwitched = true;			
					Team2 [(int)(costomProperties ["TeamNum"])].SetActive (false);	
					texts = Team1 [i].GetComponentsInChildren<Text> ();			
                    texts [0].text = PhotonNetwork.NickName;				
					if(PhotonNetwork.IsMasterClient)texts[1].text="Master";			
					else texts [1].text = "UnReady";							
					Team1 [i].SetActive (true);		
					costomProperties = new ExitGames.Client.Photon.Hashtable ()	
					{ { "Team","Team1" }, { "TeamNum",i } };
                    PhotonNetwork.LocalPlayer.SetCustomProperties (costomProperties);
					break;
				}
			}
		}
		if (!isSwitched)
            promptMessage.text = "The other team is full and cannot switch";
		else
			promptMessage.text = "";
	}
    
	public void ClickReadyButton(){
        bool isReady = (bool)PhotonNetwork.LocalPlayer.CustomProperties ["isReady"];	
		costomProperties = new ExitGames.Client.Photon.Hashtable (){ { "isReady",!isReady } };	
        PhotonNetwork.LocalPlayer.SetCustomProperties (costomProperties);
		Text readyButtonText = readyButton.GetComponentInChildren<Text> ();
	    if (isReady) readyButtonText.text = "Ready";
	    else readyButtonText.text = "Cancel Ready";
	}
    
	public void ClickStartGameButton(){
        foreach (Player p in PhotonNetwork.PlayerList) {
			if (p.IsLocal) continue;
			if ((bool)p.CustomProperties ["isReady"] == false) {
                promptMessage.text = "Someone is not ready, the game can't start";
				return;	
			}
		}
        if (PhotonNetwork.PlayerList.Length < 2)
        {
            promptMessage.text = "The team is not full, the game can't start";
            return;
        }
        promptMessage.text = "";	
        PhotonNetwork.CurrentRoom.IsOpen = false;	
        pView.RPC ("LoadGameScene", RpcTarget.All, "MainScene");
        PhotonNetwork.IsMessageQueueRunning = false;

    }

    [PunRPC]
	public void LoadGameScene(string sceneName){	
		PhotonNetwork.LoadLevel (sceneName);
	}
}
