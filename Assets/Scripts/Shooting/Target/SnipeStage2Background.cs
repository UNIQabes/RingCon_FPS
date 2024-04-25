using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;


public class SnipeStage2Background : ShootingTarget
{
    public SnipeStage2 snipeStage2;

    private void Start()
    {
        
    }

    private async UniTaskVoid UpdateAsync()
    {

    }


    public override void OnContact(ShootingContactData contactData)
    {
        snipeStage2.TransNextWave();
    }
}
