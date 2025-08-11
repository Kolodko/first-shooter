using UnityEngine;

namespace Game.Save
{
    public class SaveService : ISaveService
    {
        private const string SavePrefix = "GameSave_";

        public void Save<T>(string key, T data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SavePrefix + key, json);
            PlayerPrefs.Save();
        }

        public T Load<T>(string key)
        {
            string fullKey = SavePrefix + key;
            if (!PlayerPrefs.HasKey(fullKey))
                return default(T);

            string json = PlayerPrefs.GetString(fullKey);
            return JsonUtility.FromJson<T>(json);
        }

        public void DeleteSave(string key)
        {
            PlayerPrefs.DeleteKey(SavePrefix + key);
            PlayerPrefs.Save();
        }

        public bool HasSave(string key)
        {
            return PlayerPrefs.HasKey(SavePrefix + key);
        }
    }
}
