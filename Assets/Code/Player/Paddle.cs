using Pong.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pong {
    public class Paddle : MonoBehaviour
    {
        [SerializeField]
        float speed;
        
        float height;

        string input;

        public bool isRight;
        [Header("Class References")]
        [SerializeField]
        private NetworkIdentity networkIdentity;
        
        // Start is called before the first frame update
        void Start()
        {
            height = transform.localScale.y;
        }
        
        public void Init(bool isRightPaddle)
        {
            Vector2 pos;
            if (isRightPaddle) {
                pos = new Vector2(GameManager.topRight.x, 0);
                pos -= Vector2.right * transform.localScale.x;
                input = "PaddleRight";
            } else {
                pos = new Vector2(GameManager.bottomLeft.x, 0);
                pos += Vector2.right * transform.localScale.x;
                input = "PaddleLeft";
            }
            if (networkIdentity.IsControlling()) {
                UpdateCamera(isRightPaddle);
            }
            transform.position = pos;
            transform.name = input;
            isRight = isRightPaddle;
        }

        void UpdateCamera(bool rotateLeft) {
            if (rotateLeft) {
                Camera.main.transform.Rotate(0,0,90);
            } else {
                Camera.main.transform.Rotate(0,0,-90);
            }
            Camera.main.fieldOfView = 11;
        }

        // Update is called once per frame
        void Update()
        {
            if (networkIdentity.IsControlling()) {
                float move = Input.GetAxis(input) * speed * Time.deltaTime;

                if (transform.position.y < GameManager.bottomLeft.y + height/2 && move < 0) {
                    move = 0;
                }

                if (transform.position.y > GameManager.topRight.y - height/2 && move > 0) {
                    move = 0;
                }

                transform.Translate(move * Vector2.up);
            }
        }
    }

}
