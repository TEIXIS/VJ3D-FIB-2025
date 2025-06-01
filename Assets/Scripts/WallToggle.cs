using UnityEngine;

public class WallToggle : MonoBehaviour
{
    public GameObject backWall;
    public GameObject Arceus;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (backWall != null)
                backWall.SetActive(!backWall.activeSelf);
            if( Arceus != null)
                Arceus.SetActive(!Arceus.activeSelf);
        }
    }
}
