using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazerTurret : Tile
{

    [SerializeField] private GameObject lazer;

    private Game game;
    private Vector3 beamStartPosition;

    private void Start()
    {
        beamStartPosition = lazer.transform.localPosition;
        game = FindObjectOfType<Game>();    
    }

    //lazer will probably have to redraw on random ticks too
    public override void Tick(int dieFace)
    {
        if(DetermineActive(dieFace))
        {
            lazer.SetActive(true);
            //calculate lazer path 
            var collisionLocation = game.GetCollision(position, direction);
            var vectorToCollision = (position.Item1 - collisionLocation.Item1, position.Item2 - collisionLocation.Item2);

            //one of them will always be 0 so just get the value stupidly
            float distance = Mathf.Abs(vectorToCollision.Item1 + vectorToCollision.Item2);
            distance /= 2.0f; //cylinders of 0.5f units are 1.0f tall or something
            lazer.transform.localScale = new Vector3(lazer.transform.localScale.x, distance, lazer.transform.localScale.z);
            lazer.transform.localPosition = new Vector3(0, transform.localPosition.y -1.0f, distance) + beamStartPosition;

            //play lazer animation
        }
        else
        {
            lazer.SetActive(false);
        }
    }
}
