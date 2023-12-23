using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Item", menuName = "ShopItem")]
public class ShopItem : ScriptableObject{

    public string itemName;
    public Sprite itemImage;
    public int itemPrice;

}
