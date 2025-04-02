using UnityEngine;

public class animOn : MonoBehaviour
{
    [SerializeField] private Animator anim;

    private void Start()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
        if (anim == null)
        {
            Debug.LogWarning("Animator component is missing on " + gameObject.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && anim != null)
        {
            anim.SetBool("AnimOn", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && anim != null)
        {
            anim.SetBool("AnimOn", false);
        }
    }
}