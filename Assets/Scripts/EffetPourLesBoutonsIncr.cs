using UnityEngine;
using UnityEngine.EventSystems; // Nécessaire pour les interfaces de gestion d'événements
using System.Collections; // Nécessaire pour les Coroutines

// On implémente IPointerEnterHandler et IPointerExitHandler pour détecter le survol de la souris
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Paramètres d'animation")]
    [Tooltip("Taille cible lorsque la souris survole le bouton (ex: 1.1 pour 110%)")]
    public float targetScale = 1.1f;

    [Tooltip("Durée de l'animation de mise à l'échelle (en secondes)")]
    public float animationDuration = 0.15f;

    [Header("Effets Audio (Optionnel)")]
    public AudioSource audioSource;
    public AudioClip hoverSound;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    void Start()
    {
        // Enregistrer la taille de départ pour y revenir
        originalScale = transform.localScale;

        // S'assurer qu'un AudioSource est présent si un clip est défini
        if (hoverSound != null && audioSource == null)
        {
            // Tente d'obtenir un AudioSource sur l'objet ou un parent
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // Ajoute un AudioSource si non trouvé
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    // --- 1. DÉTECTION DU SURVOL ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Arrêter toute animation en cours
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);

        // Lancer l'animation d'agrandissement
        scaleCoroutine = StartCoroutine(ScaleButton(Vector3.one * targetScale));

        // Déclencher l'effet audio
        PlayHoverSound();

        // **Espace pour d'autres effets :**
        // Exemple : Changer la couleur du texte ou l'image
        // myTextComponent.color = Color.red; 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Arrêter toute animation en cours
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);

        // Lancer l'animation de retour à la taille initiale
        scaleCoroutine = StartCoroutine(ScaleButton(originalScale));
    }

    // --- 2. FONCTIONS DE MODULARITÉ ---

    void PlayHoverSound()
    {
        if (hoverSound != null && audioSource != null)
        {
            // Jouer le son si l'AudioSource n'est pas déjà en train de jouer un autre clip
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(hoverSound);
            }
        }
    }

    // Coroutine pour l'animation douce (Interpolation Linéaire - Lerp)
    IEnumerator ScaleButton(Vector3 target)
    {
        Vector3 startScale = transform.localScale;
        float time = 0;

        while (time < animationDuration)
        {
            // Calcul du ratio de progression
            float ratio = time / animationDuration;

            // Interpolation : Déplacer la taille de la taille de départ vers la taille cible
            transform.localScale = Vector3.Lerp(startScale, target, ratio);

            time += Time.unscaledDeltaTime; // Utiliser unscaledDeltaTime pour ne pas être affecté par Time.timeScale = 0 (pause)
            yield return null;
        }

        // S'assurer que la taille finale est atteinte
        transform.localScale = target;
    }
}