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
    public float PhotonStartPos = -8;
    public float PhotonMaxPos = 8;
    public float Speed = 1.0f; // meters per second
    public int WaitBeforePhotons = 10000; // milliseconds
    public int ShowPhotons = 10000;
    public int CoolAtoms = 10000;
    public int WaitAfterCooledAtoms = 10000;
    public int AreaSize = 6; // The dimensions of the area the grid happens in
    public int AreaHeight = 3;

    public string Photon = string.Empty;
    #endregion

    private List<GameObject> _atoms;
    private List<ArObject> _atomArObjects;

    private List<string> _photonNames = new();
    private List<GameObject> _zDirectionBeamsPhotons;
    private List<GameObject> _xDirectionBeamsPhotons;
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
        else if (label.Equals(nameof(PhotonStartPos)))
        {
            PhotonStartPos = ParameterHelper.SetParameter(setValue, value, PhotonStartPos).Value;
        }
        else if (label.Equals(nameof(PhotonMaxPos)))
        {
            PhotonMaxPos = ParameterHelper.SetParameter(setValue, value, PhotonMaxPos).Value;
        }
        else if (label.Equals(nameof(WaitBeforePhotons)))
        {
            WaitBeforePhotons = ParameterHelper.SetParameter(setValue, value, WaitBeforePhotons).Value;
        }
        else if (label.Equals(nameof(ShowPhotons)))
        {
            ShowPhotons = ParameterHelper.SetParameter(setValue, value, ShowPhotons).Value;
        }
        else if (label.Equals(nameof(CoolAtoms)))
        {
            CoolAtoms = ParameterHelper.SetParameter(setValue, value, CoolAtoms).Value;
        }
        else if (label.Equals(nameof(WaitAfterCooledAtoms)))
        {
            WaitAfterCooledAtoms = ParameterHelper.SetParameter(setValue, value, WaitAfterCooledAtoms).Value;
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

    public void DestroyAtoms()
    {
        if (_atomArObjects != null)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is not null)
            {
                foreach (var atomArObject in _atomArObjects)
                {
                    arObjectState.DestroyArObject(atomArObject);
                }
            }
        }
        _atomArObjects = null;
        _atoms = null;
    }

    private List<Vector3> _atomPositions = new();

    private List<ArObject> CreateAtoms()
    {
        ArObjects = new List<ArObject>();
        _atomArObjects = new List<ArObject>();
        _atoms = new List<GameObject>();

        if (Pois.Count > 0)
        {
            _atomPositions = new();
            while (ArObjects.Count < MaxNofObjects)
            {
                var poi = Pois[Random.Next(Pois.Count)];
                var poiObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == poi);
                var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
                if (arObjectState is null || poiObject is null)
                {
                    return ArObjects;
                }

                var result = ArBehaviour.CreateArObject(
                    arObjectState,
                    poiObject.gameObject,
                    null,
                    transform,
                    poiObject.poi,
                    ArBehaviourArObject.ArObjectId,
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
                    _atomPositions.Add(position);
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
            Fade(); // Set the initial fade value
            _animations = null;
        }
        return ArObjects;
    }

    private void DestroyPhotons()
    {
        if (_photonArObjects != null)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is not null)
            {
                foreach (var _photonArObject in _photonArObjects)
                {
                    arObjectState.DestroyArObject(_photonArObject);
                }
            }
        }
        _photonArObjects = null;
        _zDirectionBeamsPhotons = null;
        _xDirectionBeamsPhotons = null;
        _animations = null;
        _animationStartTimes.Clear();
    }

    private void CreatePhotons()
    {
        _photonArObjects = new List<ArObject>();
        _zDirectionBeamsPhotons = new List<GameObject>();
        _xDirectionBeamsPhotons = new List<GameObject>();

        if (_photonNames.Count > 0)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null)
            {
                return;
            }

            var photon = _photonNames[0];
            var photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photon);

            if (photonObject is not null)
            {
                for (int x = -1; x < 2; x++)
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        var positionZ = PhotonStartPos + 1f * i;
                        if (Math.Abs(positionZ) > PhotonMaxPos)
                        {
                            break;
                        }
                        var result = ArBehaviour.CreateArObject(
                            arObjectState,
                            photonObject.gameObject,
                            null,
                            transform,
                            photonObject.poi,
                            ArBehaviourArObject.ArObjectId,
                            out var _photonObject,
                            out var _photonArObject);

                        if (_photonObject != null)
                        {
                            if (!_photonObject.activeSelf)
                            {
                                _photonObject.SetActive(true);
                            }
                            var position = new Vector3(x, -1, positionZ);
                            _photonObject.transform.localPosition = position;
                        }
                        if (_photonArObject != null)
                        {
                            Add(_photonArObject);
                            _photonArObjects.Add(_photonArObject);
                        }
                        if (_photonObject != null)
                        {
                            _zDirectionBeamsPhotons.Add(_photonObject);
                        }
                    }
                }
            }

            photon = _photonNames[1 % _photonNames.Count];
            photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photon);

            if (photonObject is not null)
            {
                for (int z = -1; z < 2; z++)
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        var positionX = PhotonStartPos + 1f * i;
                        if (Math.Abs(positionX) > PhotonMaxPos)
                        {
                            break;
                        }
                        var result = ArBehaviour.CreateArObject(
                            arObjectState,
                            photonObject.gameObject,
                            null,
                            transform,
                            photonObject.poi,
                            ArBehaviourArObject.ArObjectId,
                            out var _photonObject,
                            out var _photonArObject);

                        if (_photonObject != null)
                        {
                            if (!_photonObject.activeSelf)
                            {
                                _photonObject.SetActive(true);
                            }
                            var position = new Vector3(positionX, -1, z);
                            _photonObject.transform.localPosition = position;
                        }
                        if (_photonArObject != null)
                        {
                            Add(_photonArObject);
                            _photonArObjects.Add(_photonArObject);
                        }
                        if (_photonObject != null)
                        {
                            _xDirectionBeamsPhotons.Add(_photonObject);
                        }
                    }
                }
            }
        }
    }

    private enum AtomPhotonState
    {
        WaitBeforePhotons,
        ShowPhotons,
        CoolAtoms,
        WaitAfterCooledAtoms
    }

    private DateTime? _nextStateChange = null;
    private AtomPhotonState _state = AtomPhotonState.WaitBeforePhotons;
    private AtomPhotonState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                _nextStateChange = null;
            }
        }
    }

    private long? _lastTicks = null;
    private long? _coolAtomsStartTicks = null;
    protected override void Update()
    {
        base.Update();

        if (!gameObject.activeSelf)
        {
            DestroyPhotons();
            DestroyAtoms();
            _lastTicks = null;
            State = AtomPhotonState.WaitBeforePhotons;
            return;
        }

        if (ArObjects is null)
        {
            SeedRandom(GetInstanceID());
            _atoms = new List<GameObject>();
            _atomArObjects = new List<ArObject>();
            _zDirectionBeamsPhotons = new List<GameObject>();
            _xDirectionBeamsPhotons = new List<GameObject>();
            _photonArObjects = new List<ArObject>();
            ArObjects = CreateAtoms();
        }

        float? lerpFactor = null;
        switch (State)
        {
            case AtomPhotonState.WaitBeforePhotons:
                if (_atomArObjects is null)
                {
                    CreateAtoms();
                }
                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(WaitBeforePhotons * .5f + Random.Next(WaitBeforePhotons));
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    DestroyPhotons();
                    State = AtomPhotonState.ShowPhotons;
                }
                break;

            case AtomPhotonState.ShowPhotons:
                if (_photonArObjects is null)
                {
                    CreatePhotons();
                    _lastTicks = null;
                }

                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(ShowPhotons * .5f + Random.Next(ShowPhotons));
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    State = AtomPhotonState.CoolAtoms;
                    _coolAtomsStartTicks = null;
                }
                break;

            case AtomPhotonState.CoolAtoms:
                if (_coolAtomsStartTicks is null)
                {
                    _coolAtomsStartTicks = DateTime.Now.Ticks;
                }

                int i = 0;
                var duration = DateTime.Now.Ticks - _coolAtomsStartTicks;
                lerpFactor = (int)((1000 * duration) / TimeSpan.TicksPerSecond) / (CoolAtoms * .5f);

                for (int x = -1; x < 2; x++)
                {
                    for (int z = -1; z < 2; z++, i++)
                    {
                        _atoms[i % _atoms.Count].transform.localPosition = Vector3.Lerp(_atomPositions[i % _atomPositions.Count], new Vector3(x, -1, z), lerpFactor.Value);
                    }
                }
                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(CoolAtoms * .5f + Random.Next(CoolAtoms));
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    State = AtomPhotonState.WaitAfterCooledAtoms;
                }
                break;

            case AtomPhotonState.WaitAfterCooledAtoms:
                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(WaitAfterCooledAtoms * .5f + Random.Next(WaitAfterCooledAtoms));
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    DestroyPhotons();
                    DestroyAtoms();
                    State = AtomPhotonState.WaitBeforePhotons;
                }
                break;
        }


        if (_animations is null)
        {
            _animations = new List<ArAnimation>();
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is not null)
            {
                _animations.AddRange(arObjectState.AnimationsWithName.Where(x => x.Name != null && x.Name.Contains("GridRandomDelay")));
            }
        }

        if (_animations is not null)
        {
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
                            if (lerpFactor is not null)
                            {
                                if (lerpFactor.Value > 1)
                                {
                                    lerpFactor = 1;
                                }
                                animation.SetTo(Mathf.Lerp(1, 0.1f, lerpFactor.Value));
                            }
                            animation.Activate(ArBehaviour.StartTicks, DateTime.Now.Ticks);
                        }
                    }
                    else
                    {
                        _animationStartTimes[animation] = animation.NextActivation;
                    }
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
        var distance = Speed * deltaTime;

        if (_zDirectionBeamsPhotons is not null)
        {
            foreach (var photon in _zDirectionBeamsPhotons)
            {
                var positionZ = photon.transform.localPosition.z + distance;
                if (Math.Abs(positionZ) > PhotonMaxPos)
                {
                    photon.transform.localPosition = new Vector3(photon.transform.localPosition.x, photon.transform.localPosition.y, PhotonStartPos + (Math.Abs(positionZ) - PhotonMaxPos));
                }
                else
                {
                    photon.transform.localPosition = new Vector3(photon.transform.localPosition.x, photon.transform.localPosition.y, positionZ);
                }
            }
        }

        if (_xDirectionBeamsPhotons is not null)
        {
            foreach (var photon in _xDirectionBeamsPhotons)
            {
                var positionX = photon.transform.localPosition.x + distance;
                if (Math.Abs(positionX) > PhotonMaxPos)
                {
                    photon.transform.localPosition = new Vector3(PhotonStartPos + (Math.Abs(positionX) - PhotonMaxPos), photon.transform.localPosition.y, photon.transform.localPosition.z);
                }
                else
                {
                    photon.transform.localPosition = new Vector3(positionX, photon.transform.localPosition.y, photon.transform.localPosition.z);
                }
            }
        }
    }
}
