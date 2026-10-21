using System;
using System.Collections.Generic;
using System.Linq;
using TCCGame.Core;
using TCCGame.Gameplay.Recruitment;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
/// Exibe uma onda de aventureiros dentro do BoxCollider da taverna.
/// Quantidade: nível 1 = duas ofertas; cada nível adicional adiciona uma.
/// O serviço persistente escolhe a onda e inicia seu prazo quando esta cena é
/// carregada. Ao expirar, as ofertas somem e só uma nova entrada na cena cria
/// outra onda — nunca há troca diante do jogador.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public sealed class TavernRecruitableSpawner : MonoBehaviour
{
    [Header("Prefab e aventureiros possíveis")]
    [FormerlySerializedAs("recruitablePrefab")]
    [Tooltip("Compatibilidade: usado apenas quando o Scriptable do aventureiro ainda não possui Recruitable Prefab.")]
    [SerializeField] private Aventureiro fallbackRecruitablePrefab;
    [Tooltip("Definições que podem ser sorteadas para uma onda.")]
    [SerializeField] private Char_base[] adventurerPool =
        Array.Empty<Char_base>();

    [Header("Regras da onda")]
    [Tooltip("Duração da chance de recrutamento depois que a onda é visualizada.")]
    [SerializeField, Min(1)] private int waveDurationSeconds = 120;
    [Tooltip("Quantidade exibida no nível 1 da Taverna.")]
    [SerializeField, Min(1)] private int offersAtLevelOne = 2;
    [Tooltip("Ofertas adicionais por nível acima do primeiro.")]
    [SerializeField, Min(0)] private int offersPerAdditionalLevel = 1;

    [Header("Distribuição dentro do trigger")]
    [SerializeField, Min(0f)] private float spawnPadding = 0.25f;
    [SerializeField, Min(0f)] private float minimumSpawnDistance = 0.75f;

    private readonly List<Aventureiro> spawnedRecruitables = new();
    private readonly List<Vector3> usedSpawnPoints = new();
    private BoxCollider spawnArea;
    private TavernRecruitmentWaveService waveService;
    private bool waveExpiredInThisVisit;

    private void Awake()
    {
        spawnArea = GetComponent<BoxCollider>();
        waveService = GameSessionProvider.Current.TavernRecruitment;

        // Personagens colocados manualmente na antiga configuração eram
        // reaproveitados como qualquer classe. Eles são ocultados para que a
        // onda passe a nascer exclusivamente do prefab associado a cada
        // ScriptableObject.
        foreach (Aventureiro candidate in
                 FindObjectsByType<Aventureiro>(FindObjectsSortMode.None))
        {
            if (candidate != null &&
                spawnArea.bounds.Contains(candidate.transform.position))
            {
                candidate.gameObject.SetActive(false);
            }
        }
    }

    private void Start()
    {
        ShowWaveForCurrentTavernLevel();
    }

    private void Update()
    {
        if (waveExpiredInThisVisit)
        {
            return;
        }

        HideAdventurersAlreadyRecruited();
        if (!waveService.IsCurrentWaveExpired)
        {
            return;
        }

        // Não chamamos EnterTavern novamente aqui. A próxima onda fica
        // pendente até este componente ser recriado por uma nova entrada na
        // cena, conforme a regra de atualização solicitada.
        waveExpiredInThisVisit = true;
        foreach (Aventureiro recruitable in spawnedRecruitables)
        {
            CloseRecruitmentPanel(recruitable);
            if (recruitable != null)
            {
                recruitable.gameObject.SetActive(false);
            }
        }
    }

    private void ShowWaveForCurrentTavernLevel()
    {
        int tavernLevel = GameSessionProvider.Current.Buildings.GetLevel(
            BuildingType.Tavern
        );
        int offerCount = tavernLevel <= 0
            ? 0
            : offersAtLevelOne +
              (tavernLevel - 1) * offersPerAdditionalLevel;

        IReadOnlyList<string> waveIds = waveService.EnterTavern(
            (adventurerPool ?? Array.Empty<Char_base>())
                .Where(definition => definition != null)
                .Select(definition => definition.PersistenceId),
            offerCount,
            waveDurationSeconds
        );
        Dictionary<string, Char_base> definitions =
            (adventurerPool ?? Array.Empty<Char_base>())
            .Where(definition => definition != null)
            .GroupBy(definition => definition.PersistenceId)
            .ToDictionary(group => group.Key, group => group.First());
        List<Char_base> visibleDefinitions = waveIds
            .Where(id => !GameSessionProvider.Current.Inventory
                .IsAdventurerRecruited(id))
            .Where(definitions.ContainsKey)
            .Select(id => definitions[id])
            .ToList();

        usedSpawnPoints.Clear();
        foreach (Char_base definition in visibleDefinitions)
        {
            Aventureiro prefab = definition.recruitablePrefab != null
                ? definition.recruitablePrefab
                : fallbackRecruitablePrefab;
            if (prefab == null)
            {
                Debug.LogError(
                    $"O aventureiro '{definition.name}' não possui Recruitable Prefab configurado.",
                    definition
                );
                continue;
            }

            Vector3 spawnPoint = GetSeparatedSpawnPoint();
            Aventureiro recruitable = Instantiate(
                prefab,
                spawnPoint,
                Quaternion.identity
            );
            spawnedRecruitables.Add(recruitable);
            recruitable.Configure(definition);
            recruitable.name = "Recrutável - " + definition.name;
            usedSpawnPoints.Add(spawnPoint);
        }
    }

    private void HideAdventurersAlreadyRecruited()
    {
        foreach (Aventureiro recruitable in spawnedRecruitables)
        {
            if (recruitable == null || !recruitable.gameObject.activeSelf ||
                recruitable.charBase == null ||
                !GameSessionProvider.Current.Inventory.IsAdventurerRecruited(
                    recruitable.charBase.PersistenceId))
            {
                continue;
            }

            CloseRecruitmentPanel(recruitable);
            recruitable.gameObject.SetActive(false);
        }
    }

    private static void CloseRecruitmentPanel(Aventureiro recruitable)
    {
        if (recruitable == null || !recruitable.IsBeingInspected)
        {
            return;
        }

        FindFirstObjectByType<AdventurerRecruitmentController>()?
            .Close(recruitable);
    }

    private Vector3 GetSeparatedSpawnPoint()
    {
        Vector3 bestPoint = GetRandomPointInsideArea();
        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector3 candidate = GetRandomPointInsideArea();
            bestPoint = candidate;
            if (usedSpawnPoints.All(point =>
                    Vector2.Distance(point, candidate) >=
                    minimumSpawnDistance))
            {
                break;
            }
        }

        return bestPoint;
    }

    private Vector3 GetRandomPointInsideArea()
    {
        Vector3 halfSize = spawnArea.size * 0.5f;
        float usableX = Mathf.Max(0f, halfSize.x - spawnPadding);
        float usableY = Mathf.Max(0f, halfSize.y - spawnPadding);
        Vector3 localPoint = spawnArea.center + new Vector3(
            Random.Range(-usableX, usableX),
            Random.Range(-usableY, usableY),
            0f
        );
        Vector3 worldPoint = transform.TransformPoint(localPoint);
        worldPoint.z = transform.position.z;
        return worldPoint;
    }

    private void OnValidate()
    {
        waveDurationSeconds = Mathf.Max(1, waveDurationSeconds);
        offersAtLevelOne = Mathf.Max(1, offersAtLevelOne);
        offersPerAdditionalLevel = Mathf.Max(0,
            offersPerAdditionalLevel);
        spawnPadding = Mathf.Max(0f, spawnPadding);
        minimumSpawnDistance = Mathf.Max(0f, minimumSpawnDistance);
        adventurerPool ??= Array.Empty<Char_base>();
    }
}
