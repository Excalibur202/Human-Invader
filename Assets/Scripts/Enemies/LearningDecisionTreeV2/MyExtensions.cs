using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class Minhas_Ext
{
    //Criar copia
    public static T DeepClone<T>(this T obj)
    {
        T objResult;
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, obj);
            ms.Position = 0;
            objResult = (T)bf.Deserialize(ms);
        }
        return objResult;
    }

}
