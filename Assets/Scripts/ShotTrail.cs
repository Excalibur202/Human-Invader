using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotTrail : MonoBehaviour
{
    public Material material;
    public LineRenderer line;
    public float interpolationTime = 0.3f;
    public bool ready = true;
    private float interpolation = 0f;
    private Vector3[] positions;
    private Material _mat;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.enabled = false;

        _mat = new Material(material);
        line.material = _mat;
    }

    void Update()
    {
        if (ready)
            return;

        interpolation += Time.deltaTime / interpolationTime;
        _mat.SetFloat("_Delta", interpolation);

        if (interpolation > 1f)
            Stop();
    }
    
    public void Setup(Vector3 startPoint, Vector3 endPoint)
    {
        interpolation = 0f;
        ready = false;
        positions = new Vector3[] { startPoint, endPoint};
        line.SetPositions(positions);
        line.enabled = true;
        _mat.SetFloat("_Delta", 0);
    }

    private void Stop()
    {
        ready = true;
        line.enabled = false;
        _mat.SetFloat("_Delta", 0);
    }
}
