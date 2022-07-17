using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazerTurret : Tile
{

    [SerializeField] private GameObject lazer;

    //private Vector3 beamStartPosition;
    private (int, int) collisionLocation;
    private bool lazerActive = false;
    private Vector3 initialLazerLocalScale;
    private Vector3 initialLazerLocalPosition;

    private float tickTimer = 0.0f;
    private float transitionDuration = 0.25f;
    private bool initialStartFinished = false;

    private void Start()
    {
        //beamStartPosition = lazer.transform.localPosition;
        Game.LateTickEvent += LateTick;

        initialLazerLocalScale = lazer.transform.localScale;
        initialLazerLocalPosition = lazer.transform.localPosition;
        LazerStuff(game.ActivePlayer.Face);
    }

    private void OnDestroy()
    {
        Game.LateTickEvent -= LateTick;
        Game.Tick -= Tick;
    }

    //lazer will probably have to redraw on random ticks too
    public override void Tick(int dieFace)
    {
        LazerStuff(dieFace);
    }

    private void LazerStuff(int dieFace)
    {
        lazerActive = DetermineActive(dieFace);
        if(lazerActive)
        {
            lazer.SetActive(true);
            //calculate lazer path 
            collisionLocation = game.GetCollision(position, direction);
            //play lazer animation
            StartCoroutine(ExtendLazer());
        }
        else
        {
            lazer.transform.localPosition = initialLazerLocalPosition;
            lazer.transform.localScale = initialLazerLocalScale;
            lazer.SetActive(false);
        }
    }

    private void LateTick(int dieFace)
    {
        if(!lazerActive) return;
        var hitsPlayer = HitsPlayer();
        if(hitsPlayer)
        {
            Player.DieEvent.Invoke();
        }
    }

    private IEnumerator ExtendLazer()
    {
        var vectorToCollision = (position.Item1 - collisionLocation.Item1, position.Item2 - collisionLocation.Item2);

        //one of them will always be 0 so just get the value stupidly
        float distance = Mathf.Abs(vectorToCollision.Item1 + vectorToCollision.Item2);
        distance /= 2.0f; //cylinders of 0.5f units are 1.0f tall or something
        var targetLocalScale = new Vector3(lazer.transform.localScale.x, distance, lazer.transform.localScale.z);
        var targetLocalPosition = new Vector3(0, transform.localPosition.y - 1.0f, distance) + initialLazerLocalPosition;

        if(initialStartFinished)
        { 
            while(Game.tickTimer < Game.transitionDuration)
            {
                lazer.transform.localPosition = Vector3.Lerp(initialLazerLocalPosition, targetLocalPosition, Game.tickTimer / Game.transitionDuration);
                lazer.transform.localScale = Vector3.Lerp(initialLazerLocalScale, targetLocalScale, Game.tickTimer / Game.transitionDuration);

                yield return null;
            }
        }
        else
        {
            while(this.tickTimer < this.transitionDuration) 
            {
                this.tickTimer+= Time.deltaTime;
                lazer.transform.localPosition = Vector3.Lerp(initialLazerLocalPosition, targetLocalPosition, this.tickTimer / this.transitionDuration);
                lazer.transform.localScale = Vector3.Lerp(initialLazerLocalScale, targetLocalScale, this.tickTimer / this.transitionDuration);

                yield return null;
            }
            initialStartFinished = true;
        }
    }

    private bool HitsPlayer()
    {
        switch(direction)
        {
            case Direction.none:
                return false;
            case Direction.up:
                if(collisionLocation.Item1 == game.ActivePlayer.Pos.Item1 &&
                    collisionLocation.Item2 > game.ActivePlayer.Pos.Item2 && 
                    game.ActivePlayer.Pos.Item2 > this.position.Item2)
                {
                    return true;
                }
                return false;
            case Direction.right:
                if(collisionLocation.Item2 == game.ActivePlayer.Pos.Item2 &&
                    collisionLocation.Item1 > game.ActivePlayer.Pos.Item1 &&
                    game.ActivePlayer.Pos.Item1 > this.position.Item1)
                {
                    return true;
                }
                return false;
            case Direction.down:
                if(collisionLocation.Item1 == game.ActivePlayer.Pos.Item1 &&
                    collisionLocation.Item2 < game.ActivePlayer.Pos.Item2 &&
                    game.ActivePlayer.Pos.Item2 < this.position.Item2)
                {
                    return true;
                }
                return false;
            case Direction.left:
                if(collisionLocation.Item2 == game.ActivePlayer.Pos.Item2 &&
                    collisionLocation.Item1 < game.ActivePlayer.Pos.Item1 &&
                    game.ActivePlayer.Pos.Item1 < this.position.Item1)
                {
                    return true;
                }
                return false;
        }
        return false;
    }
}