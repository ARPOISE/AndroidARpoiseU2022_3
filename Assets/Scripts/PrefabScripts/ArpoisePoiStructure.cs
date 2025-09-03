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

    protected List<ArObject> ArObjects = null;

    public virtual void SetParameter(bool setValue, string label, string value)
    {
        if (label.Equals(nameof(MaxNofObjects)))
        {
            MaxNofObjects = ParameterHelper.SetParameter(setValue, value, MaxNofObjects).Value;
        }
        else if (label.Equals(nameof(Poi)))
        {
            if (setValue && !string.IsNullOrWhiteSpace(value))
            {
                if (value.Contains(","))
                {
                    foreach (var p in value.Split(",", StringSplitOptions.RemoveEmptyEntries))
                    {
                        var v = p.Trim();
                        if (!string.IsNullOrEmpty(v) && !Pois.Contains(v))
                        {
                            Pois.Add(v);
                        }
                    }
                }
                else
                {
                    Pois.Add(value);
                }
            }
        }
    }

    public void CallUpdate()
    {
        Update();
    }
    protected virtual void Update()
    {
        if (!gameObject.activeSelf)
        {
            if (ArObjects != null)
            {
                ArBehaviour?.ArObjectState?.DestroyArObjects(ArObjects);
                ArObjects = null;
            }
        }
    }

    protected void Add(ArObject arObject)
    {
        if (arObject != null && ArObjects != null)
        {
            ArObjects.Add(arObject);
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
        }
        return materials;
    }

    private bool _doFade = false;
    public void Fade(float value)
    {
        _doFade = true;
        foreach (var arObject in ArObjects ?? Enumerable.Empty<ArObject>())
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
                Fade(fadeValue);
            }
        }
    }
}
