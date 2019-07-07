using Pong;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=1oY--Zk9b6w
namespace Pong {
    public class GameManager : MonoBehaviour
    {
        public Paddle paddle;
        public Ball ball;

        public static Vector2 bottomLeft;
        public static Vector2 topRight;
        public static bool gameStart;
        // Start is called before the first frame update
        void Start()
        {
            // Convert screen's pixel coordinate into the game's coordinates
            bottomLeft = Camera.main.ScreenToWorldPoint(new Vector2(0, 0));
            topRight = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            Camera.main.transform.rotation = Quaternion.Euler(0,0,90);
            gameStart = false;
        }
    }

}
