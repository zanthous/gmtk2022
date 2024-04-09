using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timescale : MonoBehaviour
{
    public float timescale = 1.0f;

    private void Update()
    {
        Time.timeScale = timescale;
    }
}
