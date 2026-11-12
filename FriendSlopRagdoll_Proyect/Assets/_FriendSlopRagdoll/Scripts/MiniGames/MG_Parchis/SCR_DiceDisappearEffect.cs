using System.Collections;
using UnityEngine;

public class DiceDisappearEffect : MonoBehaviour
{
    [Header("Suelo")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Tiempo")]
    [SerializeField] private float waitAfterLanding = 4f;
    [SerializeField] private float movementThreshold = 0.08f;

    [Header("Desaparición")]
    [SerializeField] private float shrinkDuration = 0.4f;
    [SerializeField] private float shakeAmount = 0.04f;
    [SerializeField] private float shakeSpeed = 35f;

    [Header("Efecto POF")]
    [SerializeField] private ParticleSystem poofEffect;

    private Rigidbody rb;
    private Vector3 originalScale;
    private bool hasLanded;
    private bool disappearing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer.value) != 0)
        {
            hasLanded = true;
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForLanding());
    }

    private IEnumerator WaitForLanding()
    {
        yield return new WaitUntil(() =>
            hasLanded &&
            rb.linearVelocity.sqrMagnitude < movementThreshold * movementThreshold
        );

        yield return new WaitForSeconds(waitAfterLanding);

        StartCoroutine(Disappear());
    }

    private IEnumerator Disappear()
    {
        if (disappearing)
            yield break;

        disappearing = true;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 originalPosition = transform.position;
        Quaternion originalRotation = transform.rotation;

        // Primera fase: temblor
        float shakeDuration = 0.7f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float shakeX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float shakeZ = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeAmount;

            transform.position = originalPosition +
                new Vector3(shakeX, 0f, shakeZ);

            transform.rotation = originalRotation *
                Quaternion.Euler(shakeX * 100f, 0f, shakeZ * 100f);

            yield return null;
        }

        // Segunda fase: encogimiento
        elapsed = 0f;

        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / shrinkDuration;

            transform.localScale = Vector3.Lerp(
                originalScale,
                Vector3.zero,
                progress
            );

            yield return null;
        }

        if (poofEffect != null)
        {
            poofEffect.transform.SetParent(null, true);
            poofEffect.Play(true);

            float duration = poofEffect.main.duration +
                             poofEffect.main.startLifetime.constantMax;

            Destroy(poofEffect.gameObject, duration);
        }

        Destroy(gameObject);
    }
}