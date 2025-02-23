using UnityEngine;
using UnityEngine.SceneManagement;

namespace vrm
{
    public class PlayerCheck : MonoBehaviour
    {
        const string PlayerScene = "PlayerScene";
        const string Shared = "Shared";

        private void Awake()
        {
            if (!Methods.IsSceneLoaded(Shared))
                SceneManager.LoadScene(Shared, LoadSceneMode.Additive);
            if (!Methods.IsSceneLoaded(PlayerScene))
                SceneManager.LoadScene(PlayerScene, LoadSceneMode.Additive);
        }
    }
}
