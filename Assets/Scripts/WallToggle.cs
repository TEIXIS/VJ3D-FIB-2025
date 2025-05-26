using UnityEngine;

public class WallToggle : MonoBehaviour
{
    public GameObject backWall;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (backWall != null)
                backWall.SetActive(!backWall.activeSelf);
        }
    }
}
