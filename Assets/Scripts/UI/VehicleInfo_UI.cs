using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class VehicleInfo_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI distance;
    [SerializeField] private TextMeshProUGUI time;
    [SerializeField] private TextMeshProUGUI speed;
    [SerializeField] private TextMeshProUGUI p_Count;

    [SerializeField] private TextMeshProUGUI itemDeliveryTmp;
    [SerializeField] private TextMeshProUGUI infoTmp;

    public void UpdateDistance(float _distance)
    {
        distance.text = "Distance : " + _distance.ToString("F2");
    }

    public void UpdateSpeed(float _speed)
    {
        speed.text = "Speed : " + _speed.ToString("F2");
    }

    public void UpdateTime(float _time)
    {
        time.text = "Time : " + _time.ToString("F2");
    }

    public void UpdateP_Count(int _pCount)
    {
        p_Count.text = "Count : " + _pCount.ToString();
    }


    private Coroutine HideInfoRoutine;

    public void UpdateInformation(string _info)
    {
        infoTmp.text = _info;
    }

    public void HideInformationText()
    {
        infoTmp.text = "";
    }

    public void ShowDeliveryText()
    {
        itemDeliveryTmp.alpha = 1;
        itemDeliveryTmp.text = "Item Dropped!";
        StartCoroutine(HideText());
    }

    IEnumerator HideText()
    {
        yield return new WaitForSeconds(1);
        itemDeliveryTmp.alpha = 0;
    }
}