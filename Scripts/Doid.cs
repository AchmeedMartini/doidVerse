using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Doid : MonoBehaviour
{
    public Transform doid;
    public Rigidbody2D rb;
    public Brain brain;
    public GameObject Baby;
    public GameObject Arm;
    public GameObject Mouth;
    public GameObject Food;
    public GameObject Worldd;
    public World world;
    public UnityEngine.Rendering.Universal.Light2D light;

    public float width;
    public float realWidth;
    public float realHeight;
    public float height;
    public float radiusMax = 3;
    public float radiusMin = .3f;
    public float radiusMutation = .25f;
    public float radius = 1;//dna.radius;
    public int numRays = 6;
    public float forceMax = 3;
    [Range(0f, 1f)]
    public float forceThreshold = 0.25f;
    public float forceMutation = 0.5f;
    public float velocityMax = 1.2f;
    public float[] sounds;
    public float[] sizes;
    [Range(0, 100)]
    public float volDiffPercent = 50;
    public float health;
    public float maxHealth = 10;
    public float energyMax = 1000;
    public float energyMin = -50;
    public float energy;
    public float energyDamage = 500;
    public float age;
    public float ageMax = 1000;
    public float lastBirthAge = 0;
    public float growthRate = 0.5f;
    public float moveCost = 1;
    public float birthCost = 50;
    public int births = 0;
    public float birthChance = 25;
    public float ageBirthChance;
    public float birthMutation = 50;
    public float birthMin = 200;
    public bool birthing = false;
    public float foodEnergy = 50;
    public float foodEaten = 0;
    public bool justAte = false;
    public float eatCost = 50; 
    public float eatChance = 100;
    public float eatingDistance = 0.1f;
    public float eatingFraction = 0.1f;
    public float grabCost = 50;
    public float grabForce = 1;
    public float grabChance = 25;
    public float grabDistance;
    public float grabbingFraction = 0.3f;
    public float sexFraction = 0.05f;
    public float ageFoodRatio = 0.15f;
    public string species;
    public float memMove = .1f;
    public float dir = 0;
    public float dirMove = .5f;
    public int generation = 0;
    public string dna;
    public bool dontSee = false;
    public bool foodDontSee = false;
    public float[] inputs;
    public float[] middle = new float[14];
    public bool[] middleSeen;
    public float[] outputs = new float[4];

    void Start()
    {
        dir = UnityEngine.Random.Range(0f, 1f);
        Worldd = GameObject.FindWithTag("World");
        world = Worldd.GetComponent<World>();
        foodEaten = 0;
        justAte = false;
        age = 0;
        grabDistance = radius * grabbingFraction ;
        eatingDistance = radius * eatingFraction;
        ageMax = growthRate * 4500;
        health = maxHealth;
        Vector2 Direction = (new Vector2(UnityEngine.Random.value, UnityEngine.Random.value)).normalized;
        doid.Rotate(0, 0, UnityEngine.Random.Range(0, 100) / 100);
        sounds = new float[numRays];
        sizes = new float[numRays];
        ageBirthChance = birthChance;
        inputs = new float[3 * numRays + 7];
        middleSeen = new bool[brain.middleSize];
        dna = brain.toReadable();
    }

    public LayerMask targetMask;
    public LayerMask foodMask;
    public LayerMask walls;

    public float angle()
    {
        return -360 / radiusMax * radius + 360;
    }

    void Empty(Collider2D[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            list[i] = null;
        }
    }

    public Vector2 angleToDir(float ang)
    {
        return new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
    }

    void emptyInputs(float[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 0;
        }
    }

    float averageSound()
    {
        float topSum = 0;
        float botSum = 0;
        for(int i = 0; i < numRays; i++)
        {
            topSum += sounds[i] * sounds[i];
            botSum += sounds[i];
        }
        if(botSum == 0) { return 0; }
        return topSum / botSum;
    }

    float averageWeight()
    {
        float sum = 0;
        float num = 0;
        for (int i = 0; i < numRays; i++)
        {
            sum += sizes[i];
            num++;
        }
        if (num == 0) { return 0; }
        return sum / num;
    }

    void inputVision(int numRays, int start)
    {
        Vector2 Pos = doid.position;
        float ang = angle();
        float startAngle = (ang / 2 + doid.eulerAngles.z) * Mathf.Deg2Rad;
        float stepAngle = (ang / (numRays - 1)) * Mathf.Deg2Rad;

        for (int i = start; i < start + numRays; i++)
        {
            int rayInd = i - start;
            Vector2 visionDir = angleToDir(startAngle - stepAngle * rayInd);

            RaycastHit2D hit = Physics2D.Raycast(Pos + visionDir / 6, visionDir, radius, ~(1 << LayerMask.NameToLayer("spawnerMask")));
            if (hit.collider != null && hit.distance != -1 /*&& (canSee(hit.collider.transform.position - doid.position) || hit.collider.gameObject.tag == "Wall")*/)
            {
                if (world.selected == this.gameObject) { drawArm(Pos, Pos + visionDir * hit.distance); }
                //Debug.DrawLine(Pos, Pos + visionDir * hit.distance);
                switch (hit.collider.gameObject.tag)
                {
                        case ("Doid"):
                        float volume = (hit.collider.gameObject.GetComponent<Doid>().realHeight * hit.collider.gameObject.GetComponent<Doid>().realWidth);
                        sounds[i] = hit.collider.gameObject.GetComponent<Doid>().outputs[4];
                        sizes[i] = volume / (world.heightMax * world.widthMax);
                        inputs[i] = (1 - (hit.distance / radius));
                        inputs[i + numRays] = (hit.distance / radius);
                        inputs[i + 2 * numRays] = 0;
                        break;

                        case ("Food"):
                        sounds[i] = 0;
                        sizes[i] = 0;
                        inputs[i] = 0;
                        inputs[i + numRays] = (hit.distance / radius);
                        inputs[i + 2 * numRays] = 0;
                        break;

                        case ("Wall"):
                        sounds[i] = 0;
                        sizes[i] = 0;
                        inputs[i] = 0;
                        inputs[i + numRays] = 0;
                        inputs[i + 2 * numRays] = (hit.distance / radius);
                        break;
                }
            }
            else
            {
                if (world.selected == this.gameObject) { drawArm(Pos, Pos + visionDir * radius); }
                sounds[i] = 0;
                sizes[i] = 0;
                inputs[i] = 0;
                inputs[i + numRays] = 0;
                inputs[i + 2*numRays] = 0;
            }

        }

    }

    void input()
    {   
        inputVision(numRays, 0);

        //Sound
        inputs[3*numRays] = averageSound();

        //Volume
        inputs[3 * numRays + 1] = averageWeight();

        //Memory
        inputs[3 * numRays + 2] = outputs[3]; 
        inputs[3 * numRays + 6] = outputs[5]; 

        //Constant
        inputs[3 * numRays + 3] = 1;
        
        //Energy
        inputs[3 * numRays + 4] = energy / energyMax;
        
        //Age
        inputs[3 * numRays + 5] = age / ageMax;
    }
    
    void OnCollisionEnter2D(Collision2D hit)
    {
        if(hit.gameObject.tag == "Food")
        {
            eat(hit); 
            
        }
        if(hit.gameObject.tag == "Doid")
        {
            if(realHeight * realWidth > (1 + (volDiffPercent / 100f)) * hit.gameObject.GetComponent<Doid>().realHeight * hit.gameObject.GetComponent<Doid>().realWidth && (species != hit.gameObject.GetComponent<Doid>().species || generation <= hit.gameObject.GetComponent<Doid>().generation - 3))
            {
                float volume = hit.gameObject.GetComponent<Doid>().realHeight * hit.gameObject.GetComponent<Doid>().realWidth;
                energy += hit.gameObject.GetComponent<Doid>().birthCost;
                Destroy(hit.gameObject);
                foodEaten += 2*(volume / (world.heightMax * world.widthMax));
            }
            else
            {
                if((energy - birthCost) > 0 && GameObject.FindGameObjectsWithTag("Doid").Length < world.doidMax && age > hit.gameObject.GetComponent<Doid>().age && age > ageFoodRatio * 1.25f && hit.gameObject.GetComponent<Doid>().age > ageFoodRatio * 1.25f && !birthing && !hit.gameObject.GetComponent<Doid>().birthing /*&& !String.Equals(species, hit.gameObject.GetComponent<Doid>().species)*/)
                    {
                        //Debug.Log("Sex");
                        Sex(hit.gameObject);
                    }
            }
            
        }
        
    }

    public static double Exp(float value)
    {
        double val = (double)value;
        long tmp = (long)(1512775 * val + 1072632447);
        return BitConverter.Int64BitsToDouble(tmp << 32);
    }

    static float Sigmoid(float value)
    {
        if(value == 0) { return 0.5f; }
        float k = (float)Exp(value);
        if(float.IsNaN(k) || k / (1.0f + k) < 0) { return 0; }
        return k / (1.0f + k);
    }

    void unSee()
    {
        for(int i = 0; i < middleSeen.Length; i++)
        {
            middleSeen[i] = false;
        }
    }

    float adjCompute((string, int) node)
    {
        if (!birthing)
        {
            int type = node.Item1 == "input" ? 0 : (node.Item1 == "middle" ? 1 : 2) ;
            int index = node.Item2;
            if (type == 0) { return inputs[index]; }

            if (type == 1 && middleSeen[index]) { return middle[index]; }

            if (type == 1) { middleSeen[index] = true; }

            float sum = 0;
            float weightSum = 0;
        
            List<((string, int), float, int)> neighbors = brain.adjConnections.FindNeighbours(node);
            foreach (((string, int) male, float weight, int variableIndex) in neighbors)
            {
                float variable = 1;
                if (variableIndex != -1)
                {
                    variable = inputs[variableIndex];
                }

                if (weight != 0 && variable != 0) { sum += variable * weight * adjCompute(male); }
            }

            float result = Sigmoid(sum);

            if (sum == 0 || float.IsNaN(result)) { result = 0; }

            if (type == 1) { middle[index] = result; }
            return result;
        }
            return 0;   
    }

    void calcOutputs()
    {
        for (int i = 0; i < outputs.Length; i++)
        {
            if (brain.adjConnections.FindNeighbours(("output", i)).Count > 0) { outputs[i] = adjCompute(("output", i)); }
        }
    }

    public bool notDead()
    {
        bool box = GetComponent<BoxCollider2D>().isActiveAndEnabled;
        bool brain = GetComponent<Brain>().isActiveAndEnabled;
        bool doidd = GetComponent<Doid>().isActiveAndEnabled;
        return box && brain && doidd;
    }

    void aSex()
    {
        birthing = true;
        //Debug.Log("birth");
        if (notDead())
        {
            GameObject baby = Instantiate(Baby, doid.position, doid.rotation) as GameObject;
            //baby.GetComponent<Brain>().mutate(brain.connections, brain.mutationRate/Sigmoid(age * 25));
            baby.GetComponent<Brain>().adjMutate(brain.connections(), brain.mutationRate * (1 - age / ageMax));

            float newRadius = radius + UnityEngine.Random.Range(-radiusMutation, radiusMutation);
            if (UnityEngine.Random.Range(0f, 100f) < brain.mutationRate * 100 * (1 - age/ageMax)  && newRadius >= radiusMin && newRadius <= radiusMax)
            {
                baby.GetComponent<Doid>().radius = newRadius;
            } else { baby.GetComponent<Doid>().radius = radius; }

            float newForceMax = forceMax + UnityEngine.Random.Range(-forceMutation, forceMutation);
            if (UnityEngine.Random.Range(0f, 100f) < brain.mutationRate * 100 * (1 - age / ageMax) && newForceMax >= 0 && newForceMax <= world.forceMax)
            {
                baby.GetComponent<Doid>().forceMax = newForceMax;
            } else { baby.GetComponent<Doid>().forceMax = forceMax; }

            float newBirthCost = birthCost + UnityEngine.Random.Range(-birthMutation, birthMutation);
            if (UnityEngine.Random.Range(0f, 100f) < brain.mutationRate * 100 * (1 - age / ageMax) && newBirthCost >= birthMin && newBirthCost <= energyMax)
            {
                baby.GetComponent<Doid>().birthCost = newBirthCost;
            }
            else { baby.GetComponent<Doid>().birthCost = birthCost; }

            float newWidth = width + UnityEngine.Random.Range(-.01f, .01f);
            if (UnityEngine.Random.Range(0f, 100f) < brain.mutationRate * 100 * (1 - age / ageMax) && newWidth >= world.widthMin && newWidth <= world.widthMax)
            {
                baby.GetComponent<Doid>().width = newWidth;
            }
            else { baby.GetComponent<Doid>().width = width; }

            float newHeight = height + UnityEngine.Random.Range(-.01f, .01f);
            if (UnityEngine.Random.Range(0f, 100f) < brain.mutationRate * 100 * (1 - age / ageMax) && newHeight>= world.heightMin && newHeight <= world.heightMax)
            {
                baby.GetComponent<Doid>().height = newHeight;
            }
            else { baby.GetComponent<Doid>().height = height; }

            baby.GetComponent<Doid>().species = species;
            baby.GetComponent<Doid>().generation = generation + 1;

            baby.GetComponent<Doid>().energy = birthCost / 2;

            Vector3 newColor = (new Vector3(light.color[0] + UnityEngine.Random.Range(-.25f, .25f), light.color[1] + UnityEngine.Random.Range(-.25f, .25f), light.color[2] + UnityEngine.Random.Range(-.25f, .25f))).normalized;
            baby.GetComponent<Doid>().light.color = new Color(newColor.x, newColor.y, newColor.z, 0);

            baby.GetComponent<Doid>().doid.localScale = new Vector3(baby.GetComponent<Doid>().height / 3, baby.GetComponent<Doid>().width / 3, 1);
            float volume = baby.GetComponent<Doid>().height * baby.GetComponent<Doid>().width;
            float offset = (world.heightMax * world.widthMax) / 4;
            baby.GetComponent<Doid>().forceMax = world.forceMax * (1 - (volume / ((world.heightMax * world.widthMax) + offset)));

            baby.GetComponent<Doid>().birthing = false;
            energy -= birthCost;
            births++;
        }
        birthing = false;
    }

    void Sex(GameObject Male)
    {
        birthing = true;
        if (notDead() /*&& Male.GetComponent<Doid>().notDead()*/)
        {
            GameObject baby = Instantiate(Baby, doid.position, doid.rotation) as GameObject;
            //baby.GetComponent<Brain>().fuse(Male.GetComponent<Brain>().connections, brain.connections, brain.mutationRate / Sigmoid(age * 25));
            baby.GetComponent<Brain>().adjFuse(Male.GetComponent<Brain>().connections(), brain.connections(), brain.mutationRate * (1 - age / ageMax));

            
            if((Male.GetComponent<Doid>().radius + radius) / 2 >= radiusMin && (Male.GetComponent<Doid>().radius + radius) / 2 <= radiusMin)
            {
                baby.GetComponent<Doid>().radius = radiusMin;
            }
            float newRadius = baby.GetComponent<Doid>().radius + UnityEngine.Random.Range(-radiusMutation, radiusMutation);
            if (UnityEngine.Random.Range(0f, 100f) < brain.mutationRate * 100 * (1 - age / ageMax) && newRadius >= radiusMin && newRadius <= radiusMax)
            {
                baby.GetComponent<Doid>().radius = newRadius;
            }


            baby.GetComponent<Doid>().forceMax = (Male.GetComponent<Doid>().forceMax + forceMax) / 2;
            float newForceMax = baby.GetComponent<Doid>().forceMax + UnityEngine.Random.Range(-forceMutation, forceMutation);
            if (UnityEngine.Random.Range(0f, 100f) < brain.mutationRate * 100 * (1 - age / ageMax) && newForceMax >= 0 && newForceMax <= world.forceMax)
            {
                baby.GetComponent<Doid>().forceMax = newForceMax;
            }


            baby.GetComponent<Doid>().birthCost = (Male.GetComponent<Doid>().birthCost + birthCost) / 2;
            float newBirthCost = baby.GetComponent<Doid>().birthCost + UnityEngine.Random.Range(-birthMutation, birthMutation);
            if (UnityEngine.Random.Range(0f, 100f) < brain.mutationRate * 100 * (1 - age / ageMax) && newBirthCost >= 0 && newBirthCost <= world.forceMax)
            {
                baby.GetComponent<Doid>().birthCost = newBirthCost;
            }

            baby.GetComponent<Doid>().width = (Male.GetComponent<Doid>().width + width) / 2;
            float newWidth = baby.GetComponent<Doid>().width + UnityEngine.Random.Range(-.01f, .01f);
            if (UnityEngine.Random.Range(0f, 100f) < brain.mutationRate * 100 * (1 - age / ageMax) && newWidth >= world.widthMin && newWidth <= world.widthMax)
            {
                baby.GetComponent<Doid>().width = newWidth;
            }

            baby.GetComponent<Doid>().height = (Male.GetComponent<Doid>().height + height) / 2;
            float newHeight = baby.GetComponent<Doid>().height + UnityEngine.Random.Range(-.01f, .01f);
            if (UnityEngine.Random.Range(0f, 100f) < brain.mutationRate * 100 * (1 - age / ageMax) && newHeight >= world.heightMin && newHeight <= world.heightMax)
            {
                baby.GetComponent<Doid>().height = newHeight;
            }

            if (!String.Equals(species, Male.GetComponent<Doid>().species))
            {
                baby.GetComponent<Doid>().species = "(" + species + "," + Male.GetComponent<Doid>().species + ")";
            } else
            {
                baby.GetComponent<Doid>().species = species;
            }

            baby.GetComponent<Doid>().generation = generation + 1;

            baby.GetComponent<Doid>().energy = birthCost / 2;

            baby.GetComponent<Doid>().light.color = (light.color + Male.GetComponent<Doid>().light.color) / 2;
            
            baby.GetComponent<Doid>().doid.localScale = new Vector3(baby.GetComponent<Doid>().height / 3, baby.GetComponent<Doid>().width / 3, 1);
            float volume = baby.GetComponent<Doid>().height * baby.GetComponent<Doid>().width;
            float offset = (world.heightMax * world.widthMax) / 4;
            baby.GetComponent<Doid>().forceMax = world.forceMax * (1 - (volume / ((world.heightMax * world.widthMax) + offset)));

            baby.GetComponent<Doid>().birthing = false;
            
            Male.GetComponent<Doid>().energy -= birthCost / 2;
            energy -= birthCost / 2;
            births++;
        }
        birthing = false;
    }

    void drawArm(Vector3 pos1, Vector3 pos2)
    {
        GameObject arm = Instantiate(Arm, doid.position, doid.rotation) as GameObject;
        arm.GetComponent<LineRenderer>().SetPosition(0, pos1);
        arm.GetComponent<LineRenderer>().SetPosition(1, pos2);
        arm.GetComponent<LineRenderer>().SetColors(Color.white, Color.white);
    }

    void drawMouth(Vector3 pos1, Vector3 pos2)
    {
        GameObject mouth = Instantiate(Mouth, doid.position, doid.rotation) as GameObject;
        mouth.GetComponent<LineRenderer>().SetPosition(0, pos1);
        mouth.GetComponent<LineRenderer>().SetPosition(1, pos2);
        mouth.GetComponent<LineRenderer>().SetColors(Color.white, light.color);
    }

    void spawnFood(int num)
    {
        for(int i = 0; i < num; i++)
        {
            if(GameObject.FindGameObjectsWithTag("Food").Length < world.foodMax)
            {
                Instantiate(Food, doid.position, doid.rotation);
            }
        }
    }

    void eat(Collision2D pellet)
    {
        float volume = height * width;
        Destroy(pellet.gameObject);
        energy += foodEnergy * (1 - (volume / (world.heightMax * world.widthMax)));
        foodEaten += 2 * (1 - (volume / (world.heightMax * world.widthMax)));
        drawMouth(doid.position, pellet.transform.position);
    }

    void eat(Collider2D pellet)
    {
        float volume = height * width;
        Destroy(pellet.gameObject);
        energy += foodEnergy * (1 - (volume / (world.heightMax * world.widthMax)));
        foodEaten += 2 * (1 - (volume / (world.heightMax * world.widthMax)));
        drawMouth(doid.position, pellet.transform.position);
    }

    void act()
    {
        float magnitude = outputs[0] * forceMax * 35;
        if(dir + (outputs[1] * dirMove) - (dirMove / 2) > 0 && dir + (outputs[1] * dirMove) - (dirMove /2)  <= 1) { dir += (outputs[1] * dirMove) - (dirMove / 2); }
        Vector2 newForce = angleToDir(360 * dir) * magnitude;
        float moveEnergy = rb.velocity.magnitude * 5;//((outputs[0] * forceMax)/world.forceMax)* moveCost;
        //Debug.Log(moveEnergy);
        float volume = height * width;
        float massEnergy = 15 * volume;
        if (energy - (moveEnergy + massEnergy) > 0 && outputs[0] > forceThreshold) { rb.AddForce(newForce * Time.deltaTime, ForceMode2D.Force); energy -= moveEnergy + massEnergy; }
        Vector2 pos = doid.position;
        //Debug.DrawLine(pos, pos + newForce / (forceMax * 4 * 35) , Color.black);
        

        if (UnityEngine.Random.Range(0f, 100f) < outputs[2]*ageBirthChance && (energy - birthCost) > 0 && GameObject.FindGameObjectsWithTag("Doid").Length < world.doidMax && age > ageFoodRatio * 2)
        {
            aSex();
            lastBirthAge = age;
        }
        
    }

    void FixedUpdate()
    {
        Vector4 vecCol = new Vector4(light.color[0], light.color[1], light.color[2], outputs[4]/*age / ageMax*/).normalized * 100;
        light.color = new Color(vecCol.x, vecCol.y, vecCol.z, vecCol.w);
        realHeight = height / 3 + ((age / ageMax) * (2*height/3));
        realWidth = width / 3 + ((age / ageMax) * (2 * width / 3));
        //Debug.Log((1 / width).ToString() + " : " + realWidth.ToString());
        doid.localScale = new Vector3(realHeight, realWidth, 1);
        doid.right = rb.velocity.normalized;
        ageBirthChance = birthChance + age * 2;
        if (age >= ageMax || age >= ageFoodRatio * 2 && foodEaten == 0 || age > ageFoodRatio * 3 && age - ageFoodRatio > ageFoodRatio * foodEaten && foodEaten > 0 || health <= 0)
        {
            spawnFood((int)(foodEaten / 2));
            Destroy(gameObject);
        }

        if (rb.velocity.magnitude > velocityMax)
        {
            rb.velocity = velocityMax * doid.right;
        }
        if (energy < energyMax) { energy += 0.5f; }
        if (energy < energyMin) { energy = energyMin; }
        if (energy > energyMax) { energy = energyMax; }
        input();
        unSee();
        calcOutputs();
        if (energy > 0) { act(); }
        age += growthRate;
    }
}
