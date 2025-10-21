/*
ArpoisePoiAtomPhoton.cs - A script handling an 'atom - photon' for ARpoise.

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

public class ArpoisePoiAtomPhoton : ArpoisePoiStructure
{
    #region AtomPhoton parameters
    public Vector3 PhotonOuterRange = new Vector3(12, 12, 12);
    public Vector3 PhotonInnerRange = new Vector3(2, 2, 2);
    public Vector3 PhotonStartPosition = Vector3.zero;

    public float Speed = 1.0f; // meters per second
    public int WaitBeforePhoton = 5000; // milliseconds
    public int ExcitedDuration = 10000; // milliseconds
    public int WaitAfterExcited = 5000; // milliseconds

    public string Photon = string.Empty;
    public string ExcitedAtom = string.Empty;
    #endregion

    private GameObject _atom;
    private readonly List<ArObject> _atomArObjects = new();

    private readonly List<string> _photonNames = new();
    private GameObject _photon;
    private readonly List<ArObject> _photonArObjects = new();

    private readonly List<string> _excitedAtomNames = new();
    private GameObject _excitedAtom;
    private readonly List<ArObject> _excitedAtomArObjects = new();

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
        else if (label.Equals(nameof(WaitBeforePhoton)))
        {
            WaitBeforePhoton = ParameterHelper.SetParameter(setValue, value, WaitBeforePhoton).Value;
        }
        else if (label.Equals(nameof(ExcitedDuration)))
        {
            ExcitedDuration = ParameterHelper.SetParameter(setValue, value, ExcitedDuration).Value;
        }
        else if (label.Equals(nameof(WaitAfterExcited)))
        {
            WaitAfterExcited = ParameterHelper.SetParameter(setValue, value, WaitAfterExcited).Value;
        }
        else if (label.Equals(nameof(Photon)))
        {
            ParameterHelper.SetParameter(setValue, value, _photonNames);
        }
        else if (label.Equals(nameof(ExcitedAtom)))
        {
            ParameterHelper.SetParameter(setValue, value, _excitedAtomNames);
        }
        else
        {
            base.SetParameter(setValue, label, value);
        }
    }

    private List<ArObject> CreateAtom()
    {
        ArObjects = new List<ArObject>();

        if (Pois.Count > 0)
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
            var newAtomTransform = _atom?.transform;
            if (newAtomTransform != null)
            {
                newAtomTransform.localPosition = Vector3.zero;
            }
            if (atomArObject != null)
            {
                Add(atomArObject);
                _atomArObjects.Clear();
                _atomArObjects.Add(atomArObject);
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
                //CreatePhotonPosition();
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

    private void CreateExcitedAtom()
    {
        if (_excitedAtomNames.Count > 0)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null)
            {
                return;
            }

            var atomTransform = _atom?.transform;
            var atomName = _excitedAtomNames[Random.Next(_excitedAtomNames.Count)];
            var excitedAtomObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == atomName);
            if (excitedAtomObject is not null && atomTransform != null)
            {
                var result = ArBehaviour.CreateArObject(
                    arObjectState,
                    excitedAtomObject.gameObject,
                    null,
                    atomTransform,
                    excitedAtomObject.poi,
                    ArBehaviourArObject.ArObjectId,
                    out _excitedAtom,
                    out var excitedAtomArObject
                    );

                if (_excitedAtom != null)
                {
                    if (!_excitedAtom.activeSelf)
                    {
                        _excitedAtom.SetActive(true);
                    }
                }
                if (excitedAtomArObject != null)
                {
                    ArObjectsToFade.Add(excitedAtomArObject);
                    _excitedAtomArObjects.Clear();
                    _excitedAtomArObjects.Add(excitedAtomArObject);
                }
            }
        }
    }

    private long? _lastTicks = null;
    private float? _lastDistanceToAtom = null;

    private enum AtomPhotonState
    {
        WaitBeforePhoton,
        ShowPhoton,
        ShowExcitedAtom,
        WaitAfterExcitedAtom
    }

    private DateTime? _nextStateChange = null;
    private AtomPhotonState _state = AtomPhotonState.WaitBeforePhoton;
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

    protected override void Update()
    {
        base.Update();

        if (!gameObject.activeSelf)
        {
            SetActive(false, _atomArObjects);
            SetActive(false, _photonArObjects);
            SetActive(false, _excitedAtomArObjects);

            _lastTicks = null;
            _lastDistanceToAtom = null;
            State = AtomPhotonState.WaitBeforePhoton;
            return;
        }

        if (_atomArObjects is null || _atomArObjects.Count == 0)
        {
            SeedRandom(GetInstanceID());
            UnityEngine.Random.InitState(Random.Next(int.MaxValue));
            ArObjects = CreateAtom();
        }

        switch (State)
        {
            case AtomPhotonState.WaitBeforePhoton:
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
                    State = AtomPhotonState.ShowPhoton;
                    _lastTicks = null;
                    _lastDistanceToAtom = null;
                    CreatePhotonPosition();
                }
                break;

            case AtomPhotonState.ShowPhoton:
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
                    State = AtomPhotonState.ShowExcitedAtom;
                }
                else
                {
                    _lastDistanceToAtom = distanceToAtom;
                }
                break;

            case AtomPhotonState.ShowExcitedAtom:
                if (_excitedAtomArObjects is null || _excitedAtomArObjects.Count == 0)
                {
                    CreateExcitedAtom();
                }
                SetActive(gameObject.activeSelf, _excitedAtomArObjects);
                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(ExcitedDuration);
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    SetActive(false, _excitedAtomArObjects);
                    State = AtomPhotonState.WaitAfterExcitedAtom;
                }
                break;

            case AtomPhotonState.WaitAfterExcitedAtom:
                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(WaitAfterExcited);
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    State = AtomPhotonState.WaitBeforePhoton;
                }
                break;
        }
    }
}
