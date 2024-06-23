/*using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;*/
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;

    void Update()
    {
        //if (!GetComponent<NetworkEntity>().IsLocalPlayer) return;

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        float3 movement = new float3(moveHorizontal, 0.0f, moveVertical);
        transform.Translate(movement * Time.deltaTime * speed);
    }
}
