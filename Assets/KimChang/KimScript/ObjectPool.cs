using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab;   //전투 데미지
    [SerializeField] private GameObject battleUnitPrefab;   //전투화면 유닛
    [SerializeField] private Transform canvasTransform;     //캔버스
    [SerializeField] private GameObject abilityPrefab;      //특성+기술 아이콘
    [SerializeField] private GameObject warRelicPrefab;     //전쟁유산
    [SerializeField] private GameObject onlyUnitPrefab;     //배경 없는 유닛
    [SerializeField] private GameObject selectUnitPrefab;   //선택 가능한 유닛
    [SerializeField] private GameObject orderUnitPrefab;
    [SerializeField] private GameObject weaponImagePrefab;  //무기(이미지) 프리팹
    [SerializeField] private GameObject crashEffectPrefab;  //크래시(이미지) 프리팹
    [SerializeField] private GameObject buffDeBuffPrefab;   //버프/디버프 아이콘

    private readonly Queue<GameObject> buffDeBuffPool = new();
    private readonly List<GameObject> activeBuffDeBuffs = new();

    private readonly Queue<GameObject> weaponImagePool = new();
    private readonly Queue<GameObject> crashEffectPool = new();

    private readonly HashSet<GameObject> weaponImagesInPool = new();
    private readonly HashSet<GameObject> activeWeaponImages = new();

    private readonly Queue<GameObject> damageTextPool = new();
    private readonly Queue<GameObject> battleUnitPool = new();
    private readonly Queue<GameObject> abilityPool = new();
    private readonly Queue<GameObject> warRelicPool = new();
    private readonly Queue<GameObject> onlyUnitPool = new();
    private readonly Queue<GameObject> selectUnitPool = new();
    private readonly Queue<GameObject> orderUnitPool = new();

    private readonly List<GameObject> activeBattleUnits = new();
    private readonly List<GameObject> activeAbilitys = new();

    private int poolSize = 20;

    public int ActiveWeaponImageCount =>
        activeWeaponImages.Count;

    // 초기 풀 생성
    private void Awake()
    {
        if (orderUnitPrefab == null)
        {
            orderUnitPrefab =
                Resources.Load<GameObject>(
                    "Prefabs/OrderUnit");
        }

        if (weaponImagePrefab == null)
        {
            weaponImagePrefab =
                Resources.Load<GameObject>(
                    "Prefabs/WeaponImage");
        }

        if (crashEffectPrefab == null)
        {
            crashEffectPrefab =
                Resources.Load<GameObject>(
                    "Prefabs/CrashEffect");
        }

        if (buffDeBuffPrefab == null)
        {
            buffDeBuffPrefab =
                Resources.Load<GameObject>(
                    "Prefabs/BuffDeBuff");
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject damageInstance =
                Instantiate(
                    damageTextPrefab,
                    transform);

            GameObject unitInstance =
                Instantiate(
                    battleUnitPrefab,
                    transform);

            GameObject abilityInstance =
                Instantiate(
                    abilityPrefab,
                    transform);

            GameObject warRelicInstance =
                Instantiate(
                    warRelicPrefab,
                    transform);

            GameObject selectUnitInstance =
                Instantiate(
                    selectUnitPrefab,
                    transform);

            GameObject orderUnitInstance =
                Instantiate(
                    orderUnitPrefab,
                    transform);

            damageInstance.SetActive(false);
            unitInstance.SetActive(false);
            abilityInstance.SetActive(false);
            warRelicInstance.SetActive(false);
            selectUnitInstance.SetActive(false);
            orderUnitInstance.SetActive(false);

            damageTextPool.Enqueue(
                damageInstance);

            battleUnitPool.Enqueue(
                unitInstance);

            abilityPool.Enqueue(
                abilityInstance);

            warRelicPool.Enqueue(
                warRelicInstance);

            selectUnitPool.Enqueue(
                selectUnitInstance);

            orderUnitPool.Enqueue(
                orderUnitInstance);

            if (onlyUnitPrefab != null)
            {
                GameObject onlyUnitInstance =
                    Instantiate(
                        onlyUnitPrefab,
                        transform);

                onlyUnitInstance.SetActive(false);

                onlyUnitPool.Enqueue(
                    onlyUnitInstance);
            }

            if (buffDeBuffPrefab != null)
            {
                GameObject buffDeBuffInstance =
                    Instantiate(
                        buffDeBuffPrefab,
                        transform);

                buffDeBuffInstance.SetActive(false);

                buffDeBuffPool.Enqueue(
                    buffDeBuffInstance);
            }

            GameObject weaponImgInstance =
                Instantiate(
                    weaponImagePrefab,
                    transform);

            GameObject crashImgInstance =
                Instantiate(
                    crashEffectPrefab,
                    transform);

            weaponImgInstance.SetActive(false);
            crashImgInstance.SetActive(false);

            weaponImagePool.Enqueue(
                weaponImgInstance);

            weaponImagesInPool.Add(
                weaponImgInstance);

            crashEffectPool.Enqueue(
                crashImgInstance);
        }
    }

    // 사용처: 특성/기술 아이콘을 풀에서 꺼내 사용할 때 이전 UI에서 변경한 Transform 상태 초기화
    public GameObject GetAbility()
    {
        GameObject instance;

        if (abilityPool.Count > 0)
        {
            instance =
                abilityPool.Dequeue();
        }
        else
        {
            instance =
                Instantiate(
                    abilityPrefab,
                    transform);
        }

        Transform tr =
            instance.transform;

        tr.SetParent(
            canvasTransform,
            false);

        tr.localScale =
            Vector3.one;

        tr.localRotation =
            Quaternion.identity;

        RectTransform rt =
            tr as RectTransform;

        if (rt != null)
            rt.anchoredPosition = Vector2.zero;

        instance.SetActive(true);

        activeAbilitys.Add(
            instance);

        return instance;
    }

    // 활성화된 아이콘 리스트 반환
    public List<GameObject> GetActiveAbilitys()
    {
        return new List<GameObject>(
            activeAbilitys);
    }

    // 사용처: 특성/기술 아이콘을 풀에 반환하면서 상세창 등에서 변경한 Transform 상태 초기화
    public void ReturnAbility(
        GameObject gameObject)
    {
        if (gameObject == null)
            return;

        gameObject.SetActive(false);

        Transform tr =
            gameObject.transform;

        tr.SetParent(
            canvasTransform,
            false);

        tr.localScale =
            Vector3.one;

        tr.localRotation =
            Quaternion.identity;

        RectTransform rt =
            tr as RectTransform;

        if (rt != null)
            rt.anchoredPosition = Vector2.zero;

        activeAbilitys.Remove(
            gameObject);

        abilityPool.Enqueue(
            gameObject);
    }

    // 사용처: 활성화된 특성/기술 아이콘을 모두 풀에 반환
    public void ClearActiveAbilitys()
    {
        for (int i = activeAbilitys.Count - 1;
             i >= 0;
             i--)
        {
            GameObject ability =
                activeAbilitys[i];

            if (ability == null)
                continue;

            ability.SetActive(false);

            Transform tr =
                ability.transform;

            tr.SetParent(
                canvasTransform,
                false);

            tr.localScale =
                Vector3.one;

            tr.localRotation =
                Quaternion.identity;

            RectTransform rt =
                tr as RectTransform;

            if (rt != null)
                rt.anchoredPosition = Vector2.zero;

            abilityPool.Enqueue(
                ability);
        }

        activeAbilitys.Clear();
    }

    // 유닛 가져오기
    public GameObject GetBattleUnit()
    {
        GameObject instance;

        if (battleUnitPool.Count > 0)
        {
            instance =
                battleUnitPool.Dequeue();
        }
        else
        {
            instance =
                Instantiate(
                    battleUnitPrefab,
                    transform);
        }

        instance.SetActive(true);

        instance.transform.SetParent(
            canvasTransform,
            false);

        activeBattleUnits.Add(
            instance);

        return instance;
    }

    // 활성화된 유닛 리스트 반환
    public List<GameObject> GetActiveBattleUnits()
    {
        return new List<GameObject>(
            activeBattleUnits);
    }

    // 유닛 반환
    public void ReturnBattleUnit(
        GameObject unitImage)
    {
        unitImage.name =
            "Ready";

        unitImage.SetActive(false);

        unitImage.transform.SetParent(
            canvasTransform,
            false);

        activeBattleUnits.Remove(
            unitImage);

        battleUnitPool.Enqueue(
            unitImage);
    }

    // 유닛 전부 비활성화
    public void ClearActiveBattleUnits()
    {
        foreach (var unit in activeBattleUnits)
        {
            unit.SetActive(false);

            unit.transform.SetParent(
                canvasTransform,
                false);

            battleUnitPool.Enqueue(
                unit);
        }

        activeBattleUnits.Clear();
    }

    // 함수 호출 시 비활성화된 데미지 텍스트를 반환하고 없으면 생성
    public GameObject GetDamageText()
    {
        GameObject instance;

        if (damageTextPool.Count > 0)
        {
            instance =
                damageTextPool.Dequeue();
        }
        else
        {
            instance =
                Instantiate(
                    damageTextPrefab,
                    transform);
        }

        instance.SetActive(true);

        instance.transform.SetParent(
            canvasTransform,
            false);

        RectTransform rectTransform =
            instance.GetComponent<RectTransform>();

        rectTransform.anchoredPosition =
            Vector2.zero;

        return instance;
    }

    // 함수 호출 시 데미지 텍스트 비활성화 후 풀에 반환
    public void ReturnDamageText(
        GameObject damageText)
    {
        damageText.transform.DOKill(false);

        damageText.SetActive(false);

        damageText.transform.SetParent(
            canvasTransform,
            false);

        damageTextPool.Enqueue(
            damageText);
    }

    // 유산 가져오기
    public GameObject GetWarRelic()
    {
        GameObject instance;

        if (warRelicPool.Count > 0)
        {
            instance =
                warRelicPool.Dequeue();
        }
        else
        {
            instance =
                Instantiate(
                    warRelicPrefab,
                    transform);
        }

        instance.SetActive(true);

        instance.transform.SetParent(
            canvasTransform,
            false);

        return instance;
    }

    // 유산 반환
    public void ReturnWarRelic(
        GameObject gameObject)
    {
        gameObject.SetActive(false);

        gameObject.transform.SetParent(
            canvasTransform,
            false);

        warRelicPool.Enqueue(
            gameObject);
    }

    // 배경 없는 유닛 가져오기
    public GameObject GetOnlyUnit()
    {
        GameObject instance;

        if (onlyUnitPool.Count > 0)
        {
            instance =
                onlyUnitPool.Dequeue();
        }
        else
        {
            instance =
                Instantiate(
                    onlyUnitPrefab,
                    transform);
        }

        instance.SetActive(true);

        instance.transform.SetParent(
            canvasTransform,
            false);

        return instance;
    }

    // 배경 없는 유닛 반환
    public void ReturnOnlyUnit(
        GameObject gameObject)
    {
        gameObject.SetActive(false);

        onlyUnitPool.Enqueue(
            gameObject);
    }

    // 선택 가능한 유닛 가져오기
    public GameObject GetSelectUnit()
    {
        GameObject instance;

        if (selectUnitPool.Count > 0)
        {
            instance =
                selectUnitPool.Dequeue();
        }
        else
        {
            instance =
                Instantiate(
                    selectUnitPrefab,
                    transform);
        }

        instance.SetActive(true);

        instance.transform.SetParent(
            canvasTransform,
            false);

        return instance;
    }

    // 선택 가능한 유닛 회수
    public void ReturnSelectUnit(
        GameObject parentObj)
    {
        foreach (Transform child in parentObj.transform)
        {
            GameObject childObj =
                child.gameObject;

            childObj.SetActive(false);

            selectUnitPool.Enqueue(
                childObj);
        }
    }

    public GameObject GetOrderUnit()
    {
        GameObject instance;

        if (orderUnitPool.Count > 0)
        {
            instance =
                orderUnitPool.Dequeue();
        }
        else
        {
            instance =
                Instantiate(
                    orderUnitPrefab,
                    transform);
        }

        instance.SetActive(true);

        instance.transform.SetParent(
            canvasTransform,
            false);

        return instance;
    }

    public void ReturnOrderUnit(
        GameObject orderUnit)
    {
        orderUnit.SetActive(false);

        orderUnit.transform.SetParent(
            canvasTransform,
            false);

        orderUnitPool.Enqueue(
            orderUnit);
    }

    // 사용처: 무기 이미지를 풀에서 가져와 충돌/투사체 연출에 사용
    public GameObject GetWeaponImage()
    {
        GameObject instance =
            weaponImagePool.Count > 0
                ? weaponImagePool.Dequeue()
                : Instantiate(
                    weaponImagePrefab,
                    transform);

        weaponImagesInPool.Remove(
            instance);

        // 같은 인스턴스가 큐에 중복으로 들어간 과거 상태가 있더라도 활성 오브젝트를 재대여하지 않는다.
        if (!activeWeaponImages.Add(instance))
        {
            Debug.LogError(
                $"[WeaponPool] frame={Time.frameCount} duplicate dequeue InstanceID={instance.GetInstanceID()}. " +
                "새 인스턴스로 교체합니다.",
                this);

            instance =
                Instantiate(
                    weaponImagePrefab,
                    transform);

            activeWeaponImages.Add(
                instance);
        }

        instance.SetActive(true);

        instance.transform.SetParent(
            canvasTransform,
            false);

        RectTransform rt =
            (RectTransform)instance.transform;

        rt.anchoredPosition =
            Vector2.zero;

        rt.localScale =
            Vector3.one;

        rt.localRotation =
            Quaternion.identity;

        Image img =
            instance.GetComponent<Image>();

        if (img != null)
            img.enabled = true;

        return instance;
    }

    // 사용처: 무기 이미지 연출 완료 후 풀에 반환
    public void ReturnWeaponImage(
        GameObject go)
    {
        if (go == null)
            return;

        if (weaponImagesInPool.Contains(go))
        {
            Debug.LogWarning(
                $"[WeaponPool] frame={Time.frameCount} duplicate return ignored InstanceID={go.GetInstanceID()} " +
                $"activeWeaponImages={activeWeaponImages.Count}",
                this);

            return;
        }

        activeWeaponImages.Remove(
            go);

        go.SetActive(false);

        go.transform.SetParent(
            canvasTransform,
            false);

        RectTransform rt =
            (RectTransform)go.transform;

        rt.anchoredPosition =
            Vector2.zero;

        rt.localScale =
            Vector3.one;

        rt.localRotation =
            Quaternion.identity;

        Image img =
            go.GetComponent<Image>();

        if (img != null)
            img.sprite = null;

        weaponImagesInPool.Add(
            go);

        weaponImagePool.Enqueue(
            go);
    }

    // 사용처: 크래시 이미지를 풀에서 가져와 충돌 연출에 사용
    public GameObject GetCrashEffect()
    {
        GameObject instance =
            crashEffectPool.Count > 0
                ? crashEffectPool.Dequeue()
                : Instantiate(
                    crashEffectPrefab,
                    transform);

        instance.SetActive(true);

        instance.transform.SetParent(
            canvasTransform,
            false);

        RectTransform rt =
            (RectTransform)instance.transform;

        rt.anchoredPosition =
            Vector2.zero;

        rt.localScale =
            Vector3.one;

        rt.localRotation =
            Quaternion.identity;

        Image img =
            instance.GetComponent<Image>();

        if (img != null)
            img.enabled = false;

        return instance;
    }

    // 사용처: 크래시 연출 완료 후 이미지를 풀에 반환
    public void ReturnCrashEffect(
        GameObject go)
    {
        if (go == null)
            return;

        go.SetActive(false);

        go.transform.SetParent(
            canvasTransform,
            false);

        Image img =
            go.GetComponent<Image>();

        if (img != null)
            img.enabled = false;

        crashEffectPool.Enqueue(
            go);
    }

    // 사용처: 버프/디버프 아이콘을 풀에서 꺼내 UI에 표시
    public GameObject GetBuffDeBuff()
    {
        if (buffDeBuffPrefab == null)
        {
            Debug.LogWarning(
                "[ObjectPool] BuffDeBuff 프리팹이 없습니다. Resources/Prefabs/BuffDeBuff 경로를 확인하세요.");

            return null;
        }

        GameObject instance =
            buffDeBuffPool.Count > 0
                ? buffDeBuffPool.Dequeue()
                : Instantiate(
                    buffDeBuffPrefab,
                    transform);

        instance.SetActive(true);

        instance.transform.SetParent(
            canvasTransform,
            false);

        RectTransform rt =
            instance.GetComponent<RectTransform>();

        if (rt != null)
        {
            rt.anchoredPosition =
                Vector2.zero;

            rt.localScale =
                Vector3.one;

            rt.localRotation =
                Quaternion.identity;
        }

        Image img =
            instance.GetComponent<Image>();

        if (img != null)
        {
            img.enabled = true;

            img.sprite = null;

            img.color =
                Color.white;
        }

        ItemInformation itemInfo =
            instance.GetComponent<ItemInformation>();

        if (itemInfo != null)
            itemInfo.Clear();

        activeBuffDeBuffs.Add(
            instance);

        return instance;
    }

    // 사용처: 사용이 끝난 버프/디버프 아이콘을 풀로 반환
    public void ReturnBuffDeBuff(
        GameObject go)
    {
        if (go == null)
            return;

        go.SetActive(false);

        go.transform.SetParent(
            canvasTransform,
            false);

        Image img =
            go.GetComponent<Image>();

        if (img != null)
            img.sprite = null;

        ItemInformation itemInfo =
            go.GetComponent<ItemInformation>();

        if (itemInfo != null)
            itemInfo.Clear();

        activeBuffDeBuffs.Remove(
            go);

        buffDeBuffPool.Enqueue(
            go);
    }

    // 사용처: 체력/상태 UI 갱신 전에 현재 표시 중인 버프/디버프 아이콘을 전부 정리
    public void ClearActiveBuffDeBuffs()
    {
        for (int i = activeBuffDeBuffs.Count - 1;
             i >= 0;
             i--)
        {
            GameObject go =
                activeBuffDeBuffs[i];

            if (go == null)
                continue;

            go.SetActive(false);

            go.transform.SetParent(
                canvasTransform,
                false);

            Image img =
                go.GetComponent<Image>();

            if (img != null)
                img.sprite = null;

            ItemInformation itemInfo =
                go.GetComponent<ItemInformation>();

            if (itemInfo != null)
                itemInfo.Clear();

            buffDeBuffPool.Enqueue(
                go);
        }

        activeBuffDeBuffs.Clear();
    }
}