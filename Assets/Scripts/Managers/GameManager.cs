using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon;
using Photon.Realtime;

namespace Complete
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public int m_NumRoundsToWin = 3;            // The number of rounds a single player has to win to win the game.
        public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases.
        public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases.
        public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.
        public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc.
        public GameObject m_TankPrefab;             // Reference to the prefab the players will control.
        public TankManager[] m_Tanks;               // A collection of managers for enabling and disabling different aspects of the tanks.
        public GameObject loadingPanel;
        public Text stateText;


        public GameObject scorePanel;
        public GameObject[] Players;

        private int m_RoundNumber;                  // Which round the game is currently on.
        private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts.
        private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends.
        private TankManager m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won.
        private TankManager m_GameWinner;           // Reference to the winner of the game.  Used to make an announcement of who won.

        private List<Transform> transforms;
        private Transform transform;

        private int otherViewID;
        private int localTankNum;
        private int loadedPlayerNum;
        private int roomPlayerCount;

        private int tankListNum;

        private bool OtherTankStateLeft;

        private void Start()
        {
            roomPlayerCount = PhotonNetwork.PlayerList.Length;
            localTankNum = (int)PhotonNetwork.LocalPlayer.CustomProperties["PlayerNum"];
            Debug.Log(localTankNum);

            scorePanel.SetActive(false);
            loadingPanel.SetActive(true);
            transforms = new List<Transform>();
            // Create the delays so they only have to be made once.
            m_StartWait = new WaitForSeconds(m_StartDelay);
            m_EndWait = new WaitForSeconds(m_EndDelay);


            SpawnTank();
            photonView.RPC("ConfirmLoad", RpcTarget.All);


            PhotonNetwork.IsMessageQueueRunning = true;
            //// Once the tanks have been created and the camera is using them as targets, start the game.

            StartCoroutine(GameLoop());

        }

        void Update()
        {
            stateText.text = PhotonNetwork.NetworkClientState.ToString();
            scorePanel.SetActive(Input.GetKey(KeyCode.Tab));

        }

        [PunRPC]
        void ConfirmLoad()
        {
            loadedPlayerNum++;
        }



        bool CheckPlayerConnected()
        {
            return (loadedPlayerNum == roomPlayerCount);
        }


        [PunRPC]
        public void updateViewID(int viewID, int tankNum)
        {

            m_Tanks[tankNum].m_Instance = PhotonView.Find(viewID).gameObject;
            m_Tanks[tankNum].m_PlayerNumber = tankNum;
            m_Tanks[tankNum].m_PlayerNickName = PhotonView.Find(viewID).Owner.NickName;
            m_Tanks[tankNum].Setup();
            m_Tanks[tankNum].m_Movement = null;
            m_Tanks[tankNum].m_Shooting = null;

            var tr = PhotonView.Find(viewID).gameObject.transform;
            transforms.Add(tr);
            SetCameraTarget();
            //SetCameraTargets();
        }

        private void SpawnTank()
        {
            m_Tanks[localTankNum].m_Instance = PhotonNetwork.Instantiate(
                         m_TankPrefab.name,
                         m_Tanks[localTankNum].m_SpawnPoint.position,
                         m_Tanks[localTankNum].m_SpawnPoint.rotation
                         ) as GameObject;

            m_Tanks[localTankNum].m_Instance.name = PhotonNetwork.IsMasterClient ? "MasterTank" : "Tank2";
            m_Tanks[localTankNum].m_PlayerNumber = localTankNum + 1;

            m_Tanks[localTankNum].m_PlayerNickName = PhotonNetwork.NickName;

            m_Tanks[localTankNum].Setup();
            transforms.Add(m_Tanks[localTankNum].m_Instance.transform);
            transform = m_Tanks[localTankNum].m_Instance.transform;
            photonView.RPC("updateViewID", RpcTarget.Others, m_Tanks[localTankNum].m_Instance.GetPhotonView().ViewID, localTankNum);
            Debug.Log("SpawnTank");

        }





        //private void SetCameraTargets()
        //{
        //    m_CameraControl.m_Targets = transforms.ToArray();
        //}

        private void SetCameraTarget()
        {
            m_CameraControl.m_Target = transform;
        }




        // This is called from start and will run each phase of the game one after another.
        private IEnumerator GameLoop()
        {

            // Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundStarting());



            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished.
            yield return StartCoroutine(RoundEnding());

            // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found.
            if (m_GameWinner != null)
            {
                // If there is a game winner, restart the level.
                LeaveRoom();
            }
            else
            {
                // If there isn't a winner yet, restart this coroutine so the loop continues.
                // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end.
                StartCoroutine(GameLoop());
            }
        }


        private IEnumerator RoundStarting()
        {
            while (!CheckPlayerConnected())
            {
                // ... return on the next frame.
                yield return null;
            }
            // As soon as the round starts reset the tanks and make sure they can't move.
            ResetLocalTanks();
            DisableTankControl();

            // Snap the camera's zoom and position to something appropriate for the reset tanks.
            //m_CameraControl.SetStartPositionAndSize();

            // Increment the round number and display text showing the players what round it is.
            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;
            for (int i = 0; i < 4; i++)
            {
                Players[i].SetActive(false);
            }
            for (int i = 0; i < roomPlayerCount; i++)
            {
                Players[i].SetActive(true);
                Players[i].GetComponentsInChildren<Image>()[0].color = new Color(m_Tanks[i].m_PlayerColor.r, 
                    m_Tanks[i].m_PlayerColor.g, 
                    m_Tanks[i].m_PlayerColor.b,
                    1);

                Debug.Log(i);
                Debug.Log(Players[i].GetComponentsInChildren<Image>()[0].color);
                Players[i].GetComponentsInChildren<Text>()[0].text ="Name:  "+ m_Tanks[i].m_PlayerNickName;
            }
            loadingPanel.SetActive(false);


            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_StartWait;
        }


        private IEnumerator RoundPlaying()
        {
            // As soon as the round begins playing let the players control the tanks.
            EnableTankControl();

            // Clear the text from the screen.
            m_MessageText.text = string.Empty;

            // While there is not one tank left...
            while (!LocalTankLeft())
            {
                // ... return on the next frame.
                yield return null;
            }
        }


        private IEnumerator RoundEnding()
        {
            // Stop tanks from moving.
            DisableTankControl();

            // Clear the winner from the previous round.
            m_RoundWinner = null;

            // See if there is a winner now the round is over.
            m_RoundWinner = GetRoundWinner();

            // If there is a winner, increment their score.
            if (m_RoundWinner != null)
                m_RoundWinner.m_Wins++;

            // Now the winner's score has been incremented, see if someone has one the game.
            m_GameWinner = GetGameWinner();

            // Get a message based on the scores and whether or not there is a game winner and display it.
            string message = EndMessage();
            m_MessageText.text = message;
            //for (int i = 0; i < roomPlayerCount; i++)
            //{

            //    Destroy(m_Tanks[i].m_Instance);
            //}
            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_EndWait;
        }


        // This is used to check if there is one or fewer tanks remaining and thus the round should end.
        private bool LocalTankLeft()
        {
            // Start the count of tanks left at zero.
            int numTanksLeft = 0;

            // Go through all the tanks...

            for (int i = 0; i < roomPlayerCount; i++)
            {
                // ... and if they are active, increment the counter.

                if (m_Tanks[i].m_Instance.activeSelf)

                    numTanksLeft++;

            }

            // If there are one or fewer tanks remaining return true, otherwise return false.
            return numTanksLeft <= 1;
        }


        private TankManager GetRoundWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < roomPlayerCount; i++)
            {
                // ... and if one of them is active, it is the winner so return it.
                if (m_Tanks[i].m_Instance.activeSelf)
                    return m_Tanks[i];
            }

            // If none of the tanks are active it is a draw so return null.
            return null;
        }


        // This function is to find out if there is a winner of the game.
        private TankManager GetGameWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < roomPlayerCount; i++)
            {
                // ... and if one of them has enough rounds to win the game, return it.
                if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                    return m_Tanks[i];
            }

            // If no tanks have enough rounds to win, return null.
            return null;
        }


        // Returns a string message to display at the end of each round.
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw.
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that.
            if (m_RoundWinner != null)
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            // Add some line breaks after the initial message.
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message.
            for (int i = 0; i < roomPlayerCount; i++)
            {
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
                Players[i].GetComponentsInChildren<Text>()[1].text = "Score: " + m_Tanks[i].m_Wins;

            }

            if (m_GameWinner != null)
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }


        // This function is used to turn all the tanks back on and reset their positions and properties.
        private void ResetLocalTanks()
        {
            for (int i = 0; i < roomPlayerCount; i++)
            {
                m_Tanks[i].Reset();
            }
        }


        private void EnableTankControl()
        {
            //for (int i = 0; i < roomPlayerCount; i++)
            //{
            m_Tanks[localTankNum].EnableControl();
            //}
        }

        public void DisableTankControl()
        {
            for (int i = 0; i < roomPlayerCount; i++)
            {
                m_Tanks[localTankNum].DisableControl();
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            LeaveGame();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            LeaveRoom();
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel("Launch");
        }
        public void LeaveGame()
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            PhotonNetwork.LoadLevel("Launch");
        }
    }
}