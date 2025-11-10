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
using Unity.Mathematics;
using UnityEngine;

public class ArpoisePoiGrid : ArpoisePoiStructure
{
    #region Grid parameters
    public float PhotonStartPos = -8;
    public float PhotonMaxPos = 8;
    public float PhotonHeightPos = -0.5f;
    public float Sweep = 0f;
    public float Speed = 1.0f; // meters per second
    public int WaitBeforePhotons = 10000; // milliseconds
    public int ShowPhotons = 5000;
    public int TrapAtoms = 10000;
    public int TweezeAtoms = 10000;
    public int WaitAfterTweezedAtoms = 10000;
    public int AreaSize = 6; // The dimensions of the area the grid happens in
    public int AreaHeight = 3;
    public int Beams = 3;
    public float AnimationSmoothFactor = 1;
    public string Photon = string.Empty;
    public string Tweezer = string.Empty;
    #endregion

    private readonly List<GameObject> _atoms = new();
    private readonly List<ArObject> _atomArObjects = new();

    private readonly List<string> _photonNames = new();
    private readonly List<string> _tweezerNames = new();
    private readonly List<GameObject> _xDirectionBeamsPhotons = new();
    private readonly List<GameObject> _yDirectionBeamsPhotons = new();
    private readonly List<GameObject> _zDirectionBeamsPhotons = new();
    private readonly List<GameObject> _xDirectionBeamsPhotonsR = new();
    private readonly List<GameObject> _yDirectionBeamsPhotonsR = new();
    private readonly List<GameObject> _zDirectionBeamsPhotonsR = new();
    private readonly List<ArObject> _photonArObjects = new();
    private readonly List<GameObject> _tweezers = new();
    private readonly List<ArObject> _tweezerArObjects = new();

