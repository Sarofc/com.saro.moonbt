using UnityEngine;
using System.Collections;
using Saro.BT;
using System;

public class ServiceExample : MonoBehaviour
{
    private Root m_Root;

    private int count = 0;

    // Use this for initialization
    void Start()
    {
        m_Root = new Root().Decorate(
            new DefaultService(1f, .1f, () => ++count).Decorate(
                new Actions(null, () =>
                {
                    Debug.Log(count);
                    return null;
                })
            )
        );

        m_Root.RepeatRoot = false;

        m_Root.Start();
    }

}
