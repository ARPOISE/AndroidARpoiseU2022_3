/*
ArpoisePoiBeam.cs - A script handling a 'poi beam' for ARpoise.

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

public class ArpoisePoiBeam : ArpoisePoiStructure
{
    #region Grid parameters
    public float PhotonStartPos = -8;
    public float PhotonMaxPos = 8;
    public float Speed = 1.0f; // meters per second
    public int AreaSize = 3;
    #endregion

    private List<GameObject> _photons;
    private List<ArObject> _photonArObjects;

    public override void SetParameter(bool setValue, string label, string value)
    {
        if (label.Equals(nameof(AreaSize)))
        {
            AreaSize = ParameterHelper.SetParameter(setValue, value, AreaSize).Value;
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
        else
        {
            base.SetParameter(setValue, label, value);
        }
    }

    public void DestroyPhotons()
    {
        if (_photonArObjects != null)
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is not null)
            {
                foreach (var photonArObject in _photonArObjects)
                {
                    arObjectState.DestroyArObject(photonArObject);
                }
            }
        }
        _photonArObjects = null;
        _photons = null;
    }

    private List<ArObject> CreatePhotons()
    {
        ArObjects = new List<ArObject>();
        _photonArObjects = new List<ArObject>();
        _photons = new List<GameObject>();

        if (Pois.Count > 0)
        {
            var photon = Pois[Random.Next(Pois.Count)];
            var photonObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == photon);
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null || photonObject is null)
            {
                return ArObjects;
            }

            for (int x = (int)(0 - AreaSize / 2f); x <= (int)(AreaSize / 2f); x++)
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
                        photonObject.poi.id,
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
                        _photons.Add(_photonObject);
                    }
                }
            }
        }
        Fade(); // Set the initial fade value

        return ArObjects;
    }

    private long? _lastTicks = null;
    protected override void Update()
    {
        base.Update();

        if (!gameObject.activeSelf)
        {
            DestroyPhotons();
            _lastTicks = null;
            return;
        }

        if (ArObjects is null)
        {
            SeedRandom(GetInstanceID());
            _photons = new List<GameObject>();
            _photonArObjects = new List<ArObject>();
            ArObjects = CreatePhotons();
        }

        if (_lastTicks is null)
        {
            _lastTicks = DateTime.Now.Ticks;
        }
        var now = DateTime.Now.Ticks;
        var deltaTime = (now - _lastTicks.Value) / (float)TimeSpan.TicksPerSecond;
        _lastTicks = now;
        var distance = Speed * deltaTime;

        if (_photons is not null)
        {
            foreach (var photon in _photons)
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
    }
}
