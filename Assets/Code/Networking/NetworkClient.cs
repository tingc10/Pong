using System;
using System.Collections.Generic;
using UnityEngine;
using Project.Utility;
using BestHTTP.SocketIO;

// Tutorial from https://www.youtube.com/watch?v=J0udhTJwR88
namespace Pong.Networking {
    public class NetworkClient : MonoBehaviour {
        [Header("Network Client")]
        [SerializeField]
        private Transform networkContainer;
        private Dictionary<string, NetworkIdentity> serverObjects;
        private SocketManager manager;
        [SerializeField]
        private Paddle paddle;
        public static string ClientID { get; private set; }
        
        // Start is called before the first frame update
        public void Start() {
            initialize();
            setupEvents();

            // The argument will be an Error object.
            manager.Socket.On(SocketIOEventTypes.Error, (socket, packet, args) => Debug.LogError(string.Format("Error: {0}", args[0].ToString())));
            // We set SocketOptions' AutoConnect to false, so we have to call it manually.
            manager.Open();
        }

        // Update is called once per frame
        void Update() {
        }

        private void initialize() {
            // Initialize socket to connect with server
            int port = 52300;
            SocketOptions options = new SocketOptions();
            options.AutoConnect = false;
            options.ConnectWith = BestHTTP.SocketIO.Transports.TransportTypes.WebSocket;
            manager = new SocketManager(new System.Uri(string.Format("http://localhost:{0}/socket.io/", port)));

            serverObjects = new Dictionary<string, NetworkIdentity>();
        }

        private void setupEvents() {
            manager.Socket.On("open", OnOpen);
            manager.Socket.On("register", OnRegister);
            manager.Socket.On("spawn", OnSpawn);
            manager.Socket.On("disconnected", OnDisconnected);
            manager.Socket.On("updatePosition", OnUpdatePosition);
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

            // Second param sets the parent of the transform
            // same as go.transform.SetParent(networkContainer)
            Paddle go = Instantiate(paddle, networkContainer);
            go.Init(publicId == "player-1");
            go.name = string.Format("Player ({0})", id);
            NetworkIdentity ni = go.GetComponent<NetworkIdentity>();
            ni.SetControllerID(id);
            ni.SetSocketRef(manager);

            serverObjects.Add(id, ni);
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
            var position = data["position"] as Dictionary<string, float>;
            // float x = Convert.ToSingle(position["x"]);
            // float y = Convert.ToSingle(position["y"]);
            float x = position["x"];
            float y = position["y"];

            NetworkIdentity ni = serverObjects[id];
            ni.transform.position = new Vector3(x, y, 0);
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
}
