using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Being : MonoBehaviour
{
    public Species species;
    public bool alive = false;

    public float currentHunger;
    public float currentMating;

    public GameObject targetFood;
    public GameObject targetMate;

    public Vector3 targetPosition;

    public DNA individualDNA;
    public DNA dna;

    public SpriteRenderer sr;

    public Sprite deathSprite;

    public GameObject beingPrefab;

    public enum State
    {
        MovingToFood,
        MovingToMate,
        Mating,
        Idle,
    }
    public State state;

    public enum Sex
    {
        Male,
        Female
    }

    public Sex sex;

    public List<Being> failedPartners;

    private float resetFailedPartnersTime = 5;

    private void Start()
    {
        Birth(species);
    }

    void Awake()
    {
        Birth(species);
    }

    public void Birth(Species s, DNA parentA = new DNA(), DNA parentB = new DNA())
    {
        species = s;
        // GenerateDNA(parentA, parentB);
        dna = species.dna;

        sex = (Sex)Random.Range(0, 2);

        Reset();
    }

    void GenerateDNA(DNA parentA, DNA parentB)
    {
        individualDNA.speed = Random.Range(parentA.speed, parentB.speed) * Random.Range(-0.3f, 0.3f);
        individualDNA.sightRange = Random.Range(parentA.sightRange, parentB.sightRange) * Random.Range(-0.3f, 0.3f);
        individualDNA.hungerRate = Random.Range(parentA.hungerRate, parentB.hungerRate) * Random.Range(-0.3f, 0.3f);
        individualDNA.maxHunger = Random.Range(parentA.maxHunger, parentB.maxHunger) * Random.Range(-0.3f, 0.3f);
        individualDNA.maxMating = Random.Range(parentA.maxMating, parentB.maxMating) * Random.Range(-0.3f, 0.3f);
        individualDNA.matingRate = Random.Range(parentA.matingRate, parentB.matingRate) * Random.Range(-0.3f, 0.3f);
        individualDNA.spawnRadius = Random.Range(parentA.spawnRadius, parentB.spawnRadius) * Random.Range(-0.3f, 0.3f);
        individualDNA.hungerTreshHold = Random.Range(parentA.hungerTreshHold, parentB.hungerTreshHold) * Random.Range(-0.3f, 0.3f);
        individualDNA.eatingValue = Random.Range(parentA.eatingValue, parentB.eatingValue) * Random.Range(-0.3f, 0.3f);


        dna.speed = species.dna.speed + individualDNA.speed;
        dna.sightRange = species.dna.sightRange + individualDNA.sightRange;
        dna.hungerRate = species.dna.hungerRate + individualDNA.hungerRate;
        dna.maxHunger = species.dna.maxHunger + individualDNA.maxHunger;
        dna.maxMating = species.dna.maxMating + individualDNA.maxMating;
        dna.matingRate = species.dna.matingRate + individualDNA.matingRate;
        dna.spawnRadius = species.dna.spawnRadius + individualDNA.spawnRadius;
        dna.hungerTreshHold = species.dna.hungerTreshHold + individualDNA.hungerTreshHold;
        dna.eatingValue = species.dna.eatingValue + individualDNA.eatingValue;
    }

    // Update is called once per frame
    void Update()
    {
        if (alive)
        {
            Behave();
        }

        if (currentHunger <= 0)
        {
            Die();
        }
    }

    void Behave()
    {
        currentHunger = currentHunger - (dna.hungerRate * Time.deltaTime);
        currentMating = currentMating + (dna.matingRate * Time.deltaTime);

        if (dna.speed > 0)
        {
            GameObject target = null;

            if (currentHunger / dna.maxHunger > currentMating / dna.maxMating)
            {
                if (dna.hungerTreshHold > currentHunger)
                {
                    if (state == State.MovingToFood)
                    {

                    }
                    else
                    {
                        SeekFood();
                    }

                    target = targetFood;
                }
            }
            else
            {
                switch (state)
                {
                    case State.MovingToMate:
                        target = targetMate;
                        break;
                    case State.Mating:
                        target = gameObject;
                        break;
                    default:
                        SeekMate();
                        target = targetMate;
                        break;
                }
            }

            MoveToTarget(target);
        }

    }

    void MoveToTarget(GameObject target)
    {
        if (target == null)
        {
            targetPosition = Wander();
        }
        else
        {
            targetPosition = target.transform.position;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * dna.speed);
    }

    Vector3 Wander()
    {
        Vector3 newTargetPosition = targetPosition;

        if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
        {
            newTargetPosition = Helper.Random(new Vector3(-species.dna.sightRange, -species.dna.sightRange, 1), new Vector3(species.dna.sightRange, species.dna.sightRange, 1));
            state = State.Idle;
        }

        return newTargetPosition;
    }

    void SeekMate()
    {

        switch (species.reproduction)
        {
            case Species.Reproduction.Polen:

                GiveBirth();

                break;

            case Species.Reproduction.Fertilization:

                if (sex == Sex.Male)
                {
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, dna.sightRange);

                    foreach (Collider2D c in colliders)
                    {
                        if (c.gameObject.tag == species.tag && c.gameObject != gameObject)
                        {
                            Being possibleMate = c.gameObject.GetComponent<Being>();
                            if (possibleMate.sex == Sex.Female && !failedPartners.Contains(possibleMate))
                            {
                                Debug.Log("Macho requisitando...");
                                if (possibleMate.MateRequest(gameObject))
                                {
                                    Debug.Log("Aceito...");
                                    targetMate = c.gameObject;
                                }
                                else
                                {
                                    failedPartners.Add(possibleMate);
                                    StartCoroutine(ResetFailedPartners(possibleMate));
                                }
                                break;
                            }
                        }
                    }
                }


                break;
        }

        if (targetMate)
        {
            state = State.MovingToMate;
        }

    }

    void SeekFood()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, dna.sightRange);

        foreach (Collider2D c in colliders)
        {
            if (c.gameObject.tag == species.targetSpecies.tag)
            {
                targetFood = c.gameObject;
                break;
            }
        }

        if (targetFood)
        {
            state = State.MovingToFood;
        }

    }

    void Eat(GameObject food)
    {
        if (food.tag == species.tag)
        {
            // DNA partnerDNA = other.gameObject.GetComponent<Being>().dna;

            // other.gameObject.GetComponent<Being>().currentMating = 0;

            // GiveBirth();
        }
        else
        {
            food.GetComponent<Being>().Die();

            currentHunger += dna.eatingValue;

            targetFood = null;
        }
    }

    public void Die()
    {
        alive = false;

        sr.sprite = deathSprite;

        Destroy(gameObject);
    }

    void GiveBirth(DNA partnerDNA = new DNA())
    {
        Vector3 spawnLocation = transform.position + new Vector3(Random.Range(-dna.spawnRadius, dna.spawnRadius), Random.Range(-dna.spawnRadius, dna.spawnRadius), 1);

        GameObject g = Instantiate(beingPrefab, spawnLocation, Quaternion.identity);

        currentMating = 0f;
    }

    void Mate(GameObject partner)
    {
        state = State.Mating;

        StartCoroutine("MateCoroutine", partner);
    }

    public bool MateRequest(GameObject male)
    {
        if (currentHunger / dna.maxHunger > currentMating / dna.maxMating)
        {
            return false;
        }
        else
        {
            state = State.MovingToMate;

            targetMate = male;

            return true;
        }

    }

    IEnumerator MateCoroutine(GameObject partner)
    {
        yield return new WaitForSeconds(1.5f);

        state = State.Idle;
        currentMating = 0;
        targetMate = null;

        if (sex == Sex.Female)
        {
            GiveBirth();
        }

        StopCoroutine("MateCoroutine");
    }

    IEnumerator ResetFailedPartners(Being refusedPartner)
    {
        yield return new WaitForSeconds(resetFailedPartnersTime);

        failedPartners.Remove(refusedPartner);
    }


    void Reset()
    {
        alive = true;

        sr = GetComponent<SpriteRenderer>();

        sr.sprite = species.sprite;

        gameObject.tag = species.tag;

        targetFood = null;
        targetMate = null;

        targetPosition = transform.position;

        currentHunger = dna.maxHunger;

        currentMating = 0;

        state = State.Idle;

        failedPartners = new List<Being>();



        if (!species.collision)
        {
            Destroy(gameObject.GetComponent<Rigidbody2D>());
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (species.collision)
            return;

        Eat(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D other)
    {

        if (!species.collision)
            return;

        if (other.gameObject == targetFood)
        {
            Eat(other.gameObject);
        }

        if (other.gameObject == targetMate)
        {
            if (state == State.MovingToMate)
            {
                if (other.gameObject.GetComponent<Being>().targetMate == gameObject)
                {
                    Mate(other.gameObject);
                }
            }
        }

    }

    void OnDrawGizmos()
    {
        if (species != null && dna.speed > 0)
        {
            // Gizmos.DrawWireSphere(transform.position, species.sightRange);

            Gizmos.color = Color.white;

            if (targetFood != null)
            {
                Gizmos.color = Color.green;
            }

            if (targetMate != null)
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawLine(transform.position, targetPosition);

        }
    }
}
