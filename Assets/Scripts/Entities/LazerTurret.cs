using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazerTurret : Tile
{
    struct LazerTurretState
    {
        public (int, int) collisionLocation;
        public bool lazerActive;
    }

    [SerializeField] private GameObject lazer;

    //private Vector3 beamStartPosition;
    private (int, int) collisionLocation;
    private bool lazerActive = false;
    private Vector3 initialLazerLocalScale;
    private Vector3 initialLazerLocalPosition;

    //since there is no tick at the start of the game, and the lazer playing is after a tick, I had to simulate a tick
    //I should do something to improve this (ex. game wide initial/start tick or just tick at game start)
    private float tickTimer = 0.0f;
    private float transitionDuration = Game.normalDuration;
    private bool initialStartFinished = false;

    private Stack<LazerTurretState> lazerTurretStates = new Stack<LazerTurretState>();

    private void Start()
    {
        initialLazerLocalScale = lazer.transform.localScale;
        initialLazerLocalPosition = lazer.transform.localPosition;
        LazerStuff(game.ActivePlayer.Face);
        AddCurrentState();
    }

    //lazer will probably have to redraw on random ticks too
    public override void Tick(int dieFace, Direction direction)
    {
        LazerStuff(dieFace);
    }

    private void LazerStuff(int dieFace)
    {
        lazerActive = DetermineFaceMatch(dieFace);
        if(lazerActive)
        {
            lazer.SetActive(true);
            //calculate lazer path 
            collisionLocation = game.GetCollision(Pos, pointingDirection, (int?)EntityID.player);
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

    public override void LateTick(int dieFace)
    {
        AddCurrentState();
        if(!lazerActive) return;
        var hitsPlayer = HitsPlayer();
        if(hitsPlayer)
        {
            Player.DieEvent.Invoke();
        }
    }

    public override void Undo()
    {
        lazerTurretStates.Pop();
        this.lazerActive = lazerTurretStates.Peek().lazerActive;
        this.collisionLocation= lazerTurretStates.Peek().collisionLocation;

        if(!lazerActive)
        { 
            lazer.transform.localPosition = initialLazerLocalPosition;
            lazer.transform.localScale = initialLazerLocalScale;
            lazer.SetActive(false);
        }
        else
        {
            lazer.SetActive(true);

            StartCoroutine(ExtendLazer());
        }
        
    }

    private IEnumerator ExtendLazer()
    {
        var vectorToCollision = (Pos.Item1 - collisionLocation.Item1, Pos.Item2 - collisionLocation.Item2);

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
        switch(pointingDirection)
        {
            case Direction.none:
                return false;
            case Direction.up:
                if(collisionLocation.Item1 == game.ActivePlayer.Pos.Item1 &&
                    collisionLocation.Item2 > game.ActivePlayer.Pos.Item2 && 
                    game.ActivePlayer.Pos.Item2 > this.Pos.Item2)
                {
                    return true;
                }
                return false;
            case Direction.right:
                if(collisionLocation.Item2 == game.ActivePlayer.Pos.Item2 &&
                    collisionLocation.Item1 > game.ActivePlayer.Pos.Item1 &&
                    game.ActivePlayer.Pos.Item1 > this.Pos.Item1)
                {
                    return true;
                }
                return false;
            case Direction.down:
                if(collisionLocation.Item1 == game.ActivePlayer.Pos.Item1 &&
                    collisionLocation.Item2 < game.ActivePlayer.Pos.Item2 &&
                    game.ActivePlayer.Pos.Item2 < this.Pos.Item2)
                {
                    return true;
                }
                return false;
            case Direction.left:
                if(collisionLocation.Item2 == game.ActivePlayer.Pos.Item2 &&
                    collisionLocation.Item1 < game.ActivePlayer.Pos.Item1 &&
                    game.ActivePlayer.Pos.Item1 < this.Pos.Item1)
                {
                    return true;
                }
                return false;
        }
        return false;
    }

    public override void AddCurrentState()
    {
        LazerTurretState lazerTurretState = new LazerTurretState();
        lazerTurretState.collisionLocation = collisionLocation;
        lazerTurretState.lazerActive = lazerActive;
        lazerTurretStates.Push(lazerTurretState);
    }
}