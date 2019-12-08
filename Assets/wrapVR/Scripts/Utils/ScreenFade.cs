using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    // Should live on main camera
    [RequireComponent(typeof(Camera))]
    public class ScreenFade : MonoBehaviour
    {
        public class Blender
        {
            public virtual void setBlendFactor(float blendFactor, Material fadeMat)
            {
                fadeMat.color = new Color(0, 0, 0, blendFactor);
            }
        }

        bool m_bDrawFade = false;

        Coroutine _coroFade;
        public Material _FadeMat;
        Blender _blender;

        // Callbacks for when fade starts / finishes
        public event System.Action OnFadeInStarted;
        public event System.Action OnFadeInComplete;
        public event System.Action OnFadeOutStarted;
        public event System.Action OnFadeOutComplete;

        // Fade in or out
        IEnumerator coroFade(bool bIn, float fFadeTime)
        {
            if (bIn && OnFadeInStarted != null)
                OnFadeInStarted();
            else if (!bIn && OnFadeOutStarted != null)
                OnFadeOutStarted();

            m_bDrawFade = true;
            float fElapsed = 0;
            float curBlendFactor = bIn ? 0 : 1;
            _blender.setBlendFactor(curBlendFactor, _FadeMat);

            while(fElapsed < fFadeTime)
            {
                yield return true;
                float fX = fElapsed / fFadeTime;
                curBlendFactor = bIn ? fX : (1 - fX);
                _blender.setBlendFactor(curBlendFactor, _FadeMat);
                fElapsed += Time.deltaTime;
            }

            _coroFade = null;
            curBlendFactor = bIn ? 1 : 0;
            _blender.setBlendFactor(curBlendFactor, _FadeMat);

            if (bIn && OnFadeInComplete != null)
                OnFadeInComplete();
            else if (!bIn && OnFadeOutComplete != null)  
                OnFadeOutComplete();

            if (!bIn)
                m_bDrawFade = false;

            yield break;
        }

        // Fade in / out  
        public void Fade(bool bIn, float fFadeTime, Blender blender = null)
        {
            if (_FadeMat == null)
                _FadeMat = new Material(Shader.Find("wrapVR/Unlit Fade Transparent"));
            else
                _FadeMat = new Material(_FadeMat);

            if (blender == null)
                _blender = new Blender();
            else
                _blender = blender;

            if (_coroFade != null)
                StopCoroutine(_coroFade);
            _coroFade = StartCoroutine(coroFade(bIn, fFadeTime));
        }

        // Draw a quad with our color if we're fading
        private void OnPostRender()
        {
            if (m_bDrawFade)
            {
                _FadeMat.SetPass(0);
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