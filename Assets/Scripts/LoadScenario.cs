using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScenario : MonoBehaviour
{
    public void LoadBarcelona()
    {
        SceneManager.LoadScene("Barcelona");
    }
    public void LoadArcDeTriomphe()
    {
        SceneManager.LoadScene("Arc de Triomphe");
    }
    public void LoadKuVicinity()
    {
        SceneManager.LoadScene("KU Vicinity");
    }
}
