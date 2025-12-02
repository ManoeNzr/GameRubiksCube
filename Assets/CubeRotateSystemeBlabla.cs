using UnityEngine;
using System.Linq;

public class RubiksInput : MonoBehaviour
{
    public RubiksManager manager;
    public Camera cam;
    public LayerMask stickerLayer;

    private bool dragging = false;
    private Vector3 startMouse;
    private Transform clickedFace;
    private Vector3 faceNormal;

    void Update()
    {
        if (!manager.CanRotate())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, stickerLayer))
            {
                dragging = true;
                startMouse = Input.mousePosition;
                clickedFace = hit.transform;
                faceNormal = hit.normal;
            }
        }

        if (Input.GetMouseButtonUp(0) && dragging)
        {
            dragging = false;

            Vector3 delta = Input.mousePosition - startMouse;
            if (delta.magnitude < 20f) return;

            ProcessSwipe(delta);
        }
    }

    void ProcessSwipe(Vector3 delta)
    {
        // 1. Initialisation des axes
        Vector3 localPos = manager.transform.InverseTransformPoint(clickedFace.position);
        Vector3 localNormal = manager.transform.InverseTransformDirection(faceNormal);
        localNormal = GetClosestMajorAxis(localNormal);

        // Trouver les deux axes dans le plan de la face cliquée
        Vector3 axisA = Vector3.Cross(localNormal, Vector3.up);
        if (axisA.sqrMagnitude < 0.01f) axisA = Vector3.Cross(localNormal, Vector3.right);
        axisA.Normalize();
        Vector3 axisB = Vector3.Cross(localNormal, axisA);

        // 2. Déterminer l'AXE de ROTATION (orthogonal au mouvement)
        Vector3 rotationAxisCandidate1 = Vector3.Cross(localNormal, axisA);
        Vector3 rotationAxisCandidate2 = Vector3.Cross(localNormal, axisB);

        // Projections écran
        Vector2 screenCenter = cam.WorldToScreenPoint(clickedFace.position);
        Vector2 screenAxisA = ((Vector2)cam.WorldToScreenPoint(clickedFace.position + manager.transform.TransformDirection(axisA)) - screenCenter).normalized;
        Vector2 screenAxisB = ((Vector2)cam.WorldToScreenPoint(clickedFace.position + manager.transform.TransformDirection(axisB)) - screenCenter).normalized;

        float dotA = Vector2.Dot(delta.normalized, screenAxisA);
        float dotB = Vector2.Dot(delta.normalized, screenAxisB);

        Vector3 detectedRotationAxis;
        if (Mathf.Abs(dotA) > Mathf.Abs(dotB))
        {
            detectedRotationAxis = rotationAxisCandidate1;
        }
        else
        {
            detectedRotationAxis = rotationAxisCandidate2;
        }

        // 3. Normaliser l'Axe de Rotation et déterminer la Tranche

        // L'axe final est toujours positif (X+, Y+, Z+)
        Vector3 finalRotationAxis = new Vector3(
            Mathf.Abs(detectedRotationAxis.x),
            Mathf.Abs(detectedRotationAxis.y),
            Mathf.Abs(detectedRotationAxis.z)
        );

        // L'index est la position du sticker cliqué le long de cet axe POSITIF
        float coord = Vector3.Dot(localPos, finalRotationAxis);
        float sliceIndex = Mathf.Round(coord / manager.spacing);

        // 4. Déterminer le SENS (Logique Conditionnelle Explicite pour le FIX)

        // 4a. Définir le vecteur de swipe qui donne une rotation Anti-Horaire (par rapport à l'axe positif)
        Vector3 antiClockwiseSwipeVector = Vector3.Cross(finalRotationAxis, localNormal);

        // 4b. Projeter ce vecteur sur l'écran et comparer le swipe
        Vector2 screenAntiClockwiseVector = ((Vector2)cam.WorldToScreenPoint(clickedFace.position + manager.transform.TransformDirection(antiClockwiseSwipeVector)) - screenCenter).normalized;

        // Détection de base : Si le swipe est aligné avec le vecteur anti-horaire, c'est anti-horaire.
        bool isAntiClockwiseBrut = Vector2.Dot(delta.normalized, screenAntiClockwiseVector) > 0;

        bool clockwise;

        // --- LOGIQUE CONDITIONNELLE MANUELLE (Fixe les 4 cas) ---

        if (Mathf.Abs(finalRotationAxis.y) > 0.9f) // Rotation autour de l'axe Y (tranches U/D)
        {
            // La tranche du Haut (U) est à sliceIndex = 1
            if (sliceIndex > 0) // Cas : Haut (U) -> Problème Inversé
            {
                // Si Haut (U) est inversé, on prend le sens brut anti-horaire
                clockwise = !isAntiClockwiseBrut;
            }
            else // Cas : Bas (D) -> Correct
            {
                // Si Bas (D) est correct, on inverse le sens brut
                clockwise = !isAntiClockwiseBrut;
            }
        }
        else if (Mathf.Abs(finalRotationAxis.x) > 0.9f) // Rotation autour de l'axe X (tranches R/L)
        {
            // La tranche de Droite (R) est à sliceIndex = 1
            if (sliceIndex > 0) // Cas : Droite (R) -> Correct
            {
                // Si Droite (R) est correct, on inverse le sens brut
                clockwise = !isAntiClockwiseBrut;
            }
            else // Cas : Gauche (L) -> Problème Inversé
            {
                // Si Gauche (L) est inversé, on prend le sens brut anti-horaire
                clockwise = !isAntiClockwiseBrut;
            }
        }
        else // Rotation autour de l'axe Z (tranches F/B)
        {
            // La tranche de Face (F) est à sliceIndex = 1
            if (sliceIndex > 0) // Cas : Face (F) -> On suppose Correct
            {
                clockwise = !isAntiClockwiseBrut;
            }
            else // Cas : Arrière (B) -> On suppose Inversé
            {
                clockwise = !isAntiClockwiseBrut;
            }
        }

        // 5. Action !
        manager.RotateFace(finalRotationAxis, sliceIndex, clockwise);
    }

    // Fonction GetClosestMajorAxis (inchangée)
    private Vector3 GetClosestMajorAxis(Vector3 inputAxis)
    {
        Vector3 localAxis = inputAxis;

        if (Mathf.Abs(localAxis.x) > Mathf.Abs(localAxis.y) && Mathf.Abs(localAxis.x) > Mathf.Abs(localAxis.z))
            return localAxis.x > 0 ? Vector3.right : Vector3.left;
        if (Mathf.Abs(localAxis.y) > Mathf.Abs(localAxis.z))
            return localAxis.y > 0 ? Vector3.up : Vector3.down;

        return localAxis.z > 0 ? Vector3.forward : Vector3.back;
    }
}