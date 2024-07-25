using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutoutObject : MonoBehaviour
{
    [SerializeField] Transform tarjetObject;
    [SerializeField] LayerMask layerMask;
    Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GetComponent<Camera>();
    }

    RaycastHit[] hitObjects;
    // Update is called once per frame
    void Update()
    {
        Vector2 cutoutPos = mainCamera.WorldToViewportPoint(tarjetObject.position);
        cutoutPos.y /= (Screen.width / Screen.height);

        Vector3 offset = tarjetObject.position - transform.position;
        hitObjects = Physics.RaycastAll(transform.position, offset, offset.magnitude, layerMask);

        for (int i = 0; i < hitObjects.Length; i++)
        {
            Material[] materials = hitObjects[i].transform.GetComponent<Renderer>().materials;

            for (int j = 0; j < materials.Length; j++)
            {
                float cameraWallDist = Vector3.Distance(mainCamera.transform.position, hitObjects[i].point);
                float cameraPlayerDist = Vector3.Distance(tarjetObject.position, mainCamera.transform.position);
                float normalized = 1 - cameraWallDist / cameraPlayerDist;
                materials[j].SetVector("_Cutout_Position", cutoutPos);
                materials[j].SetFloat("_Cutout_Size", normalized);
                materials[j].SetFloat("_Falloff_Size", 0.1f);
            }
        }
    }
}
