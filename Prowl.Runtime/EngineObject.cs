﻿using Prowl.Runtime.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Prowl.Runtime
{
    public class EngineObject : IDisposable
    {
        private static readonly Stack<EngineObject> destroyed = new Stack<EngineObject>();

        private static readonly List<EngineObject> allObjects = new List<EngineObject>();
        private static readonly MultiValueDictionary<Type, EngineObject> cachedObjectTypes = new();

        static int NextID = 1;

        protected int _instanceID;
        public int InstanceID => _instanceID;

        // Asset path if we have one
        [JsonProperty("AssetID"), HideInInspector]
        public Guid AssetID = Guid.Empty;

        [JsonProperty("Name"), HideInInspector]
        public string Name;
        
        [JsonIgnore, HideInInspector] 
        public bool IsDestroyed = false;

        public EngineObject() : this(null) { }

        public EngineObject(string? name = "New Object")
        {
            CreatedInstance();
            Name = name ?? Name;
        }

        public virtual void CreatedInstance()
        {
            _instanceID = NextID++;
            Name = "New" + GetType().Name;
            allObjects.Add(this);
            cachedObjectTypes.Add(GetType(), this);
        }

        public virtual void OnValidate() { }

        public static T? FindObjectOfType<T>() where T : EngineObject => cachedObjectTypes[typeof(T)].FirstOrDefault() as T;
        public static T[] FindObjectsOfType<T>() where T : EngineObject => cachedObjectTypes[typeof(T)].Cast<T>().ToArray();
        public static T? FindObjectByID<T>(int id) where T : EngineObject => cachedObjectTypes[typeof(T)].FirstOrDefault(o => o.InstanceID == id && o is T) as T;

        public static void Foreach<T>(Action<T> action) where T : EngineObject
        {
            foreach (T obj in cachedObjectTypes[typeof(T)])
                if (!obj.IsDestroyed)
                    action(obj);
        }

        public void Destroy() => Destroy(this);
        public void DestroyImmediate() => DestroyImmediate(this);

        public static void Destroy(EngineObject obj)
        {
            if (obj.IsDestroyed) throw new Exception(obj.Name + " is already destroyed.");
            obj.IsDestroyed = true;
            destroyed.Push(obj);
        }

        public static void DestroyImmediate(EngineObject obj)
        {
            if(obj.IsDestroyed) throw new Exception(obj.Name + " is already destroyed.");
            obj.IsDestroyed = true;
            obj.Dispose();
        }

        public static void HandleDestroyed()
        {
            while (destroyed.TryPop(out var obj))
            {
                if (!obj.IsDestroyed) continue;
                obj.Dispose();
            }
        }

        public static EngineObject Instantiate(EngineObject obj)
        {
            if (obj.IsDestroyed) throw new Exception(obj.Name + " has been destroyed.");
            // Serialize and deserialize to get a new object
            var serialized = JsonUtility.Serialize(obj);
            // dont need to assign ID or add it to objects list the constructor will do that automatically
            var newObj = JsonUtility.Deserialize(serialized, obj.GetType()) as EngineObject;
            // Some objects might have a readonly name (like components) in that case it should remain the same, so if name is different set it
            newObj.Name = obj.Name;
            // Need to make sure to set GUID to empty so the engine knows this isn't the original Asset file
            newObj.AssetID = Guid.Empty;
            return newObj;
        }

        /// <summary>
        /// Force the object to dispose immediately
        /// You are advised to not use this! Use Destroy() Instead.
        /// </summary>
        [Obsolete("You are advised to not use this! Use Destroy() Instead.")]
        public void Dispose()
        {
            IsDestroyed = true;
            allObjects.Remove(this);
            cachedObjectTypes.Remove(GetType(), this);
            GC.SuppressFinalize(this);
            OnDispose();
            //AssetProvider.RemoveAsset(this, false);
        }

        public virtual void OnDispose() { }

        public override string ToString() { return Name; }

    }
}