using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.Globalization;

public class PlanetManager : MonoBehaviour
{
    public TextAsset initialConditionsCsv;
    public float dt = 0.01f;
    public double G = 0.01f;

    private GameObject[] bodies;

    // forces[i,j] is the force that the i-th body experiences from the j-th
    private Vector3[,] forces;

    private int stepSkipIndex = 0;
    

    // Start is called before the first frame update
    void Awake()
    {
        PlanetSpawner spawner = new PlanetSpawner(initialConditionsCsv);
        bodies = spawner.bodies;
        forces = new Vector3[bodies.Length, bodies.Length];
    }

    void FixedUpdate()
    {   


        //partial step skipping
        // The first calculation in the nested for loop gets reevaluated only
        // every n-th frame
        int n = 9;
        stepSkipIndex++;
        int quantity = bodies.Length / n;
        if (stepSkipIndex == n) {
            stepSkipIndex = 0;
        }
        int start = quantity * (stepSkipIndex);
        int end = quantity * (stepSkipIndex + 1);
        if (stepSkipIndex == (n-1)) {
            end = bodies.Length;
        }




        // the calculation of forces.
        // G is provided on the top of the class.

        for (int i = start; i < end; i++)
        {   
            PlanetScript bodyScript1 = bodies[i].GetComponent<PlanetScript>();
            Vector3 position1 = bodyScript1.transform.position;
            double mass1 = bodyScript1.mass;

            for (int j = i+1; j < bodies.Length; j++)
            {   

                PlanetScript bodyScript2 = bodies[j].GetComponent<PlanetScript>();
                Vector3 distanceVector = bodyScript2.transform.position - position1;

                double mass2 = bodyScript2.mass;

                float scalar = (float) ( G * mass1 * mass2 / (Math.Pow(distanceVector.magnitude, 3)) );
                Vector3 force = scalar * distanceVector;



                forces[i, j] = force;
                forces[j, i] = -force;


            
            }
        }

        
        

        // Force summation and assignment
        for (int i = start; i < end; i++)
        {   
            Vector3 summedForce = new Vector3(0, 0, 0);

            for (int j = 0; j < bodies.Length; j++)
            {   
                if(i == j)
                {
                    continue;
                }

                summedForce += forces[i, j];
            
            }

            PlanetScript bodyScript = bodies[i].GetComponent<PlanetScript>();
            bodyScript.assignForce(summedForce);

        }




        // Apply calculated forces
        for (int i = 0; i < bodies.Length; i++)
        {
            PlanetScript bodyScript = bodies[i].GetComponent<PlanetScript>();
            bodyScript.applyForce(dt);
        }
    }
}
