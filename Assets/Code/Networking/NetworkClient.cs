using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using Project.Utility;

// Tutorial from https://www.youtube.com/watch?v=J0udhTJwR88
namespace Pong.Networking {
    public class NetworkClient : SocketIOComponent {
        [Header("Network Client")]
        [SerializeField]
        private Transform networkContainer;
        private Dictionary<string, GameObject> serverObjects;
        
        // Start is called before the first frame update
        public override void Start() {
            base.Start();
            initialize();
            setupEvents();
        }

        // Update is called once per frame
        public override void Update() {
            base.Update();
        }

        private void initialize() {
            serverObjects = new Dictionary<string, GameObject>();
        }

        private void setupEvents() {
            On("open", (E) => {
                Debug.Log("Connection was made to server!");
            });

            On("register", (E) => {
                string id = E.data["id"].ToString().RemoveQuotes();

                Debug.LogFormat("Our Client's ID ({0})",id);
            });

            On("spawn", (E) => {
                string id = E.data["id"].ToString().RemoveQuotes();

                GameObject go = new GameObject("Server ID: " + id);
                go.transform.SetParent(networkContainer);

                serverObjects.Add(id, go);
            });

            On("disconnected", (E) => {
                string id = E.data["id"].ToString().RemoveQuotes();
                GameObject go = serverObjects[id];
                // Remove the gameobject
                Destroy(go);
                // Remove from memory
                serverObjects.Remove(id);
            });
        }

    }
}
