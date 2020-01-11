using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioKeyCodeConstants
{
    public static KeyCode GetKeyCodeFromID(int id)
    {
        switch (id)
        {
            case 2:
                return KeyCode.LeftArrow;
            case 6:
                return KeyCode.LeftArrow;
            case 3:
                return KeyCode.UpArrow;
            case 7:
                return KeyCode.UpArrow;
            case 4:
                return KeyCode.RightArrow;
            case 8:
                return KeyCode.RightArrow;
            case 5:
                return KeyCode.DownArrow;
            case 9:
                return KeyCode.DownArrow;
        }
        return KeyCode.None;
    }
}
