using UnityEngine;
using System.Collections;

//=============================================================================
//  HaloTrafficLight
//  by Healthbar Games (http://healthbargames.pl)
//  author: Mariusz Skowroński
//
//  Simple implementation of TrafficLight
//  For each of three light colors (red, yellow and green) it uses
//  one mesh renderer and one object with halo effect attached.
//  To visualize the states of lights (on / off) it requires two materials:
//  - one with a texture for the lights turned off
//  - and one with a texture for the lights turned on.
//  You can use (like in demo scene) two different materials with single,
//  common texture for light states.
//=============================================================================

namespace HealthbarGames
{
    public class HaloTrafficLight : TrafficLightBase
    {
        public Renderer RedRenderer;
        public GameObject RedHalo;

        public Renderer YellowRenderer;
        public GameObject YellowHalo;

        public Renderer GreenRenderer;
        public GameObject GreenHalo;

        public Material LightsOnMat;
        public Material LightsOffMat;

        public float RedBlinkInterval = 0.5f;

        private bool mInitialized = false;
        private Coroutine mRedBlinkCoroutine = null;

        void Awake()
        {
            if (    (RedRenderer != null || RedHalo != null)
                &&  (YellowRenderer != null || YellowHalo != null)
                &&  (GreenRenderer != null || GreenHalo != null)
                )
            {
                mInitialized = true;
            }
            else
            {
                mInitialized = false;
                Debug.LogError("Some variables haven't been assigned correctly for HaloTrafficLight script.", this);
            }
        }

        // implementation of the callback from TrafficLight - called when lights state has changed
        public override void OnLightStateChanged(bool redLightState, bool yellowLightState, bool greenLightState)
        {
            if (!mInitialized)
                return;

            Debug.Log($"[{name}] OnLightStateChanged red={redLightState} yellow={yellowLightState} green={greenLightState}", this);

            // stop any previous red blinking coroutine before applying the new state
            if (mRedBlinkCoroutine != null)
            {
                StopCoroutine(mRedBlinkCoroutine);
                mRedBlinkCoroutine = null;
            }

            if (redLightState)
            {
                mRedBlinkCoroutine = StartCoroutine(RedBlinkLoop());
            }
            else
            {
                SetRedLight(false);
            }

            if (YellowHalo != null)
                YellowHalo.SetActive(yellowLightState);

            if (YellowRenderer != null)
                YellowRenderer.material = (yellowLightState) ? LightsOnMat : LightsOffMat;

            if (GreenHalo != null)
                GreenHalo.SetActive(greenLightState);

            if (GreenRenderer != null)
                GreenRenderer.material = (greenLightState) ? LightsOnMat : LightsOffMat;
        }

        // continuously toggles the red light on and off while the traffic light is in the 'Stop' state
        private IEnumerator RedBlinkLoop()
        {
            bool lightOn = true;
            while (true)
            {
                SetRedLight(lightOn);
                lightOn = !lightOn;
                yield return new WaitForSeconds(RedBlinkInterval);
            }
        }

        // applies the on/off visual state of the red light
        private void SetRedLight(bool state)
        {
            if (RedHalo != null)
                RedHalo.SetActive(state);

            if (RedRenderer != null)
                RedRenderer.material = (state) ? LightsOnMat : LightsOffMat;
        }
    }
}