    private float _sweepDirectionXZ = 1;
    private float _sweepDirectionXY = -1;
    private float _sweepDirectionYX = 1;
    private float _sweepDirectionYZ = -1;
    private float _sweepDirectionZX = 1;

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
        else if (label.Equals(nameof(Sweep)))
        {
            Sweep = ParameterHelper.SetParameter(setValue, value, Sweep).Value;
        }
        else if (label.Equals(nameof(Beams)))
        {
            Beams = ParameterHelper.SetParameter(setValue, value, Beams).Value;
        }
        else if (label.Equals(nameof(AnimationSmoothFactor)))
        {
            AnimationSmoothFactor = ParameterHelper.SetParameter(setValue, value, AnimationSmoothFactor).Value;
        }
        else if (label.Equals(nameof(PhotonStartPos)))
        {
            PhotonStartPos = ParameterHelper.SetParameter(setValue, value, PhotonStartPos).Value;
        }
        else if (label.Equals(nameof(PhotonMaxPos)))
        {
            PhotonMaxPos = ParameterHelper.SetParameter(setValue, value, PhotonMaxPos).Value;
        }
        else if (label.Equals(nameof(PhotonHeightPos)))
        {
            PhotonHeightPos = ParameterHelper.SetParameter(setValue, value, PhotonHeightPos).Value;
        }
        else if (label.Equals(nameof(WaitBeforePhotons)))
        {
            WaitBeforePhotons = ParameterHelper.SetParameter(setValue, value, WaitBeforePhotons).Value;
        }
        else if (label.Equals(nameof(ShowPhotons)))
        {
            ShowPhotons = ParameterHelper.SetParameter(setValue, value, ShowPhotons).Value;
        }
        else if (label.Equals(nameof(TrapAtoms)))
        {
            TrapAtoms = ParameterHelper.SetParameter(setValue, value, TrapAtoms).Value;
        }
        else if (label.Equals(nameof(TweezeAtoms)))
        {
            TweezeAtoms = ParameterHelper.SetParameter(setValue, value, TweezeAtoms).Value;
        }
        else if (label.Equals(nameof(WaitAfterTweezedAtoms)))
        {
            WaitAfterTweezedAtoms = ParameterHelper.SetParameter(setValue, value, WaitAfterTweezedAtoms).Value;
        }
        else if (label.Equals(nameof(Photon)))
        {
            ParameterHelper.SetParameter(setValue, value, _photonNames);
        }
        else if (label.Equals(nameof(Tweezer)))
        {
            ParameterHelper.SetParameter(setValue, value, _tweezerNames);
        }
        else
        {
            base.SetParameter(setValue, label, value);
        }
    }

    private readonly List<ArAnimation> _animationsToSmooth = new();
    private readonly Dictionary<ArAnimation, float> _animationsToExcite = new();

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
                    _atoms.Add(gridObject);
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
            }
            Fade(); // Set the initial fade value
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

    private List<ArObject> CreateTweezers()
    {
        _tweezerArObjects.Clear();

        if (_tweezerNames.Count > 0)
        {
            for (int i = 0; i < 2; i++)
            {
                var tweezer = _tweezerNames[i % _tweezerNames.Count];
                var tweezerObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == tweezer);
                var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
                if (arObjectState is null || tweezerObject is null)
                {
                    return ArObjects;
                }

                var result = ArBehaviour.CreateArObject(
                    arObjectState,
                    tweezerObject.gameObject,
                    null,
                    transform,
                    tweezerObject.poi,
                    ArBehaviourArObject.ArObjectId,
                    out var gridObject,
                    out var gridArObject
                    );

                if (gridObject != null)
                {
                    _tweezers.Add(gridObject);
                    if (!gridObject.activeSelf)
                    {
                        gridObject.SetActive(true);
                    }
                }
                if (gridArObject != null)
                {
                    Add(gridArObject);
                    _tweezerArObjects.Add(gridArObject);
                }
                Fade(); // Set the initial fade value
            }
        }
        return _tweezerArObjects;
    }

    private void CreatePhotons()
    {
        var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
        if (arObjectState is null)
        {
            return;
        }

        if (_photonNames.Count > 0)
        {
            var photon = _photonNames[0];
            var photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photon);

            if (photonObject is not null)
            {
                var halfBeams = Beams / 2;
                for (int x = -halfBeams; x <= halfBeams; x++)
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
                            var position = new Vector3(x, PhotonHeightPos, positionZ);
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
        }
        if (_photonNames.Count > 1)
        {
            var photon = _photonNames[1];
            var photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photon);

            if (photonObject is not null)
            {
                var halfBeams = Beams / 2;
                for (int z = -halfBeams; z <= halfBeams; z++)
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
                            var position = new Vector3(positionX, PhotonHeightPos, z);
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
        if (_photonNames.Count > 2)
        {
            var photon = _photonNames[2];
            var photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photon);

            if (photonObject is not null)
            {
                var halfBeams = Beams / 2;
                for (int x = -halfBeams; x <= halfBeams; x++)
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        var positionY = PhotonMaxPos - 1f * i;
                        if (positionY <= PhotonStartPos)
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
                            var position = new Vector3(x, positionY, 0);
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
                            _yDirectionBeamsPhotons.Add(newPhotonObject);
                        }
                    }
                }
            }
        }
        if (_photonNames.Count > 3)
        {
            var photon = _photonNames[3];
            var photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photon);

            if (photonObject is not null)
            {
                var halfBeams = Beams / 2;
                for (int x = -halfBeams; x <= halfBeams; x++)
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        var positionZ = PhotonMaxPos - 1f * i;
                        if (positionZ <= PhotonStartPos)
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
                            var position = new Vector3(x, PhotonHeightPos, positionZ);
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
                            _zDirectionBeamsPhotonsR.Add(newPhotonObject);
                        }
                    }
                }
            }
        }
        if (_photonNames.Count > 4)
        {
            var photon = _photonNames[4];
            var photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photon);

            if (photonObject is not null)
            {
                var halfBeams = Beams / 2;
                for (int z = -halfBeams; z <= halfBeams; z++)
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        var positionX = PhotonMaxPos - 1f * i;
                        if (positionX <= PhotonStartPos)
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
                            var position = new Vector3(positionX, PhotonHeightPos, z);
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
                            _xDirectionBeamsPhotonsR.Add(newPhotonObject);
                        }
                    }
                }
            }
        }
        if (_photonNames.Count > 5)
        {
            var photon = _photonNames[5];
            var photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photon);

            if (photonObject is not null)
            {
                var halfBeams = Beams / 2;
                for (int x = -halfBeams; x <= halfBeams; x++)
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        var positionY = PhotonStartPos + 1f * i;
                        if (positionY > PhotonMaxPos)
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
                            var position = new Vector3(x, positionY, 0);
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
                            _yDirectionBeamsPhotonsR.Add(newPhotonObject);
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
        TrapAtoms,
        TweezeAtoms,
        WaitAfterTweezedAtoms
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

    private int _tweezerIndex = -1;
    private int _tweezerIndex2 = -1;
    private long? _lastTicks = null;
    private long? _trapAtomsStartTicks = null;
    private long? _tweezeAtomsStartTicks = null;
    private DateTime? _animationSmoothTime = null;

    protected override void Update()
    {
        base.Update();

        if (!gameObject.activeSelf)
        {
            if (_atomPositions.Count > 0)
            {
                _atomPositions.Clear();
            }
            SetActive(false, _photonArObjects);
            _lastTicks = null;
            ResetAnimations();
            _tweezerIndex = _tweezerIndex2 = -1;
            _animationSmoothTime = null;
            State = AtomGridState.WaitBeforePhotons;
            return;
        }

        if (_atomArObjects.Count == 0)
        {
            SeedRandom(GetInstanceID());
            ArObjects = CreateAtoms();
        }

        if (_atomPositions.Count == 0)
        {
            CreateAtomPositions();
        }

        switch (State)
        {
            case AtomGridState.WaitBeforePhotons:
                {
                    if (_animationsToExcite.Count > 0)
                    {
                        foreach (var pair in _animationsToExcite)
                        {
                            pair.Key.To = pair.Value;
                        }
                        _animationsToExcite.Clear();
                    }
                    SetActive(false, _photonArObjects);
                    if (_tweezers.Count > 0)
                    {
                        _tweezers[0].SetActive(false);
                    }
                    if (_tweezers.Count > 1)
                    {
                        _tweezers[1].SetActive(false);
                    }
                    if (_nextStateChange is null)
                    {
                        _nextStateChange = DateTime.Now.AddMilliseconds(WaitBeforePhotons);
                    }
                    else if (DateTime.Now >= _nextStateChange.Value)
                    {
                        State = AtomGridState.ShowPhotons;
                        _lastTicks = null;
                        if (Animations is not null && AnimationSmoothFactor != 1)
                        {
                            _animationSmoothTime = DateTime.Now.AddMilliseconds(1000);
                            _animationsToSmooth.Clear();
                            _animationsToSmooth.AddRange(Animations);
                        }
                    }
                }
                break;

            case AtomGridState.ShowPhotons:
                {
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
                        State = AtomGridState.TrapAtoms;
                        _trapAtomsStartTicks = null;
                        _tweezerIndex = _tweezerIndex2 = -1;
                    }
                }
                break;

            case AtomGridState.TrapAtoms:
                {
                    if (_trapAtomsStartTicks is null)
                    {
                        _trapAtomsStartTicks = DateTime.Now.Ticks;
                    }

                    int i = 0;
                    var duration = DateTime.Now.Ticks - _trapAtomsStartTicks;
                    var lerpFactor = (int)((1000 * duration) / TimeSpan.TicksPerSecond) / (TrapAtoms * .5f);

                    if (_tweezerNames.Count > 0 && _tweezerIndex == -1)
                    {
                        var maxDistance = -1f;
                        for (int x = -1; x < 2; x++)
                        {
                            for (int z = -1; z < 2; z++, i++)
                            {
                                var distance = Vector3.Distance(_atomPositions[i % _atomPositions.Count], new Vector3(x, PhotonHeightPos, z));
                                if (distance > maxDistance)
                                {
                                    maxDistance = distance;
                                    _tweezerIndex = i % _atomPositions.Count;
                                }
                            }
                        }
                        i = 0;
                        maxDistance = -1f;
                        for (int x = -1; x < 2; x++)
                        {
                            for (int z = -1; z < 2; z++, i++)
                            {
                                var distance = Vector3.Distance(_atomPositions[i % _atomPositions.Count], new Vector3(x, PhotonHeightPos, z));
                                if (distance > maxDistance && _tweezerIndex != i % _atomPositions.Count)
                                {
                                    maxDistance = distance;
                                    _tweezerIndex2 = i % _atomPositions.Count;
                                }
                            }
                        }
                    }

                    i = 0;
                    for (int x = -1; x < 2; x++)
                    {
                        for (int z = -1; z < 2; z++, i++)
                        {
                            if (i != _tweezerIndex && i != _tweezerIndex2)
                            {
                                _atoms[i % _atoms.Count].transform.localPosition = Vector3.Lerp(_atomPositions[i % _atomPositions.Count], new Vector3(x, PhotonHeightPos, z), lerpFactor);
                            }
                        }
                    }

                    if (_nextStateChange is null)
                    {
                        _nextStateChange = DateTime.Now.AddMilliseconds(TrapAtoms);
                    }
                    else if (DateTime.Now >= _nextStateChange.Value)
                    {
                        if (_tweezerIndex >= 0)
                        {
                            State = AtomGridState.TweezeAtoms;
                            _tweezeAtomsStartTicks = null;
                        }
                        else
                        {
                            State = AtomGridState.WaitAfterTweezedAtoms;
                        }
                    }
                }
                break;

            case AtomGridState.TweezeAtoms:
                {
                    if (_tweezeAtomsStartTicks is null)
                    {
                        _tweezeAtomsStartTicks = DateTime.Now.Ticks;
                    }

                    int i = 0;
                    var duration = DateTime.Now.Ticks - _tweezeAtomsStartTicks;
                    var lerpFactor = (int)((1000 * duration) / TimeSpan.TicksPerSecond) / (TweezeAtoms * .5f);

                    for (int x = -1; _tweezerIndex >= 0 && x < 2; x++)
                    {
                        for (int z = -1; z < 2; z++, i++)
                        {
                            if (i == _tweezerIndex)
                            {
                                if (_tweezers.Count == 0)
                                {
                                    CreateTweezers();
                                }
                                _tweezers[0].transform.localPosition = _atoms[i % _atoms.Count].transform.localPosition = Vector3.Lerp(_atomPositions[i % _atomPositions.Count], new Vector3(x, PhotonHeightPos, z), lerpFactor);
                                _tweezers[0].SetActive(true);
                            }
                            if (i == _tweezerIndex2)
                            {
                                if (_tweezers.Count == 0)
                                {
                                    CreateTweezers();
                                }
                                _tweezers[1].transform.localPosition = _atoms[i % _atoms.Count].transform.localPosition = Vector3.Lerp(_atomPositions[i % _atomPositions.Count], new Vector3(x, PhotonHeightPos, z), lerpFactor);
                                _tweezers[1].SetActive(true);
                            }
                        }
                    }

                    if (_nextStateChange is null)
                    {
                        _nextStateChange = DateTime.Now.AddMilliseconds(TweezeAtoms);
                    }
                    else if (DateTime.Now >= _nextStateChange.Value)
                    {
                        State = AtomGridState.WaitAfterTweezedAtoms;
                        if (_tweezers.Count > 0)
                        {
                            _tweezers[0].SetActive(false);
                        }
                        if (_tweezers.Count > 1)
                        {
                            _tweezers[1].SetActive(false);
                        }
                    }
                }
                break;

            case AtomGridState.WaitAfterTweezedAtoms:
                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(WaitAfterTweezedAtoms);
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    State = AtomGridState.WaitBeforePhotons;
                }
                break;
        }
        HandleAnimations();
        MovePhotons();
    }

    protected override void HandleAnimations()
    {
        base.HandleAnimations();

        if (_animationsToSmooth.Count > 0)
        {
            if (_animationSmoothTime.HasValue && DateTime.Now >= _animationSmoothTime.Value && AnimationSmoothFactor != 1)
            {
                var animation = _animationsToSmooth.FirstOrDefault();
                if (animation is null)
                {
                    _animationSmoothTime = null;
                }
                else
                {
                    animation = _animationsToSmooth[Random.Next(_animationsToSmooth.Count)];
                    _animationSmoothTime = DateTime.Now.AddMilliseconds(100);
                    var to = animation.To;
                    animation.To *= AnimationSmoothFactor;
                    if (!_animationsToExcite.ContainsKey(animation))
                    {
                        _animationsToExcite[animation] = to;
                    }
                }
            }
        }
    }

    private void MovePhotons()
    {
        if (AtomGridState.WaitBeforePhotons != State)
        {
            if (_lastTicks is null)
            {
                _lastTicks = DateTime.Now.Ticks;
            }
            var halfAreaSize = AreaSize / 2.0f;
            var now = DateTime.Now.Ticks;
            var deltaTime = (now - _lastTicks.Value) / (float)TimeSpan.TicksPerSecond;
            _lastTicks = now;
            var distance = Speed * deltaTime;
            var sweepDistance = Sweep * deltaTime;

            if (_xDirectionBeamsPhotons?.Count > 0)
            {
                foreach (var photon in _xDirectionBeamsPhotons)
                {
                    var positionY = photon.transform.localPosition.y + sweepDistance * _sweepDirectionYX * 0.4f;
                    if (positionY > halfAreaSize || positionY < -halfAreaSize)
                    {
                        _sweepDirectionYX *= -1;
                    }
                    var positionZ = photon.transform.localPosition.z + sweepDistance * _sweepDirectionZX * 0.2f;
                    if (positionZ > halfAreaSize || positionZ < -halfAreaSize)
                    {
                        _sweepDirectionZX *= -1;
                    }
                    var positionX = photon.transform.localPosition.x + distance;
                    if (Math.Abs(positionX) > PhotonMaxPos)
                    {
                        photon.transform.localPosition = new Vector3(PhotonStartPos + (Math.Abs(positionX) - PhotonMaxPos), positionY, positionZ);
                    }
                    else
                    {
                        photon.transform.localPosition = new Vector3(positionX, positionY, positionZ);
                    }
                }
            }
            if (_xDirectionBeamsPhotonsR?.Count > 0)
            {
                foreach (var photon in _xDirectionBeamsPhotonsR)
                {
                    var positionY = photon.transform.localPosition.y + sweepDistance * _sweepDirectionYX * 0.4f;
                    if (positionY > halfAreaSize || positionY < -halfAreaSize)
                    {
                        _sweepDirectionYX *= -1;
                    }
                    var positionZ = photon.transform.localPosition.z + sweepDistance * _sweepDirectionZX * 0.2f;
                    if (positionZ > halfAreaSize || positionZ < -halfAreaSize)
                    {
                        _sweepDirectionZX *= -1;
                    }
                    var positionX = photon.transform.localPosition.x - distance;
                    if (positionX <= PhotonStartPos)
                    {
                        photon.transform.localPosition = new Vector3(PhotonMaxPos - (Math.Abs(positionX) - Math.Abs(PhotonStartPos)), positionY, positionZ);
                    }
                    else
                    {
                        photon.transform.localPosition = new Vector3(positionX, positionY, positionZ);
                    }
                }
            }
            if (_yDirectionBeamsPhotons?.Count > 0)
            {
                foreach (var photon in _yDirectionBeamsPhotons)
                {
                    var positionX = photon.transform.localPosition.x + sweepDistance * _sweepDirectionXY * 0.3f;
                    if (positionX > halfAreaSize || positionX < -halfAreaSize)
                    {
                        _sweepDirectionXY *= -1;
                    }
                    //var positionZ = photon.transform.localPosition.z + sweepDistance * _sweepDirectionZY * 0.9f;
                    //if (positionZ > halfAreaSize || positionZ < -halfAreaSize)
                    //{
                    //    _sweepDirectionZY *= -1;
                    //}
                    var positionY = photon.transform.localPosition.y - distance;
                    if (positionY <= PhotonStartPos)
                    {
                        photon.transform.localPosition = new Vector3(positionX, PhotonMaxPos - (Math.Abs(positionY) - Math.Abs(PhotonStartPos)), photon.transform.localPosition.z);
                    }
                    else
                    {
                        photon.transform.localPosition = new Vector3(positionX, positionY, photon.transform.localPosition.z);
                    }
                }
            }
            if (_yDirectionBeamsPhotonsR?.Count > 0)
            {
                foreach (var photon in _yDirectionBeamsPhotonsR)
                {
                    var positionX = photon.transform.localPosition.x + sweepDistance * _sweepDirectionXY * 0.3f;
                    if (positionX > halfAreaSize || positionX < -halfAreaSize)
                    {
                        _sweepDirectionXY *= -1;
                    }
                    //var positionZ = photon.transform.localPosition.z + sweepDistance * _sweepDirectionZY * 0.9f;
                    //if (positionZ > halfAreaSize || positionZ < -halfAreaSize)
                    //{
                    //    _sweepDirectionZY *= -1;
                    //}
                    var positionY = photon.transform.localPosition.y + distance;
                    if (positionY > PhotonMaxPos)
                    {
                        photon.transform.localPosition = new Vector3(positionX, PhotonStartPos + (Math.Abs(positionY) - PhotonMaxPos), photon.transform.localPosition.z);
                    }
                    else
                    {
                        photon.transform.localPosition = new Vector3(positionX, positionY, photon.transform.localPosition.z);
                    }
                }
            }
            if (_zDirectionBeamsPhotons?.Count > 0)
            {
                foreach (var photon in _zDirectionBeamsPhotons)
                {
                    var positionX = photon.transform.localPosition.x + sweepDistance * _sweepDirectionXZ * 0.2f;
                    if (positionX > halfAreaSize || positionX < -halfAreaSize)
                    {
                        _sweepDirectionXZ *= -1;
                    }

                    var positionY = photon.transform.localPosition.y + sweepDistance * _sweepDirectionYZ * 0.4f;
                    if (positionY > halfAreaSize || positionY < -halfAreaSize)
                    {
                        _sweepDirectionYZ *= -1;
                    }

                    var positionZ = photon.transform.localPosition.z + distance;
                    if (positionZ > PhotonMaxPos)
                    {
                        photon.transform.localPosition = new Vector3(positionX, positionY, PhotonStartPos + (Math.Abs(positionZ) - PhotonMaxPos));
                    }
                    else
                    {
                        photon.transform.localPosition = new Vector3(positionX, positionY, positionZ);
                    }
                }
            }
            if (_zDirectionBeamsPhotonsR?.Count > 0)
            {
                foreach (var photon in _zDirectionBeamsPhotonsR)
                {
                    var positionX = photon.transform.localPosition.x + sweepDistance * _sweepDirectionXZ * 0.2f;
                    if (positionX > halfAreaSize || positionX < -halfAreaSize)
                    {
                        _sweepDirectionXZ *= -1;
                    }

                    var positionY = photon.transform.localPosition.y + sweepDistance * _sweepDirectionYZ * 0.4f;
                    if (positionY > halfAreaSize || positionY < -halfAreaSize)
                    {
                        _sweepDirectionYZ *= -1;
                    }

                    var positionZ = photon.transform.localPosition.z - distance;
                    if (positionZ <= PhotonStartPos)
                    {
                        photon.transform.localPosition = new Vector3(positionX, positionY, PhotonMaxPos - (Math.Abs(positionZ) - Math.Abs(PhotonStartPos)));
                    }
                    else
                    {
                        photon.transform.localPosition = new Vector3(positionX, positionY, positionZ);
                    }
                }
            }
        }
    }
}
