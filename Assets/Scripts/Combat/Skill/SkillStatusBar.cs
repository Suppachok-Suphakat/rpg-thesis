using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillStatusBar : MonoBehaviour
{
    [SerializeField] Slider bar;
    public int chargeAmount;
    public int maxChargeAmount;

    public void Set(int curr, int max)
    {
        bar.maxValue = max;
        bar.value = curr;
    }
}
