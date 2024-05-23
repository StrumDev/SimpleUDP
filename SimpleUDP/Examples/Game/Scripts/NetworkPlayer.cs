using SimpleUDP;
using UnityEngine;

namespace SimpleUDP.Examples
{
    public class NetworkPlayer : MonoBehaviour
    {
        public float Speed = 8f;
        public float RotateSpeed = 180f;

        public uint ClientId { get; set; }
        public bool IsLocal { get; set; }
        
        public bool IsDestroy { get; set; }

        private void Update()
        {
            if (IsLocal && Input.GetKeyDown(KeyCode.R))
                transform.position = Vector3.zero;
        }

        private void FixedUpdate()
        {
            if (IsLocal && !IsDestroy)
            {
                Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

                transform.Translate(0, 0, input.y * Speed * Time.fixedDeltaTime);
                transform.Rotate(0, input.x * RotateSpeed * Time.fixedDeltaTime, 0);

                NetworkManager.Client.SendReliable(GetTransform());
            }

            if (IsDestroy)
                Destroy(gameObject);
        }

        public void SetTransform(Packet packet)
        {
            transform.position = new Vector3(packet.Float(), packet.Float(), packet.Float());
            transform.rotation = Quaternion.Euler(packet.Float(), packet.Float(), packet.Float());
        }

        private Packet GetTransform()
        {
            Packet packet = Packet.Write(32);
            packet.Byte((byte)Header.Movement);
            packet.UInt(ClientId);
            
            packet.Float(transform.position.x);
            packet.Float(transform.position.y);
            packet.Float(transform.position.z);

            packet.Float(transform.rotation.eulerAngles.x);
            packet.Float(transform.rotation.eulerAngles.y);
            packet.Float(transform.rotation.eulerAngles.z);

            return packet;
        }
    }    
}