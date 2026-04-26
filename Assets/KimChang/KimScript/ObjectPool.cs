using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab;   //전투 데미지
    [SerializeField] private GameObject battleUnitPrefab;    //전투화면 유닛
    [SerializeField] private Transform canvasTransform;         //캔버스
    [SerializeField] private GameObject abilityPrefab;      //특성+기술 아이콘
    [SerializeField] private GameObject warRelicPrefab;     //전쟁유산
    [SerializeField] private GameObject onlyUnitPrefab;     //배경 없는 유닛
    [SerializeField] private GameObject selectUnitPrefab;   //선택 가능한 유닛
    [SerializeField] private GameObject orderUnitPrefab;
    [SerializeField] private GameObject weaponImagePrefab;   // 무기(이미지) 프리팹
    [SerializeField] private GameObject crashEffectPrefab;   // 크래시(이미지) 프리팹
    [SerializeField] private GameObject buffDeBuffPrefab; // 버프/디버프 아이콘
    private readonly Queue<GameObject> buffDeBuffPool = new();
    private readonly List<GameObject> activeBuffDeBuffs = new();

    private readonly Queue<GameObject> weaponImagePool = new();
    private readonly Queue<GameObject> crashEffectPool = new();
    private readonly Queue<GameObject> damageTextPool = new();
    private readonly Queue<GameObject> battleUnitPool = new();
    private readonly Queue<GameObject> abilityPool = new();
    private readonly Queue<GameObject> warRelicPool = new();
    private readonly Queue<GameObject> onlyUnitPool = new();
    private readonly Queue<GameObject> selectUnitPool = new();
    private readonly Queue<GameObject> orderUnitPool = new();

    private readonly List<GameObject> activeBattleUnits = new(); // 활성화된 유닛을 추적
    private readonly List<GameObject> activeAbilitys= new();      //활성화된 능력 아이콘 추적
    private int poolSize = 20;

    // 초기 풀 생성
    private void Awake()
    {
        if(orderUnitPrefab == null)
        {
            orderUnitPrefab = Resources.Load<GameObject>("Prefabs/OrderUnit");
        }
        if (weaponImagePrefab == null)
        {
            weaponImagePrefab = Resources.Load<GameObject>("Prefabs/WeaponImage");
        }
        if (crashEffectPrefab == null)
        {
            crashEffectPrefab = Resources.Load<GameObject>("Prefabs/CrashEffect");
        }
        if (buffDeBuffPrefab == null)
        {
            buffDeBuffPrefab = Resources.Load<GameObject>("Prefabs/BuffDeBuff");
        }
        for (int i = 0; i < poolSize; i++)
        {
            GameObject damageInstance = Instantiate(damageTextPrefab, transform);
            GameObject unitInstance = Instantiate(battleUnitPrefab, transform);
            GameObject abilityInstance= Instantiate(abilityPrefab, transform);
            GameObject warRelicInstance = Instantiate(warRelicPrefab, transform);
            GameObject selectUnitInstance = Instantiate(selectUnitPrefab, transform);
            GameObject orderUnitInstance = Instantiate(orderUnitPrefab, transform);

            damageInstance.SetActive(false);
            unitInstance.SetActive(false);
            abilityInstance.SetActive(false);
            warRelicInstance.SetActive(false);
            selectUnitInstance.SetActive(false);
            orderUnitInstance.SetActive(false);

            damageTextPool.Enqueue(damageInstance);
            battleUnitPool.Enqueue(unitInstance);
            abilityPool.Enqueue(abilityInstance);
            warRelicPool.Enqueue(warRelicInstance);
            selectUnitPool.Enqueue(selectUnitInstance);
            orderUnitPool.Enqueue(orderUnitInstance);

            if (onlyUnitPrefab != null)
            {
                GameObject onlyUnitInstance = Instantiate(onlyUnitPrefab, transform);
                onlyUnitInstance.SetActive(false);
                onlyUnitPool.Enqueue(onlyUnitInstance);
            }
            if (buffDeBuffPrefab != null)
            {
                GameObject buffDeBuffInstance = Instantiate(buffDeBuffPrefab, transform);
                buffDeBuffInstance.SetActive(false);
                buffDeBuffPool.Enqueue(buffDeBuffInstance);
            }
            GameObject weaponImgInstance = Instantiate(weaponImagePrefab, transform);
            GameObject crashImgInstance = Instantiate(crashEffectPrefab, transform);

            weaponImgInstance.SetActive(false);
            crashImgInstance.SetActive(false);

            weaponImagePool.Enqueue(weaponImgInstance);
            crashEffectPool.Enqueue(crashImgInstance);
        }
    }

    //능력 아이콘 가져오기
    public GameObject GetAbility()
    {
        GameObject instance;

        if (abilityPool.Count > 0)
        {
            instance = abilityPool.Dequeue();
        }
        else
        {
            instance =Instantiate(abilityPrefab, transform);
        }

        instance.SetActive(true);
        instance.transform.SetParent(canvasTransform,false);
        activeAbilitys.Add(instance);
        return instance;
    }

    // 활성화된 아이콘 리스트 반환
    public List<GameObject> GetActiveAbilitys()
    {
        return new List<GameObject>(activeAbilitys); // 활성화된 유닛 복사본 반환
    }

    //능력 아이콘 반환
    public void ReturnAbility(GameObject gameObject)
    {
        gameObject.SetActive(false);
        gameObject.transform.SetParent(canvasTransform,false);
        activeAbilitys.Remove(gameObject);
        abilityPool.Enqueue(gameObject);

    }

    //능력 아이콘 전부 비활성화
    public void ClearActiveAbilitys()
    {
        foreach (var unit in activeAbilitys)
        {
            unit.SetActive(false);
            unit.transform.SetParent(canvasTransform, false);
            abilityPool.Enqueue(unit);
        }
        activeAbilitys.Clear();
    }


    // 유닛 가져오기
    public GameObject GetBattleUnit()
    {
        GameObject instance;

        if (battleUnitPool.Count > 0)
        {
            instance = battleUnitPool.Dequeue();
        }
        else
        {
            instance = Instantiate(battleUnitPrefab, transform);
        }

        instance.SetActive(true);
        instance.transform.SetParent(canvasTransform, false);
        activeBattleUnits.Add(instance); // 활성화된 유닛 리스트에 추가
        return instance;
    }

    // 활성화된 유닛 리스트 반환
    public List<GameObject> GetActiveBattleUnits()
    {
        return new List<GameObject>(activeBattleUnits); // 활성화된 유닛 복사본 반환
    }

    //유닛반환
    public void ReturnBattleUnit(GameObject unitImage)
    {
        unitImage.name = "Ready";
        unitImage.SetActive(false);
        unitImage.transform.SetParent(canvasTransform, false);
        activeBattleUnits.Remove(unitImage);
        battleUnitPool.Enqueue(unitImage);
    }

    //유닛 전부 비활성화
    public void ClearActiveBattleUnits()
    {
        foreach (var unit in activeBattleUnits)
        {
            unit.SetActive(false);
            unit.transform.SetParent(canvasTransform, false);
            battleUnitPool.Enqueue(unit);
        }
        activeBattleUnits.Clear();
    }


    //함수 호출 시 있다면 비활성화된 text반환 및 풀에서 제거 없다면 생성
    public GameObject GetDamageText()
    {
        GameObject instance;

        if (damageTextPool.Count > 0)
        {
            instance = damageTextPool.Dequeue();
        }
        else
        {
            instance = Instantiate(damageTextPrefab, transform);
        }

        instance.SetActive(true);
        instance.transform.SetParent(canvasTransform, false);
        RectTransform rectTransform = instance.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero; // 초기 위치 설정
        return instance;
    }

    //함수 호출 시 text비활성화 시키고 pooling
    public void ReturnDamageText(GameObject damageText)
    {
        damageText.transform.DOKill(false); // 사용처: 풀 반환 시 잔여 트윈 제거(완료 처리 X)
        damageText.SetActive(false);
        damageText.transform.SetParent(canvasTransform, false);
        damageTextPool.Enqueue(damageText);
    }


    //유산 가져오기
    public GameObject GetWarRelic()
    {
        GameObject instance;

        if(warRelicPool.Count > 0)
        {
            instance=warRelicPool.Dequeue();
        }
        else
        {
            instance = Instantiate(warRelicPrefab,transform);
        }
        instance.SetActive(true);
        instance.transform.SetParent(canvasTransform, false);

        return instance;
    }

    //유산 반환
    public void ReturnWarRelic(GameObject gameObject)
    {
        gameObject.SetActive(false);
        gameObject.transform.SetParent(canvasTransform, false);
        warRelicPool.Enqueue(gameObject);
    }

    //배경 없는 유닛 가져오기
    public GameObject GetOnlyUnit()
    {
        GameObject instance;

        if (onlyUnitPool.Count > 0)
        {
            instance = onlyUnitPool.Dequeue();
        }
        else
        {
            instance = Instantiate(onlyUnitPrefab, transform);
        }
        instance.SetActive(true);
        instance.transform.SetParent(canvasTransform, false);

        return instance;
    }

    //배경 없는 유닛 반환
    public void ReturnOnlyUnit(GameObject gameObject)
    {
        gameObject.SetActive(false);
        onlyUnitPool.Enqueue(gameObject);
    }

    //선택가능한 유닛 가져오기
    public GameObject GetSelectUnit()
    {
        GameObject instance;

        if (selectUnitPool.Count > 0)
        {
            instance = selectUnitPool.Dequeue();
        }
        else
        {
            instance = Instantiate(selectUnitPrefab, transform);
        }

        instance.SetActive(true);
        instance.transform.SetParent(canvasTransform, false);
        return instance;
    }
    // 선택 가능한 유닛 회수 (부모는 유지, 자식만 풀에 등록)
    public void ReturnSelectUnit(GameObject parentObj)
    {
        foreach (Transform child in parentObj.transform)
        {
            GameObject childObj = child.gameObject;
            childObj.SetActive(false);
            selectUnitPool.Enqueue(childObj);
        }
    }

    public GameObject GetOrderUnit()
    {
        GameObject instance;

        if (orderUnitPool.Count > 0)
        {
            instance = orderUnitPool.Dequeue();
        }
        else
        {
            instance = Instantiate(orderUnitPrefab, transform);
        }

        instance.SetActive(true);
        instance.transform.SetParent(canvasTransform, false);
        return instance;
    }

    public void ReturnOrderUnit(GameObject orderUnit)
    {
        orderUnit.SetActive(false);
        orderUnit.transform.SetParent(canvasTransform, false);
        orderUnitPool.Enqueue(orderUnit);
    }

    // 3) 무기 이미지 가져오기/반환
    public GameObject GetWeaponImage()
    {
        GameObject instance = weaponImagePool.Count > 0
            ? weaponImagePool.Dequeue()
            : Instantiate(weaponImagePrefab, transform);

        instance.SetActive(true);
        instance.transform.SetParent(canvasTransform, false);

        // 초기화(성능 우선: 필수만)
        var rt = (RectTransform)instance.transform;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        var img = instance.GetComponent<Image>();
        if (img != null) img.enabled = true;

        return instance;
    }

    public void ReturnWeaponImage(GameObject go)
    {
        if (go == null) return;
        go.SetActive(false);
        go.transform.SetParent(canvasTransform, false);

        // 안전 초기화
        var rt = (RectTransform)go.transform;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        var img = go.GetComponent<Image>();
        if (img != null) img.sprite = null;

        weaponImagePool.Enqueue(go);
    }

    // 4) 크래시 이미지 가져오기/반환
    public GameObject GetCrashEffect()
    {
        GameObject instance = crashEffectPool.Count > 0
            ? crashEffectPool.Dequeue()
            : Instantiate(crashEffectPrefab, transform);

        instance.SetActive(true);
        instance.transform.SetParent(canvasTransform, false);

        var rt = (RectTransform)instance.transform;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        var img = instance.GetComponent<Image>();
        if (img != null) img.enabled = false; // 표시 타이밍은 연출측에서 on

        return instance;
    }

    public void ReturnCrashEffect(GameObject go)
    {
        if (go == null) return;
        go.SetActive(false);
        go.transform.SetParent(canvasTransform, false);

        var img = go.GetComponent<Image>();
        if (img != null) img.enabled = false;

        crashEffectPool.Enqueue(go);
    }

    // 사용처: 버프/디버프 아이콘을 풀에서 꺼내 UI에 표시
    public GameObject GetBuffDeBuff()
    {
        if (buffDeBuffPrefab == null)
        {
            Debug.LogWarning("[ObjectPool] BuffDeBuff 프리팹이 없습니다. Resources/Prefabs/BuffDeBuff 경로를 확인하세요.");
            return null;
        }

        GameObject instance = buffDeBuffPool.Count > 0
            ? buffDeBuffPool.Dequeue()
            : Instantiate(buffDeBuffPrefab, transform);

        instance.SetActive(true);
        instance.transform.SetParent(canvasTransform, false);

        RectTransform rt = instance.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
        }

        Image img = instance.GetComponent<Image>();
        if (img != null)
        {
            img.enabled = true;
            img.sprite = null;
            img.color = Color.white;
        }

        ItemInformation itemInfo = instance.GetComponent<ItemInformation>();
        if (itemInfo != null)
            itemInfo.Clear();

        activeBuffDeBuffs.Add(instance);
        return instance;
    }

    // 사용처: 사용이 끝난 버프/디버프 아이콘을 풀로 반환
    public void ReturnBuffDeBuff(GameObject go)
    {
        if (go == null)
            return;

        go.SetActive(false);
        go.transform.SetParent(canvasTransform, false);

        Image img = go.GetComponent<Image>();
        if (img != null)
            img.sprite = null;

        ItemInformation itemInfo = go.GetComponent<ItemInformation>();
        if (itemInfo != null)
            itemInfo.Clear();

        activeBuffDeBuffs.Remove(go);
        buffDeBuffPool.Enqueue(go);
    }

    // 사용처: 체력/상태 UI 갱신 전에 현재 표시 중인 버프/디버프 아이콘을 전부 정리
    public void ClearActiveBuffDeBuffs()
    {
        for (int i = activeBuffDeBuffs.Count - 1; i >= 0; i--)
        {
            GameObject go = activeBuffDeBuffs[i];
            if (go == null)
                continue;

            go.SetActive(false);
            go.transform.SetParent(canvasTransform, false);

            Image img = go.GetComponent<Image>();
            if (img != null)
                img.sprite = null;

            ItemInformation itemInfo = go.GetComponent<ItemInformation>();
            if (itemInfo != null)
                itemInfo.Clear();

            buffDeBuffPool.Enqueue(go);
        }

        activeBuffDeBuffs.Clear();
    }

}

