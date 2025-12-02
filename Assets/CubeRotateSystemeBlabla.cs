using UnityEngine;

public class RubiksInput : MonoBehaviour
{
    public RubiksManager manager;
    public Camera cam;
    public LayerMask stickerLayer;

    private bool isDragging = false;
    private Vector3 startMousePos;
    private Vector3 hitNormal;
    private Transform hitTransform;

    void Update()
    {
        if (manager == null || !manager.CanRotate()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, stickerLayer))
            {
                isDragging = true;
                startMousePos = Input.mousePosition;
                hitNormal = hit.normal; // La normale de la face touchée (ex: Vector3.up)
                hitTransform = hit.transform;
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Vector3 dragDelta = Input.mousePosition - startMousePos;

            // Seuil minimum pour valider un drag
            if (dragDelta.magnitude > 20f)
            {
                HandleInput(dragDelta);
            }
            isDragging = false;
        }
    }

    void HandleInput(Vector3 dragDelta)
    {
        // 1. Convertir le mouvement souris (2D) en direction 3D approximative par rapport à la caméra
        // Si je bouge la souris horizontalement (X), c'est souvent "Right" pour la caméra
        Vector3 moveDir = (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y)) ?
                          cam.transform.right : cam.transform.up;

        // Si le drag est inversé (gauche ou bas), on garde le signe pour plus tard
        float sign = (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y)) ?
                     Mathf.Sign(dragDelta.x) : Mathf.Sign(dragDelta.y);

        // 2. Produit vectoriel pour trouver l'axe de rotation
        // Mathématiquement : Axe = NormaleFace X DirectionMouvement
        Vector3 rotationAxis = Vector3.Cross(hitNormal, moveDir).normalized;

        // 3. Nettoyer l'axe (on veut un axe pur : 1,0,0 ou 0,1,0 etc)
        rotationAxis = SnapToAxis(rotationAxis);

        // Si l'axe est nul (ex: on drag dans le sens de la normale), on annule
        if (rotationAxis == Vector3.zero) return;

        // 4. Trouver quelle tranche tourner
        // On demande au manager de tourner la tranche où se trouve le sticker touché
        // On doit trouver l'index (-1, 0, 1) sur l'axe concerné.
        float sliceIndex = 0;

        // Comme le manager utilise les positions locales, on convertit la pos du sticker en local au Manager
        Vector3 localPos = manager.transform.InverseTransformPoint(hitTransform.position);

        if (Mathf.Abs(rotationAxis.x) > 0.9f) sliceIndex = Mathf.Round(localPos.x / manager.spacing);
        if (Mathf.Abs(rotationAxis.y) > 0.9f) sliceIndex = Mathf.Round(localPos.y / manager.spacing);
        if (Mathf.Abs(rotationAxis.z) > 0.9f) sliceIndex = Mathf.Round(localPos.z / manager.spacing);

        // Calcul du sens (Clockwise ou pas)
        // C'est souvent la partie la plus dure à régler, dépend du repère.
        // Une astuce est de tester le sens par rapport au produit vectoriel brut.
        bool clockwise = (sign > 0);

        // Correction de signe selon la face et la caméra (empirique)
        float dotCam = Vector3.Dot(cam.transform.forward, hitNormal);
        if (dotCam > 0) clockwise = !clockwise; // Si on regarde la face arrière

        manager.RotateFace(rotationAxis, sliceIndex, clockwise);
    }

    Vector3 SnapToAxis(Vector3 v)
    {
        // Retourne l'axe dominant (X, Y ou Z)
        float x = Mathf.Abs(v.x);
        float y = Mathf.Abs(v.y);
        float z = Mathf.Abs(v.z);

        if (x > y && x > z) return new Vector3(Mathf.Sign(v.x), 0, 0);
        if (y > x && y > z) return new Vector3(0, Mathf.Sign(v.y), 0);
        if (z > x && z > y) return new Vector3(0, 0, Mathf.Sign(v.z));
        return Vector3.zero;
    }
}