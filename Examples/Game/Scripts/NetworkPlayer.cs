using SimpleUDP;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Collections;

namespace SimpleUDP.Examples
{
    public class NetworkPlayer : MonoBehaviour
    {
        public float Speed = 8f;
        public float RotateSpeed = 180f;

        public uint ClientId { get; set; }
        public bool IsLocal { get; set; }
        
        private bool isDestroy;
        
        private bool moveToZero;
        private Vector3 newPosition;
        private Quaternion newQuaternion;
        private Packet packet = Packet.Write(32);

        private void Update()
        {
            float lerpSpeed = Speed * 1.3f;

            if (IsLocal)
            {
                if (!moveToZero)
                {
                    newPosition = transform.position;

                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        newPosition = Vector3.zero;
                        moveToZero = true;    
                    }
                }
                else
                {
                    if (Vector3.Distance(transform.position, Vector3.zero) <= 0.1f)
                        moveToZero = false;

                    transform.position = Vector3.Lerp(transform.position, Vector3.zero, lerpSpeed * Time.deltaTime);
                }

                return;
            }
            
            if (newPosition != transform.position)
                transform.position = Vector3.Lerp(transform.position, newPosition, lerpSpeed * Time.deltaTime);

            if (newQuaternion != transform.rotation)
                transform.rotation = Quaternion.Lerp(transform.rotation, newQuaternion, lerpSpeed * Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (IsLocal && !isDestroy && !moveToZero)
            {
                Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

                transform.Translate(0, 0, input.y * Speed * Time.fixedDeltaTime);
                transform.Rotate(0, input.x * RotateSpeed * Time.fixedDeltaTime, 0);
            }

            if (isDestroy)
                Destroy(gameObject);
        }

        public async void SendAsyncPosition()
        {
            while (IsLocal)
            {
                if (isDestroy || this == null) 
                    return;
                
                // 1000ms / 40 = TickRate: 25
                NetworkManager.Client.SendReliable(GetTransform());
                
                packet.Reset();
                await Task.Delay(40);
            }
        }

        public void SetTransform(Packet packet)
        {
            newPosition = new Vector3(packet.Float(), packet.Float(), packet.Float());
            newQuaternion = Quaternion.Euler(packet.Float(), packet.Float(), packet.Float());
        }

        private Packet GetTransform()
        {   
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
        
        public void Destroy()
        {
            isDestroy = true;
        }
    }    
}