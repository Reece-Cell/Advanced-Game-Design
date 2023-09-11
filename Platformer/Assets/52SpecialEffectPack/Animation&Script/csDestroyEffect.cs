using UnityEngine;
using System.Collections;

public class csDestroyEffect : MonoBehaviour
{
    void Start()
    {
        // Start a coroutine to destroy the GameObject after 1 second
        StartCoroutine(DestroyAfterDelay(1.0f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(delay);

        // Destroy the GameObject
        Destroy(gameObject);
    }

    void Update()
    {
        // Check for input to destroy the GameObject immediately
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.C))
        {
            Destroy(gameObject);
        }
    }
}