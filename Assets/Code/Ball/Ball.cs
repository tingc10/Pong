using Pong.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pong {
    public class Ball : MonoBehaviour
    {
        [SerializeField]
        float speed;
        float radius;
        Vector2 direction;
        
        [Header("Class References")]
        [SerializeField]
        private NetworkIdentity networkIdentity;
        // Start is called before the first frame update
        void Start()
        {
            radius = transform.localScale.x/2;
            ResetPositionAndDirection();
        }

        // Update is called once per frame
        void Update()
        {
            if (networkIdentity.IsControlling() && GameManager.gameStart) {
            
                transform.Translate(speed * direction * Time.deltaTime);

                if ((transform.position.y < GameManager.bottomLeft.y + radius && direction.y < 0) ||
                    (transform.position.y > GameManager.topRight.y - radius && direction.y > 0)) {
                    direction.y = -direction.y;
                    // Each time the ball collides with the paddle, the ball will speed up
                }

                if (transform.position.x < GameManager.bottomLeft.x + radius && direction.x < 0) {
                    Debug.Log("Player 2 wins!");
                    EndGame();
                }

                if (transform.position.x > GameManager.topRight.x + radius && direction.x > 0) {
                    Debug.Log("Player 1 wins!");
                    EndGame();
                }
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.tag == "Paddle") {
                Debug.Log("collide");
                // bool isRight = other.GetComponent<Paddle>().isRight;
                direction.x = -direction.x;
                speed = speed * 1.05f;
            }
        }

        void EndGame() {
            enabled = false;
            networkIdentity.GetSocket().Emit("gameOver");
        }

        void ResetPositionAndDirection() {
            float x = Random.Range(-10, 10);
            direction = (new Vector2(x, x/3)).normalized;
            transform.position = new Vector2(0, 0);
        }

        public void ResetGame() {
            if (networkIdentity.IsControlling()) {
                ResetPositionAndDirection();
                enabled = true;
            }
        }
    }
}
