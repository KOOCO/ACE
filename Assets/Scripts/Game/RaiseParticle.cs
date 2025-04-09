using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using static UnityEngine.ParticleSystem;

public class RaiseParticle : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Slider slider;
    public ParticleSystem particle, particle2;
    public ParticleSystem particleBomb;
    float preValue = 0;
    void Start()
    {
        particle.transform.SetAsFirstSibling();
        particle2.transform.SetAsFirstSibling();
        slider.onValueChanged.AddListener((value) =>
        {
            if (value == slider.maxValue)
            {
                particleBomb.gameObject.SetActive(true);
                particle.gameObject.SetActive(false);
                particle2.gameObject.SetActive(false);
                var emission = particle.emission;
                var emission2 = particle2.emission;
                emission.rateOverDistance = 0;
                emission2.rateOverDistance = 0;
            }
            else
            {
                if (preValue < value)
                {
                    var emission = particle.emission;
                    var emission2 = particle2.emission;
                    emission.rateOverDistance = 2;
                    emission2.rateOverDistance = 2;
                    particle.gameObject.SetActive(true);
                    particle2.gameObject.SetActive(true);
                    particleBomb.gameObject.SetActive(false);
                }
                else
                {
                    var emission = particle.emission;
                    var emission2 = particle2.emission;
                    emission.rateOverDistance = 0;
                    emission2.rateOverDistance = 0;
                }    
            }
            preValue = value;
        });
    }
    private void OnDisable()
    {
        particleBomb.gameObject.SetActive(false);
        particle.gameObject.SetActive(false);
        particle2.gameObject.SetActive(false);
    }

    public void OnEndDrag()
    {
        var emission = particle.emission;
        var emission2 = particle2.emission;
        emission.rateOverDistance = 0;
        emission2.rateOverDistance = 0;
        particleBomb.gameObject.SetActive(false);
    }
}
