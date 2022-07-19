using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField]
    private AudioClip jumpSound;
    [SerializeField]
    private AudioClip damageSound;
    [SerializeField]
    private AudioClip dashSound;
    [SerializeField]
    private AudioClip healSound;
    [SerializeField]
    private AudioClip pauseSound;
    [SerializeField]
    private AudioClip enemyJumpSound;

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
    public void playJumpSound() {
        playClip(jumpSound);
    }

    public void playDashSound() {
        playClip(dashSound);
    }

    public void playHealSound() {
        playClip(healSound);
    }

    public void playDamageSound() {
        playClip(damageSound);
    }

    public void playEnemyJumpSound() {
        playClip(enemyJumpSound);
    }

    // Uses a specific sequence to make sure static doesn't go on forever
    public void playPauseSound() {
        StartCoroutine(pauseSoundSequence());
    }


    // Private pause sound sequence
    private IEnumerator pauseSoundSequence() {
        speaker.Stop();
        speaker.clip = pauseSound;
        speaker.Play();

        yield return new WaitForSeconds(0.3f);

        if (speaker.clip == pauseSound) {
            speaker.Stop();
        }
    }
}
