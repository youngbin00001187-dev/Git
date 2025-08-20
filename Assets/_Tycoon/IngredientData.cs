using UnityEngine;
using System.Collections.Generic;

// ����� Ÿ���� �����ϱ� ���� ������ (Enum)
public enum IngredientType
{
    Base,    // �⺻ ��� (���, ��, ��)
    Additive // �߰� ��� (����, �ұ� ��)
}

// ��ᰡ ���� �Ӽ��� �����ϴ� Ŭ���� (��: �ſ��, �ܸ� ��)
// �� Ŭ������ ������ ���� (IngredientProperty.cs)�� �и��Ǿ� ���� ���ɼ��� �����ϴ�.
// ���� IngredientData.cs�� CustomerData.cs �ܿ� IngredientProperty.cs ������ �ִٸ�,
// �Ʒ��� IngredientProperty ���Ǵ� �� ���Ͽ��� �־�� �մϴ�.
[System.Serializable]
public class IngredientProperty
{
    public string propertyName; // �Ӽ� �̸� (��: "������")
    public int value;           // �Ӽ� ��ġ
}

// ScriptableObject�� ����� ���� �޴� �׸��� �߰��մϴ�.
[CreateAssetMenu(fileName = "New Ingredient", menuName = "ǳ���/Ingredient")]
public class IngredientData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string ingredientName; // ��� �̸�
    public Sprite icon;          // ��� ������ �̹��� (�κ��丮/��ư�� ǥ��)
    public IngredientType type;  // ��� Ÿ�� (�⺻/�߰�)

    // === ����� �κ�: ���ư� �� ���� Sprite �ʵ� �߰� ===
    [Header("�ִϸ��̼� ����")]
    [Tooltip("�丮�� �� ����� ���ư� �ִϸ��̼ǿ� ��������Ʈ�Դϴ�. (����θ� �⺻ ������ ���)")]
    public Sprite animatedFlightSprite; // ����� ���ư� �� ���� ���� ��������Ʈ
    // ====================================================

    [Header("�丮 �Ӽ�")]
    public IngredientProperty[] properties; // �� ��ᰡ ���� �Ӽ���
}