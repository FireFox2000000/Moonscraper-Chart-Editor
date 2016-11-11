using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class StrikelineController : MonoBehaviour {

    public AudioClip clap;
    AudioSource source;
    MovementController movement;

    float lastClapPos = -1;

    void Start()
    {
        source = GetComponent<AudioSource>();
        movement = Camera.main.GetComponent<MovementController>();
    }

    void OnTriggerEnter2D (Collider2D col)
    {
        if (movement.movementMode == MovementController.MovementMode.Playing && col.transform.position.y != lastClapPos)
        {
            source.PlayOneShot(clap);

            lastClapPos = col.transform.position.y;
        }
    }
}
