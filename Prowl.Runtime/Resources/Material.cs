﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;

using Matrix4x4F = System.Numerics.Matrix4x4;
using Vector2F = System.Numerics.Vector2;
using Vector3F = System.Numerics.Vector3;
using Vector4F = System.Numerics.Vector4;

using Prowl.Runtime.Rendering;
using Prowl.Echo;

namespace Prowl.Runtime;

public sealed class Material : EngineObject, ISerializationCallbackReceiver
{
    private static Material s_defaultMaterial;

    public static Material GetDefaultMaterial()
    {
        if (s_defaultMaterial == null)
        {
            s_defaultMaterial = CreateDefaultMaterial();
            s_defaultMaterial.SetColor("_MainColor", Color.white);
        }

        return s_defaultMaterial;
    }

    public static Material CreateDefaultMaterial()
    {
        return new Material(Application.AssetProvider.LoadAsset<Shader>("Defaults/Standard.shader"));
    }


    [SerializeField]
    private AssetRef<Shader> _shader;

    public AssetRef<Shader> Shader
    {
        get => _shader;
        set => SetShader(value);
    }

    [SerializeField]
    private List<ShaderProperty> _serializedProperties;

    [SerializeIgnore]
    // DO NOT RENAME, its name is used in MaterialEditor to find the property "_propertyLookup", NameOf is not used since its private
    private Dictionary<string, int> _propertyLookup;

    [SerializeIgnore]
    internal PropertyState _properties;

    [SerializeIgnore]
    internal KeywordState _localKeywords;


    internal Material() : base("New Material")
    {
        _properties = new();
        _localKeywords = KeywordState.Empty;
    }

    public Material(AssetRef<Shader> shader, PropertyState? properties = null, KeywordState? keywords = null) : base("New Material")
    {
        if (shader.Res == null)
            throw new ArgumentNullException(nameof(shader));

        Shader = shader;
        _properties = properties ?? new();
        _localKeywords = keywords ?? KeywordState.Empty;
    }

    public void SetKeyword(string keyword, string value) => _localKeywords.SetKey(keyword, value);

    public void SetColor(string name, Color value) => _properties.SetColor(name, value);
    public void SetVector(string name, Vector2F value) => _properties.SetVector(name, value);
    public void SetVector(string name, Vector3F value) => _properties.SetVector(name, value);
    public void SetVector(string name, Vector4F value) => _properties.SetVector(name, value);
    public void SetFloat(string name, float value) => _properties.SetFloat(name, value);
    public void SetInt(string name, int value) => _properties.SetInt(name, value);
    public void SetMatrix(string name, Matrix4x4F value) => _properties.SetMatrix(name, value);
    public void SetTexture(string name, Texture value) => _properties.SetTexture(name, value);


    public void SetFloatArray(string name, float[] values) => _properties.SetFloatArray(name, values);
    public void SetIntArray(string name, int[] values) => _properties.SetIntArray(name, values);
    public void SetVectorArray(string name, Vector2F[] values) => _properties.SetVectorArray(name, values);
    public void SetVectorArray(string name, Vector3F[] values) => _properties.SetVectorArray(name, values);
    public void SetVectorArray(string name, Vector4F[] values) => _properties.SetVectorArray(name, values);
    public void SetColorArray(string name, Color[] values) => _properties.SetColorArray(name, values);
    public void SetMatrixArray(string name, Matrix4x4F[] values) => _properties.SetMatrixArray(name, values);

    #region Global Properties

    public static void SetGlobalColor(string name, Color value) => PropertyState.SetGlobalColor(name, value);
    public static void SetGlobalVector(string name, Vector2F value) => PropertyState.SetGlobalVector(name, value);
    public static void SetGlobalVector(string name, Vector3F value) => PropertyState.SetGlobalVector(name, value);
    public static void SetGlobalVector(string name, Vector4F value) => PropertyState.SetGlobalVector(name, value);
    public static void SetGlobalFloat(string name, float value) => PropertyState.SetGlobalFloat(name, value);
    public static void SetGlobalInt(string name, int value) => PropertyState.SetGlobalInt(name, value);
    public static void SetGlobalMatrix(string name, Matrix4x4F value) => PropertyState.SetGlobalMatrix(name, value);
    public static void SetGlobalTexture(string name, Texture value) => PropertyState.SetGlobalTexture(name, value);


    public static void SetGlobalFloatArray(string name, float[] values) => PropertyState.SetGlobalFloatArray(name, values);
    public static void SetGlobalIntArray(string name, int[] values) => PropertyState.SetGlobalIntArray(name, values);
    public static void SetGlobalVectorArray(string name, Vector2F[] values) => PropertyState.SetGlobalVectorArray(name, values);
    public static void SetGlobalVectorArray(string name, Vector3F[] values) => PropertyState.SetGlobalVectorArray(name, values);
    public static void SetGlobalVectorArray(string name, Vector4F[] values) => PropertyState.SetGlobalVectorArray(name, values);
    public static void SetGlobalColorArray(string name, Color[] values) => PropertyState.SetGlobalColorArray(name, values);
    public static void SetGlobalMatrixArray(string name, Matrix4x4F[] values) => PropertyState.SetGlobalMatrixArray(name, values);

    #endregion

    public void SetProperty(string name, ShaderProperty value)
    {
        if (_propertyLookup.TryGetValue(name, out int val))
        {
            ShaderProperty prop = _serializedProperties[val];

            prop.Set(value);

            UpdatePropertyState(prop);

            _serializedProperties[val] = prop;
        }
    }


    public bool GetProperty(string name, out ShaderProperty value)
    {
        if (_propertyLookup.TryGetValue(name, out int val))
        {
            value = _serializedProperties[val];
            return true;
        }

        value = default;
        return false;
    }


    public void SyncPropertyBlock()
    {
        foreach (ShaderProperty prop in _serializedProperties)
            UpdatePropertyState(prop);
    }


    private void UpdatePropertyState(ShaderProperty property)
    {
        switch (property.PropertyType)
        {
            case ShaderPropertyType.Texture2D:
                _properties.SetTexture(property.Name, property.Texture2DValue.Res);
                break;

            case ShaderPropertyType.Texture3D:
                _properties.SetTexture(property.Name, property.Texture3DValue.Res);
                break;

            case ShaderPropertyType.Float:
                _properties.SetFloat(property.Name, (float)property);
                break;

            case ShaderPropertyType.Vector2:
                _properties.SetVector(property.Name, (Vector2)property);
                break;

            case ShaderPropertyType.Vector3:
                _properties.SetVector(property.Name, (Vector3)property);
                break;

            case ShaderPropertyType.Vector4:
                _properties.SetVector(property.Name, (Vector4)property);
                break;

            case ShaderPropertyType.Color:
                _properties.SetColor(property.Name, (Color)property);
                break;

            case ShaderPropertyType.Matrix:
                _properties.SetMatrix(property.Name, ((Matrix4x4)property).ToFloat());
                break;
        }
    }


    internal void SetShader(AssetRef<Shader> shader)
    {
        if (shader == _shader)
            return;

        _shader = shader;

        _serializedProperties ??= [];
        _propertyLookup ??= [];

        _serializedProperties.Clear();
        _propertyLookup.Clear();

        foreach (ShaderProperty prop in shader.Res.Properties)
        {
            _serializedProperties.Add(prop);
            _propertyLookup.Add(prop.Name, _serializedProperties.Count - 1);
        }
    }


    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        _propertyLookup ??= [];

        for (int i = 0; i < _serializedProperties.Count; i++)
            _propertyLookup.Add(_serializedProperties[i].Name, i);

        SyncPropertyBlock();
    }
}
