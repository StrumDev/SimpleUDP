using UnityEngine;

public class Player : MonoBehaviour
{
    public float Speed = 8f;
    public float RotateSpeed = 180f;

    public uint ClientId { get; set; }
    public bool IsLocal { get; set; }

    private void Update()
    {
        if (IsLocal && Input.GetKeyDown(KeyCode.R))
            transform.position = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (!IsLocal)
            return;

        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        transform.Translate(0, 0, input.y * Speed * Time.fixedDeltaTime);
        transform.Rotate(0, input.x * RotateSpeed * Time.fixedDeltaTime, 0);
    }

    public void SetPosition(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }

    public Packet GetTransform()
    {
        Packet packet = new Packet();
        packet.AddByte((byte)Header.Transform);
        packet.AddUInt(ClientId);
        
        packet.AddFloat(transform.position.x);
        packet.AddFloat(transform.position.y);
        packet.AddFloat(transform.position.z);

        packet.AddFloat(transform.rotation.x);
        packet.AddFloat(transform.rotation.y);
        packet.AddFloat(transform.rotation.z);
        packet.AddFloat(transform.rotation.w);

        return packet;
    }
}
