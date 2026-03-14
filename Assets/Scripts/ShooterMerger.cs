using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class ShooterMerger
{
    public static async UniTask<Shooter> TryMergeShooters(Color colour, ShooterDock shooterDock, ShooterManager shooterManager)
    {
        if (shooterDock.TryGetMergeableShooters(colour, out List<Shooter> mergeableShooters))
        {
            return await PlayMergeAnimation(mergeableShooters, shooterDock, shooterManager);
        }
        else
        {
            return null;
        }
    }

    // Merges three shooters
    private static async UniTask<Shooter> PlayMergeAnimation(List<Shooter> mergeableShooters, ShooterDock shooterDock, ShooterManager shooterManager)
    {
        var midIndex = 1;
        var centreGridNode = mergeableShooters[midIndex].GridNode;
        var totalAmmo = 0;
        var midPosition = mergeableShooters[midIndex].transform.position;
        shooterManager.PlayStartMergeSFX();
        for (int i = 0; i < mergeableShooters.Count; i++)
        {
            var shooter = mergeableShooters[i];
            shooter.StopAttack();
            shooter.HideAmmoCount();
            totalAmmo += shooter.CurrentAmmo;

            if (i != midIndex)
            {
                shooterDock.RemoveShooter(mergeableShooters[i]);
                shooter.transform
                .DOMove(midPosition, 0.35f).SetEase(Ease.OutSine)
                .OnComplete(() => shooter.gameObject.SetActive(false));
            }
            else
            {
                shooter.gameObject.SetActive(false);
            }
        }
        await UniTask.Delay(360);

        shooterManager.PlayEndMergeSFX();
        for (int i = 0; i < mergeableShooters.Count; i++)
        {
            shooterManager.RemoveShooter(mergeableShooters[i]);
            shooterDock.RemoveShooter(mergeableShooters[i]);
        }
        
        return shooterManager.AddShooter(centreGridNode, mergeableShooters[0].Colour, totalAmmo, true);
    }
}
