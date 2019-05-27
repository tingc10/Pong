using Pong;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=1oY--Zk9b6w
public class GameManager : MonoBehaviour
{
    public Paddle paddle;
    public Ball ball;

    public static Vector2 bottomLeft;
    public static Vector2 topRight;
    
    // Start is called before the first frame update
    void Start()
    {
        // Convert screen's pixel coordinate into the game's coordinates
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector2(0, 0));
        topRight = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        // Instantiate(ball);
        // Paddle paddle1 = Instantiate(paddle) as Paddle;
        // Paddle paddle2 = Instantiate(paddle) as Paddle;

        // paddle1.Init(true);
        // paddle2.Init(false);
    }
}
