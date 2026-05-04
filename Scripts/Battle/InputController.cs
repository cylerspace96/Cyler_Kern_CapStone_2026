// Handles player input to control the characters in battle

using UnityEngine;

public class InputController : MonoBehaviour
{
    public PlayerAttackManager attackManager;

    // Maps to player indices 0-3
    readonly KeyCode[] keys =
    {
        KeyCode.I,
        KeyCode.L,
        KeyCode.O,
        KeyCode.K
    };

    void Update()
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                attackManager.AttackWithPlayer(i);
                break;
            }
        }
    }
}