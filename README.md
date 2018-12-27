# Introducing
This repository is base on [Tanks! Tutorial](https://assetstore.unity.com/packages/essentials/tutorial-projects/tanks-tutorial-46209) and been redeveloped into a multiplayer game using the Unity with Playfab and Photon.


## Demo illustrates:
  * Accounts uses Device ID &/or Email Authentication
  * Example of how to create real-time game with Photon PUN
  * Photon networking implementation using PUN, with some examples of Views, Syncs and RPCs

## Configuration and Setup
### Prerequisites:
This project is a simple example that uses some essential functions of PlayFab and Photon.

- You should be familiar with Unity3d & UUI 
- Have a [basic understanding](https://api.playfab.com/) of the PlayFab API
- Be familiar with fundamental concepts on which Photon Unity Networking (PUN) is based, and you are already familiar with Photon API

### Back-end Setup:

#### Setting up the PlayFab Application
1. Go to the [Game Manager](https://developer.playfab.com/en-US/my-games) and click "Create a new game" 
2. Fill out the page for the new title. Click "Create Game". 
3. You should now see your new title appear in your game studio. If it's not there right away, wait a minute then refresh your browser since it makes take a few seconds to show up. Take note of the Title ID for your new title.

#### Setting up Photon Application
1. Go To: [Photon Engine](https://dashboard.photonengine.com/en-US/account/signin) Sign Up for a free account (or is the one you already have)
2. Go to your account page and setup a new app
3. Find and note the App Id of this app.

### Client Setup:
The next step is to get the client running on your own device, and communicating with your own back-end.

To compile yourself in Unity, you'll want to first download this entire Source Project onto your local PC.

1. The project is already set up to use [ PlayFab Editor Extension](https://github.com/PlayFab/UnityEditorExtensions) for Unity.  you need Login to your PlayFab account in the editor extension and select the title ID you set up previously. You can read more about the Editor Extensions on their GitHub repository page.
2. Add your appid from your created photon app to the PUN Wizard Window for setup Pun.

## More information:
For a complete list of available PlayFab APIs, check out the [online documentation](http://api.playfab.com/).

PlayFab Developer Team can assist with answering any questions as well as process any feedback you have about PlayFab services in Forums.
[Forums](https://community.playfab.com/index.html)

There is an [Online Documentation](http://doc.photonengine.com/en-us/pun/v2), which is considered a manual for PUN. This might become your primary source for information.
