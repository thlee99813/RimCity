using UnityEngine;

public class CharacterEntity : MonoBehaviour
{

    //캐릭터의 기본단위이자, Data, Brain, Condition 을 참조하고있음.

    public CharacterData Data;


    public void Initialize(CharacterData data)
    {
        Data = data; 
    }

    private void OnEnable()
    {
        CharacterManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        CharacterManager.Instance.Unregister(this);
    }

}
