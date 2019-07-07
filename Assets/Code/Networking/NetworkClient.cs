using System;
using System.Collections.Generic;
using UnityEngine;
using Project.Utility;
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

        private Dictionary<string, NetworkIdentity> serverObjects;
        private SocketManager manager;
        public static string ClientID { get; private set; }
        private string host;
        
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
            // if (Debug.isDebugBuild) {
            //     host = string.Format("http://localhost:{0}/socket.io/", port);
            // } else {
                host = "https://young-gorge-52676.herokuapp.com";
            // }
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
        }

        void OnSpawn(Socket socket, Packet packet, params object[] args) {
            var data = args[0] as Dictionary<string, object>;
            string id = data["id"] as string;
            string publicId = data["publicId"] as string;

            if (publicId == "host") {
                Ball go = Instantiate(ball, networkContainer);
                go.name = "Host";
                NetworkIdentity ni = go.GetComponent<NetworkIdentity>();
                ni.SetControllerID(id);
                ni.SetSocketRef(manager.Socket);

                serverObjects.Add(id, ni);
            } else {
                // Second param sets the parent of the transform
                // same as go.transform.SetParent(networkContainer)
                Paddle go = Instantiate(paddle, networkContainer);
                go.Init(publicId == "player-1");
                go.name = string.Format("Player ({0})", id);
                NetworkIdentity ni = go.GetComponent<NetworkIdentity>();
                ni.SetControllerID(id);
                ni.SetSocketRef(manager.Socket);

                serverObjects.Add(id, ni);
            }
            // this is super janky, just brute forcing start game after player enters
            if (publicId == "player-1") {
                GameManager.gameStart = true;
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
        // https://buildplease.com/pages/json/
        // https://assetstore.unity.com/packages/tools/input-management/json-net-for-unity-11347
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

        // void OnServerSpawn(Socket socket, Packet packet, params object[] args) {
        //     var data = args[0] as Dictionary<string, object>;
        //     string id = data["id"] as string;
        //     string name = data["name"] as string;
        //     var position = data["position"] as Dictionary<string, float>;
        //     float x = position["x"];
        //     float y = position["y"];
        //     Debug.LogFormat("Server wants us to spawn a '{0}'", name);

        //     if (!serverObjects.ContainsKey(id)) {
        //         ServerObjectData sod = serverSpawnables.GetObjectByName(name);
        //         var spawnedObject = Instantiate(sod.Prefab, networkContainer);
        //         spawnedObject.transform.position = new Vector3(x, y, 0);
        //         NetworkIdentity ni = spawnedObject.GetComponent<NetworkIdentity>();
        //         ni.SetControllerID(id);
        //         ni.SetSocketRef(manager.Socket);

        //         if (name == "Ball") {
        //             const float ROTATION_OFFSET = 90;
        //             var direction = data["direction"] as Dictionary<string, float>;
        //             float directionX = direction["x"];
        //             float directionY = direction["y"];

        //             float rot = Mathf.Atan2(directionY, directionX) * Mathf.Rad2Deg;
        //             Vector3 currentRotation = new Vector3(0, 0, rot - ROTATION_OFFSET);
        //             spawnedObject.transform.rotation = Quaternion.Euler(currentRotation);
        //         }

        //         serverObjects.Add(id, ni);
        //     }
        // }

        // void OnServerUnspawn(Socket socket, Packet packet, params object[] args) {
        //     var data = args[0] as Dictionary<string, object>;
        //     string id = data["id"] as string;
        //     NetworkIdentity ni = serverObjects[id];
        //     serverObjects.Remove(id);
        //     DestroyImmediate(ni.gameObject);
        // }
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
