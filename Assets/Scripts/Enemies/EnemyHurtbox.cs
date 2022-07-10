using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHurtbox : MonoBehaviour
{
    [SerializeField]
    private EnemyStatus enemy;

    
    // Public function to trigger when hitting hurtbox
    public void hurt(int dmg) {
        enemy.damage(dmg);
    }
}
