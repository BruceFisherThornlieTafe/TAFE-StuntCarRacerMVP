// SaveData.cs
//
// Plain data container that holds everything that needs to be
// persisted between sessions. Add new fields here as the save
// system grows —> position/rotation is the only thing saved for now.
//
// Marked [System.Serializable] so Unity's JsonUtility can
// convert it to and from a JSON string automatically.

[System.Serializable]
public class SaveData
{
    // Car position in world space —> X, Y, Z , rotation Y stored as plain floats
    // because JsonUtility cannot serialize Vector3 directly
    public float positionX;
    public float positionY;
    public float positionZ;
    public float rotationY;
}