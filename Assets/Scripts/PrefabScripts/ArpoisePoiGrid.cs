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
    public int ShowPhotons = 5000;
    public int CoolAtoms = 10000;
    public int WaitAfterCooledAtoms = 10000;
    public int AreaSize = 6; // The dimensions of the area the grid happens in
    public int AreaHeight = 3;

    public string Photon = string.Empty;
    #endregion

    private readonly List<GameObject> _atoms = new();
    private readonly List<ArObject> _atomArObjects = new();

    private readonly List<string> _photonNames = new();
    private readonly List<GameObject> _zDirectionBeamsPhotons = new();
    private readonly List<GameObject> _xDirectionBeamsPhotons = new();
    private readonly List<ArObject> _photonArObjects = new();

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

    private readonly List<Vector3> _atomPositions = new();

    private List<ArObject> CreateAtoms()
    {
        ArObjects = new List<ArObject>();

        if (Pois.Count > 0)
        {
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

    private void CreateAtomPositions()
    {
        foreach (var atom in _atoms)
        {
            Vector3 position = new Vector3(
                Random.Next(-1000 * AreaSize / 2, 1000 * AreaSize / 2) / 1000.0f,
                Random.Next(-1000 * AreaHeight / 2, 1000 * AreaHeight / 2) / 1000.0f,
                Random.Next(-1000 * AreaSize / 2, 1000 * AreaSize / 2) / 1000.0f);

            atom.transform.localPosition = position;
            _atomPositions.Add(position);
        }
    }

    private void CreatePhotons()
    {
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
                            out var newPhotonObject,
                            out var newPhotonArObject);

                        if (newPhotonObject != null)
                        {
                            if (!newPhotonObject.activeSelf)
                            {
                                newPhotonObject.SetActive(true);
                            }
                            var position = new Vector3(x, -1, positionZ);
                            newPhotonObject.transform.localPosition = position;
                            newPhotonObject.transform.localEulerAngles = new Vector3(0, 0, 1);
                        }
                        if (newPhotonArObject != null)
                        {
                            ArObjectsToFade.Add(newPhotonArObject);
                            _photonArObjects.Add(newPhotonArObject);
                        }
                        if (newPhotonObject != null)
                        {
                            _zDirectionBeamsPhotons.Add(newPhotonObject);
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
                            out var newPhotonObject,
                            out var newPhotonArObject);

                        if (newPhotonObject != null)
                        {
                            if (!newPhotonObject.activeSelf)
                            {
                                newPhotonObject.SetActive(true);
                            }
                            var position = new Vector3(positionX, -1, z);
                            newPhotonObject.transform.localPosition = position;
                            newPhotonObject.transform.localEulerAngles = new Vector3(0, 0, 1);
                        }
                        if (newPhotonArObject != null)
                        {
                            ArObjectsToFade.Add(newPhotonArObject);
                            _photonArObjects.Add(newPhotonArObject);
                        }
                        if (newPhotonObject != null)
                        {
                            _xDirectionBeamsPhotons.Add(newPhotonObject);
                        }
                    }
                }
            }
        }
    }

    private enum AtomGridState
    {
        WaitBeforePhotons,
        ShowPhotons,
        CoolAtoms,
        WaitAfterCooledAtoms
    }

    private DateTime? _nextStateChange = null;
    private AtomGridState _state = AtomGridState.WaitBeforePhotons;
    private AtomGridState State
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
            if(_atomPositions.Count > 0)
            {
                _atomPositions.Clear();
            }
            SetActive(false, _photonArObjects);
            _lastTicks = null;
            State = AtomGridState.WaitBeforePhotons;
            return;
        }

        if (_atomArObjects is null || _atomArObjects.Count == 0)
        {
            SeedRandom(GetInstanceID());
            ArObjects = CreateAtoms();
        }

        float? lerpFactor = null;
        switch (State)
        {
            case AtomGridState.WaitBeforePhotons:
                if (_atomArObjects is null || _atomArObjects.Count == 0)
                {
                    ArObjects = CreateAtoms();
                }
                if (_atomPositions.Count == 0)
                {
                    CreateAtomPositions();
                }
                SetActive(false, _photonArObjects);
                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(WaitBeforePhotons);
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    State = AtomGridState.ShowPhotons;
                    _lastTicks = null;
                }
                break;

            case AtomGridState.ShowPhotons:
                if (_photonArObjects is null || _photonArObjects.Count == 0)
                {
                    CreatePhotons();
                }
                SetActive(gameObject.activeSelf, _photonArObjects);

                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(ShowPhotons);
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    State = AtomGridState.CoolAtoms;
                    _coolAtomsStartTicks = null;
                }
                break;

            case AtomGridState.CoolAtoms:
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
                    _nextStateChange = DateTime.Now.AddMilliseconds(CoolAtoms);
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    State = AtomGridState.WaitAfterCooledAtoms;
                }
                break;

            case AtomGridState.WaitAfterCooledAtoms:
                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(WaitAfterCooledAtoms);
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    State = AtomGridState.WaitBeforePhotons;
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

        if (AtomGridState.WaitBeforePhotons != State)
        {
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
}
