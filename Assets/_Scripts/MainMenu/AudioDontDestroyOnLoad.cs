using UnityEngine;

namespace MainMenu
{
    public class AudioDontDestroyOnLoad : MonoBehaviour
    {
        public static AudioDontDestroyOnLoad Instance;

        void Start()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
