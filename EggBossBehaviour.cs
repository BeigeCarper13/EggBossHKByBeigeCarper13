using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SocialPlatforms;
using static tk2dSpriteCollectionDefinition;
using static UnityEngine.UI.Image;

namespace EggBossHKByBeigeCarper13
{ 
    public class EggBossBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject SteamBuble = GameObject.Find("SteamBuble"); 
        [SerializeField] private GameObject Projectile = GameObject.Find("Projectile");
        [SerializeField] private GameObject rage_start = GameObject.Find("rage_start");
        [SerializeField] private GameObject lockArena = GameObject.Find("lock");
        public float jumpAngle = 60f;
        private GameObject PlayerObj;
        private Rigidbody2D rb;
        private SpriteRenderer sr;
        private bool isAttacking;
        [SerializeField] private float jumpTime = 0.5f;
        [SerializeField] private float projectileSpeed = 50;
        [SerializeField] private float steamTime = 0.5f;
        [SerializeField] private float projectileTime = 1f;
        [SerializeField] private float projectileNum = 12f;
        [SerializeField] private float rage_start_time = 2f;
        [SerializeField] private float rage_start_speed = 135f;
        [SerializeField] private float fadeTime = 5f;
        [SerializeField] private int maxHealth = 1000;
        private bool facingRight = false;
        private Dictionary<string, Sprite> animSprites = new Dictionary<string, Sprite>();
        private Dictionary<string, AudioClip> allAudio = new Dictionary<string, AudioClip>();
        private AudioSource audioSouceEgg;
        private AudioSource audioSouceAttackHandle;
        private List<Sprite> endSprites = new List<Sprite>();
        IEnumerator Start()
        {
            SpriteCreator();
            initializeScene();
            sr = GameObject.Find("spriteRenderer").GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
        
            while (PlayerObj == null)
            {
                yield return new WaitForEndOfFrame();
                try { PlayerObj = GameObject.FindGameObjectsWithTag("Player")[0]; }
                catch { }
            }
            while (transform.position.x - PlayerObj.transform.position.x > 7f)
            {
                yield return new WaitForEndOfFrame();
            }
            AddStartComponents();
            lockArena.SetActive(true);
            float time = 0f;
            rage_start.SetActive(true);
            sr.sprite = animSprites["start"];
            audioSouceEgg.clip = allAudio["start"];
            audioSouceEgg.Play();
            rb.AddForce(new Vector2(0f, 15f * rb.mass), ForceMode2D.Impulse);
            Destroy(GameObject.Find("gnezdo2"));
            while (time < rage_start_time) 
            {
                rage_start.transform.Rotate(0f, 0f, rage_start_speed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
                time += Time.deltaTime;
            }
            rb.velocity = Vector2.zero;
            rage_start.SetActive(false);
            isAttacking = false;
            StartCoroutine(selectAttack());
        }

        void SpriteCreator() 
        {
            foreach (UnityEngine.Object i in EggBossHKByBeigeCarper13.EggBossAssets.LoadAllAssets())
            {
                if (i.name.Contains("anim_"))
                {
                    Texture2D tex = EggBossHKByBeigeCarper13.EggBossAssets.LoadAsset(i.name) as Texture2D;
                    if (tex == null)
                    {
                        Modding.Logger.Log("Failed to load: " + i.name);
                        continue;
                    }
                    try
                    {
                        animSprites.Add(i.name.Substring("anim_".Length),
                            Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 500f));
                    }
                    catch { }
                }
                if (i.name.Contains("end_explosion"))
                {
                    Texture2D tex = EggBossHKByBeigeCarper13.EggBossAssets.LoadAsset(i.name) as Texture2D;
                    if (tex == null)
                    {
                        Modding.Logger.Log("Failed to load: " + i.name);
                        continue;
                    }
                    try
                    {

                        endSprites.Add(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 125f));
                    }
                    catch
                    {
                    }
                
                }
                if (i.name.Contains("audio_"))
                {
                    AudioClip aud = EggBossHKByBeigeCarper13.EggBossAssets.LoadAsset<AudioClip>(i.name);
                    if (aud == null)
                    {
                        Modding.Logger.Log("Failed to load: " + i.name);
                        continue;
                    }
                    try
                    {

                        allAudio.Add(i.name.Substring("audio_".Length), aud);
                    }
                    catch
                    {
                    }
                }
            }
        }
        void initializeScene()
        {
            MusicCue cue = ScriptableObject.CreateInstance<MusicCue>();
            var channelInfo = new MusicCue.MusicChannelInfo();
            AccessPrivateFields(channelInfo, "clip", null);
            AccessPrivateFields(channelInfo, "sync", MusicChannelSync.Implicit);

            var channelInfos = new MusicCue.MusicChannelInfo[Enum.GetValues(typeof(MusicChannels)).Length];
            channelInfos[(int)MusicChannels.Main] = channelInfo;

            AccessPrivateFields(cue, "channelInfos", channelInfos);

            GameObject.FindObjectOfType<AudioManager>().ApplyMusicCue(cue, 0f, 0f, true);

            lockArena.SetActive(false);
            GameObject.Find("rage_start").SetActive(false);
            GameObject.Find("SteamBuble").AddComponent<DamageHero>();
            GameObject.Find("CameraLockArea").AddComponent<CameraLockArea>();
            CameraLockArea cameraLock = GameObject.Find("CameraLockArea").AddComponent<CameraLockArea>();
            cameraLock.cameraXMin = 35f;
            cameraLock.cameraXMax = 47.5f;
            cameraLock.cameraYMin = 11f;
            cameraLock.cameraYMin = 11f;
            cameraLock.cameraYMax = 15f; 

            this.gameObject.AddComponent<DamageHero>();
            EnemyDreamnailReaction edr = this.gameObject.AddComponent<EnemyDreamnailReaction>();

            AccessPrivateFields(edr, "convoTitle", "eggboss");
            AccessPrivateFields(edr, "convoAmount", 4);
            AccessPrivateFields(edr, "startSuppressed", false);
            AccessPrivateFields(edr, "noSoul", false);
            AccessPrivateFields(edr, "allowUseChildColliders", false);
            AccessPrivateFields(edr, "cooldownTimeRemaining", 0.1f);

            audioSouceEgg = this.gameObject.AddComponent<AudioSource>();
            audioSouceAttackHandle = this.gameObject.AddComponent<AudioSource>();
            AudioMixerGroup sfxGroup = HeroController.instance.gameObject.GetComponent<AudioSource>().outputAudioMixerGroup;
            audioSouceEgg.outputAudioMixerGroup = sfxGroup;
            audioSouceAttackHandle.outputAudioMixerGroup = sfxGroup;
        }

        void AddStartComponents() 
        {
            MusicCue cue = ScriptableObject.CreateInstance<MusicCue>();
            var channelInfo = new MusicCue.MusicChannelInfo();
            AccessPrivateFields(channelInfo, "clip", allAudio["music"]);
            AccessPrivateFields(channelInfo, "sync", MusicChannelSync.Implicit);

            var channelInfos = new MusicCue.MusicChannelInfo[Enum.GetValues(typeof(MusicChannels)).Length];
            channelInfos[(int)MusicChannels.Main] = channelInfo;
            AccessPrivateFields(cue, "channelInfos", channelInfos);
            GameObject.FindObjectOfType<AudioManager>().ApplyMusicCue(cue, 0f, 0f, true);

            HealthManager hm = this.gameObject.AddComponent<HealthManager>();
            hm.hp = maxHealth;
            GameObject shellShard = GameObject.Find("eggBrokenS");
            AccessPrivateFields(hm, "strikeNailPrefab", shellShard);
            AccessPrivateFields(hm, "slashImpactPrefab", shellShard);
            AccessPrivateFields(hm, "fireballHitPrefab", shellShard);
            AccessPrivateFields(hm, "blockHitPrefab", shellShard);
            AccessPrivateFields(hm, "sharpShadowImpactPrefab", shellShard);
            AccessPrivateFields(hm, "corpseSplatPrefab", shellShard);
            AccessPrivateFields(hm, "alternateInvincibleSound", allAudio["crack"]);

            this.gameObject.AddComponent<TriggerEnterEvent>();
            this.gameObject.GetComponent<HealthManager>().OnDeath += this.OnEnemyDeath;

            Recoil recoil = this.gameObject.AddComponent<Recoil>();
            recoil.freezeInPlace = false;
            recoil.SetRecoilSpeed(20f);
            recoil.SkipFreezingByController = false;
        }
    
        void AccessPrivateFields(object compType, string fieldName, object newValue) 
        {
            FieldInfo privateField = compType.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            privateField?.SetValue(compType, newValue);
        }

        void OnEnemyDeath() 
        {
            lockArena.SetActive(false);
            SteamBuble.SetActive(false);
            DeleteComponents();
            StopAllCoroutines();
            StartCoroutine(endAnimation());
        }
        void DeleteComponents() 
        {
            MusicCue cue = ScriptableObject.CreateInstance<MusicCue>();
            var channelInfo = new MusicCue.MusicChannelInfo();
            AccessPrivateFields(channelInfo, "clip", null);
            AccessPrivateFields(channelInfo, "sync", MusicChannelSync.Implicit);

            var channelInfos = new MusicCue.MusicChannelInfo[Enum.GetValues(typeof(MusicChannels)).Length];
            channelInfos[(int)MusicChannels.Main] = channelInfo;

            AccessPrivateFields(cue, "channelInfos", channelInfos);

            GameObject.FindObjectOfType<AudioManager>().ApplyMusicCue(cue, 0f, 0f, true);
            lockArena.SetActive(false);
            SteamBuble.SetActive(false);
            rage_start.SetActive(false);

            Destroy(this.gameObject.GetComponent<DamageHero>());
            Destroy(this.gameObject.AddComponent<EnemyDreamnailReaction>());


            Destroy(this.gameObject.AddComponent<TriggerEnterEvent>());
            Destroy(this.gameObject.AddComponent<HealthManager>());
            Destroy(this.gameObject.AddComponent<Recoil>());

            EnemyDreamnailReaction edr = this.gameObject.AddComponent<EnemyDreamnailReaction>();

            AccessPrivateFields(edr, "convoTitle", "eggbossdefeated");
            AccessPrivateFields(edr, "convoAmount", 3);
            AccessPrivateFields(edr, "startSuppressed", false);
            AccessPrivateFields(edr, "noSoul", true);
            AccessPrivateFields(edr, "allowUseChildColliders", false);
            AccessPrivateFields(edr, "cooldownTimeRemaining", 0.1f);

            rb.velocity = Vector2.zero;
        }
        IEnumerator endAnimation()
        {
        
            audioSouceEgg.clip = allAudio["explosion"];
            audioSouceEgg.Play();
            float frameRate = 60f;
            float frameTime = 1f / frameRate;
            int frameIndex = 0;
            float timer = 0f;

            while (frameIndex < endSprites.Count)
            {
                timer += Time.deltaTime;

                if (timer >= frameTime)
                {
                    sr.sprite = endSprites[frameIndex];
                    frameIndex++;
                    timer -= frameTime;
                }

                yield return new WaitForEndOfFrame();
            }
        }
        void Update()
        {
            if (maxHealth != GetComponent<HealthManager>().hp) 
            {
                audioSouceAttackHandle.clip = allAudio["crack"];
                audioSouceAttackHandle.Play();
                maxHealth = GetComponent<HealthManager>().hp;
            }
        }
        public GameObject createPrefab(GameObject gmobj) 
        {
            return Instantiate(gmobj);
        }
        IEnumerator selectAttack()
        {
            while (true)
            {
                yield return new WaitWhile((() => isAttacking));
                Rotate();
                System.Random rand = new System.Random();
                int a = rand.Next(0, 4);
                //a = 1;
                switch (a)
                {
                    case 0:
                        sr.sprite = animSprites["steam"];
                        StartCoroutine(steamAttack());
                        break;
                    case 1:
                        sr.sprite = animSprites["shoot"];
                        StartCoroutine(shootAttack());
                        break;
                    case >1:
                        sr.sprite = animSprites["jump"];
                        StartCoroutine(jumpAttack());
                        break;
                }
            }
        }

        void OnCollisionEnter2D(Collision2D collision) 
        {
            if (collision.gameObject.layer == 8 && isAttacking)
            {
                audioSouceEgg.clip = allAudio["land"];
                audioSouceEgg.Play();
            }
        }
        IEnumerator jumpAttack()
        {
            
            audioSouceEgg.clip = allAudio["jump"];
            audioSouceEgg.Play();
            isAttacking = true;
            Vector2 start = transform.position;
            Vector2 target = PlayerObj.transform.position;

            float dx = target.x - start.x;
            float dy = target.y - start.y;

            float gravity = Mathf.Abs(Physics2D.gravity.y);

            float vx = dx / jumpTime;
            float vy = dy / jumpTime + 0.5f * gravity * jumpTime;

            rb.velocity = new Vector2(vx, vy);

            float nowTime = 0;
            while ((1 < Vector2.Distance(target, this.transform.position)) && (nowTime<2*jumpTime))
            {
                nowTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            sr.sprite = animSprites["stand"];
            rb.velocity = Vector2.zero;
            yield return new WaitForSeconds(0.5f);
            isAttacking = false;
        }
        IEnumerator steamAttack()
        {
            SteamBuble.SetActive(true);
            Color previousCol = SteamBuble.GetComponent<SpriteRenderer>().color;
            SteamBuble.GetComponent<SpriteRenderer>().color = new Color(previousCol.r, previousCol.g, previousCol.b, 1);
            isAttacking = true;

            audioSouceEgg.clip = allAudio["steam"];
            audioSouceEgg.Play();
            while (SteamBuble.transform.localScale.x < 2.3)
            {
                float updateSize = Time.deltaTime / steamTime;
                SteamBuble.transform.localScale =
                    new Vector3(SteamBuble.transform.localScale.x + updateSize,
                    SteamBuble.transform.localScale.y + updateSize,
                    SteamBuble.transform.localScale.z + updateSize);
                yield return new WaitForEndOfFrame();
            }
            sr.sprite = animSprites["stand"];
            while (SteamBuble.GetComponent<SpriteRenderer>().color.a > 0)
            {
                previousCol = SteamBuble.GetComponent<SpriteRenderer>().color;
                SteamBuble.GetComponent<SpriteRenderer>().color = new Color(previousCol.r, previousCol.g, previousCol.b, previousCol.a - fadeTime * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
            SteamBuble.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
            SteamBuble.SetActive(false);
            yield return new WaitForSeconds(0.5f);
            isAttacking = false;
        }
        IEnumerator shootAttack()
        {

            isAttacking = true;

            yield return new WaitForSeconds(0.5f);
            Transform projectileSpawnPoint = new GameObject("null").transform;
            projectileSpawnPoint.position = new Vector3(-1.3f* (!facingRight ? 1 : -1), -0.3f, 0f) + this.transform.position;
            projectileSpawnPoint.rotation = new Quaternion(0, 0, 0, 1);
            Vector3 projectileRotationPoint = new Vector3(0.2f * (!facingRight ? 1 : -1), -0.3f, 0f) + this.transform.position;

            projectileSpawnPoint.transform.RotateAround(this.transform.position, Vector3.forward, (!facingRight ? 1 : -1) * 10);

            for (int i = 0; i < projectileNum; i++)
            {

                audioSouceEgg.clip = allAudio["shoot"];
                audioSouceEgg.Play();
                GameObject proj = Instantiate(Projectile, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
                proj.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
                proj.AddComponent<ProjectileMove>().projLand = allAudio["projLand"];
                proj.AddComponent<DamageHero>();
                proj.GetComponent<Rigidbody2D>().AddForce((proj.transform.position - projectileRotationPoint).normalized * projectileSpeed, ForceMode2D.Impulse);
                projectileSpawnPoint.transform.RotateAround(projectileRotationPoint, Vector3.forward, (facingRight ? 1 : -1) * 60 / projectileNum);
                yield return new WaitForSeconds(projectileTime / projectileNum);
            }
            sr.sprite = animSprites["stand"];

            yield return new WaitForSeconds(0.5f);
            isAttacking = false;
        }

        private void Rotate()
        {
            if ((facingRight ? 1 : -1) * (transform.position.x - PlayerObj.transform.position.x) > 0f)
            {
                Vector3 trans = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                transform.localScale = trans;
                facingRight = !facingRight;
            }
        }

    }
}