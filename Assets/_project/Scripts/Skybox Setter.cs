using System.Collections.Generic;
using UnityEngine;

public class SkyboxSetter : MonoBehaviour
{
    [SerializeField] private List<Material> _skyboxMaterials;

    private void OnEnable()
    {
        ChangeSkybox(0);
    }

    public void ChangeSkybox(int skybox)
    {
        if (skybox >= 0 && skybox < _skyboxMaterials.Count)
        {
            RenderSettings.skybox = _skyboxMaterials[skybox];
        }
    }
}