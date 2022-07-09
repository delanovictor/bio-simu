using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Being : MonoBehaviour
{
    [SerializeField] private Species species;
    [SerializeField] private bool alive = false;

    [SerializeField] private Being father;
    [SerializeField] private Being mother;

    [SerializeField] private float currentHunger;
    [SerializeField] private float currentMating;

    [SerializeField] private GameObject targetFood;
    [SerializeField] private GameObject targetMate;

    [SerializeField] private Vector3 targetPosition;

    [SerializeField] public DNA dna;

    [SerializeField] private SpriteRenderer sr;

    [SerializeField] private Sprite deathSprite;

    [SerializeField] private GameObject beingPrefab;

    private enum State
    {
        MovingToFood,
        MovingToMate,
        Mating,
        Idle,
    }
    [SerializeField] private State state;

    private enum Sex
    {
        Male,
        Female
    }

    [SerializeField] private Sex sex;

    [SerializeField] private bool isChild = false;

    [SerializeField] private List<Being> failedPartners;

    private float resetFailedPartnersTime = 5;

    void Awake()
    {
        Debug.Log("AWAKE");
        Birth(species);
    }

    public void Birth(Species s, Being parentA = null, Being parentB = null)
    {
        species = s;

        if (parentA == null || parentB == null)
        {
            dna = species.baseDNA;

            mother = parentA;
            father = parentB;
        }
        else
        {
            dna = GenerateDNA(parentA.dna, parentB.dna);
        }

        gameObject.name = species.name + Random.value.ToString().Replace("0,", "");

        sex = (Sex)Random.Range(0, 2);

        Reset();
    }

    DNA GenerateDNA(DNA parentA, DNA parentB)
    {
        Debug.Log("GenerateDNA");
        DNA newDNA = new DNA();

        newDNA.speed = RandomGene(parentA, parentB).speed + Random.Range(-species.mutation.speed, species.mutation.speed);
        newDNA.sightRange = RandomGene(parentA, parentB).sightRange + Random.Range(-species.mutation.sightRange, species.mutation.sightRange);
        newDNA.hungerRate = RandomGene(parentA, parentB).hungerRate + Random.Range(-species.mutation.hungerRate, species.mutation.hungerRate);
        newDNA.maxHunger = RandomGene(parentA, parentB).maxHunger + Random.Range(-species.mutation.maxHunger, species.mutation.maxHunger);
        newDNA.maxMating = RandomGene(parentA, parentB).maxMating + Random.Range(-species.mutation.maxMating, species.mutation.maxMating);
        newDNA.matingRate = RandomGene(parentA, parentB).matingRate + Random.Range(-species.mutation.matingRate, species.mutation.matingRate);
        newDNA.spawnRadius = RandomGene(parentA, parentB).spawnRadius + Random.Range(-species.mutation.spawnRadius, species.mutation.spawnRadius);
        newDNA.hungerTreshHold = RandomGene(parentA, parentB).hungerTreshHold + Random.Range(-species.mutation.hungerTreshHold, species.mutation.hungerTreshHold);
        newDNA.eatingValue = RandomGene(parentA, parentB).eatingValue + Random.Range(-species.mutation.eatingValue, species.mutation.eatingValue);


        Debug.Log(newDNA);
        return newDNA;
    }

    DNA RandomGene(DNA parentA, DNA parentB)
    {
        float rand = Random.value;

        if (rand > 0.5f)
        {

            Debug.Log("Mother Gene");
            return parentA;
        }
        else
        {
            Debug.Log("Father Gene");
            return parentB;
        }
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

        if (!isChild)
        {
            currentMating = currentMating + (dna.matingRate * Time.deltaTime);
        }

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
            newTargetPosition = Helper.Random(new Vector3(-species.baseDNA.sightRange, -species.baseDNA.sightRange, 1), new Vector3(species.baseDNA.sightRange, species.baseDNA.sightRange, 1));
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
                                    StartCoroutine(ResetFailedPartnersCoroutine(possibleMate));
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

    void GiveBirth(GameObject partner = null)
    {
        Vector3 spawnLocation = transform.position + new Vector3(Random.Range(-dna.spawnRadius, dna.spawnRadius), Random.Range(-dna.spawnRadius, dna.spawnRadius), 1);

        GameObject g = Instantiate(beingPrefab, spawnLocation, Quaternion.identity);

        currentMating = 0f;

        g.GetComponent<Being>().Birth(species, this, partner.GetComponent<Being>());
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


    IEnumerator GestationCoroutine(GameObject partner)
    {
        yield return new WaitForSeconds(dna.gestationDuration);

        GiveBirth(partner);
    }

    IEnumerator MateCoroutine(GameObject partner)
    {
        yield return new WaitForSeconds(1.5f);

        state = State.Idle;
        currentMating = 0;
        targetMate = null;

        if (sex == Sex.Female)
        {
            StartCoroutine(GestationCoroutine(partner));
        }
    }

    IEnumerator ResetFailedPartnersCoroutine(Being refusedPartner)
    {
        yield return new WaitForSeconds(resetFailedPartnersTime);

        failedPartners.Remove(refusedPartner);
    }

    IEnumerator AgeCoroutine()
    {
        yield return new WaitForSeconds(species.adulthoodTime);

        Debug.Log("Age");
        isChild = false;
        transform.localScale = new Vector3(1f, 1f, 1f);
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

        isChild = true;

        transform.localScale = new Vector3(species.childhoodSize, species.childhoodSize, species.childhoodSize);

        StartCoroutine(AgeCoroutine());

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
