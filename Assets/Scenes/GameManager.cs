using Pong;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=1oY--Zk9b6w
namespace Pong {
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        public GameObject playArea;
        public static Vector2 bottomLeft;
        public static Vector2 topRight;
        public static bool gameStart;
        // Start is called before the first frame update
        void Start()
        {
            Vector2 playAreaSize = playArea.transform.localScale;
            topRight = playAreaSize / 2;
            bottomLeft = playAreaSize / -2;
            
            gameStart = false;
        }
    }

}
