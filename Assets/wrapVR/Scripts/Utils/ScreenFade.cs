using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    // Should live on main camera
    [RequireComponent(typeof(Camera))]
    public class ScreenFade : MonoBehaviour
    {
        Coroutine m_coroFade;
        bool m_bDrawFade = false;
        public Material m_FadeMat;

        private void Start()
        {
            if (m_FadeMat == null)
            {
                Debug.LogError("Missing Fade material");
                Destroy(this);
            }
        }

        // Callbacks for when fade starts / finishes
        public event System.Action OnFadeInStarted;
        public event System.Action OnFadeInComplete;
        public event System.Action OnFadeOutStarted;
        public event System.Action OnFadeOutComplete;

        // Fade in or out
        IEnumerator coroFade(bool bIn, float fFadeTime, Color fadeColor)
        {
            if (bIn && OnFadeInStarted != null)
                OnFadeInStarted();
            else if (!bIn && OnFadeOutStarted != null)
                OnFadeOutStarted();

            m_bDrawFade = true;
            float fElapsed = 0;
            Color curFadeColor = fadeColor;
            curFadeColor.a = bIn ? 0 : 1;

            while(fElapsed < fFadeTime)
            {
                yield return true;
                float fX = fElapsed / fFadeTime;
                curFadeColor.a = bIn ? fX : (1 - fX);
                m_FadeMat.color = curFadeColor;
                fElapsed += Time.deltaTime;
            }

            m_coroFade = null;

            if (bIn && OnFadeInComplete != null)
                OnFadeInComplete();
            else if (!bIn && OnFadeOutComplete != null)  
                OnFadeOutComplete();

            if (!bIn)
                m_bDrawFade = false;

            yield break;
        }

        // Fade in / out  
        public void Fade(bool bIn, float fFadeTime, Color fadeColor)
        {
            if (m_coroFade != null)
                StopCoroutine(m_coroFade);
            m_coroFade = StartCoroutine(coroFade(bIn, fFadeTime, fadeColor));
        }

        // Draw a quad with our color if we're fading
        private void OnPostRender()
        {
            if (m_bDrawFade)
            {
                m_FadeMat.SetPass(0);
                GL.PushMatrix();
                GL.LoadOrtho();
                GL.Color(m_FadeMat.color);
                GL.Begin(GL.QUADS);
                GL.Vertex3(0, 0, -12);
                GL.Vertex3(0, 1, -12);
                GL.Vertex3(1, 1, -12);
                GL.Vertex3(1, 0, -12);
                GL.End();
                GL.PopMatrix();
            }
        }
    }
}