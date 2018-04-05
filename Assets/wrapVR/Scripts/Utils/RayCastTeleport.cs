using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class RayCastTeleport : MonoBehaviour
    {
        public System.Action OnPreTeleport;
        public System.Action OnTeleport;

        public Transform ToTeleport;
        public VRRayCaster RayCaster;
        public EActivation Activation;

        public bool DoubleClick;
        public float DoubleClickTimer;

        public bool Fade;
        public float FadeTime;
        float m_fFadeAlpha;

        Texture2D m_BlackTex;

        // Use this for initialization
        void Start()
        {
            if (Fade)
            {
                m_BlackTex = new Texture2D(100, 100);
                for (int i = 0; i < m_BlackTex.width; i++)
                    for (int j = 0; j < m_BlackTex.height; j++)
                        m_BlackTex.SetPixel(i, j, Color.black);
            }

            if (ToTeleport == null)
                ToTeleport = transform;

            RayCaster = Util.DestroyEnsureComponent(gameObject, RayCaster);

            switch (Activation)
            {
                case EActivation.NONE:
                    break;
                case EActivation.TOUCH:
                    RayCaster.Input.OnTouchpadTouchDown += Input_OnActivationDown;
                    break;
                case EActivation.TOUCHPAD:
                    RayCaster.Input.OnTouchpadDown += Input_OnActivationDown;
                    break;
                case EActivation.TRIGGER:
                    RayCaster.Input.OnTriggerDown += Input_OnActivationDown;
                    break;
            }
        }

        Coroutine m_coroDoubleClick;
        IEnumerator doubleClickCoro()
        {
            yield return new WaitForSeconds(DoubleClickTimer);
            m_coroDoubleClick = null;
            yield break;
        }

        //Coroutine m_coroFade;
        //IEnumerator coroTeleport(Vector3 v3Dest)
        //{
        //    if (Fade == false)
        //    {
        //        if (OnPreTeleport != null)
        //            OnPreTeleport();

        //        ToTeleport.transform.position = v3Dest;

        //        if (OnTeleport != null)
        //            OnTeleport();

        //        yield break;
        //    }

        //    Camera cam = Camera.main;
        //    float tStart = Time.time;
        //    float tEnd = tStart + FadeTime;

        //    while(Time.time < tEnd)
        //    {
        //        m_fFadeAlpha = Mathf.Lerp(0, 1, (Time.time - tStart) / FadeTime);
        //        GUI.color = m_fFadeAlpha * Color.white;
        //        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), m_BlackTex);
        //        yield return true;
        //    }

        //    if (OnPreTeleport != null)
        //        OnPreTeleport();

        //    ToTeleport.transform.position = v3Dest;

        //    while (Time.time < tEnd)
        //    {
        //        m_fFadeAlpha = Mathf.Lerp(1, 0, (Time.time - tStart) / FadeTime);
        //        GUI.color = m_fFadeAlpha * Color.white;
        //        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), m_BlackTex);
        //        yield return true;
        //    }

        //    if (OnTeleport != null)
        //        OnTeleport();

        //    m_coroFade = null;
        //    yield break;
        //}

        enum FadeState
        {
            NONE,
            IN,
            OUT
        }
        FadeState m_eFadeState = FadeState.NONE;
        float m_fFadeStart;
        private void OnGUI()
        {
            if (m_eFadeState == FadeState.NONE)
                return;

            float fFadeAmount = 0;
            if (m_eFadeState == FadeState.IN)
            {
                if (Time.time > m_fFadeStart + FadeTime)
                {
                    m_eFadeState = FadeState.OUT;
                    if (OnPreTeleport != null)
                        OnPreTeleport();

                    Vector3 delta = RayCaster.GetLastHitPosition() - transform.position;
                    VRCapabilityManager.instance.transform.parent.position += delta;

                    ToTeleport.transform.position = RayCaster.GetLastHitPosition();
                    if (OnTeleport != null)
                        OnTeleport();
                }
                else
                {
                    fFadeAmount = (Time.time - m_fFadeStart) / FadeTime;
                    fFadeAmount = Mathf.Lerp(0, 1, fFadeAmount);
                }
            }
            if (m_eFadeState == FadeState.OUT)
            {
                if (Time.time > m_fFadeStart + 2 * FadeTime)
                {
                    m_eFadeState = FadeState.NONE;
                    return;
                }
                fFadeAmount = (Time.time - FadeTime - m_fFadeStart) / FadeTime;
                fFadeAmount = Mathf.Lerp(1, 0, fFadeAmount);
            }

            GUI.color = fFadeAmount * Color.black;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), m_BlackTex);
        }

        void startFade()
        {
            m_fFadeStart = Time.time;
            m_eFadeState = FadeState.IN;
        }
            
        private void Input_OnActivationDown()
        {
            if (DoubleClick)
            {
                if (m_coroDoubleClick != null)
                {
                    StopCoroutine(m_coroDoubleClick);
                    m_coroDoubleClick = null;
                    teleport();
                }
                else
                {
                    m_coroDoubleClick = StartCoroutine(doubleClickCoro());
                }
            }
            else
            {
                teleport();
            }
        }

        void teleport()
        {
            // Don't do nothin if we aren't over an interactible
            if (RayCaster.CurrentInteractible == null)
                return;

            if (m_coroDoubleClick != null)
                return;

            if (Fade)
                startFade();
            else
            {
                if (OnPreTeleport != null)
                    OnPreTeleport();
                ToTeleport.transform.position = RayCaster.GetLastHitPosition();
                if (OnTeleport != null)
                    OnTeleport();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}