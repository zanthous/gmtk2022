using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : Tile
{
    private bool goalActive = false;

    private void Start()
    {
        Game.LateTickEvent += LateTick;
        game = FindObjectOfType<Game>();
    }

    private void OnDestroy()
    {
        Game.LateTickEvent -= LateTick;
        Game.Tick -= Tick;
    }

    public override void Tick(int dieFace)
    {
        goalActive = DetermineActive(dieFace);
    }

    private void LateTick(int dieFace)
    {
        if(game.ActivePlayer.Pos == position && (goalActive || activateNumber == -1) && gameObject.activeSelf) 
        {
            game.lockingObject = gameObject;
            Game.LevelCompleteEvent.Invoke();
        }
    }
}
