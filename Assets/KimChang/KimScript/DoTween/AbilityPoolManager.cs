using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPoolManager : MonoBehaviour
{
    [SerializeField] private MoveAbilityUI moveAbilityUI;

    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.AddComponent<MoveAbilityUI>();
        }
    }

}
