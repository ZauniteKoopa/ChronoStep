using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    [SerializeField]
    private AudioClip deathSound;
    [SerializeField]
    private AudioClip attackSound;

    private AudioSource speaker;


    // On awake, set up speaker
    private void Awake() {
        speaker = GetComponent<AudioSource>();
    }


    // Main private helper to do a simple play
    private void playClip(AudioClip clip) {
        speaker.Stop();
        speaker.clip = clip;
        speaker.Play();
    }


    // Main functions to play specific sounds
    public void playAttackSound() {
        playClip(attackSound);
    }

    public void playDeathSound() {
        playClip(deathSound);
    }
}
