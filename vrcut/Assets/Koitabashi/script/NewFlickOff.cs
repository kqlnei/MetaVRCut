using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewFlickOff : MonoBehaviour
{
    private Animator anim;
    //private Rigidbody rbPare;
    private Transform parentTransform;
    public float forceAmount = 10f;
    public float rotationSpeed = 60f;

    private Knockback knockbackScript;

    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        parentTransform = gameObject.GetComponentInParent<Transform>();
        knockbackScript = transform.parent.GetComponent<Knockback>();
    }

    // Update is called once per frame
    void Update()
    {
        if (anim.enabled == false)
        {
            // X²•ûŒü‚É‰ñ“]‚³‚¹‚é
            float rotationAmount = -rotationSpeed * Time.deltaTime;  // 1ƒtƒŒ[ƒ€‚Å‰ñ“]‚·‚éŠp“x
            transform.Rotate(rotationAmount, 0f, 0f);  // X²‚É‰ñ“]‚ğ‰Á‚¦‚é

            // ‰ñ“]Šp“x‚ğ0`360“x‚É§ŒÀ‚·‚é
            float currentRotationX = transform.eulerAngles.x;
            if (currentRotationX > 360f)
            {
                currentRotationX -= 360f;
            }

            // ‰ñ“]Šp“x‚ğ0‚©‚ç360“x‚É•Û‚Â
            transform.eulerAngles = new Vector3(Mathf.Repeat(currentRotationX, 360f), transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(SetAnimOnWithDelay(0.75f)); // 0.5•b‚Ì’x‰„‚ğw’è
        }

        if (other.CompareTag("katana"))
        {
            knockbackScript.ApplyKnockback();
            anim.enabled = false;

            Rigidbody rb = this.GetComponent<Rigidbody>();
            rb.mass = 1;
            rb.useGravity = true;

            Vector3 knockbackDirection = new Vector3(0, 1, 1); // •ûŒü‚ğ³‹K‰»
            rb.AddForce(knockbackDirection * forceAmount, ForceMode.Impulse);
            parentTransform.position += new Vector3(0, 0, 1) * forceAmount * Time.deltaTime;
        }
    }

    // Coroutine‚ğg‚Á‚Ä’x‰„‚ğ’Ç‰Á
    private IEnumerator SetAnimOnWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        anim.SetBool("AnimOn", true);
    }


}
