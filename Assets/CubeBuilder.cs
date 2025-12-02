using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Nécessaire pour les requêtes LINQ dans RotateRoutine

// Cette classe représentera chaque mini-cube


public class RubiksManager : MonoBehaviour
{
    [Header("Paramètres")]
    public GameObject cubiePrefab;
    [Tooltip("La moitié de la distance entre deux centres de pièces (ex: 0.5 * 2.1 = 1.05)")]
    public float spacing = 1.05f;
    public float rotationSpeed = 500f;
    public int shuffleMoves = 20;

    private List<Cubie> allCubies = new List<Cubie>();
    private bool isRotating = false;

    void Start()
    {
        BuildCube();
        StartCoroutine(ShuffleRoutine());
    }

    // --- CONSTRUCTION DU CUBE ---
    void BuildCube()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    // Ignorer la pièce centrale invisible
                    if (x == 0 && y == 0 && z == 0) continue;

                    GameObject go = Instantiate(cubiePrefab, transform);
                    Vector3 localPos = new Vector3(x * spacing, y * spacing, z * spacing);
                    go.transform.localPosition = localPos;

                    Cubie cubie = new Cubie
                    {
                        transform = go.transform,
                        initialPosition = localPos // On stocke la position initiale pour référence
                    };
                    allCubies.Add(cubie);
                }
            }
        }
    }

    public bool CanRotate() { return !isRotating; }

    // Fonction pour le slider (inchangée)
    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = Mathf.Clamp(newSpeed, 100f, 1500f);
    }

    // --- ROTATION ---

    // Simplifiée : l'axe est toujours un axe principal positif (X+, Y+, ou Z+)
    public void RotateFace(Vector3 axis, float sliceIndex, bool clockwise = true)
    {
        if (isRotating) return;

        // Si clockwise est VRAI, on veut l'horaire -> Angle NÉGATIF 
        float finalAngle = clockwise ? -90f : 90f;

        StartCoroutine(RotateRoutine(axis, sliceIndex, finalAngle));
    }

    private IEnumerator RotateRoutine(Vector3 axis, float sliceCoordinate, float angle)
    {
        isRotating = true;

        // 1. Trouver les cubies concernés
        List<Transform> sliceTransforms = new List<Transform>();
        Transform pivot = new GameObject("Pivot").transform;
        pivot.SetParent(transform);
        pivot.localPosition = Vector3.zero;
        pivot.localRotation = Quaternion.identity;

        // L'axe pour la vérification est déjà positif (X, Y ou Z)
        float targetCoord = sliceCoordinate * spacing;

        foreach (Cubie cubie in allCubies)
        {
            // On vérifie la position de la pièce dans l'espace local du Rubik's Manager.
            // Vector3.Dot projette la position locale sur l'axe de rotation.
            float posOnAxis = Vector3.Dot(cubie.transform.localPosition, axis);

            // On utilise une petite marge pour la comparaison
            if (Mathf.Abs(posOnAxis - targetCoord) < 0.1f)
            {
                sliceTransforms.Add(cubie.transform);
                cubie.transform.SetParent(pivot);
            }
        }

        // 2. Animer la rotation (inchangé)
        float currentAngle = 0f;
        while (Mathf.Abs(currentAngle) < Mathf.Abs(angle))
        {
            float step = Time.deltaTime * rotationSpeed * Mathf.Sign(angle);
            if (Mathf.Abs(currentAngle + step) > Mathf.Abs(angle))
                step = angle - currentAngle;

            pivot.Rotate(axis, step, Space.Self);
            currentAngle += step;
            yield return null;
        }

        // 3. Nettoyage et Snap (CRUCIAL)
        foreach (Transform t in sliceTransforms)
        {
            t.SetParent(transform);

            // On force les positions et rotations à des multiples parfaits (Snap)
            t.localPosition = new Vector3(
                Mathf.Round(t.localPosition.x / spacing) * spacing,
                Mathf.Round(t.localPosition.y / spacing) * spacing,
                Mathf.Round(t.localPosition.z / spacing) * spacing
            );

            Vector3 euler = t.localEulerAngles;
            t.localEulerAngles = new Vector3(
                Mathf.Round(euler.x / 90f) * 90f,
                Mathf.Round(euler.y / 90f) * 90f,
                Mathf.Round(euler.z / 90f) * 90f
            );
        }

        Destroy(pivot.gameObject);
        isRotating = false;
    }

    // --- MÉLANGE ---
    private IEnumerator ShuffleRoutine()
    {
        yield return new WaitForSeconds(1f);

        // On utilise les axes positifs pour la rotation
        Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward };
        // On choisit aléatoirement les tranches externes (1 ou -1)
        float[] indices = { 1f, -1f };

        for (int i = 0; i < shuffleMoves; i++)
        {
            Vector3 ax = axes[Random.Range(0, axes.Length)];
            float sliceIndex = indices[Random.Range(0, indices.Length)];
            bool dir = Random.value > 0.5f;

            // On utilise la rotation de face externe
            RotateFace(ax, sliceIndex, dir);

            // Attendre la fin de la rotation
            yield return new WaitUntil(() => !isRotating);
            yield return new WaitForSeconds(0.1f); // Petite pause
        }
    }
}

public class Cubie
{
    public Transform transform;
    public Vector3 initialPosition; // Position d'origine (utilisée pour la détection)
}