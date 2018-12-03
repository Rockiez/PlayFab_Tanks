﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;

public class LoginPanelController : MonoBehaviourPunCallbacks, IConnectionCallbacks
{

	public GameObject loginPanel;		
	public GameObject userMessage;		
	public Button backButton;			
	public GameObject lobbyPanel;		
	public GameObject roomPanel;		
	public Text connectionState;

    public InputField Username;
    public InputField Password;

    private PlayFabAuthService _AuthService = PlayFabAuthService.Instance;


    void Start() {
        if (!(PhotonNetwork.IsConnected)) {
			SetLoginPanelActive ();	
			//username.text = PlayerPrefs.GetString ("Username");
            
		} 
		else
			SetLobbyPanelActive ();
		connectionState.text = "";
        if (PhotonNetwork.InRoom)
        {
            lobbyPanel.SetActive(false);
            roomPanel.SetActive(true);
        }

        PlayFabAuthService.OnLoginSuccess += RequestPhotonToken;
        PlayFabAuthService.OnPlayFabError += OnPlayFabError;
    }



	void Update(){		
        connectionState.text = PhotonNetwork.NetworkClientState.ToString ();
	}

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
        _AuthService.Email = Username.text;
        _AuthService.Password = Password.text;
        LogMessage("Email:*"+Username.text +"*   Password:*"+ Password.text+"*");
        _AuthService.AuthenticateEmailPassword();

        PhotonNetwork.GameVersion = "1.0";
        if (!PhotonNetwork.IsConnected)						
            PhotonNetwork.ConnectUsingSettings ();		
		//if (username.text == "")							
		//	username.text = "Ghost" + Random.Range (1, 9999);
  //      PhotonNetwork.LocalPlayer.NickName = username.text;			
		//PlayerPrefs.SetString ("Username", username.text);	
	}

    public void ClickGuestButton()
    {
        SetLobbyPanelActive();

        PhotonNetwork.GameVersion = "1.0";
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
        _AuthService.SilentlyAuthenticate();


        //PhotonNetwork.LocalPlayer.NickName = PlayFabAuthService.PlayFabId;


        //if (username.text == "")							
        //	username.text = "Ghost" + Random.Range (1, 9999);
        //      PhotonNetwork.LocalPlayer.NickName = username.text;			
        //PlayerPrefs.SetString ("Username", username.text);	
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

    private void RequestPhotonToken(LoginResult obj)
    {
        LogMessage("PlayFab authenticated. Requesting photon token...");

        PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest()
        {
            PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime
        }, AuthenticateWithPhoton, OnPlayFabError);
    }


    private void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj)
    {
        LogMessage("Photon token acquired: " + obj.PhotonCustomAuthenticationToken + "  Authentication complete.");

        //We set AuthType to custom, meaning we bring our own, PlayFab authentication procedure.
        var customAuth = new AuthenticationValues { AuthType = CustomAuthenticationType.Custom };

        //We add "username" parameter. Do not let it confuse you: PlayFab is expecting this parameter to contain player PlayFab ID (!) and not username.
        customAuth.AddAuthParameter("username", PlayFabAuthService.PlayFabId);    // expected by PlayFab custom auth service

        //We add "token" parameter. PlayFab expects it to contain Photon Authentication Token issues to your during previous step.
        customAuth.AddAuthParameter("token", obj.PhotonCustomAuthenticationToken);

        //We finally tell Photon to use this authentication parameters throughout the entire application.
        PhotonNetwork.AuthValues = customAuth;
    }
    private void OnPlayFabError(PlayFabError obj)
    {
        LogMessage(obj.GenerateErrorReport());
    }
    public void LogMessage(string message)
    {
        Debug.Log("PlayFab + Photon Example: " + message);
    }
    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        LogMessage(debugMessage);
    }
}
