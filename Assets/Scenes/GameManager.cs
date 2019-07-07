using Pong;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=1oY--Zk9b6w
namespace Pong {
    public class GameManager : MonoBehaviour
    {
        public static GameObject panel;

        public static Vector2 bottomLeft;
        public static Vector2 topRight;
        public static bool gameStart;
        // Start is called before the first frame update
        void Start()
        {
            panel = GameObject.Find("Panel");
            Vector2 size = panel.GetComponent<BoxCollider2D>().bounds.size;
            Debug.Log(size);
            // Convert screen's pixel coordinate into the game's coordinates
            bottomLeft = size/-2;
            topRight = size/2;
            // Camera.main.transform.rotation = Quaternion.Euler(0,0,90);
            gameStart = false;
        }
    }

}
