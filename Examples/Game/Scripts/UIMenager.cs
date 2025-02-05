using System;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleUDP
{
    public class UIMenager : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject MainPanel;
        public GameObject GamePanel;

        [Header("Game Panel")]
        public GameObject Pause;

        [Header("UI Elements")]
        public InputField InputAddress;

        [Header("UI Buttons")]
        public Text TextConnect;
        public Button ButtonConnect;
        public Text TextDisconnect;
        public Button ButtonDisconnect;

        [Header("UI Text")]
        public Text RttScore;
        public Text LocalPortText;
        
        public void Connected()
        {
            MainPanel.SetActive(false);
            GamePanel.SetActive(true);

            Pause.SetActive(false);
        }

        public void Disconnected()
        {
            MainPanel.SetActive(true);
            GamePanel.SetActive(false);

            Pause.SetActive(false);
        }

        public void SetRttText(uint rtt)
        {
            RttScore.text = $"Rtt: {rtt}ms";
        }
        
        public void SetActionConnect(string text, Action action)
        {
            TextConnect.text = text;
            ButtonConnect.onClick.RemoveAllListeners();
            ButtonConnect.onClick.AddListener(() => action());
        }

        public void SetActionDisconnect(string text, Action action)
        {
            TextDisconnect.text = text;
            ButtonDisconnect.onClick.RemoveAllListeners();
            ButtonDisconnect.onClick.AddListener(() => action());
        }

        private void Update()
        {
            if (GamePanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    Pause.SetActive(!Pause.activeSelf);
            }
        }
    }
}
