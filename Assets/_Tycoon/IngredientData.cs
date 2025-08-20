using UnityEngine;
using System.Collections.Generic;

// 재료의 타입을 구분하기 위한 열거형 (Enum)
public enum IngredientType
{
    Base,    // 기본 재료 (고기, 면, 국)
    Additive // 추가 재료 (설탕, 소금 등)
}

// 재료가 가진 속성을 정의하는 클래스 (예: 매운맛, 단맛 등)
// 이 클래스는 별도의 파일 (IngredientProperty.cs)로 분리되어 있을 가능성이 높습니다.
// 만약 IngredientData.cs와 CustomerData.cs 외에 IngredientProperty.cs 파일이 있다면,
// 아래의 IngredientProperty 정의는 그 파일에만 있어야 합니다.
[System.Serializable]
public class IngredientProperty
{
    public string propertyName; // 속성 이름 (예: "매콤함")
    public int value;           // 속성 수치
}

// ScriptableObject를 만들기 위한 메뉴 항목을 추가합니다.
[CreateAssetMenu(fileName = "New Ingredient", menuName = "풍운객잔/Ingredient")]
public class IngredientData : ScriptableObject
{
    [Header("기본 정보")]
    public string ingredientName; // 재료 이름
    public Sprite icon;          // 재료 아이콘 이미지 (인벤토리/버튼에 표시)
    public IngredientType type;  // 재료 타입 (기본/추가)

    // === 변경된 부분: 날아갈 때 사용될 Sprite 필드 추가 ===
    [Header("애니메이션 설정")]
    [Tooltip("요리할 때 냄비로 날아갈 애니메이션용 스프라이트입니다. (비워두면 기본 아이콘 사용)")]
    public Sprite animatedFlightSprite; // 냄비로 날아갈 때 사용될 별도 스프라이트
    // ====================================================

    [Header("요리 속성")]
    public IngredientProperty[] properties; // 이 재료가 가진 속성들
}