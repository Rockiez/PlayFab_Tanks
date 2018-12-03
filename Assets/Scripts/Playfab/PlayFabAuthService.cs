using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabAuthService{

    public delegate void LoginSuccessEvent(LoginResult success);
    public static event LoginSuccessEvent OnLoginSuccess;

    public delegate void PlayFabErrorEvent(PlayFabError error);
    public static event PlayFabErrorEvent OnPlayFabError;

    public static string PlayFabId { get { return _playFabId; } }
    private static string _playFabId;
    public static string SessionTicket { get { return _sessionTicket; } }
    private static string _sessionTicket;

    public string Email;
    public string Username;
    public string Password;

    public GetPlayerCombinedInfoRequestParams InfoRequestParams;


    public static PlayFabAuthService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PlayFabAuthService();
            }
            return _instance;
        }
    }


    private static PlayFabAuthService _instance;

    public PlayFabAuthService()
    {
        _instance = this;
    }

    public void AuthenticateEmailPassword()
    {

        //We have not opted for remember me in a previous session, so now we have to login the user with email & password.
        PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            Email = Email,
            Password = Password,
            InfoRequestParameters = InfoRequestParams
        }, (result) =>
        {
            //store identity and session
            _playFabId = result.PlayFabId;
            _sessionTicket = result.SessionTicket;

            if (OnLoginSuccess != null)
            {
                //report login result back to subscriber
                OnLoginSuccess.Invoke(result);
            }
        }, (error) =>
        {
            if (OnPlayFabError != null)
            {
                //Report error back to subscriber
                OnPlayFabError.Invoke(error);
            }
            if (error.Error == PlayFabErrorCode.AccountNotFound)
            {
                AddAccountAndPassword();
            }
        });
    }

    private void AddAccountAndPassword()
    {
        //Any time we attempt to register a player, first silently authenticate the player.
        //This will retain the players True Origination (Android, iOS, Desktop)
        SilentlyAuthenticate((result) => {

            if (result == null)
            {
                //something went wrong with Silent Authentication, Check the debug console.
                OnPlayFabError.Invoke(new PlayFabError()
                {
                    Error = PlayFabErrorCode.UnknownError,
                    ErrorMessage = "Silent Authentication by Device failed"
                });
            }

            //Note: If silent auth is success, which is should always be and the following 
            //below code fails because of some error returned by the server ( like invalid email or bad password )
            //this is okay, because the next attempt will still use the same silent account that was already created.

            //Now add our username & password.
            PlayFabClientAPI.AddUsernamePassword(new AddUsernamePasswordRequest()
            {
                Username = !string.IsNullOrEmpty(Username) ? Username : result.PlayFabId, //Because it is required & Unique and not supplied by User.
                Email = Email,
                Password = Password,
            }, (addResult) => {
                if (OnLoginSuccess != null)
                {
                    //Store identity and session
                    _playFabId = result.PlayFabId;
                    _sessionTicket = result.SessionTicket;

                    //Report login result back to subscriber.
                    OnLoginSuccess.Invoke(result);
                }
            }, (error) => {
                if (OnPlayFabError != null)
                {
                    //Report error result back to subscriber
                    OnPlayFabError.Invoke(error);
                    
                }
                if (error.Error == PlayFabErrorCode.AccountAlreadyLinked)
                    RegisterAuthenticate(result);
            });

        });
    }

    private void RegisterAuthenticate(LoginResult result)
    {
        PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest()
        {
            Username = !string.IsNullOrEmpty(Username) ? Username : result.PlayFabId, //Because it is required & Unique and not supplied by User.
            Email = Email,
            Password = Password,
        }, (addResult) =>
        {
            if (OnLoginSuccess != null)
            {
                //Store identity and session
                _playFabId = result.PlayFabId;
                _sessionTicket = result.SessionTicket;

                //Report login result back to subscriber.
                OnLoginSuccess.Invoke(result);
            }
        }, (Registererror) =>
        {
            if (OnPlayFabError != null)
            {
                //Report error result back to subscriber
                OnPlayFabError.Invoke(Registererror);

            }
        });
    }

    public void SilentlyAuthenticate(System.Action<LoginResult> callback = null)
    {
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CustomId = PlayFabSettings.DeviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = InfoRequestParams
        }, (result) =>
        {
            //Store Identity and session
            _playFabId = result.PlayFabId;
            _sessionTicket = result.SessionTicket;

            //check if we want to get this callback directly or send to event subscribers.
            if (callback == null && OnLoginSuccess != null)
            {
                //report login result back to the subscriber
                OnLoginSuccess.Invoke(result);
            }
            else if (callback != null)
            {
                //report login result back to the caller
                callback.Invoke(result);
            }
        }, (error) =>
        {
            LogMessage("PlayFab authenticating using Custom ID...");
            //report errro back to the subscriber
            if (callback == null && OnPlayFabError != null)
            {
                OnPlayFabError.Invoke(error);
            }
            else
            {
                //make sure the loop completes, callback with null
                callback.Invoke(null);
                //Output what went wrong to the console.
                Debug.LogError(error.GenerateErrorReport());
            }

        });
    }





    public void LogMessage(string message)
    {
        Debug.Log("PlayFab + Photon Example: " + message);
    }
}
