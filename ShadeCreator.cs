using UnityEngine;

namespace EggBossHKByBeigeCarper13
{
    public class ShadeCreator : MonoBehaviour
    {
        public static GameObject CreatePrefab() 
        {
            GameObject gmb = Instantiate(GameObject.FindObjectOfType<SceneManager>().hollowShadeObject);
            gmb.transform.position = HeroController.instance.transform.position;
            DontDestroyOnLoad(gmb);
            return gmb;
        }
    }
}