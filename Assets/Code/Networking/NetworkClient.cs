using System.Collections;
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
        private Dictionary<string, GameObject> serverObjects;

        private SocketManager Manager;
        
        // Start is called before the first frame update
        public void Start() {
            SocketOptions options = new SocketOptions();
            options.AutoConnect = false;
            options.ConnectWith = BestHTTP.SocketIO.Transports.TransportTypes.WebSocket;
            Manager = new SocketManager(new System.Uri("http://localhost:52300/socket.io/"));
            initialize();
            setupEvents();
            // The argument will be an Error object.
            Manager.Socket.On(SocketIOEventTypes.Error, (socket, packet, args) => Debug.LogError(string.Format("Error: {0}", args[0].ToString())));
            // We set SocketOptions' AutoConnect to false, so we have to call it manually.
            Manager.Open();
        }

        // Update is called once per frame
        void Update() {
        }

        private void initialize() {
            serverObjects = new Dictionary<string, GameObject>();
        }

        private void setupEvents() {
            Manager.Socket.On("open", OnOpen);
            Manager.Socket.On("register", OnRegister);
            Manager.Socket.On("spawn", OnSpawn);
            Manager.Socket.On("disconnected", OnDisconnected);
        }

        void OnOpen(Socket socket, Packet packet, params object[] args) {
            Debug.Log("Connection was made to server!");
        }

        void OnRegister(Socket socket, Packet packet, params object[] args){
            var data = args[0] as Dictionary<string, object>;
            string id = data["id"] as string;

            Debug.LogFormat("Our Client's ID ({0})",id);
        }

        void OnSpawn(Socket socket, Packet packet, params object[] args) {
            var data = args[0] as Dictionary<string, object>;
            string id = data["id"] as string;

            GameObject go = new GameObject("Server ID: " + id);
            go.transform.SetParent(networkContainer);

            serverObjects.Add(id, go);
        }

        void OnDisconnected(Socket socket, Packet packet, params object[] args) {
            var data = args[0] as Dictionary<string, object>;
            string id = data["id"].ToString().RemoveQuotes();
            GameObject go = serverObjects[id];
            // Remove the gameobject
            Destroy(go);
            // Remove from memory
            serverObjects.Remove(id);
        }
    }
}
