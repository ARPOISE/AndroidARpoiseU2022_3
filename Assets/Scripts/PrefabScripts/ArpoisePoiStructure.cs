/*
ArpoisePoiStructure.cs - A base script handling a 'poi structure' for ARpoise.

Copyright (C) 2025, Tamiko Thiel and Peter Graf - All Rights Reserved

ARpoise - Augmented Reality point of interest service environment 

This file is part of ARpoise.

    ARpoise is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ARpoise is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ARpoise.  If not, see <https://www.gnu.org/licenses/>.

For more information on 

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
ARpoise, see www.ARpoise.com/

*/
using com.arpoise.arpoiseapp;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArpoisePoiStructure : MonoBehaviour
{
    public ArBehaviourArObject ArBehaviour { get; set; }

    #region Structure parameters
    public int MaxNofObjects = 100;
    public string Poi = string.Empty;
    protected List<string> Pois = new();
    #endregion

    protected readonly List<ArObject> ArObjectsToFade = new();
    protected List<ArObject> ArObjects = null;
    protected ArObject ArObject = null;

    public void SetArObject(ArObject arObject)
    {
        ArObject = arObject;
    }

    public void SeedRandom(int seed)
    {
        Random = new System.Random(Random.Next(int.MaxValue) ^ seed);
    }
    public System.Random Random = new System.Random((int)DateTime.Now.Ticks);

    public virtual void SetParameter(bool setValue, string label, string value)
    {
        if (label.Equals(nameof(MaxNofObjects)))
        {
            MaxNofObjects = ParameterHelper.SetParameter(setValue, value, MaxNofObjects).Value;
        }
        else if (label.Equals(nameof(Poi)))
        {
            ParameterHelper.SetParameter(setValue, value, Pois);
        }
    }

    public void CallUpdate()
    {
        Update();
    }
    protected virtual void Update()
    {
        SetActive(gameObject.activeSelf, ArObjects);

        if (_fadeValue.HasValue && (DateTime.Now - _lastFadeTime).TotalMilliseconds > 100)
        {
            DoFade(_fadeValue.Value);
            _fadeValue = null;
            _lastFadeTime = DateTime.Now;
        }
    }

    protected void SetActive(bool activeSelf, List<ArObject> arObjects)
    {
        var firstObject = arObjects != null && arObjects.Count > 0 ? arObjects[0]?.WrapperObject : null;
        if (firstObject != null && firstObject.activeSelf != activeSelf)
        {
            foreach (var arObject in arObjects)
            {
                if (arObject.WrapperObject != null && arObject.WrapperObject.activeSelf != activeSelf)
                {
                    arObject.WrapperObject.SetActive(activeSelf);
                }
            }
        }
    }

    protected void Add(ArObject arObject)
    {
        if (arObject != null && ArObjects != null)
        {
            ArObjects.Add(arObject);
        }
        if (arObject != null && ArObjectsToFade != null)
        {
            ArObjectsToFade.Add(arObject);
        }
    }

    protected List<Material> GetMaterialsToFade(GameObject objectToFade)
    {
        List<Material> materials = new();
        if (objectToFade != null)
        {
            foreach (var renderer in objectToFade.GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer != null && renderer.material != null)
                {
                    materials.Add(renderer.material);
                }
            }
            foreach (var renderer in objectToFade.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (renderer != null && renderer.material != null)
                {
                    materials.Add(renderer.material);
                }
            }
        }
        return materials;
    }

    private float? _fadeValue;
    private DateTime _lastFadeTime = DateTime.MinValue;
    private bool _doFade = false;
    public void Fade(float value)
    {
        _doFade = true;
        DoFade(value);
    }

    private void DoFade(float value)
    {
        _doFade = true;
        foreach (var arObject in ArObjectsToFade ?? Enumerable.Empty<ArObject>())
        {
            foreach (var objectToFade in arObject.GameObjects ?? Enumerable.Empty<GameObject>())
            {
                List<Material> materialsToFade = GetMaterialsToFade(objectToFade);
                foreach (var material in materialsToFade)
                {
                    var color = material.color;
                    material.color = new Color(color.r, color.g, color.b, value);
                }
            }
        }
    }

    public void Fade()
    {
        if (_doFade)
        {
            List<Material> materialsToFade = GetMaterialsToFade(gameObject);
            var fadeValue = materialsToFade.FirstOrDefault()?.color.a ?? -1f;
            if (fadeValue >= 0f)
            {
                DoFade(fadeValue);
            }
        }
    }
}
