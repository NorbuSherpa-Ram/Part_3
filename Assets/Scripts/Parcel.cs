using UnityEngine;

public class Parcel : MonoBehaviour
{
    [SerializeField] private GameObject parcelObject; 
    public void ShowParcel()
    {
        parcelObject.SetActive(true);
    }

    public void HideParcel()
    {
        parcelObject.SetActive(false);
    }
}