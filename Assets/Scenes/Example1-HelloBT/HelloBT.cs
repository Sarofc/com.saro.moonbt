using UnityEngine;
using System.Collections;
using Saro.BT;

public class HelloBT : MonoBehaviour
{
    private Root m_Root;

    // Use this for initialization
    void Start()
    {
        m_Root = new Root().Decorate(
            new Log("Hello BT~")
        );

        m_Root.RepeatRoot = false;

        m_Root.Start();
    }
}
