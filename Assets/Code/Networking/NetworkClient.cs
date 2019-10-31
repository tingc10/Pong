using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BestHTTP.SocketIO;
using Project.Scriptable;

// Tutorial from https://www.youtube.com/watch?v=J0udhTJwR88
namespace Pong.Networking {
    public class NetworkClient : MonoBehaviour {
        [Header("Network Client")]
        [SerializeField]
        private Transform networkContainer;
        [SerializeField]
        private Paddle paddle;
        [SerializeField]
        private Ball ball;
        [SerializeField]
        private ServerObjects serverSpawnables;
        [SerializeField]
        public bool useProdServer;
        
        [SerializeField]
        private GameObject localCameras;

        [SerializeField]
        private GameObject mainCamera;
        

        private Dictionary<string, NetworkIdentity> serverObjects;
        private SocketManager manager;
        public static string ClientID { get; private set; }
        private string host;

        private Ball ballHost;
        
        // Start is called before the first frame update
        public void Start() {
            initialize();
            setupEvents();
            // The argument will be an Error object.
            manager.Socket.On(SocketIOEventTypes.Error, (socket, packet, args) => Debug.LogError(string.Format("Error: {0}", args[0].ToString())));
            // We set SocketOptions' AutoConnect to false, so we have to call it manually.
            manager.Open();
        }

        private void initialize() {
            // Initialize socket to connect with server
            int port = 52300;
            SocketOptions options = new SocketOptions();
            options.AutoConnect = false;
            options.ConnectWith = BestHTTP.SocketIO.Transports.TransportTypes.WebSocket;
            if (!useProdServer && Debug.isDebugBuild) {
                host = string.Format("http://localhost:{0}/socket.io/", port);
            } else {
                host = "https://young-gorge-52676.herokuapp.com";
            }
            manager = new SocketManager(new System.Uri(string.Format("{0}/socket.io/", host)));

            serverObjects = new Dictionary<string, NetworkIdentity>();
        }

        private void setupEvents() {
            manager.Socket.On("open", OnOpen);
            manager.Socket.On("register", OnRegister);
            // Player triggered new objects
            manager.Socket.On("spawn", OnSpawn);
            manager.Socket.On("disconnected", OnDisconnected);
            manager.Socket.On("updatePosition", OnUpdatePosition);
            manager.Socket.On("registerController", OnRegisterController);
            manager.Socket.On("controllerInput", OnControllerInput);
            manager.Socket.On("startGame", OnStartGame);
            manager.Socket.On("restartGame", OnRestartGame);            
            // Server triggered new objects
            // manager.Socket.On("serverSpawn", OnServerSpawn);
            // manager.Socket.On("serverUnspawn", OnServerUnspawn);
        }

        void OnOpen(Socket socket, Packet packet, params object[] args) {
            Debug.Log("Connection was made to server!");
        }

        void OnRegister(Socket socket, Packet packet, params object[] args){
            var data = args[0] as Dictionary<string, object>;
            ClientID = data["id"] as string;

            Debug.LogFormat("Our Client's ID ({0})", ClientID);
            GameObject clientIdDisplay = GameObject.Find("ClientId");
            Text text = clientIdDisplay.GetComponent<Text>();
            text.text = ClientID;
        }

        void OnRegisterController(Socket socket, Packet packet, params object[] args){
            var data = args[0] as Dictionary<string, object>;
            string controllerId = data["id"] as string;
            string playerId = data["playerId"] as string;
            GameObject go = serverObjects[playerId].gameObject;
            NetworkInput ni = go.GetComponent<NetworkInput>();
            ni.SetID(controllerId);
        }

