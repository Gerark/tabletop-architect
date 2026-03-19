using System.IO;
using TTA;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public class GameLogicComponent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var engine = new Engine();
        var data = engine.GetData();

        // Create a file a write the content of the data as if it was a json file
        JsonSerializer serializer = new JsonSerializer();
        serializer.NullValueHandling = NullValueHandling.Ignore;
        serializer.Formatting = Formatting.Indented;

        using (StreamWriter sw = new StreamWriter(Application.dataPath + "/data.json"))
        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            serializer.Serialize(writer, data);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
