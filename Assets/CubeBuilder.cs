using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RubiksManager : MonoBehaviour
{
    [Header("Paramètres")]
    public GameObject cubiePrefab;
    public float spacing = 1.05f; // Espace entre les cubes
    public float rotationSpeed = 500f;
    public int shuffleMoves = 20;

    private List<Transform> allCubies = new List<Transform>();
    private bool isRotating = false; // Empêche de lancer 2 rotations en même temps

    void Start()
    {
        BuildCube();
        StartCoroutine(ShuffleRoutine());
    }

    void BuildCube()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    GameObject go = Instantiate(cubiePrefab, transform);
                    go.transform.localPosition = new Vector3(x * spacing, y * spacing, z * spacing);

                    // Ajout des stickers (version simplifiée via code ou prefab)
                    // Assure-toi que ton prefab a bien les stickers ou utilise ton ancienne méthode AddStickers ici

                    allCubies.Add(go.transform);
                }
            }
        }
    }

    public bool CanRotate() { return !isRotating; }

    // Appelé par le script d'input ou de mélange
    public void RotateFace(Vector3 axis, float sliceIndex, bool clockwise = true)
    {
        if (isRotating) return;
        StartCoroutine(RotateRoutine(axis, sliceIndex, clockwise ? 90f : -90f));
    }

    private IEnumerator RotateRoutine(Vector3 axis, float sliceCoordinate, float angle)
    {
        isRotating = true;

        // 1. Trouver les cubies concernés
        List<Transform> sliceCubies = new List<Transform>();
        Transform pivot = new GameObject("Pivot").transform;
        pivot.SetParent(transform);
        pivot.localPosition = Vector3.zero;
        pivot.localRotation = Quaternion.identity;

        foreach (Transform t in allCubies)
        {
            // On vérifie la position locale par rapport à l'axe demandé
            float posOnAxis = Vector3.Dot(t.localPosition, axis);

            // On utilise une petite marge d'erreur (epsilon) pour comparer les float
            if (Mathf.Abs(posOnAxis - (sliceCoordinate * spacing)) < 0.1f)
            {
                sliceCubies.Add(t);
                t.SetParent(pivot); // Attacher temporairement au pivot
            }
        }

        // 2. Animer la rotation
        float currentAngle = 0f;
        while (Mathf.Abs(currentAngle) < Mathf.Abs(angle))
        {
            float step = Time.deltaTime * rotationSpeed * Mathf.Sign(angle);
            if (Mathf.Abs(currentAngle + step) > Mathf.Abs(angle))
                step = angle - currentAngle; // Finir exactement sur l'angle

            pivot.Rotate(axis, step, Space.Self);
            currentAngle += step;
            yield return null;
        }

        // 3. Nettoyage et Snap (CRUCIAL pour éviter les bugs)
        foreach (Transform t in sliceCubies)
        {
            t.SetParent(transform);
            // On force les positions à être des multiples parfaits de 'spacing'
            t.localPosition = new Vector3(
                Mathf.Round(t.localPosition.x / spacing) * spacing,
                Mathf.Round(t.localPosition.y / spacing) * spacing,
                Mathf.Round(t.localPosition.z / spacing) * spacing
            );

            // Idem pour la rotation, on force à 90° près
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

    private IEnumerator ShuffleRoutine()
    {
        // Petite pause avant de commencer
        yield return new WaitForSeconds(1f);

        Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward };
        float[] slices = { -1f, 0f, 1f }; // Gauche/Milieu/Droite

        for (int i = 0; i < shuffleMoves; i++)
        {
            Vector3 ax = axes[Random.Range(0, axes.Length)];
            float sl = slices[Random.Range(0, slices.Length)];
            bool dir = Random.value > 0.5f;

            // On lance la rotation et on attend qu'elle finisse
            yield return StartCoroutine(RotateRoutine(ax, sl, dir ? 90f : -90f));
        }
    }
}