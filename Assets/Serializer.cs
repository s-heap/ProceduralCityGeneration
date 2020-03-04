using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Serializer {
    public static T Load<T>(string filename) where T : class {
        if (File.Exists(filename)) {
            try {
                using(Stream stream = File.OpenRead(filename)) {
                    BinaryFormatter formatter = GetFormatter();
                    return formatter.Deserialize(stream) as T;
                }
            } catch (Exception e) {
                Debug.Log(e.Message);
            }
        }
        return default(T);
    }

    public static void Save<T>(string filename, T data) where T : class {
        using(Stream stream = File.OpenWrite(filename)) {
            BinaryFormatter formatter = GetFormatter();
            formatter.Serialize(stream, data);
        }
    }

    private static BinaryFormatter GetFormatter() {
        BinaryFormatter formatter = new BinaryFormatter();

        SurrogateSelector ss = new SurrogateSelector();

        Vector3SerializationSurrogate v3ss = new Vector3SerializationSurrogate();
        ss.AddSurrogate(typeof(Vector3),
            new StreamingContext(StreamingContextStates.All),
            v3ss);

        Vector2SerializationSurrogate v2ss = new Vector2SerializationSurrogate();
        ss.AddSurrogate(typeof(Vector2),
            new StreamingContext(StreamingContextStates.All),
            v2ss);

        ColorSerializationSurrogate css = new ColorSerializationSurrogate();
        ss.AddSurrogate(typeof(Color),
            new StreamingContext(StreamingContextStates.All),
            css);

        formatter.SurrogateSelector = ss;

        return formatter;
    }
}