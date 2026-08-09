/*
ArpoisePoiAtomEntangled.cs - A script handling an 'atom - entanglement' for ARpoise.

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
using UnityEngine;

public class ArpoisePoiAtomEntangled : ArpoisePoiStructure
{
    #region AtomEntangled parameters

    public Vector3 PhotonStartPosition = new Vector3(0, 7, 0);
    public Vector3 ReadoutPhotonStartPosition = new Vector3(0, 7, 0);

    public float Speed = 1.0f; // meters per second
    public float ReadoutPhotonSpeed = 1.0f;

    public int WaitBeforePhoton = 5000; // milliseconds
    public int RydbergDuration = 10000; // milliseconds
    public int WaitAfterRydberg = 10000; // milliseconds

    public int ReadoutDuration = 4000;

    public string Photon = string.Empty;
    public string RydbergAtom = string.Empty;

    public float Distance = 50f;

    public string ReadoutPhoton = string.Empty;
    public string ReadoutAtom1 = string.Empty;
    public string ReadoutAtom2 = string.Empty;

    #endregion

    private GameObject _phasedAtom;
    private GameObject _animatedAtom;
    private GameObject _atom;
    private GameObject _atom1;
    private GameObject _atom2;
    private readonly List<ArObject> _atomArObjects = new();

    private readonly List<string> _photonNames = new();
    private GameObject _photon;
    private readonly List<ArObject> _photonArObjects = new();

    private readonly List<string> _rydbergAtomNames = new();
    private GameObject _rydbergAtom;
    private readonly List<ArObject> _rydbergAtomArObjects = new();

    private readonly List<string> _readoutPhotonNames = new();
    private GameObject _readoutPhoton1;
    private GameObject _readoutPhoton2;
    private readonly List<ArObject> _readoutPhotonArObjects = new();

    private readonly List<string> _readoutAtom1Names = new();
    private GameObject _readoutAtom1;
    private readonly List<string> _readoutAtom2Names = new();
    private GameObject _readoutAtom2;
    private readonly List<ArObject> _readoutAtomArObjects = new();

    public override void SetParameter(bool setValue, string label, string value)
    {
        if (label.Equals(nameof(PhotonStartPosition)))
        {
            PhotonStartPosition = ParameterHelper.SetParameter(setValue, value, PhotonStartPosition).Value;
        }
        else if (label.Equals(nameof(ReadoutPhotonStartPosition)))
        {
            ReadoutPhotonStartPosition = ParameterHelper.SetParameter(setValue, value, ReadoutPhotonStartPosition).Value;
        }
        else if (label.Equals(nameof(Speed)))
        {
            Speed = ParameterHelper.SetParameter(setValue, value, Speed).Value;
        }
        else if (label.Equals(nameof(ReadoutPhotonSpeed)))
        {
            ReadoutPhotonSpeed = ParameterHelper.SetParameter(setValue, value, ReadoutPhotonSpeed).Value;
        }
        else if (label.Equals(nameof(Distance)))
        {
            Distance = ParameterHelper.SetParameter(setValue, value, Distance).Value;
        }
        else if (label.Equals(nameof(WaitBeforePhoton)))
        {
            WaitBeforePhoton = ParameterHelper.SetParameter(setValue, value, WaitBeforePhoton).Value;
        }
        else if (label.Equals(nameof(RydbergDuration)))
        {
            RydbergDuration = ParameterHelper.SetParameter(setValue, value, RydbergDuration).Value;
        }
        else if (label.Equals(nameof(WaitAfterRydberg)))
        {
            WaitAfterRydberg = ParameterHelper.SetParameter(setValue, value, WaitAfterRydberg).Value;
        }
        else if (label.Equals(nameof(ReadoutDuration)))
        {
            ReadoutDuration = ParameterHelper.SetParameter(setValue, value, ReadoutDuration).Value;
        }
        else if (label.Equals(nameof(Photon)))
        {
            ParameterHelper.SetParameter(setValue, value, _photonNames);
        }
        else if (label.Equals(nameof(RydbergAtom)))
        {
            ParameterHelper.SetParameter(setValue, value, _rydbergAtomNames);
        }
        else if (label.Equals(nameof(ReadoutPhoton)))
        {
            ParameterHelper.SetParameter(setValue, value, _readoutPhotonNames);
        }
        else if (label.Equals(nameof(ReadoutAtom1)))
        {
            ParameterHelper.SetParameter(setValue, value, _readoutAtom1Names);
        }
        else if (label.Equals(nameof(ReadoutAtom2)))
        {
            ParameterHelper.SetParameter(setValue, value, _readoutAtom2Names);
        }
        else
        {
            base.SetParameter(setValue, label, value);
        }
    }

    private void CreateAtoms()
    {
        if (Pois.Count == 4)
        {
            for (int i = 0; i < 2; i++)
            {
                var poi = Pois[0];
                var poiObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == poi);
                var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
                if (arObjectState is null || poiObject is null)
                {
                    return;
                }

                var result = ArBehaviour.CreateArObject(
                    arObjectState,
                    poiObject.gameObject,
                    null,
                    transform,
                    poiObject.poi,
                    ArBehaviourArObject.ArObjectId,
                    out _atom,
                    out var atomArObject
                    );

                if (_atom != null)
                {
                    if (!_atom.activeSelf)
                    {
                        _atom.SetActive(true);
                    }
                }

                var newTransform = _atom?.transform;
                if (newTransform != null)
                {
                    newTransform.localPosition = new Vector3(i * 2 * Distance - Distance, 0, 0);
                }
                if (atomArObject != null)
                {
                    Add(atomArObject);
                    ArObjectsToFade.Add(atomArObject);
                    if (i == 0)
                    {
                        _atomArObjects.Clear();
                    }
                    _atomArObjects.Add(atomArObject);
                }
                if (i == 0)
                {
                    _atom1 = _atom;
                }
                else
                {
                    _atom2 = _atom;
                }

                poi = Pois[1];
                if (i == 1)
                {
                    poi = Pois[2];
                }
                poiObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == poi);
                arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
                if (arObjectState is null || poiObject is null)
                {
                    return;
                }

                result = ArBehaviour.CreateArObject(
                    arObjectState,
                    poiObject.gameObject,
                    null,
                    transform,
                    poiObject.poi,
                    ArBehaviourArObject.ArObjectId,
                    out _animatedAtom,
                    out atomArObject
                    );

                if (_animatedAtom != null)
                {
                    if (!_animatedAtom.activeSelf)
                    {
                        _animatedAtom.SetActive(true);
                    }
                }
                newTransform = _animatedAtom?.transform;
                if (newTransform != null)
                {
                    newTransform.localPosition = new Vector3(i * 2 * Distance - Distance, 0, 0); ;
                }
                if (atomArObject != null)
                {
                    Add(atomArObject);
                    ArObjectsToFade.Add(atomArObject);
                    _atomArObjects.Add(atomArObject);
                }

                if (i == 1)
                {
                    poi = Pois[3];
                    poiObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == poi);
                    arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
                    if (arObjectState is null || poiObject is null)
                    {
                        return;
                    }

                    result = ArBehaviour.CreateArObject(
                        arObjectState,
                        poiObject.gameObject,
                        null,
                        transform,
                        poiObject.poi,
                        ArBehaviourArObject.ArObjectId,
                        out _phasedAtom,
                        out atomArObject
                        );

                    if (_phasedAtom != null)
                    {
                        if (!_phasedAtom.activeSelf)
                        {
                            _phasedAtom.SetActive(true);
                        }
                    }
                    newTransform = _phasedAtom?.transform;
                    if (newTransform != null)
                    {
                        newTransform.localPosition = new Vector3(i * 2 * Distance - Distance, 0, 0); ;
                    }
                    if (atomArObject != null)
                    {
                        Add(atomArObject);
                        ArObjectsToFade.Add(atomArObject);
                        _atomArObjects.Add(atomArObject);
                    }
                }
            }
        }
    }

    private void CreateReadoutAtoms()
    {
        _readoutAtomArObjects.Clear();
        if (_readoutAtom1Names.Count > 0)
        {
            var poi = _readoutAtom1Names[0];
            var poiObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == poi);
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null || poiObject is null)
            {
                return;
            }

            var result = ArBehaviour.CreateArObject(
                arObjectState,
                poiObject.gameObject,
                null,
                transform,
                poiObject.poi,
                ArBehaviourArObject.ArObjectId,
                out _readoutAtom1,
                out var readoutAtom1ArObject
                );

            if (_readoutAtom1 != null)
            {
                if (!_readoutAtom1.activeSelf)
                {
                    _readoutAtom1.SetActive(true);
                }
            }
            var newTransform = _readoutAtom1?.transform;
            if (newTransform != null)
            {
                newTransform.localPosition = new Vector3(-Distance, 0, 0);
            }
            if (readoutAtom1ArObject != null)
            {
                ArObjectsToFade.Add(readoutAtom1ArObject);
                _readoutAtomArObjects.Add(readoutAtom1ArObject);
            }
        }
        if (_readoutAtom2Names.Count > 0)
        {
            var poi = _readoutAtom2Names[0];
            var poiObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == poi);
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null || poiObject is null)
            {
                return;
            }
            var result = ArBehaviour.CreateArObject(
                arObjectState,
                poiObject.gameObject,
                null,
                transform,
                poiObject.poi,
                ArBehaviourArObject.ArObjectId,
                out _readoutAtom2,
                out var readoutAtom2ArObject
                );

            if (_readoutAtom2 != null)
            {
                if (!_readoutAtom2.activeSelf)
                {
                    _readoutAtom2.SetActive(true);
                }
            }
            var newTransform = _readoutAtom2?.transform;
            if (newTransform != null)
            {
                newTransform.localPosition = new Vector3(Distance, 0, 0);
            }
            if (readoutAtom2ArObject != null)
            {
                ArObjectsToFade.Add(readoutAtom2ArObject);
                _readoutAtomArObjects.Add(readoutAtom2ArObject);
            }
        }
    }

    private void CreatePhoton()
    {
        if (_photonNames.Count > 0)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null)
            {
                return;
            }

            var atomTransform = _atom?.transform;
            var photonName = _photonNames[Random.Next(_photonNames.Count)];
            var photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photonName);
            if (photonObject != null && atomTransform != null)
            {
                var result = ArBehaviour.CreateArObject(
                    arObjectState,
                    photonObject.gameObject,
                    null,
                    atomTransform,
                    photonObject.poi,
                    ArBehaviourArObject.ArObjectId,
                    out _photon,
                    out var photonArObject
                    );

                if (_photon != null)
                {
                    if (!_photon.activeSelf)
                    {
                        _photon.SetActive(true);
                    }
                }
                if (photonArObject != null)
                {
                    ArObjectsToFade.Add(photonArObject);
                    _photonArObjects.Clear();
                    _photonArObjects.Add(photonArObject);
                }
            }
        }
    }

    private void CreateReadoutPhotons()
    {
        if (_readoutPhotonNames.Count > 0)
        {
            for (int i = 0; i < 2; i++)
            {
                var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
                if (arObjectState is null)
                {
                    return;
                }

                var atomTransform = i == 0 ? _atom1?.transform : _atom2?.transform;
                var readoutPhotonName = _readoutPhotonNames[Random.Next(_readoutPhotonNames.Count)];
                var readoutPhotonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == readoutPhotonName);
                if (readoutPhotonObject != null && atomTransform != null)
                {
                    var result = ArBehaviour.CreateArObject(
                        arObjectState,
                        readoutPhotonObject.gameObject,
                        null,
                        atomTransform,
                        readoutPhotonObject.poi,
                        ArBehaviourArObject.ArObjectId,
                        out var readoutPhoton,
                        out var readoutPhotonArObject
                        );

                    if (i == 0)
                    {
                        _readoutPhoton1 = readoutPhoton;
                    }
                    else
                    {
                        _readoutPhoton2 = readoutPhoton;
                    }

                    if (readoutPhoton != null)
                    {
                        if (!readoutPhoton.activeSelf)
                        {
                            readoutPhoton.SetActive(true);
                        }
                    }
                    if (readoutPhotonArObject != null)
                    {
                        ArObjectsToFade.Add(readoutPhotonArObject);
                        _readoutPhotonArObjects.Clear();
                        _readoutPhotonArObjects.Add(readoutPhotonArObject);
                    }
                }
            }
        }
    }

    private void CreatePhotonPosition()
    {
        var photonTransform = _photon?.transform;
        if (photonTransform != null)
        {
            photonTransform.localPosition = PhotonStartPosition;
        }
    }

    private void CreateReadoutPhotonPositions()
    {
        var readoutPhotonTransform = _readoutPhoton1?.transform;
        if (readoutPhotonTransform != null)
        {
            readoutPhotonTransform.localPosition = ReadoutPhotonStartPosition;
        }
        readoutPhotonTransform = _readoutPhoton2?.transform;
        if (readoutPhotonTransform != null)
        {
            readoutPhotonTransform.localPosition = ReadoutPhotonStartPosition;
        }
    }

    private void CreateRydbergAtom()
    {
        if (_rydbergAtomNames.Count > 0)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null)
            {
                return;
            }

            var atomTransform = _atom?.transform;
            var atomName = _rydbergAtomNames[Random.Next(_rydbergAtomNames.Count)];
            var rydbergAtomObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == atomName);
            if (rydbergAtomObject != null && atomTransform != null)
            {
                var result = ArBehaviour.CreateArObject(
                    arObjectState,
                    rydbergAtomObject.gameObject,
                    null,
                    transform,
                    rydbergAtomObject.poi,
                    ArBehaviourArObject.ArObjectId,
                    out _rydbergAtom,
                    out var rydbergAtomArObject
                    );

                if (_rydbergAtom != null)
                {
                    if (!_rydbergAtom.activeSelf)
                    {
                        _rydbergAtom.SetActive(true);
                    }
                }
                if (rydbergAtomArObject != null)
                {
                    ArObjectsToFade.Add(rydbergAtomArObject);
                    _rydbergAtomArObjects.Clear();
                    _rydbergAtomArObjects.Add(rydbergAtomArObject);
                }
            }
        }
    }

    private long? _lastTicks = null;
    private float? _lastDistanceToAtom = null;
    private float? _lastDistanceToReadoutAtom1 = null;
    private float? _lastDistanceToReadoutAtom2 = null;

    private enum AtomEntangledState
    {
        WaitBeforePhoton,
        ShowPhoton,
        ShowRydbergAtom,
        WaitAfterRydbergAtom,
        ShowReadoutPhotons,
        ShowReadoutAtoms
    }

    private DateTime? _nextStateChange = null;
    private AtomEntangledState _state = AtomEntangledState.WaitBeforePhoton;
    private AtomEntangledState State
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

    protected override void Update()
    {
        base.Update();

        if (!gameObject.activeSelf)
        {
            SetActive(false, _atomArObjects);
            SetActive(false, _photonArObjects);
            SetActive(false, _rydbergAtomArObjects);
            SetActive(false, _readoutPhotonArObjects);
            SetActive(false, _readoutAtomArObjects);

            _lastTicks = null;
            _lastDistanceToAtom = null;
            _lastDistanceToReadoutAtom1 = null;
            _lastDistanceToReadoutAtom2 = null;
            State = AtomEntangledState.WaitBeforePhoton;
            ArBehaviour.Value = "Inactive";
            return;
        }

        ArBehaviour.Value = _state.ToString();

        if (_atomArObjects.Count == 0)
        {
            SeedRandom(GetInstanceID());
            UnityEngine.Random.InitState(Random.Next(int.MaxValue));
            CreateAtoms();
            Fade(); // Set the initial fade value
        }

        if (State == AtomEntangledState.WaitAfterRydbergAtom || State == AtomEntangledState.ShowReadoutPhotons)
        {
            if (_animatedAtom.transform.localScale.x != 0.0001f)
            {
                _animatedAtom.transform.localScale = new Vector3(0.0001f, 0.0001f, 0.0001f);
            }
            if (_phasedAtom.transform.localScale.x != 100f)
            {
                _phasedAtom.transform.localScale = new Vector3(100f, 100f, 100f);
            }
        }
        else
        {
            if (_animatedAtom.transform.localScale.x != 100f)
            {
                _animatedAtom.transform.localScale = new Vector3(100f, 100f, 100f);
            }
            if (_phasedAtom.transform.localScale.x != 0.0001f)
            {
                _phasedAtom.transform.localScale = new Vector3(0.0001f, 0.0001f, 0.0001f);
            }
        }
        switch (State)
        {
            case AtomEntangledState.WaitBeforePhoton:
                if (_atomArObjects.Count == 0)
                {
                    CreateAtoms();
                }
                SetActive(gameObject.activeSelf, _atomArObjects);

                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(WaitBeforePhoton);
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    State = AtomEntangledState.ShowPhoton;
                    _lastTicks = null;
                    _lastDistanceToAtom = null;
                    CreatePhotonPosition();
                }
                break;

            case AtomEntangledState.ShowPhoton:
                if (_photonArObjects is null || _photonArObjects.Count == 0)
                {
                    CreatePhoton();
                    CreatePhotonPosition();
                }
                SetActive(gameObject.activeSelf, _photonArObjects);

                var atomTransform = _atom?.transform;
                var photonTransform = _photon?.transform;
                if (atomTransform == null || photonTransform == null)
                {
                    SetActive(false, _photonArObjects);
                    State = AtomEntangledState.ShowRydbergAtom;
                    return;
                }

                if (_lastTicks is null)
                {
                    _lastTicks = DateTime.Now.Ticks;
                }
                else
                {
                    photonTransform.LookAt(atomTransform);
                    var deltaTime = (DateTime.Now.Ticks - _lastTicks.Value) / (float)TimeSpan.TicksPerSecond;
                    _lastTicks = DateTime.Now.Ticks;

                    photonTransform.localPosition += Speed * deltaTime * photonTransform.forward;
                }

                var distanceToAtom = Vector3.Distance(atomTransform.position, photonTransform.position);
                if (distanceToAtom < 0.001 || (_lastDistanceToAtom.HasValue && _lastDistanceToAtom.Value < distanceToAtom))
                {
                    SetActive(false, _photonArObjects);
                    photonTransform.localPosition = new Vector3(0, 1000, 0);
                    State = AtomEntangledState.ShowRydbergAtom;
                }
                else
                {
                    _lastDistanceToAtom = distanceToAtom;
                }
                break;

            case AtomEntangledState.ShowRydbergAtom:
                if (_rydbergAtomArObjects is null || _rydbergAtomArObjects.Count == 0)
                {
                    CreateRydbergAtom();
                }
                SetActive(gameObject.activeSelf, _rydbergAtomArObjects);

                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(RydbergDuration);
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    SetActive(false, _rydbergAtomArObjects);
                    State = AtomEntangledState.WaitAfterRydbergAtom;
                }
                break;

            case AtomEntangledState.WaitAfterRydbergAtom:
                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(WaitAfterRydberg);
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    if (_readoutPhotonArObjects is null || _readoutPhotonArObjects.Count == 0)
                    {
                        CreateReadoutPhotons();
                    }
                    if (_readoutPhotonArObjects == null || _readoutPhotonArObjects.Count == 0)
                    {
                        State = AtomEntangledState.WaitBeforePhoton;
                    }
                    else
                    {
                        State = AtomEntangledState.ShowReadoutPhotons;
                        _lastTicks = null;
                        _lastDistanceToReadoutAtom1 = null;
                        _lastDistanceToReadoutAtom2 = null;
                        CreateReadoutPhotonPositions();
                    }
                }
                break;

            case AtomEntangledState.ShowReadoutPhotons:
                {
                    if (_readoutPhotonArObjects is null || _readoutPhotonArObjects.Count == 0)
                    {
                        CreateReadoutPhotons();
                        CreateReadoutPhotonPositions();
                    }
                    SetActive(gameObject.activeSelf, _readoutPhotonArObjects);

                    var atom1Transform = _atom1?.transform;
                    var atom2Transform = _atom2?.transform;
                    var photon1Transform = _readoutPhoton1?.transform;
                    var photon2Transform = _readoutPhoton2?.transform;

                    if (atom1Transform == null || atom2Transform == null || photon1Transform == null || photon2Transform == null)
                    {
                        SetActive(false, _atomArObjects);
                        SetActive(false, _readoutPhotonArObjects);
                        State = AtomEntangledState.ShowReadoutAtoms;
                        return;
                    }

                    if (_lastTicks is null)
                    {
                        _lastTicks = DateTime.Now.Ticks;
                    }
                    else
                    {
                        var deltaTime = (DateTime.Now.Ticks - _lastTicks.Value) / (float)TimeSpan.TicksPerSecond;
                        _lastTicks = DateTime.Now.Ticks;

                        photon1Transform.LookAt(atom1Transform);
                        photon1Transform.localPosition += ReadoutPhotonSpeed * deltaTime * photon1Transform.forward;
                        photon2Transform.LookAt(atom2Transform);
                        photon2Transform.localPosition += ReadoutPhotonSpeed * deltaTime * photon2Transform.forward;
                    }

                    var readoutDistanceToAtom1 = Vector3.Distance(atom1Transform.position, photon1Transform.position);
                    var readoutDistanceToAtom2 = Vector3.Distance(atom2Transform.position, photon2Transform.position);
                    if (readoutDistanceToAtom1 < 0.001 || readoutDistanceToAtom2 < 0.001
                        || (_lastDistanceToReadoutAtom1.HasValue && _lastDistanceToReadoutAtom1.Value < readoutDistanceToAtom1)
                        || (_lastDistanceToReadoutAtom2.HasValue && _lastDistanceToReadoutAtom2.Value < readoutDistanceToAtom2)
                        )
                    {
                        SetActive(false, _atomArObjects);
                        SetActive(false, _readoutPhotonArObjects);
                        photon1Transform.localPosition = new Vector3(0, 1000, 0);
                        photon2Transform.localPosition = new Vector3(0, 1000, 0);
                        State = AtomEntangledState.ShowReadoutAtoms;
                    }
                    else
                    {
                        _lastDistanceToReadoutAtom1 = readoutDistanceToAtom1;
                        _lastDistanceToReadoutAtom2 = readoutDistanceToAtom2;
                    }
                }
                break;

            case AtomEntangledState.ShowReadoutAtoms:
                {
                    if (_readoutAtomArObjects is null || _readoutAtomArObjects.Count == 0)
                    {
                        CreateReadoutAtoms();
                    }

                    var readoutAtom1Transform = _readoutAtom1?.transform;
                    var readoutAtom2Transform = _readoutAtom2?.transform;
                    if (readoutAtom1Transform == null || readoutAtom2Transform == null)
                    {
                        SetActive(false, _readoutAtomArObjects);
                        State = AtomEntangledState.WaitBeforePhoton;
                        return;
                    }

                    if (_nextStateChange is null)
                    {
                        if (Random.Next(2) == 0)
                        {
                            readoutAtom1Transform.localPosition = new Vector3(-Distance, 0, 0);
                            readoutAtom2Transform.localPosition = new Vector3(Distance, 0, 0);
                        }
                        else
                        {
                            readoutAtom1Transform.localPosition = new Vector3(Distance, 0, 0);
                            readoutAtom2Transform.localPosition = new Vector3(-Distance, 0, 0);
                        }
                    }
                    SetActive(gameObject.activeSelf, _readoutAtomArObjects);

                    if (_nextStateChange is null)
                    {
                        _nextStateChange = DateTime.Now.AddMilliseconds(ReadoutDuration);
                    }
                    else if (DateTime.Now >= _nextStateChange.Value)
                    {
                        SetActive(false, _readoutAtomArObjects);
                        State = AtomEntangledState.WaitBeforePhoton;
                    }
                }
                break;
        }
    }
}
