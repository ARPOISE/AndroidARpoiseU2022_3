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
    public Vector3 PhotonOuterRange = new Vector3(12, 12, 12);
    public Vector3 PhotonInnerRange = new Vector3(2, 2, 2);
    public Vector3 PhotonStartPosition = Vector3.zero;

    public float Speed = 1.0f; // meters per second
    public int WaitBeforePhoton = 5000; // milliseconds
    public int RydbergDuration = 10000; // milliseconds
    public int WaitAfterRydberg = 10000; // milliseconds

    public string Photon = string.Empty;
    public string RydbergAtom = string.Empty;
    public float Distance = 50f;
    #endregion

    private GameObject _phasedAtom;
    private GameObject _animatedAtom;
    private GameObject _atom;
    private readonly List<ArObject> _atomArObjects = new();

    private readonly List<string> _photonNames = new();
    private GameObject _photon;
    private readonly List<ArObject> _photonArObjects = new();

    private readonly List<string> _rydbergAtomNames = new();
    private GameObject _rydbergAtom;
    private readonly List<ArObject> _rydbergAtomArObjects = new();

    public override void SetParameter(bool setValue, string label, string value)
    {
        if (label.Equals(nameof(PhotonStartPosition)))
        {
            PhotonStartPosition = ParameterHelper.SetParameter(setValue, value, PhotonStartPosition).Value;
        }
        else if (label.Equals(nameof(PhotonOuterRange)))
        {
            PhotonOuterRange = ParameterHelper.SetParameter(setValue, value, PhotonOuterRange).Value;
        }
        else if (label.Equals(nameof(PhotonInnerRange)))
        {
            PhotonInnerRange = ParameterHelper.SetParameter(setValue, value, PhotonInnerRange).Value;
        }
        else if (label.Equals(nameof(Speed)))
        {
            Speed = ParameterHelper.SetParameter(setValue, value, Speed).Value;
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
        else if (label.Equals(nameof(Photon)))
        {
            ParameterHelper.SetParameter(setValue, value, _photonNames);
        }
        else if (label.Equals(nameof(RydbergAtom)))
        {
            ParameterHelper.SetParameter(setValue, value, _rydbergAtomNames);
        }
        else
        {
            base.SetParameter(setValue, label, value);
        }
    }

    private List<ArObject> CreateAtom()
    {
        ArObjects = new List<ArObject>();

        if (Pois.Count == 4)
        {
            for (int i = 0; i < 2; i++)
            {
                var poi = Pois[0];
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
                    if (i == 0)
                    {
                        _atomArObjects.Clear();
                    }
                    _atomArObjects.Add(atomArObject);
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
                    return ArObjects;
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
                    _atomArObjects.Add(atomArObject);
                }

                if (i == 1)
                {
                    poi = Pois[3];
                    poiObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == poi);
                    arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
                    if (arObjectState is null || poiObject is null)
                    {
                        return ArObjects;
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
                        _atomArObjects.Add(atomArObject);
                    }
                }
            }
        }
        return ArObjects;
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
            if (photonObject is not null && atomTransform != null)
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

    private void CreatePhotonPosition()
    {
        if (PhotonStartPosition != Vector3.zero)
        {
            var photonTransform = _photon?.transform;
            if (photonTransform != null)
            {
                photonTransform.localPosition = PhotonStartPosition;
            }
        }
        else
        {
            var photonTransform = _photon?.transform;
            if (photonTransform != null)
            {
                var x = UnityEngine.Random.Range(PhotonInnerRange.x, PhotonOuterRange.x);
                var y = UnityEngine.Random.Range(PhotonInnerRange.y, PhotonOuterRange.y);
                var z = UnityEngine.Random.Range(PhotonInnerRange.z, PhotonOuterRange.z);
                var signX = UnityEngine.Random.value > 0.5f ? 1 : -1;
                var signY = UnityEngine.Random.value > 0.5f ? 1 : -1;
                var signZ = UnityEngine.Random.value > 0.5f ? 1 : -1;
                var position = new Vector3(signX * x, signY * y, signZ * z);
                photonTransform.localPosition = position;
            }
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
            if (rydbergAtomObject is not null && atomTransform != null)
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

    private enum AtomEntangledState
    {
        WaitBeforePhoton,
        ShowPhoton,
        ShowRydbergAtom,
        WaitAfterRydbergAtom
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

            _lastTicks = null;
            _lastDistanceToAtom = null;
            State = AtomEntangledState.WaitBeforePhoton;
            return;
        }

        if (_atomArObjects is null || _atomArObjects.Count == 0)
        {
            SeedRandom(GetInstanceID());
            UnityEngine.Random.InitState(Random.Next(int.MaxValue));
            ArObjects = CreateAtom();
            Fade(); // Set the initial fade value
        }

        if (State == AtomEntangledState.WaitAfterRydbergAtom)
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
                if (_atomArObjects is null || _atomArObjects.Count == 0)
                {
                    ArObjects = CreateAtom();
                }
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
                if (atomTransform != null && photonTransform != null)
                {
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
                }
                var distanceToAtom = Vector3.Distance(atomTransform.position, photonTransform.position);
                if (distanceToAtom < 0.001 || (_lastDistanceToAtom.HasValue && _lastDistanceToAtom.Value < distanceToAtom))
                {
                    SetActive(false, _photonArObjects);
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
                    State = AtomEntangledState.WaitBeforePhoton;
                }
                break;
        }
    }
}
