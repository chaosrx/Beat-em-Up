using UnityEngine;

[System.Serializable]
public struct ObjectData
{
    public int typy;
    public string breed;
    public Vector2 position;
}

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private TextAsset[] _levels;
    public int levelCount { get { return _levels.Length; } }

    [SerializeField]
    private GameObject _heroPrefab;
    [SerializeField]
    private GameObject[] _zombiePrefabs;
    [SerializeField]
    private GameObject[] _ObstaclePrefabs;
    [SerializeField]
    private GameObject[] _itemPrefabs;

    //[MenuItem("LevelManager/SerializeLevel")]
    //public static void Serialize()
    //{
    //    FileStream fs = File.Create("Assets/My Beat'em Up Game/Levels/level.txt");
    //    StreamWriter sw = new StreamWriter(fs);
    //    GameObject[] gameObjects = Selection.gameObjects;
    //    foreach(GameObject g in gameObjects)
    //    {
    //        string objectJson = JsonUtility.ToJson(new ObjectData() { typy = g.layer, breed = g.tag, position = g.transform.position });
    //        sw.WriteLine(objectJson);
    //    }
    //    sw.Close();
    //    fs.Close();
    //}

    public void LoadLevel(int levelIndex)
    {
        string jsonStr = _levels[levelIndex - 1].text;
        string[] objectJsons = jsonStr.Split('\n');
        foreach (string json in objectJsons)
        {
            if(json == "")
            {
                break;
            }
            ObjectData o = JsonUtility.FromJson<ObjectData>(json);
            switch (LayerMask.LayerToName(o.typy))
            {
                case "Player":
                    CreateInstance(_heroPrefab, o.position);
                    break;
                case "Zombie":
                    foreach (GameObject g in _zombiePrefabs)
                    {
                        if (g.name == o.breed)
                        {
                            CreateInstance(g, o.position);
                            break;
                        }
                    }
                    break;
                case "Barrel":
                    foreach (GameObject g in _ObstaclePrefabs)
                    {
                        if (g.tag == o.breed)
                        {
                            CreateInstance(g, o.position);
                            break;
                        }
                    }
                    break;
                case "Item":
                    foreach (GameObject g in _itemPrefabs)
                    {
                        if (g.tag == o.breed)
                        {
                            CreateInstance(g, o.position);
                            break;
                        }
                    }
                    break;
            }
        }
    }

    void CreateInstance(GameObject prefab,Vector2 position)
    {
        Transform.Instantiate(prefab, position, Quaternion.identity);
    }
}
