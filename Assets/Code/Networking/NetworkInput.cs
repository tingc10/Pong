using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pong.Networking {
    public class NetworkInput : MonoBehaviour
    {
        [Header("Helpful Values")]
        [SerializeField]
        private string id;
        private NetworkIdentity networkIdentity;
        private float inputCounter = 0;
        public float horizontalInput = 0;

        void Start()
        {
            networkIdentity = GetComponent<NetworkIdentity>();

            // if (!networkIdentity.IsControlling()) {
            //     enabled = false;
            // }
        }

        // Update is called once per frame
        void Update()
        {
            if (networkIdentity.IsControlling()) {
                // Don't need to keep updating server with position data
                // If nothing has changed, although should
                // reupdate the server now and then to notify that player is still
                // at the same location
                inputCounter += Time.deltaTime;

                if (inputCounter >= 1) {
                    inputCounter = 0;
                    ResetInputs();
                }
            }
        }

        public void SetID(string ID) {
            id = ID;
        }

        public void SetInput(float xInput) {
            if (networkIdentity.IsControlling()) {
                horizontalInput = xInput;
                inputCounter = 0;
            }
        }

        void ResetInputs() {
            horizontalInput = 0;
        }

        public bool HasAssociatedController() {
            return id != null;
        }
    }
}
