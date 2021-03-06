﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO;

namespace Pong.Networking {
    public class NetworkIdentity : MonoBehaviour
    {
        [Header("Helpful Values")]
        [SerializeField]
        private string id;
        [SerializeField]
        private bool isControlling;
        private Socket socket;
        public string publicId;

        // Start is called before the first frame update
        public void Awake()
        {
            isControlling = false;
        }

        // Update is called once per frame
        public void SetControllerID(string ID)
        {
            id = ID;
            // Check incoming id against the one saved from the server
            isControlling = (NetworkClient.ClientID == ID) ? true : false;
        }

        public void SetSocketRef(Socket s) {
            socket = s;
        }

        public string GetID() {
            return id;
        }

        public bool IsControlling() {
            return isControlling;
        }

        public Socket GetSocket() {
            return socket;
        }
    }
}