using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColision : MonoBehaviour
{
    private ArrayList numbers = new ArrayList();
    [SerializeField]
    int requiredNumbers;

    void OnCollisionEnter(Collision collisonInfo)
    {
        var objectName = collisonInfo.collider.name;
        int objectIntegerName; if(!int.TryParse(objectName, out objectIntegerName)) return;

        if(objectName == "1")
        {
            numbers.Add(objectIntegerName);
            Destroy(collisonInfo.gameObject);
        }

        if(objectName == "2")
        {
            numbers.Add(objectIntegerName);
            Destroy(collisonInfo.gameObject);
        }    

    }
}
