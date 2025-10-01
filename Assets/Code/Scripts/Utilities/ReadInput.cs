using UnityEngine;

public class ReadInput : MonoBehaviour
{
    [SerializeField] private Transform _camera;
    [SerializeField] private float _speed = 2;

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            _camera.transform.position += new Vector3(0, 0, 1) * Time.deltaTime * _speed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            _camera.transform.position += new Vector3(0, 0, -1) * Time.deltaTime * _speed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            _camera.transform.position += new Vector3(-1, 0, 0) * Time.deltaTime * _speed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            _camera.transform.position += new Vector3(1, 0, 0) * Time.deltaTime * _speed;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            _camera.transform.position += new Vector3(0, -1, 0) * Time.deltaTime * _speed;
        }
        if (Input.GetKey(KeyCode.E))
        {
            _camera.transform.position += new Vector3(0, 1, 0) * Time.deltaTime * _speed;
        }
    }
}
