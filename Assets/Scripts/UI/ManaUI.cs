using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ManaUI : MonoBehaviour
{
    [Header("Orb Slot")]
    public CanvasGroup orbCanvasGroup;
    public float fadeInDuration = 0.5f;

    [Header("Charge Count (optional)")]
    public TextMeshProUGUI chargeCountText; // shows "x2" when stacked; hide when 1

    [Header("Unlock Popup")]
    public GameObject unlockPopup;
    public float popupDuration = 2f;

    [Header("Spell")]
    public GameObject spellPrefab;
    public float spellSpeed = 7f;
    public float spellMaxDistance = 12f;
    public float stunDuration = 5f;

    void Start()
    {
        if (orbCanvasGroup != null)
        {
            orbCanvasGroup.alpha = 0f;
            orbCanvasGroup.interactable = false;
            orbCanvasGroup.blocksRaycasts = false;
        }
        if (unlockPopup != null) unlockPopup.SetActive(false);

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnManaChanged += OnManaChanged;
            // Sync to current value (e.g. returning from another scene with charges saved)
            OnManaChanged(GameStateManager.Instance.GetManaCharges());
        }
    }

    void OnDisable()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnManaChanged -= OnManaChanged;
    }

    void Update()
    {
        bool hasMana = GameStateManager.Instance != null && GameStateManager.Instance.GetManaCharges() > 0;
        if (hasMana && Input.GetKeyDown(KeyCode.Space))
            CastSpell();
    }

    // Called by EmberNPC via CollectMana()
    public void CollectMana()
    {
        int current = GameStateManager.Instance?.GetManaCharges() ?? 0;
        GameStateManager.Instance?.AddMana();

        // Only show popup if this is the first charge
        if (current == 0)
            StartCoroutine(ShowUnlockPopup());
    }

    void OnManaChanged(int charges)
    {
        if (charges > 0)
        {
            if (orbCanvasGroup != null && orbCanvasGroup.alpha < 1f)
                StartCoroutine(FadeInOrb());

            if (chargeCountText != null)
            {
                chargeCountText.gameObject.SetActive(charges > 1);
                chargeCountText.text = $"x{charges}";
            }
        }
        else
        {
            StartCoroutine(FadeOutOrb());
            if (chargeCountText != null)
                chargeCountText.gameObject.SetActive(false);
        }
    }

    IEnumerator ShowUnlockPopup()
    {
        if (unlockPopup == null) yield break;
        unlockPopup.SetActive(true);
        yield return new WaitForSeconds(popupDuration);
        unlockPopup.SetActive(false);
    }

    IEnumerator FadeInOrb()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            if (orbCanvasGroup != null) orbCanvasGroup.alpha = elapsed / fadeInDuration;
            yield return null;
        }
        if (orbCanvasGroup != null)
        {
            orbCanvasGroup.alpha = 1f;
            orbCanvasGroup.interactable = true;
            orbCanvasGroup.blocksRaycasts = true;
        }
    }

    IEnumerator FadeOutOrb()
    {
        if (orbCanvasGroup != null)
        {
            orbCanvasGroup.interactable = false;
            orbCanvasGroup.blocksRaycasts = false;
        }
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            if (orbCanvasGroup != null) orbCanvasGroup.alpha = 1f - (elapsed / fadeInDuration);
            yield return null;
        }
        if (orbCanvasGroup != null) orbCanvasGroup.alpha = 0f;
    }

    void CastSpell()
    {
        EnemyPatrol nearest = FindNearestEnemy();
        if (nearest == null) return;
        if (spellPrefab == null) return;

        GameObject siera = GameObject.FindGameObjectWithTag("Player");
        if (siera == null) return;

        Vector3 direction = (nearest.transform.position - siera.transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject spell = Instantiate(spellPrefab, siera.transform.position, Quaternion.Euler(0, 0, angle));
        SpellProjectile proj = spell.AddComponent<SpellProjectile>();
        proj.Init(nearest.transform, direction, spellSpeed, spellMaxDistance, stunDuration);

        GameStateManager.Instance?.UseMana();
        EventSystem.current?.SetSelectedGameObject(null);
    }

    EnemyPatrol FindNearestEnemy()
    {
        GameObject siera = GameObject.FindGameObjectWithTag("Player");
        if (siera == null) return null;

        EnemyPatrol[] enemies = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
        EnemyPatrol nearest = null;
        float nearestDist = float.MaxValue;

        foreach (EnemyPatrol enemy in enemies)
        {
            float dist = Vector2.Distance(siera.transform.position, enemy.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }
}
