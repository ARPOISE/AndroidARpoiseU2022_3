/*
ArpoisePoiGate.cs - A script handling a 'poi gate' for ARpoise.

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

public class ArpoisePoiGate : ArpoisePoiStructure
{
    #region Gate parameters
    public Vector3 PhotonStartPosition = new Vector3(0, 7, 0);
    public float AtomZPos = 0.5f;

    public float ExcitingPhotonSpeed = 1.0f; // meters per second
    public float RydbergPhotonSpeed = 1.0f;
    public float ReadoutPhotonSpeed = 1.0f;
    public string ExcitingPhoton = string.Empty;
    public string RydbergPhoton = string.Empty;
    public string ReadoutPhoton = string.Empty;
    public string ExcitedAtom = string.Empty;
    public string RydbergAtom = string.Empty;
    public string ReadoutResult = string.Empty;

    #endregion

    private readonly List<GameObject> _atoms = new();
    private readonly List<ArObject> _atomArObjects = new();

    private readonly List<string> _excitingPhotonNames = new();
    private readonly List<GameObject> _excitingPhotons = new();
    private readonly List<ArObject> _excitingPhotonArObjects = new();

    private readonly List<string> _excitedAtomNames = new();
    private readonly List<GameObject> _excitedAtoms = new();
    private readonly List<ArObject> _excitedAtomArObjects = new();

    private readonly List<string> _rydbergPhotonNames = new();
    private readonly List<GameObject> _rydbergPhotons = new();
    private readonly List<ArObject> _rydbergPhotonArObjects = new();

    private readonly List<string> _rydbergAtomNames = new();
    private readonly List<GameObject> _rydbergAtoms = new();
    private readonly List<ArObject> _rydbergAtomArObjects = new();

    private readonly List<string> _readoutPhotonNames = new();
    private readonly List<GameObject> _readoutPhotons = new();
    private readonly List<ArObject> _readoutPhotonArObjects = new();

    private readonly List<string> _readoutResultNames = new();
    private readonly List<GameObject> _readoutResults = new();
    private readonly List<ArObject> _readoutResultArObjects = new();
    private readonly List<GameObject> _readoutResults0 = new();
    private readonly List<ArObject> _readoutResult0ArObjects = new();
    private readonly List<GameObject> _readoutResults1 = new();
    private readonly List<ArObject> _readoutResult1ArObjects = new();

    public override void SetParameter(bool setValue, string label, string value)
    {
        MaxNofObjects = 9;
        if (label.Equals(nameof(PhotonStartPosition)))
        {
            PhotonStartPosition = ParameterHelper.SetParameter(setValue, value, PhotonStartPosition).Value;
        }
        else if (label.Equals(nameof(AtomZPos)))
        {
            AtomZPos = ParameterHelper.SetParameter(setValue, value, AtomZPos).Value;
        }
        else if (label.Equals(nameof(ExcitingPhotonSpeed)))
        {
            ExcitingPhotonSpeed = ParameterHelper.SetParameter(setValue, value, ExcitingPhotonSpeed).Value;
        }
        else if (label.Equals(nameof(RydbergPhotonSpeed)))
        {
            RydbergPhotonSpeed = ParameterHelper.SetParameter(setValue, value, RydbergPhotonSpeed).Value;
        }
        else if (label.Equals(nameof(ReadoutPhotonSpeed)))
        {
            ReadoutPhotonSpeed = ParameterHelper.SetParameter(setValue, value, ReadoutPhotonSpeed).Value;
        }
        else if (label.Equals(nameof(ExcitingPhoton)))
        {
            ParameterHelper.SetParameter(setValue, value, _excitingPhotonNames);
        }
        else if (label.Equals(nameof(RydbergPhoton)))
        {
            ParameterHelper.SetParameter(setValue, value, _rydbergPhotonNames);
        }
        else if (label.Equals(nameof(ReadoutPhoton)))
        {
            ParameterHelper.SetParameter(setValue, value, _readoutPhotonNames);
        }
        else if (label.Equals(nameof(ExcitedAtom)))
        {
            ParameterHelper.SetParameter(setValue, value, _excitedAtomNames);
        }
        else if (label.Equals(nameof(RydbergAtom)))
        {
            ParameterHelper.SetParameter(setValue, value, _rydbergAtomNames);
        }
        else if (label.Equals(nameof(ReadoutResult)))
        {
            ParameterHelper.SetParameter(setValue, value, _readoutResultNames);
        }
        else
        {
            base.SetParameter(setValue, label, value);
        }
    }

    private List<ArObject> CreateAtoms()
    {
        ArObjects = new List<ArObject>();

        if (Pois.Count > 0)
        {
            while (_atomArObjects.Count < MaxNofObjects)
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
                    out var gateObject,
                    out var gateArObject
                    );

                if (gateObject != null)
                {
                    _atoms.Add(gateObject);
                    if (!gateObject.activeSelf)
                    {
                        gateObject.SetActive(true);
                    }
                }
                if (gateArObject != null)
                {
                    ArObjectsToFade.Add(gateArObject);
                    _atomArObjects.Add(gateArObject);
                }
            }
            CreateAtomPositions(_atoms);
            Fade(); // Set the initial fade value
        }
        return ArObjects;
    }

    private void CreateAtomPositions(List<GameObject> atoms)
    {
        int i = 0;
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++, i++)
            {
                atoms[i % atoms.Count].transform.localPosition = new Vector3(x, y, AtomZPos);
            }
        }
    }

    private void CreateExcitingPhotons()
    {
        if (_excitingPhotonNames.Count > 0)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null)
            {
                return;
            }

            for (int i = 0; i < _atoms.Count; i++)
            {
                var atomTransform = _atoms[i].transform;
                var photonName = _excitingPhotonNames[Random.Next(_excitingPhotonNames.Count)];
                var photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photonName);
                if (photonObject is not null && atomTransform != null)
                {
                    var result = ArBehaviour.CreateArObject(
                        arObjectState,
                        photonObject.gameObject,
                        null,
                        atomTransform,
                        photonObject.poi,
                        ArBehaviourArObject.ArObjectId,
                        out var photon,
                        out var photonArObject
                        );
                    if (photon != null)
                    {
                        _excitingPhotons.Add(photon);
                        if (photon.activeSelf)
                        {
                            photon.SetActive(false);
                        }
                    }
                    if (photonArObject != null)
                    {
                        ArObjectsToFade.Add(photonArObject);
                        _excitingPhotonArObjects.Add(photonArObject);
                    }
                }
            }
            SetActive(false, _excitingPhotonArObjects);
        }
    }

    private void CreateRydbergPhotons()
    {
        if (_rydbergPhotonNames.Count > 0)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null)
            {
                return;
            }

            for (int i = 0; i < _atoms.Count; i++)
            {
                var atomTransform = _atoms[i].transform;
                var photonName = _rydbergPhotonNames[Random.Next(_rydbergPhotonNames.Count)];
                var photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photonName);
                if (photonObject is not null && atomTransform != null)
                {
                    var result = ArBehaviour.CreateArObject(
                        arObjectState,
                        photonObject.gameObject,
                        null,
                        atomTransform,
                        photonObject.poi,
                        ArBehaviourArObject.ArObjectId,
                        out var photon,
                        out var photonArObject
                        );
                    if (photon != null)
                    {
                        _rydbergPhotons.Add(photon);
                        if (photon.activeSelf)
                        {
                            photon.SetActive(false);
                        }
                    }
                    if (photonArObject != null)
                    {
                        ArObjectsToFade.Add(photonArObject);
                        _rydbergPhotonArObjects.Add(photonArObject);
                    }
                }
            }
            SetActive(false, _rydbergPhotonArObjects);
        }
    }

    private void CreateReadoutPhotons()
    {
        if (_readoutPhotonNames.Count > 0)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null)
            {
                return;
            }

            for (int i = 0; i < _atoms.Count; i++)
            {
                var atomTransform = _atoms[i].transform;
                var photonName = _readoutPhotonNames[Random.Next(_readoutPhotonNames.Count)];
                var photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photonName);
                if (photonObject is not null && atomTransform != null)
                {
                    var result = ArBehaviour.CreateArObject(
                        arObjectState,
                        photonObject.gameObject,
                        null,
                        atomTransform,
                        photonObject.poi,
                        ArBehaviourArObject.ArObjectId,
                        out var photon,
                        out var photonArObject
                        );
                    if (photon != null)
                    {
                        _readoutPhotons.Add(photon);
                        if (photon.activeSelf)
                        {
                            photon.SetActive(false);
                        }
                    }
                    if (photonArObject != null)
                    {
                        ArObjectsToFade.Add(photonArObject);
                        _readoutPhotonArObjects.Add(photonArObject);
                    }
                }
            }
            SetActive(false, _readoutPhotonArObjects);
        }
    }

    private void CreateExcitedAtoms()
    {
        if (_excitedAtomNames.Count > 0)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null)
            {
                return;
            }

            for (int i = 0; i < _atoms.Count; i++)
            {
                var atomName = _excitedAtomNames[Random.Next(_excitedAtomNames.Count)];
                var excitedAtomObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == atomName);
                if (excitedAtomObject is not null)
                {
                    var result = ArBehaviour.CreateArObject(
                        arObjectState,
                        excitedAtomObject.gameObject,
                        null,
                        transform,
                        excitedAtomObject.poi,
                        ArBehaviourArObject.ArObjectId,
                        out var excitedAtom,
                        out var excitedAtomArObject
                        );

                    if (excitedAtom != null)
                    {
                        _excitedAtoms.Add(excitedAtom);
                        if (!excitedAtom.activeSelf)
                        {
                            excitedAtom.SetActive(false);
                        }
                    }
                    if (excitedAtomArObject != null)
                    {
                        ArObjectsToFade.Add(excitedAtomArObject);
                        _excitedAtomArObjects.Add(excitedAtomArObject);
                    }
                }
            }
            CreateAtomPositions(_excitedAtoms);
            SetActive(false, _excitedAtomArObjects);
        }
    }

    private void CreateRydbergAtoms()
    {
        if (_rydbergAtomNames.Count > 3)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null)
            {
                return;
            }

            for (int i = 0; i < _atoms.Count; i++)
            {
                var atomName = _rydbergAtomNames[Random.Next(_rydbergAtomNames.Count)];
                switch (i)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 5:
                        atomName = _rydbergAtomNames[0];
                        break;
                    case 3:
                    case 4:
                        atomName = _rydbergAtomNames[1];
                        break;
                    case 6:
                    case 7:
                        atomName = _rydbergAtomNames[2];
                        break;
                    case 8:
                        atomName = _rydbergAtomNames[3];
                        break;
                }
                var rydbergAtomObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == atomName);
                if (rydbergAtomObject is not null)
                {
                    var result = ArBehaviour.CreateArObject(
                        arObjectState,
                        rydbergAtomObject.gameObject,
                        null,
                        transform,
                        rydbergAtomObject.poi,
                        ArBehaviourArObject.ArObjectId,
                        out var rydbergAtom,
                        out var rydbergAtomArObject
                        );

                    if (rydbergAtom != null)
                    {
                        _rydbergAtoms.Add(rydbergAtom);
                        if (!rydbergAtom.activeSelf)
                        {
                            rydbergAtom.SetActive(false);
                        }
                    }
                    if (rydbergAtomArObject != null)
                    {
                        ArObjectsToFade.Add(rydbergAtomArObject);
                        _rydbergAtomArObjects.Add(rydbergAtomArObject);
                    }
                }
            }
            CreateAtomPositions(_rydbergAtoms);
            SetActive(false, _rydbergAtomArObjects);
        }
    }

    private void CreateReadoutResults()
    {
        if (_readoutResultNames.Count > 1)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null)
            {
                return;
            }

            for (int i = 0; i < _atoms.Count; i++)
            {
                var atomName = _readoutResultNames[0];
                var readoutResultObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == atomName);
                if (readoutResultObject is not null)
                {
                    var result = ArBehaviour.CreateArObject(
                        arObjectState,
                        readoutResultObject.gameObject,
                        null,
                        transform,
                        readoutResultObject.poi,
                        ArBehaviourArObject.ArObjectId,
                        out var readoutResult,
                        out var readoutResultArObject
                        );

                    if (readoutResult != null)
                    {
                        _readoutResults0.Add(readoutResult);
                        if (!readoutResult.activeSelf)
                        {
                            readoutResult.SetActive(false);
                        }
                    }
                    if (readoutResultArObject != null)
                    {
                        ArObjectsToFade.Add(readoutResultArObject);
                        _readoutResult0ArObjects.Add(readoutResultArObject);
                    }
                }
            }
            CreateAtomPositions(_readoutResults0);
            SetActive(false, _readoutResult0ArObjects);

            for (int i = 0; i < _atoms.Count; i++)
            {
                var atomName = _readoutResultNames[1];
                var readoutResultObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == atomName);
                if (readoutResultObject is not null)
                {
                    var result = ArBehaviour.CreateArObject(
                        arObjectState,
                        readoutResultObject.gameObject,
                        null,
                        transform,
                        readoutResultObject.poi,
                        ArBehaviourArObject.ArObjectId,
                        out var readoutResult,
                        out var readoutResultArObject
                        );

                    if (readoutResult != null)
                    {
                        _readoutResults1.Add(readoutResult);
                        if (!readoutResult.activeSelf)
                        {
                            readoutResult.SetActive(false);
                        }
                    }
                    if (readoutResultArObject != null)
                    {
                        ArObjectsToFade.Add(readoutResultArObject);
                        _readoutResult1ArObjects.Add(readoutResultArObject);
                    }
                }
            }
            CreateAtomPositions(_readoutResults1);
            SetActive(false, _readoutResult1ArObjects);
        }
    }

    private enum AtomGateActions
    {
        StartExcitingPhoton,
        StartRydbergPhoton,
        StartReadoutPhotons
    }

    private readonly List<(int second, AtomGateActions action, object parameter)> _actionList = new()
    {
        //(1, AtomGateActions.StartReadoutPhotons, null),
        //(1, AtomGateActions.StartRydbergPhoton, 8),
        //(1000, AtomGateActions.StartRydbergPhoton, 0),
        //(6000, AtomGateActions.StartRydbergPhoton, 8),
        //(9000, AtomGateActions.StartRydbergPhoton, 3),
        //(13000, AtomGateActions.StartRydbergPhoton, 6),
        //(19000, AtomGateActions.StartRydbergPhoton, 2),
        (14, AtomGateActions.StartRydbergPhoton, null),
        (14, AtomGateActions.StartRydbergPhoton, null),
        (14, AtomGateActions.StartRydbergPhoton, null),
        (14, AtomGateActions.StartRydbergPhoton, null),

        (24, AtomGateActions.StartExcitingPhoton, 0),
        (24, AtomGateActions.StartExcitingPhoton, 1),
        (24, AtomGateActions.StartExcitingPhoton, 2),
        (24, AtomGateActions.StartExcitingPhoton, 3),
        (24, AtomGateActions.StartExcitingPhoton, 4),
        (24, AtomGateActions.StartExcitingPhoton, 5),
        (24, AtomGateActions.StartExcitingPhoton, 6),
        (24, AtomGateActions.StartExcitingPhoton, 7),
        (24, AtomGateActions.StartExcitingPhoton, 8),

        (28, AtomGateActions.StartReadoutPhotons, null)
    };

    private List<(int millisecond, AtomGateActions action, object parameter)> _todoList = null;
    private DateTime? _startTime = null;
    private bool _showAtoms = false;
    protected override void Update()
    {
        base.Update();

        if (!gameObject.activeSelf)
        {
            _showAtoms = true;
            ResetAnimations();
            if (_todoList != null)
            {
                _todoList = null;
            }
            if (_lastExcitingTicks != null)
            {
                _lastExcitingTicks = null;
            }
            if (_lastExcitingDistanceToAtom != null)
            {
                _lastExcitingDistanceToAtom = null;
            }
            if (_lastRydbergTicks != null)
            {
                _lastRydbergTicks = null;
            }
            if (_lastRydbergDistanceToAtom != null)
            {
                _lastRydbergDistanceToAtom = null;
            }
            if (_lastReadoutTicks != null)
            {
                _lastReadoutTicks = null;
            }
            if (_lastReadoutDistanceToAtom != null)
            {
                _lastReadoutDistanceToAtom = null;
            }
            SetActive(false, _excitingPhotonArObjects);
            SetActive(false, _rydbergPhotonArObjects);
            SetActive(false, _readoutPhotonArObjects);
            SetActive(false, _excitedAtomArObjects);
            SetActive(false, _rydbergAtomArObjects);
            SetActive(false, _readoutResult0ArObjects);
            SetActive(false, _readoutResult1ArObjects);
            if (_readoutResultArObjects.Count > 0)
            {
                SetActive(false, _readoutResultArObjects);
                _readoutResultArObjects.Clear();
            }
            SetActive(false, _atomArObjects);
            foreach (var photon in _excitingPhotons)
            {
                photon.SetActive(false);
            }
            foreach (var photon in _rydbergPhotons)
            {
                photon.SetActive(false);
            }
            foreach (var photon in _readoutPhotons)
            {
                photon.SetActive(false);
            }
            foreach (var atom in _atoms)
            {
                atom.SetActive(false);
            }
            foreach (var atom in _excitedAtoms)
            {
                atom.SetActive(false);
            }
            foreach (var atom in _rydbergAtoms)
            {
                atom.SetActive(false);
            }
            foreach (var result in _readoutResults0)
            {
                result.SetActive(false);
            }
            foreach (var result in _readoutResults1)
            {
                result.SetActive(false);
            }
            if (_readoutResults.Count > 0)
            {
                foreach (var result in _readoutResults)
                {
                    result.SetActive(false);
                }
                _readoutResults.Clear();
            }
            return;
        }

        if (_atomArObjects.Count == 0)
        {
            SeedRandom(GetInstanceID());
            UnityEngine.Random.InitState(Random.Next(int.MaxValue));
            ArObjects = CreateAtoms();
            Fade(); // Set the initial fade value
        }
        else if (_showAtoms && !_atoms.First().activeSelf)
        {
            SetActive(true, _atomArObjects);
            foreach (var atom in _atoms)
            {
                atom.SetActive(true);
            }
        }

        if (_lastExcitingTicks is null)
        {
            _lastExcitingTicks = new List<long?>(new long?[_atoms.Count]);
        }
        if (_lastExcitingDistanceToAtom is null)
        {
            _lastExcitingDistanceToAtom = new List<float?>(new float?[_atoms.Count]);
        }
        if (_lastRydbergTicks is null)
        {
            _lastRydbergTicks = new List<long?>(new long?[_atoms.Count]);
        }
        if (_lastRydbergDistanceToAtom is null)
        {
            _lastRydbergDistanceToAtom = new List<float?>(new float?[_atoms.Count]);
        }
        if (_lastReadoutTicks is null)
        {
            _lastReadoutTicks = new List<long?>(new long?[_atoms.Count]);
        }
        if (_lastReadoutDistanceToAtom is null)
        {
            _lastReadoutDistanceToAtom = new List<float?>(new float?[_atoms.Count]);
        }

        if (_todoList is null)
        {
            _todoList = new();
            foreach (var element in _actionList)
            {
                switch (element.action)
                {
                    case AtomGateActions.StartExcitingPhoton:
                        _todoList.Add((1000 + Random.Next(element.second * 1000), element.action, element.parameter));
                        break;
                    case AtomGateActions.StartRydbergPhoton:
                        int? nextParameter = Random.Next(9);
                        while (_todoList.Any(x => x.parameter as int? == nextParameter && x.action == element.action))
                        {
                            nextParameter = Random.Next(9);
                        }
                        _todoList.Add((6000 + Random.Next(element.second * 1000), element.action, nextParameter));

                        break;
                    case AtomGateActions.StartReadoutPhotons:
                        _todoList.Add((1000 + element.second * 1000, element.action, element.parameter));

                        break;
                }
            }
            _todoList = _todoList.OrderBy(x => x.millisecond).ToList();
            _startTime = DateTime.Now;
        }
        if (_todoList.Count == 0 || (DateTime.Now - _startTime.Value).TotalMilliseconds < _todoList[0].millisecond)
        {
            HandlePhotons();
            return;
        }
        var action = _todoList[0].action;
        var parameter = _todoList[0].parameter;
        _todoList.RemoveAt(0);

        switch (action)
        {
            case AtomGateActions.StartExcitingPhoton:
                _showAtoms = true;
                StartExcitingPhoton(parameter as int?);
                break;

            case AtomGateActions.StartRydbergPhoton:
                _showAtoms = true;
                StartRydbergPhoton(parameter as int?);
                break;

            case AtomGateActions.StartReadoutPhotons:
                StartReadoutPhotons();
                break;
        }
        HandlePhotons();
    }

    private void StartExcitingPhoton(int? index)
    {
        if (_excitedAtoms.Count == 0)
        {
            CreateExcitedAtoms();
        }
        if (_excitingPhotons.Count == 0)
        {
            CreateExcitingPhotons();
        }
        if (index.HasValue && index.Value >= 0 && index.Value < _excitingPhotons.Count)
        {
            var photon = _excitingPhotons[index.Value];
            photon.transform.localPosition = PhotonStartPosition;
            photon.SetActive(true);
            SetActive(true, _excitingPhotonArObjects.GetRange(index.Value, 1));
        }
    }

    private void StartRydbergPhoton(int? index)
    {
        if (_rydbergAtoms.Count == 0)
        {
            CreateRydbergAtoms();
        }
        if (_rydbergPhotons.Count == 0)
        {
            CreateRydbergPhotons();
        }
        if (index.HasValue && index.Value >= 0 && index.Value < _rydbergPhotons.Count)
        {
            var photon = _rydbergPhotons[index.Value];
            photon.transform.localPosition = PhotonStartPosition;
            photon.SetActive(true);
            SetActive(true, _rydbergPhotonArObjects.GetRange(index.Value, 1));
        }
    }

    private void StartReadoutPhotons()
    {
        for (int index = 0; index < _atoms.Count; index++)
        {
            if (_readoutResults0.Count == 0)
            {
                CreateReadoutResults();
            }
            if (_readoutPhotons.Count == 0)
            {
                CreateReadoutPhotons();
            }
            if (index >= 0 && index < _readoutPhotons.Count)
            {
                var photon = _readoutPhotons[index];
                photon.transform.localPosition = PhotonStartPosition;
                photon.SetActive(true);
                SetActive(true, _readoutPhotonArObjects.GetRange(index, 1));
            }
        }
    }

    private List<long?> _lastExcitingTicks = null;
    private List<float?> _lastExcitingDistanceToAtom = null;
    private List<long?> _lastRydbergTicks = null;
    private List<float?> _lastRydbergDistanceToAtom = null;
    private List<long?> _lastReadoutTicks = null;
    private List<float?> _lastReadoutDistanceToAtom = null;

    private void HandlePhotons()
    {
        for (int i = 0; i < _excitingPhotons.Count; i++)
        {
            if (!_excitingPhotons[i].activeSelf)
            {
                continue;
            }
            var atomTransform = _atoms[i].transform;
            var photonTransform = _excitingPhotons[i].transform;
            if (atomTransform != null && photonTransform != null)
            {
                if (_lastExcitingTicks[i] is null)
                {
                    _lastExcitingTicks[i] = DateTime.Now.Ticks;
                }
                else
                {
                    photonTransform.LookAt(atomTransform);
                    var deltaTime = (DateTime.Now.Ticks - _lastExcitingTicks[i].Value) / (float)TimeSpan.TicksPerSecond;
                    _lastExcitingTicks[i] = DateTime.Now.Ticks;

                    photonTransform.localPosition += ExcitingPhotonSpeed * deltaTime * photonTransform.forward;
                }
            }
            var distanceToAtom = Vector3.Distance(atomTransform.position, photonTransform.position);
            if (distanceToAtom < 0.001 || (_lastExcitingDistanceToAtom[i].HasValue && _lastExcitingDistanceToAtom[i].Value < distanceToAtom))
            {
                _excitingPhotons[i].SetActive(false);
                SetActive(false, _excitingPhotonArObjects.GetRange(i, 1));
                _excitedAtoms[i].SetActive(true);
                SetActive(true, _excitedAtomArObjects.GetRange(i, 1));
            }
            else
            {
                _lastExcitingDistanceToAtom[i] = distanceToAtom;
            }
        }
        for (int i = 0; i < _rydbergPhotons.Count; i++)
        {
            if (!_rydbergPhotons[i].activeSelf)
            {
                continue;
            }
            var atomTransform = _atoms[i].transform;
            var photonTransform = _rydbergPhotons[i].transform;
            if (atomTransform != null && photonTransform != null)
            {
                if (_lastRydbergTicks[i] is null)
                {
                    _lastRydbergTicks[i] = DateTime.Now.Ticks;
                }
                else
                {
                    photonTransform.LookAt(atomTransform);
                    var deltaTime = (DateTime.Now.Ticks - _lastRydbergTicks[i].Value) / (float)TimeSpan.TicksPerSecond;
                    _lastRydbergTicks[i] = DateTime.Now.Ticks;

                    photonTransform.localPosition += RydbergPhotonSpeed * deltaTime * photonTransform.forward;
                }
            }
            var distanceToAtom = Vector3.Distance(atomTransform.position, photonTransform.position);
            if (distanceToAtom < 0.001 || (_lastRydbergDistanceToAtom[i].HasValue && _lastRydbergDistanceToAtom[i].Value < distanceToAtom))
            {
                _rydbergPhotons[i].SetActive(false);
                SetActive(false, _rydbergPhotonArObjects.GetRange(i, 1));
                _rydbergAtoms[i].SetActive(true);
                SetActive(true, _rydbergAtomArObjects.GetRange(i, 1));
            }
            else
            {
                _lastRydbergDistanceToAtom[i] = distanceToAtom;
            }
        }
        for (int i = 0; i < _readoutPhotons.Count; i++)
        {
            if (!_readoutPhotons[i].activeSelf)
            {
                continue;
            }
            var atomTransform = _atoms[i].transform;
            var photonTransform = _readoutPhotons[i].transform;
            if (atomTransform != null && photonTransform != null)
            {
                if (_lastReadoutTicks[i] is null)
                {
                    _lastReadoutTicks[i] = DateTime.Now.Ticks;
                }
                else
                {
                    photonTransform.LookAt(atomTransform);
                    var deltaTime = (DateTime.Now.Ticks - _lastReadoutTicks[i].Value) / (float)TimeSpan.TicksPerSecond;
                    _lastReadoutTicks[i] = DateTime.Now.Ticks;

                    photonTransform.localPosition += ReadoutPhotonSpeed * deltaTime * photonTransform.forward;
                }
            }
            var distanceToAtom = Vector3.Distance(atomTransform.position, photonTransform.position);
            if (distanceToAtom < 0.001 || (_lastReadoutDistanceToAtom[i].HasValue && _lastReadoutDistanceToAtom[i].Value < distanceToAtom))
            {
                _showAtoms = false;
                SetActive(false, _atomArObjects);
                SetActive(false, _excitingPhotonArObjects);
                SetActive(false, _rydbergPhotonArObjects);
                SetActive(false, _readoutPhotonArObjects);
                SetActive(false, _excitedAtomArObjects);
                SetActive(false, _rydbergAtomArObjects);
                if (_readoutResultArObjects.Count == 0)
                {
                    for (int index = 0; index < _atoms.Count; index++)
                    {
                        var randomValue = Random.Next(2);
                        if (randomValue == 0)
                        {
                            _readoutResultArObjects.Add(_readoutResult0ArObjects[index]);
                            _readoutResults.Add(_readoutResults0[index]);
                        }
                        else
                        {
                            _readoutResultArObjects.Add(_readoutResult1ArObjects[index]);
                            _readoutResults.Add(_readoutResults1[index]);
                        }
                    }
                }
                SetActive(true, _readoutResultArObjects);
                foreach (var photon in _excitingPhotons)
                {
                    photon.SetActive(false);
                }
                foreach (var photon in _rydbergPhotons)
                {
                    photon.SetActive(false);
                }
                foreach (var photon in _readoutPhotons)
                {
                    photon.SetActive(false);
                }
                foreach (var atom in _atoms)
                {
                    atom.SetActive(false);
                }
                foreach (var atom in _excitedAtoms)
                {
                    atom.SetActive(false);
                }
                foreach (var atom in _rydbergAtoms)
                {
                    atom.SetActive(false);
                }
                foreach (var result in _readoutResults)
                {
                    result.SetActive(true);
                }
                break;
            }
            else
            {
                _lastReadoutDistanceToAtom[i] = distanceToAtom;
            }
        }
    }
}
