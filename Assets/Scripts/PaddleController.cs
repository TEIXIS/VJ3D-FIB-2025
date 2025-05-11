using UnityEngine;

public class PaddleController : MonoBehaviour
{
    public float speed = 10f;
    public float limitX = 5f;

    void Update()
    {
        float h = Input.GetAxis("Horizontal")*-1; // <-1 a +1
        Vector3 pos = transform.position;
        pos.x += h * speed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, -limitX, +limitX);
        transform.position = pos;
    }
}
