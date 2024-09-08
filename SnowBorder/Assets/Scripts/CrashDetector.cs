using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrashDetector : MonoBehaviour
{
    [SerializeField] float delay = 0.5f;
    [SerializeField] ParticleSystem finishEffect;
    [SerializeField] AudioClip crashSFX;

    bool hasCrashed = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Ground" && !hasCrashed)
        {
            hasCrashed = true;
            FindObjectOfType<PlayerController>().DisableControls();
            finishEffect.Play();
            GetComponent<AudioSource>().PlayOneShot(crashSFX);
            Invoke("ReloadScene", delay);
        }
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }
}
