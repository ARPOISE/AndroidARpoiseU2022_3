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
    public int RydbergDuration = 10000; // milliseconds
    public int WaitAfterRydberg = 5000; // milliseconds

    public string Photon = string.Empty;
    public string RydbergAtom = string.Empty;
    #endregion

    private GameObject _atom;
    private ArObject _atomArObject;

    private List<string> _photonNames = new();
    private GameObject _photon;
    private ArObject _photonArObject;

    private List<string> _rydbergAtomNames = new();
    private GameObject _rydbergAtom;
    private ArObject _rydbergAtomArObject;

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

    public void DestroyAtom()
    {
        if (_atomArObject != null)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is not null)
            {
                arObjectState.DestroyArObject(_atomArObject);
            }
        }
        _atomArObject = null;
        _atom = null;
    }

    private List<ArObject> CreateAtom()
    {
        var arObjects = new List<ArObject>();

        if (Pois.Count > 0)
        {
            var poi = Pois[Random.Next(Pois.Count)];
            var poiObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == poi);
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null || poiObject is null)
            {
                return arObjects;
            }

            var result = ArBehaviour.CreateArObject(
                arObjectState,
                poiObject.gameObject,
                ArObject,
                ArObject.GameObjects.First().transform,
                poiObject.poi,
                ArBehaviourArObject.ArObjectId,
                out _atom,
                out _atomArObject
                );

            if (_atom != null)
            {
                if (!_atom.activeSelf)
                {
                    _atom.SetActive(true);
                }
            }
            var newAtomTransform = _atomArObject?.GameObjects?.FirstOrDefault()?.transform;
            if (newAtomTransform != null)
            {
                newAtomTransform.localPosition = Vector3.zero;
            }
            if (_atomArObject != null)
            {
                Add(_atomArObject);
            }
            Fade();
        }
        return arObjects;
    }

    private void DestroyPhoton()
    {
        if (_photonArObject != null)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is not null)
            {
                arObjectState.DestroyArObject(_photonArObject);
            }
        }
        _photonArObject = null;
        _photon = null;
    }

    private void CreatePhoton()
    {
        if (Pois.Count > 0)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null)
            {
                return;
            }

            var atomTransform = _atomArObject?.GameObjects?.FirstOrDefault()?.transform;
            var photon = _photonNames[Random.Next(_photonNames.Count)];
            var photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photon);
            if (photonObject is not null && atomTransform != null)
            {
                var result = ArBehaviour.CreateArObject(
                    arObjectState,
                    photonObject.gameObject,
                    ArObject,
                    atomTransform,
                    photonObject.poi,
                    ArBehaviourArObject.ArObjectId,
                    out _photon,
                    out _photonArObject
                    );

                if (_photon != null)
                {
                    if (!_photon.activeSelf)
                    {
                        _photon.SetActive(true);
                    }
                }
                if (PhotonStartPosition != Vector3.zero)
                {
                    var photonTransform = _photonArObject?.GameObjects?.FirstOrDefault()?.transform;
                    if (photonTransform != null)
                    {
                        photonTransform.localPosition = PhotonStartPosition;
                    }
                }
                else
                {
                    var photonTransform = _photonArObject?.GameObjects?.FirstOrDefault()?.transform;
                    if (photonTransform != null)
                    {
                        var x = UnityEngine.Random.Range(PhotonInnerRange.x, PhotonOuterRange.x);
                        var y = UnityEngine.Random.Range(PhotonInnerRange.y, PhotonOuterRange.y);
                        var z = UnityEngine.Random.Range(PhotonInnerRange.z, PhotonOuterRange.z);
                        var signX = UnityEngine.Random.value > 0.5f ? 1 : -1;
                        var signY = UnityEngine.Random.value > 0.5f ? 1 : -1;
                        var signZ = UnityEngine.Random.value > 0.5f ? 1 : -1;
                        var position = new Vector3(
                            atomTransform.localPosition.x + signX * x,
                            atomTransform.localPosition.y + signY * y,
                            atomTransform.localPosition.z + signZ * z
                            );
                        photonTransform.localPosition = position;
                    }
                }
                if (_photonArObject != null)
                {
                    Add(_photonArObject);
                }
            }
            Fade();
        }
    }

    private void DestroyRydbergAtom()
    {
        if (_rydbergAtomArObject != null)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is not null)
            {
                arObjectState.DestroyArObject(_rydbergAtomArObject);
            }
        }
        _rydbergAtomArObject = null;
        _rydbergAtom = null;
    }

    private void CreateRydbergAtom()
    {
        if (Pois.Count > 0)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null)
            {
                return;
            }

            var atomTransform = _atomArObject?.GameObjects?.FirstOrDefault()?.transform;
            var rydbergAtom = _rydbergAtomNames[Random.Next(_rydbergAtomNames.Count)];
            var rydbergAtomObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == rydbergAtom);
            if (rydbergAtomObject is not null && atomTransform != null)
            {
                var result = ArBehaviour.CreateArObject(
                    arObjectState,
                    rydbergAtomObject.gameObject,
                    ArObject,
                    atomTransform,
                    rydbergAtomObject.poi,
                    ArBehaviourArObject.ArObjectId,
                    out _rydbergAtom,
                    out _rydbergAtomArObject
                    );

                if (_rydbergAtom != null)
                {
                    if (!_rydbergAtom.activeSelf)
                    {
                        _rydbergAtom.SetActive(true);
                    }
                }
                if (_rydbergAtomArObject != null)
                {
                    Add(_rydbergAtomArObject);
                }
            }
            Fade();
        }
    }

    private long? _lastTicks = null;
    private float? _lastDistanceToAtom = null;
    private Vector3 _forward;

    private enum AtomPhotonState
    {
        WaitBeforePhoton,
        ShowPhoton,
        ShowRydbergAtom,
        WaitAfterRydbergAtom
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
            DestroyRydbergAtom();
            DestroyPhoton();
            DestroyAtom();
            _lastTicks = null;
            _lastDistanceToAtom = null;
            State = AtomPhotonState.WaitBeforePhoton;
            return;
        }

        if (ArObjects is null)
        {
            SeedRandom(GetInstanceID());
            UnityEngine.Random.InitState(Random.Next(int.MaxValue));
            ArObjects = CreateAtom();
        }

        switch (State)
        {
            case AtomPhotonState.WaitBeforePhoton:
                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(WaitBeforePhoton * .5f + Random.Next(WaitBeforePhoton));
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    State = AtomPhotonState.ShowPhoton;
                }
                break;

            case AtomPhotonState.ShowPhoton:
                if (_photonArObject is null)
                {
                    CreatePhoton();
                    _lastTicks = null;
                    _lastDistanceToAtom = null;
                }

                var atomTransform = _atomArObject?.GameObjects?.FirstOrDefault()?.transform;
                var photonTransform = _photonArObject?.GameObjects?.FirstOrDefault()?.transform;
                if (atomTransform != null && photonTransform != null)
                {
                    if (_lastTicks is null)
                    {
                        photonTransform.LookAt(atomTransform);
                        _lastTicks = DateTime.Now.Ticks;
                        _forward = photonTransform.forward;
                    }
                    else
                    {
                        var deltaTime = (DateTime.Now.Ticks - _lastTicks.Value) / (float)TimeSpan.TicksPerSecond;
                        _lastTicks = DateTime.Now.Ticks;
                        var distance = Speed * deltaTime;

                        if (photonTransform != null)
                        {
                            photonTransform.localPosition += distance * _forward;
                        }
                    }
                }
                var distanceToAtom = Vector3.Distance(atomTransform.position, photonTransform.position);
                if (distanceToAtom < 0.001 || (_lastDistanceToAtom.HasValue && _lastDistanceToAtom.Value < distanceToAtom))
                {
                    DestroyPhoton();
                    State = AtomPhotonState.ShowRydbergAtom;
                }
                else
                {
                    _lastDistanceToAtom = distanceToAtom;
                }
                break;

            case AtomPhotonState.ShowRydbergAtom:
                if (_rydbergAtomArObject is null)
                {
                    CreateRydbergAtom();
                }
                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(RydbergDuration * .5f + Random.Next(RydbergDuration));
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    DestroyRydbergAtom();
                    State = AtomPhotonState.WaitAfterRydbergAtom;
                }
                break;

            case AtomPhotonState.WaitAfterRydbergAtom:
                if (_nextStateChange is null)
                {
                    _nextStateChange = DateTime.Now.AddMilliseconds(WaitAfterRydberg * .5f + Random.Next(WaitAfterRydberg));
                }
                else if (DateTime.Now >= _nextStateChange.Value)
                {
                    State = AtomPhotonState.WaitBeforePhoton;
                }
                break;
        }
    }
}
