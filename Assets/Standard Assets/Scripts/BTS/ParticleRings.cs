    using UnityEngine;
    using System.Collections;
     
    public class ParticleRings : MonoBehaviour {
    /* -----------------------------------------------------------------------------------------------------
        -- ParticleRingEffect --
       
        This script spawns "ring" particle effects, such as are commonly used for explosion
        shockwaves, sensor effects,  propulsion effects, etc.  Note that the script can be
        adjusted to spawn any number of ring particle systems for repeating ring effects.
        Use this feature carefully so as not to adversely impact framerate.  
       
        Assign this script to the transform at the location where the ring effect will be
        centered.  The ring will be generated in the plane specified by the script transform's
        red axis (right) and centered around the green axis (up).  
       ------------------------------------------------------------------------------------------------------ */
     
    // -- ringEffect --
    // This must be set to reference the prefab that contains the particle system components.
    // Those components can be adjusted as usual to achieve the desired appearance of the
    // particles.  Typical "expanding ring" effects are achieved in combination with this script by:
    //  - Starting with default component settings, then
    //  - Particle emitter:
    //      Emit: OFF
    //      Simulate in Worldspace: ON
    //      One Shot: ON
    //  - Particle Renderer:
    //      Materials: Your favorite particle material 8^)
    //  - Particle Animator:
    //      Autodestruct: ON (Prevents accumulation of used-up particle systems!)
    public GameObject ringEffect;
     
    // The expansion speed of the ring.
    public float speed = 2.0f;
     
    // The inner and outer radii determine the width of the ring.
    public float innerRadius = 0.5f;
    public float outerRadius = 1.5f;
     
    // How many ring systems to spawn.  "Infinite" by default.
    public int numberOfRings = 9999999;
     
    // How often a new ring should be spawned.  In seconds.
    public float spawnRate = 5.0f;
     
    /* ------------------------------------------------------------------------------------------------------*/
    // Time at which the last spawn occurred.  
    private float timeOfLastSpawn = 0.0f;
     
    // Count of rings spawned so far.
    private int spawnCount = 0;
    private bool isSpawned = false;
     
    /* ------------------------------------------------------------------------------------------------------
        -- SpawnRing --
       
        This function spawns a new particle effect system each time it's called.  The system
        spawned is the prefab referenced by the public ringEffect variable.
       ------------------------------------------------------------------------------------------------------- */
    private void SpawnRing () {
           
        // Instantiate the effect prefab.
        GameObject effectObject = ((Transform) Instantiate(ringEffect.transform, this.transform.position, this.transform.rotation)).gameObject;
       
        // Parent the new effect to this script's transform.  
        effectObject.transform.parent = this.gameObject.transform;
       
        // Get the particle emitter from the new effect object.
        ParticleEmitter emitter = effectObject.particleEmitter;
       
        // Generate the particles.
        emitter.Emit();
       
        // Extract the particles from the created emitter.  Notice that we copy the particles into a new javascript array.
        // According to the Unity docs example this shouldn't be necessary, but I couldn't get it to work otherwise.  
        // Below, when the updated p array is reassigned to the emitter particle array, the assignment failed when p was
        // simply assigned the value "emitter.particles".
        Particle[] p = new Particle[emitter.particles.Length];
       
        // Loop thru the particles, giving each an initial position and velocity.
        for (var i=0; i < p.Length; i++) {
            p[i] = emitter.particles[i];
            // Generate a random unit vector in the plane defined by our transform's red axis centered around the
            // transform's green axis.  This vector serves as the basis for the initial position and velocity of the particle.
            Vector3 ruv = RandomUnitVectorInPlane(effectObject.transform, effectObject.transform.up);
               
            // Calc the initial position of the particle accounting for the specified ring radii.  Note the use of Range
            // to get a random distance distribution within the ring.
            Vector3 newPos = effectObject.transform.position
                    + ((ruv * innerRadius) + (ruv * Random.Range(innerRadius, outerRadius)));
            p[i].position = newPos; //emitter.particles[i].position;
            p[i].angularVelocity = Random.Range(30f, 60f);
               
            // The velocity vector is simply the unit vector modified by the speed.  The velocity vector is used by the
            // Particle Animator component to move the particles.
            p[i].velocity = ruv * speed; //emitter.particles[i].velocity;
        }
        // Update the actual particles.
        emitter.particles = p;
        emitter.worldVelocity = new Vector3(0f, Random.Range(10f, 20f), 0);
    }
     
    void LateUpdate() {
        // Check to see if it's time to spawn a new particle system.
        float timeSinceLastSpawn = Time.time - timeOfLastSpawn;
        if (timeSinceLastSpawn >= spawnRate && spawnCount < numberOfRings) {
            SpawnRing();
            timeOfLastSpawn = Time.time;
            spawnCount++;
        }
    }
     
    private Vector3 RandomUnitVectorInPlane(Transform xform, Vector3 axis) {
        // Rotate the specified transform's axis thru a random angle.
        xform.Rotate(axis, Random.Range(0.0f, 360.0f), Space.World);
       
        // Get a copy of the rotated axis and normalize it.
        Vector3 ruv = new Vector3(xform.right.x, xform.right.y, xform.right.z);    
        ruv.Normalize();
        return (ruv);
    }
       
    }