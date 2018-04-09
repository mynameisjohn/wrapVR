using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    // Arc pointer class that places some prefab along a curve
    // The prefab will represent points along the curve
    public class ParticleArcPointer : ArcPointer
    {
        public float LifetimeOnDeactivate = 0.5f;
        ParticleSystem.Particle[] m_aParticles;
        ParticleSystem m_ActivePS;

        Dictionary<GameObject, GameObject> m_diPrefabToReal = new Dictionary<GameObject, GameObject>();

        // Use this for initialization
        protected override void Start()
        {
            base.Start();

            System.Func<GameObject, int> rnInitPS = (GameObject goPrefab) => 
            {
                if (goPrefab == null)
                    return -1;
                if (goPrefab.GetComponent<ParticleSystem>() == null)
                {
                    Debug.LogWarning("Warning: Particle Arc Pointer has prefab without particle system");
                    return -2;
                }

                // instantiate prefab and child to us
                GameObject goReal = Instantiate(goPrefab);
                goReal.transform.position = transform.position;
                goReal.transform.SetParent(transform);

                // Ensure we can make as many particles as we need
                ParticleSystem.MainModule mm = goReal.GetComponent<ParticleSystem>().main;
                mm.maxParticles = 2 * Source.NumCurvePoints;

                m_diPrefabToReal[goPrefab] = goReal;
                
                return 0;
            };

            // Make sure our prefabs are particle systems
            rnInitPS(OffCurvePrefab);
            rnInitPS(TouchCurvePrefab);
            rnInitPS(TouchPadCurvePrefab);
            rnInitPS(TriggerCurvePrefab);
            m_aParticles = new ParticleSystem.Particle[2 * Source.NumCurvePoints];
        }

        // Set the lifetime of our active system's particles to the prescribed amount
        protected override void clear()
        {
            if (m_ActivePS == null)
                return;

            // update the particle system so its particles die off soon
            int nParticles = m_ActivePS.GetParticles(m_aParticles);
            for (int i = 0; i < nParticles; i++)
            {
                m_aParticles[i].remainingLifetime = LifetimeOnDeactivate;
            }
            m_ActivePS.SetParticles(m_aParticles, nParticles);
            ParticleSystem.MainModule mm = m_ActivePS.main;
            mm.simulationSpeed = 1;
        }

        // Make sure our particle system has enough particles to draw
        // the curve and place particles along the curve, updating system
        protected override void drawCurve(GameObject curvePrefab)
        {
            // Use the prefab to find the real particle system
            GameObject goReal = null;
            if (!m_diPrefabToReal.TryGetValue(curvePrefab, out goReal))
            {
                clear();
                return;
            }
            
            ParticleSystem ps = goReal.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                clear();
                return;
            }

            // Get all current particles and the count of particles
            int nParticles = ps.GetParticles(m_aParticles);

            // If we have to make more particles then emit them and update the array
            if (Source.NumActivePoints > nParticles)
            {
                int nDiff = Source.NumActivePoints - nParticles;
                ps.Emit(nDiff);
                nParticles = ps.GetParticles(m_aParticles);
            }
            // Or if we have too many points then cut off htat range of the array
            else if (Source.NumActivePoints < nParticles)
            {
                nParticles = Source.NumActivePoints;
            }

            // Our size should now match the curve point size
            // Match all curve point positions
            for (int i = 0; i < nParticles; i++)
            {
                m_aParticles[i].position = Source.CurvePoints[i] ;
            }

            // Update the system with the new particle array
            ps.SetParticles(m_aParticles, nParticles);
            m_ActivePS = ps;
            ParticleSystem.MainModule mm = m_ActivePS.main;
            mm.simulationSpeed = 0;

            // Create target prefab if necessary
            if (hasTarget)
            {
                if (m_goTarget == null)
                    m_goTarget = Instantiate(TargetPrefab);

                // Place target prefab
                m_goTarget.SetActive(true);
                m_goTarget.transform.position = Source.CurvePoints[Source.NumActivePoints - 1];
            }
        }
    }
}