using System;
using System.Collections.Generic;

[Serializable]
public class SerializableDictionary
{
    public List<string> keys = new List<string>();
    public List<string> values = new List<string>();
    
    public SerializableDictionary() { }
    
    public SerializableDictionary(Dictionary<string, string> dictionary)
    {
        FromDictionary(dictionary);
    }
    
    public void FromDictionary(Dictionary<string, string> dictionary)
    {
        keys.Clear();
        values.Clear();
        
        foreach (var kvp in dictionary)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }
    
    public Dictionary<string, string> ToDictionary()
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        
        for (int i = 0; i < Math.Min(keys.Count, values.Count); i++)
        {
            dictionary[keys[i]] = values[i];
        }
        
        return dictionary;
    }
}