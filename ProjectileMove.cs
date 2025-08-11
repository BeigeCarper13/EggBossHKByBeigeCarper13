using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class ProjectileMove : MonoBehaviour
{
    [SerializeField] private float fadeTime = 5f;
    public AudioClip projLand;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            AudioSource auds = this.gameObject.AddComponent<AudioSource>();
            AudioMixerGroup sfxGroup = HeroController.instance.gameObject.GetComponent<AudioSource>().outputAudioMixerGroup;
            auds.outputAudioMixerGroup = sfxGroup;
            auds.clip = projLand;
            auds.Play();
            GetComponent<Rigidbody2D>().constraints = UnityEngine.RigidbodyConstraints2D.FreezeAll;
            StartCoroutine(fade());
        }
    }

    IEnumerator fade() 
    {
        yield return new WaitForSeconds(2f);

        while (GetComponent<SpriteRenderer>().color.a > 0) 
        {
            Color previousCol = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(previousCol.r, previousCol.g, previousCol.b, previousCol.a - fadeTime*Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        Destroy(this.gameObject);
        yield return null;
    }
}
