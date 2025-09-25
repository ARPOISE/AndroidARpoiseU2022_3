/*
ArpoisePoiGrid.cs - A script handling a 'poi grid' for ARpoise.

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

public class ArpoisePoiGrid : ArpoisePoiStructure
{
    #region Grid parameters
    public float PhotonStartZ = -8;
    public float PhotonMaxZ = 8;
    public float Speed = 1.0f; // meters per second
    public int WaitBeforePhoton = 5000; // milliseconds
    public int AreaSize = 6; // The dimensions of the area the grid happens in
    public int AreaHeight = 3;

    public string Photon = string.Empty;
    #endregion

    private List<GameObject> _atoms;
    private List<ArObject> _atomArObjects;

    private List<string> _photonNames = new();
    private List<GameObject> _photons;
    private List<ArObject> _photonArObjects;

    public override void SetParameter(bool setValue, string label, string value)
    {
        MaxNofObjects = 9;
        if (label.Equals(nameof(AreaSize)))
        {
            AreaSize = ParameterHelper.SetParameter(setValue, value, AreaSize).Value;
        }
        else if (label.Equals(nameof(AreaHeight)))
        {
            AreaHeight = ParameterHelper.SetParameter(setValue, value, AreaHeight).Value;
        }
        else if (label.Equals(nameof(Speed)))
        {
            Speed = ParameterHelper.SetParameter(setValue, value, Speed).Value;
        }
        else if (label.Equals(nameof(WaitBeforePhoton)))
        {
            WaitBeforePhoton = ParameterHelper.SetParameter(setValue, value, WaitBeforePhoton).Value;
        }
        else if (label.Equals(nameof(Photon)))
        {
            ParameterHelper.SetParameter(setValue, value, _photonNames);
        }
        else
        {
            base.SetParameter(setValue, label, value);
        }
    }

    private List<ArAnimation> _animations = null;
    private Dictionary<ArAnimation, DateTime> _animationStartTimes = new Dictionary<ArAnimation, DateTime>();

    private List<ArObject> CreateAtoms()
    {
        ArObjects = new List<ArObject>();
        _atomArObjects = new List<ArObject>();
        _atoms = new List<GameObject>();

        if (Pois.Count > 0)
        {
            var poi = Pois[Random.Next(Pois.Count)];
            var poiObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == poi);
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null || poiObject is null)
            {
                return ArObjects;
            }

            while (ArObjects.Count < MaxNofObjects)
            {
                poi = Pois[Random.Next(Pois.Count)];
                poiObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == poi);
                if (poiObject is not null)
                {
                    var result = ArBehaviour.CreateArObject(
                        arObjectState,
                        poiObject.gameObject,
                        null,
                        transform,
                        poiObject.poi,
                        poiObject.poi.id,
                        out var gridObject,
                        out var gridArObject
                        );

                    if (gridObject != null)
                    {
                        if (!gridObject.activeSelf)
                        {
                            gridObject.SetActive(true);
                        }
                        Vector3 position = new Vector3(
                            Random.Next(-1000 * AreaSize / 2, 1000 * AreaSize / 2) / 1000.0f,
                            Random.Next(-1000 * AreaHeight / 2, 1000 * AreaHeight / 2) / 1000.0f,
                            Random.Next(-1000 * AreaSize / 2, 1000 * AreaSize / 2) / 1000.0f);

                        gridObject.transform.localPosition = position;
                    }
                    if (gridArObject != null)
                    {
                        Add(gridArObject);
                        _atomArObjects.Add(gridArObject);
                    }
                    if (gridObject != null)
                    {
                        _atoms.Add(gridObject);
                    }
                }
            }
            Fade(); // Set the initial fade value

        }
        return ArObjects;
    }

    private void CreatePhotons()
    {
        _photonArObjects = new List<ArObject>();
        _photons = new List<GameObject>();

        if (_photonNames.Count > 1)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null)
            {
                return;
            }

            var photon = _photonNames[1];
            var photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photon);
            if (photonObject is not null)
            {
                photon = _photonNames[0];
                photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photon);

                for (int x = -1; x < 2; x++)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        var result = ArBehaviour.CreateArObject(
                            arObjectState,
                            photonObject.gameObject,
                            null,
                            transform,
                            photonObject.poi,
                            photonObject.poi.id,
                            out var _photon,
                            out var _photonArObject);
                        var photonTransform = _photonArObject?.GameObjects?.FirstOrDefault()?.transform;
                        if (photonTransform != null)
                        {
                            var position = new Vector3(x, -1, PhotonStartZ + 1f * i);
                            photonTransform.localPosition = position;
                        }

                        if (_photon != null)
                        {
                            if (!_photon.activeSelf)
                            {
                                _photon.SetActive(true);
                            }
                        }
                        if (_photonArObject != null)
                        {
                            Add(_photonArObject);
                            _photonArObjects.Add(_photonArObject);
                        }
                        if (_photon != null)
                        {
                            _photons.Add(_photon);
                        }
                    }
                }

                photon = _photonNames[1];
                photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photon);

                for (int x = -1; x < 2; x++)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        var result = ArBehaviour.CreateArObject(
                        arObjectState,
                        photonObject.gameObject,
                        null,
                        transform,
                        photonObject.poi,
                        photonObject.poi.id,
                        out var _photon,
                        out var _photonArObject);
                        var photonTransform = _photonArObject?.GameObjects?.FirstOrDefault()?.transform;
                        if (photonTransform != null)
                        {
                            var position = new Vector3(x, -1, PhotonStartZ + 1f * i);
                            photonTransform.localPosition = position;
                        }

                        if (_photon != null)
                        {
                            if (!_photon.activeSelf)
                            {
                                _photon.SetActive(true);
                            }
                        }
                        if (_photonArObject != null)
                        {
                            Add(_photonArObject);
                            _photonArObjects.Add(_photonArObject);
                        }
                        if (_photon != null)
                        {
                            _photons.Add(_photon);
                        }
                    }
                }
            }
        }
    }

    private long? _lastTicks = null;
    protected override void Update()
    {
        if (gameObject.activeSelf)
        {
            if (ArObjects is null)
            {
                SeedRandom(GetInstanceID());
                _atoms = new List<GameObject>();
                _atomArObjects = new List<ArObject>();
                _photons = new List<GameObject>();
                _photonArObjects = new List<ArObject>();
                ArObjects = CreateAtoms();
                CreatePhotons();
            }
        }

        base.Update();

        if (_animations is null)
        {
            _animations = new List<ArAnimation>();
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null)
            {
                return;
            }
            _animations.AddRange(arObjectState.AnimationsWithName.Where(x => x.Name != null && x.Name.Contains("GridRandomDelay")));
        }
        foreach (var animation in _animations)
        {
            if (!animation.IsActive)
            {
                if (_animationStartTimes.ContainsKey(animation))
                {
                    var startTime = _animationStartTimes[animation];
                    if (DateTime.Now > startTime)
                    {
                        _animationStartTimes.Remove(animation);
                        animation.Activate(ArBehaviour.StartTicks, DateTime.Now.Ticks);
                    }
                }
                else
                {
                    _animationStartTimes[animation] = animation.NextActivation;
                }
            }
        }

        if (_lastTicks is null)
        {
            _lastTicks = DateTime.Now.Ticks;
        }
        var now = DateTime.Now.Ticks;
        var deltaTime = (now - _lastTicks.Value) / (float)TimeSpan.TicksPerSecond;
        _lastTicks = now;

        foreach (var photonArObject in _photonArObjects)
        {
            var photonTransform = photonArObject?.GameObjects?.FirstOrDefault()?.transform;
            if (photonTransform != null)
            {
                var distance = Speed * deltaTime;

                if (photonTransform != null)
                {
                    photonTransform.localPosition += distance * new Vector3(0, 0, 1);
                    if (Math.Abs(photonTransform.localPosition.z) > PhotonMaxZ)
                    {
                        photonTransform.localPosition = new Vector3(photonTransform.localPosition.x, photonTransform.localPosition.y, PhotonStartZ);
                    }
                }
            }
        }
    }
}
