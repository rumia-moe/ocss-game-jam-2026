using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChangerButton : MonoBehaviour
{
    public Button button;
    public string sceneName;

    private void Reset() => button = GetComponent<Button>();
    private void OnEnable() => button.onClick.AddListener(OnClick);
    private void OnDisable() => button.onClick.RemoveListener(OnClick);

    private void OnClick()
    {
        if (string.IsNullOrEmpty(sceneName) )return;

        SceneManager.LoadScene(sceneName);
    }
}