        void OnSpawn(Socket socket, Packet packet, params object[] args) {
            var data = args[0] as Dictionary<string, object>;
            string id = data["id"] as string;
            string clientId = data["clientId"] as string;
            string publicId = data["publicId"] as string;

            if (publicId == "host") {
                ballHost = Instantiate(ball, networkContainer);
                ballHost.name = "Host";
                NetworkIdentity ni = ballHost.GetComponent<NetworkIdentity>();
                ni.publicId = id;
                ni.SetControllerID(clientId);
                ni.SetSocketRef(manager.Socket);

                serverObjects.Add(id, ni);
            } else {
                // Second param sets the parent of the transform
                // same as go.transform.SetParent(networkContainer)
                Paddle go = Instantiate(paddle, networkContainer);
                bool isPlayer1 = publicId == "player-1";
                go.Init(isPlayer1);
                go.name = string.Format("Player ({0})", publicId);
                NetworkIdentity ni = go.GetComponent<NetworkIdentity>();
                ni.publicId = id;
                ni.SetControllerID(clientId);
                ni.SetSocketRef(manager.Socket);
                if (ni.IsControlling()) {
                    Debug.Log(isPlayer1);
                    UpdateCamera(isPlayer1);
                    RotateText(isPlayer1);
                }
                serverObjects.Add(id, ni);
            }
        }

        void OnRestartGame(Socket socket, Packet packet, params object[] args) {
            ballHost.ResetGame();
            GameManager.gameStart = true;
        }

        void UpdateCamera(bool rotateLeft) {
            if (rotateLeft) {
                Camera.main.transform.Rotate(0,0,90);
            } else {
                Camera.main.transform.Rotate(0,0,-90);
            }
            Camera.main.fieldOfView = 11;
        }

        void RotateText(bool rotateLeft) {
            GameObject clientIdDisplay = GameObject.Find("ClientId");
            RectTransform textTransform = clientIdDisplay.GetComponent<RectTransform>();
            if (rotateLeft) {
                textTransform.Rotate(0,0,90);
            } else {
                textTransform.Rotate(0,0,-90);
            }
        }

        void OnDisconnected(Socket socket, Packet packet, params object[] args) {
            var data = args[0] as Dictionary<string, object>;
            string id = data["id"] as string;
            GameObject go = serverObjects[id].gameObject;
            // Remove the gameobject
            Destroy(go);
            // Remove from memory
            serverObjects.Remove(id);
        }

        void OnUpdatePosition(Socket socket, Packet packet, params object[] args) {
            var data = args[0] as Dictionary<string, object>;
            string id = data["id"] as string;
            var position = data["position"] as Dictionary<string, object>;
            float x = Convert.ToSingle(position["x"]);
            float y = Convert.ToSingle(position["y"]);
            // float x = position["x"];
            // float y = position["y"];

            NetworkIdentity ni = serverObjects[id];
            ni.transform.position = new Vector3(x, y, 0);
        }

        void OnControllerInput(Socket socket, Packet packet, params object[] args) {
            var data = args[0] as Dictionary<string, object>;
            string playerId = data["id"] as string;
            float xInput = Convert.ToSingle(data["xInput"]);
            GameObject go = serverObjects[playerId].gameObject;
            NetworkInput ni = go.GetComponent<NetworkInput>();
            ni.SetInput(xInput);
        }

        void OnStartGame(Socket socket, Packet packet, params object[] args) {
            // var data = JsonUtility.ToJson(args[0]);
            var data = args[0] as Dictionary<string, object>;
            string isLocalGame = data["local"] as string;
            Debug.Log("start game");
            GameManager.gameStart = true;
            if (isLocalGame == "true") {
                localCameras.SetActive(true);
                mainCamera.SetActive(false);
            }
        }
    }
    // Make serializable so it can be sent over sockets
    [Serializable]
    public class Player {
        public string id;
        public Position position;
    }

    [Serializable]
    public class Position {
        public float x;
        public float y;
    }

    [Serializable]
    public class Direction {
        public float x;
        public float y;
    }

    [Serializable]
    public class Host {
        public string id;
        public Position position;
    }
}
