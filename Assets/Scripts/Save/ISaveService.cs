using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Save
{
    public interface ISaveService
    {
        void Save<T>(string key, T data);
        T Load<T>(string key);
        void DeleteSave(string key);
        bool HasSave(string key);
    }
}
