using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pong.Networking {
    // Ensure that NetworkIdentity exists before this instance is created
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkBallTransform : MonoBehaviour
    {
        [SerializeField]
        private Vector3 oldPosition;

        private NetworkIdentity networkIdentity;
        private Host ball;

        private float stillCounter = 0;
        // Start is called before the first frame update
        void Start()
        {
            networkIdentity = GetComponent<NetworkIdentity>();
            oldPosition = transform.position;
            ball = new Host();
            ball.position = new Position();
            ball.position.x = 0;
            ball.position.y = 0;
            ball.id = networkIdentity.publicId;

            if (!networkIdentity.IsControlling()) {
                enabled = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (networkIdentity.IsControlling()) {
                if (oldPosition != transform.position) {
                    oldPosition = transform.position;
                    stillCounter = 0;
                    sendData();
                } else {
                    // Don't need to keep updating server with position data
                    // If nothing has changed, although should
                    // reupdate the server now and then to notify that player is still
                    // at the same location
                    stillCounter += Time.deltaTime;

                    if (stillCounter >= 1) {
                        stillCounter = 0;
                        sendData();
                    }
                }
            }
        }

        private void sendData() {
            // Don't need positional data with lots of decimal, this limits to 3 decimal place
            ball.position.x = Mathf.Round(transform.position.x * 1000.0f) / 1000.0f;
            ball.position.y = Mathf.Round(transform.position.y * 1000.0f) / 1000.0f;
            // TODO: Wtf. JsonUtility doesn't respect 3 decimal representation...
            networkIdentity.GetSocket().Emit("updatePosition", JsonUtility.ToJson(ball));
        }
    }
}
