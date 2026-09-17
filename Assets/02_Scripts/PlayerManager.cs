using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    [Header("Test")]
    [SerializeField] private GameObject startingDuck;

    [Header("Possession")]
    [SerializeField] private float possessionDelay = 3f;

    [Header("Camera")]
    [SerializeField] private CameraController cameraController;

    [Header("UI")]
    [SerializeField] private GameObject spectatorUI;
    [SerializeField] private TMP_Text respawnText;

    private GameObject currentPlayerUnit;
    private GameObject spectatingDuck;

    private bool canPossess = true;
    private bool isSpectating = false;

    public GameObject CurrentPlayerUnit => currentPlayerUnit;

    void Start()
    {
        SetSpectatorUI(false);

        if (startingDuck != null)
            Possess(startingDuck);
    }

    void Update()
    {
        if (!isSpectating)
            return;

        if (spectatingDuck == null)
            SelectFirstSpectatingDuck();

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.wasPressedThisFrame)
            ChangeSpectatingDuck(-1);

        if (Keyboard.current.dKey.wasPressedThisFrame)
            ChangeSpectatingDuck(1);

        if (canPossess &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            spectatingDuck != null)
        {
            Possess(spectatingDuck);
        }
    }

    public void Possess(GameObject unit)
    {
        if (!canPossess || unit == null)
            return;

        UnitTeam team = unit.GetComponent<UnitTeam>();
        UnitHealth health = unit.GetComponent<UnitHealth>();
        AIController ai = unit.GetComponent<AIController>();
        PlayerController player = unit.GetComponent<PlayerController>();
        PlayerWeaponInput weaponInput =
            unit.GetComponent<PlayerWeaponInput>();
        WeaponController weapon =
            unit.GetComponent<WeaponController>();

        if (team == null ||
            health == null ||
            ai == null ||
            player == null ||
            weaponInput == null ||
            weapon == null)
        {
            Debug.LogWarning(
                $"{unit.name}: 빙의에 필요한 컴포넌트가 없습니다."
            );
            return;
        }

        if (health.IsDead ||
            team.CurrentTeam != UnitTeam.Team.Duck)
        {
            return;
        }

        if (currentPlayerUnit != null &&
            currentPlayerUnit != unit)
        {
            ReleaseCurrentUnit();
        }

        // AI 제어는 끄고 플레이어 입력만 켭니다.
        // WeaponController는 AI/Player 공용이므로 항상 켜둡니다.
        ai.enabled = false;
        player.enabled = true;
        weaponInput.enabled = true;
        weapon.enabled = true;

        currentPlayerUnit = unit;
        spectatingDuck = null;
        isSpectating = false;

        SetSpectatorUI(false);

        health.Died -= HandlePlayerUnitDied;
        health.Died += HandlePlayerUnitDied;

        if (cameraController != null)
        {
            cameraController.SetTarget(unit);
            cameraController.LockCursor();
        }

        Debug.Log($"{unit.name} 빙의 완료");
    }

    public void ReleaseCurrentUnit()
    {
        if (currentPlayerUnit == null)
            return;

        UnitHealth health =
            currentPlayerUnit.GetComponent<UnitHealth>();

        AIController ai =
            currentPlayerUnit.GetComponent<AIController>();

        PlayerController player =
            currentPlayerUnit.GetComponent<PlayerController>();

        PlayerWeaponInput weaponInput =
            currentPlayerUnit.GetComponent<PlayerWeaponInput>();

        WeaponController weapon =
            currentPlayerUnit.GetComponent<WeaponController>();

        if (health != null)
            health.Died -= HandlePlayerUnitDied;

        if (ai != null)
            ai.enabled = true;

        if (player != null)
            player.enabled = false;

        if (weaponInput != null)
            weaponInput.enabled = false;

        // AI가 계속 공격해야 하므로 WeaponController는 끄지 않습니다.
        if (weapon != null)
            weapon.enabled = true;

        currentPlayerUnit = null;
    }

    private void HandlePlayerUnitDied(UnitHealth deadUnit)
    {
        if (currentPlayerUnit == null ||
            deadUnit.gameObject != currentPlayerUnit)
        {
            return;
        }

        deadUnit.Died -= HandlePlayerUnitDied;

        currentPlayerUnit = null;
        canPossess = false;
        isSpectating = true;

        SelectFirstSpectatingDuck();
        SetSpectatorUI(true);

        Debug.Log(
            $"플레이어 Duck 사망 - {possessionDelay}초 후 빙의 가능"
        );

        StartCoroutine(PossessionCooldown());
    }

    private IEnumerator PossessionCooldown()
    {
        float remainingTime = possessionDelay;

        while (remainingTime > 0f)
        {
            if (respawnText != null)
            {
                int seconds =
                    Mathf.CeilToInt(remainingTime);

                respawnText.text =
                    $"{seconds}초 후 부활 가능\n" +
                    "A / D : 관전 대상 변경";
            }

            remainingTime -= Time.deltaTime;
            yield return null;
        }

        canPossess = true;

        if (spectatingDuck == null)
            SelectFirstSpectatingDuck();

        UpdateRespawnText();

        Debug.Log(
            "빙의 가능 - A/D로 아군 Duck 변경, Space로 빙의"
        );
    }

    private void SelectFirstSpectatingDuck()
    {
        List<GameObject> ducks =
            FindAvailableDucks();

        if (ducks.Count == 0)
        {
            spectatingDuck = null;

            if (cameraController != null)
                cameraController.SetTarget(null);

            UpdateRespawnText();

            Debug.Log(
                "관전 가능한 아군 Duck이 없습니다."
            );

            return;
        }

        spectatingDuck = ducks[0];

        SetSpectatingCamera();

        if (canPossess)
            UpdateRespawnText();
    }

    private void ChangeSpectatingDuck(int direction)
    {
        List<GameObject> ducks =
            FindAvailableDucks();

        if (ducks.Count == 0)
        {
            spectatingDuck = null;

            if (cameraController != null)
                cameraController.SetTarget(null);

            return;
        }

        int index =
            ducks.IndexOf(spectatingDuck);

        if (index < 0)
        {
            index = 0;
        }
        else
        {
            index =
                (index + direction + ducks.Count) %
                ducks.Count;
        }

        spectatingDuck = ducks[index];

        SetSpectatingCamera();

        Debug.Log(
            $"관전 중: {spectatingDuck.name}"
        );
    }

    private List<GameObject> FindAvailableDucks()
    {
        List<GameObject> ducks =
            new List<GameObject>();

        UnitTeam[] units =
    FindObjectsByType<UnitTeam>();

        foreach (UnitTeam unit in units)
        {
            if (unit.CurrentTeam !=
                UnitTeam.Team.Duck)
            {
                continue;
            }

            UnitHealth health =
                unit.GetComponent<UnitHealth>();

            AIController ai =
                unit.GetComponent<AIController>();

            if (health == null ||
                health.IsDead ||
                ai == null ||
                !ai.enabled)
            {
                continue;
            }

            ducks.Add(unit.gameObject);
        }

        return ducks;
    }

    private void SetSpectatorUI(bool active)
    {
        if (spectatorUI != null)
            spectatorUI.SetActive(active);
    }

    private void UpdateRespawnText()
    {
        if (respawnText == null)
            return;

        if (spectatingDuck == null)
        {
            respawnText.text =
                "아군 지원군을 기다리는 중...";

            return;
        }

        if (canPossess)
        {
            respawnText.text =
                "부활 가능\n" +
                "A / D : 관전 대상 변경\n" +
                "SPACE : 부활";
        }
    }

    private void SetSpectatingCamera()
    {
        if (spectatingDuck == null ||
            cameraController == null)
        {
            return;
        }

        cameraController.SetTarget(
            spectatingDuck
        );

        cameraController.LockCursor();
    }
}
