using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Modding;
using UnityEngine;

namespace EggBossHKByBeigeCarper13
{
    public class EggBossHKByBeigeCarper13 : Mod
    {
        internal static EggBossHKByBeigeCarper13 Instance;
        public static FsmTemplate health_manager_enemy;
        public static AssetBundle EggBossScene { get; private set; } = null;
        public string modname;
        public static AssetBundle EggBossAssets;
        private GameObject ShadeGOSave;
        public EggBossHKByBeigeCarper13() : base("Egg Boss HK By BeigeCarper13") { }
        public override string GetVersion() => "beta1";

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            modname = "Egg Boss HK By BeigeCarper13";
            Log("Initializing");
            Instance = this;
            On.GameManager.EnterHero += GameManager_EnterHero;
            ModHooks.LanguageGetHook += OnLanguageGet;
            Load();
            Log("Initialized");
        }
        private void GameManager_EnterHero(On.GameManager.orig_EnterHero orig, GameManager self, bool additiveGateSearch)
        {
            if (self.sceneName == "Crossroads_01")
            {
                if (ShadeGOSave == null) ShadeGOSave = ShadeCreator.CreatePrefab();
                ShadeGOSave.SetActive(false);
                GameObject go =
                CreateTP("custom_transition_right", new Vector3(48f, 4f, 0.0f), new Vector2(1.5f, 5.5f),
                "EggBossScene", "left9", false, true, GameManager.SceneLoadVisualizations.Default);
            }
            else if (self.sceneName == "EggBossScene")
            {
                if (PlayerData.instance.GetBool("soulLimited") &&
                    PlayerData.instance.GetString("shadeScene") == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
                {
                    ShadeGOSave.SetActive(true);
                    ShadeGOSave.transform.position = GameObject.Find("shade marker").transform.position;
                }
                GameObject go =
                CreateTP("left9", new Vector3(5.25f, 5.5f, 0f), new Vector2(1f, 4f),
                "Crossroads_01", "custom_transition_right", true, false, GameManager.SceneLoadVisualizations.Default);
                GameObject EggBossSelf = GameObject.Find("EggBossSelf");
                EggBossBehaviour abb = EggBossSelf.AddComponent<EggBossBehaviour>();
            }
            orig(self, additiveGateSearch);
        }
        private string OnLanguageGet(string key, string sheet, string orig)
        {
            switch (key)
            {
                case "eggboss_1": return "ЯЙКОБОСС!!!!!!";
                case "eggboss_2": return "Очень сложный босс!";
                case "eggboss_3": return "я как будто в логово пришел к боссу, как в дарксоулсе";
                case "eggboss_4": return "Полувареный Бой";
                case "eggbossdefeated_1": return "ЯЙКО....БОСС....";
                case "eggbossdefeated_2": return "я.....проиграл?.....";
                case "eggbossdefeated_3": return "ಥ_ಥ";
                default: return orig;
            }
        }
        private GameObject CreateTP(string direction, Vector3 position, Vector2 size,
            string targetScene, string entryPoint, bool alwaysEnterLeft, bool alwaysEnterRight, GameManager.SceneLoadVisualizations sceneLoadVisualization)
        {
            GameObject go = new GameObject(direction);
            go.AddComponent<GateSnap>();
            go.transform.position = position;
            go.AddComponent<BoxCollider2D>().size = size;
            go.GetComponent<BoxCollider2D>().isTrigger = true;
            go.layer = 10;
            TransitionPoint tp = go.AddComponent<TransitionPoint>();
            tp.targetScene = targetScene;
            tp.entryPoint = entryPoint;
            tp.alwaysEnterLeft = alwaysEnterLeft;
            tp.alwaysEnterRight = alwaysEnterRight;
            tp.respawnMarker = go.AddComponent<HazardRespawnMarker>();
            tp.sceneLoadVisualization = sceneLoadVisualization;


            return go;
        }
        public void Load()
        {

            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream s = asm.GetManifestResourceStream("EggBossHKByBeigeCarper13.asset_bundles.eggboss_scene"))
            {
                if (s != null)
                {
                    Log(1);
                    EggBossScene = AssetBundle.LoadFromStream(s);
                    Log(EggBossScene);

                    Log(EggBossScene);
                }
                else { LogError("can`t found eggboss_scene"); }
            }
            using (Stream s = asm.GetManifestResourceStream("EggBossHKByBeigeCarper13.asset_bundles.eggboss_assets"))
            {
                if (s != null)
                {
                    EggBossAssets = AssetBundle.LoadFromStream(s);
                }
                else { LogError("can`t found eggboss_assets"); }
            }
        }
    }
}