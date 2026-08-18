using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item")]
public class Item : ScriptableObject
{
    [SerializeField]
    private string persistenceId;

    public string PersistenceId => string.IsNullOrWhiteSpace(persistenceId)
        ? name
        : persistenceId;

    public string descricao;
    public ushort hp;
    public byte forca, destreza, inteligencia;

}
