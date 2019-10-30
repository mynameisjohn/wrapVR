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
        Material _fadeMat;
        public Shader _FadeShader;

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
            _fadeMat.color = curFadeColor;

            while(fElapsed < fFadeTime)
            {
                yield return true;
                float fX = fElapsed / fFadeTime;
                curFadeColor.a = bIn ? fX : (1 - fX);
                _fadeMat.color = curFadeColor;
                fElapsed += Time.deltaTime;
            }

            m_coroFade = null;
            curFadeColor.a = bIn ? 1 : 0;
            _fadeMat.color = curFadeColor;

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
            if (_FadeShader == null)
                _FadeShader = Shader.Find("wrapVR/Unlit Fade Transparent");
            if (_fadeMat == null)
                _fadeMat = new Material(_FadeShader);

            if (m_coroFade != null)
                StopCoroutine(m_coroFade);
            m_coroFade = StartCoroutine(coroFade(bIn, fFadeTime, fadeColor));
        }

        // Draw a quad with our color if we're fading
        private void OnPostRender()
        {
            if (m_bDrawFade)
            {
                _fadeMat.SetPass(0);
                GL.PushMatrix();
                GL.LoadOrtho();
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