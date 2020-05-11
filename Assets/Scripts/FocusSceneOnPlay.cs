using UnityEngine;
public class FocusSceneOnPlay : MonoBehaviour {
    // Start is called before the first frame update
    void Start () {
        UnityEditor.SceneView.FocusWindowIfItsOpen (typeof (UnityEditor.SceneView));
    }
}