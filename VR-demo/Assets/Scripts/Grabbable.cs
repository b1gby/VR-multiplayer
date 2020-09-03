using System;
using UnityEngine;
using OVRTouchSample;

public class Grabbable : MonoBehaviour

{
    public string m_materialColorField;

    GrabManager m_crosshairManager;
    Renderer m_renderer;
    MaterialPropertyBlock m_mpb;
    

    public bool InRange
    {
        get { return m_inRange; }
        set
        {
            m_inRange = value;
            //RefreshCrosshair();
        }
    }
    bool m_inRange;

    public bool Targeted
    {
        get { return m_targeted; }
        set
        {
            m_targeted = value;
            //RefreshCrosshair();
        }
    }
    bool m_targeted;

    public Color CurColor
    {
        get { return m_mpb.GetColor(m_materialColorField); }
    }

    void Start()
    {
        m_renderer = gameObject.GetComponent<Renderer>();
        m_crosshairManager = FindObjectOfType<GrabManager>();
        m_mpb = new MaterialPropertyBlock();
        RefreshCrosshair();
        m_mpb.SetColor(m_materialColorField, Color.white);
        m_renderer.SetPropertyBlock(m_mpb);
    }

    void RefreshCrosshair()
    {
        //if (m_crosshair)
        //{
        //    if (isGrabbed) m_crosshair.SetState(GrabbableCrosshair.CrosshairState.Disabled);
        //    else if (!InRange) m_crosshair.SetState(GrabbableCrosshair.CrosshairState.Disabled);
        //    else m_crosshair.SetState(Targeted ? GrabbableCrosshair.CrosshairState.Targeted : GrabbableCrosshair.CrosshairState.Enabled);
        //}
        if (m_materialColorField != null)
        {
            m_renderer.GetPropertyBlock(m_mpb);
            //if (isGrabbed || !InRange) m_mpb.SetColor(m_materialColorField, Color.white);
            //else if (Targeted) m_mpb.SetColor(m_materialColorField, m_crosshairManager.OutlineColorHighlighted);
            //else 
            m_mpb.SetColor(m_materialColorField, Color.white);
            //m_mpb.SetColor(m_materialColorField, m_crosshairManager.OutlineColorInRange);
            m_renderer.SetPropertyBlock(m_mpb);
        }
    }

    public void SetColor(Color focusColor)
    {
        m_mpb.SetColor(m_materialColorField, focusColor);
        m_renderer.SetPropertyBlock(m_mpb);
    }

    public void ClearColor()
    {
        m_mpb.SetColor(m_materialColorField, Color.white);
        m_renderer.SetPropertyBlock(m_mpb);
    }
}
