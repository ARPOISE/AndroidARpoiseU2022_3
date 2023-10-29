/*
ArpoiseObjectRain.cs - A script handling an 'object rain' like in 'What You Sow' for ARpoise.

Copyright (C) 2023, Tamiko Thiel and Peter Graf - All Rights Reserved

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
using System;
using System.Collections.Generic;
using UnityEngine;

public class ArpoiseObjectRain : MonoBehaviour
{
    // The prefabs to be rained,
    // they have to be set before you create the asset bundle of the rain
    // and have to be in the same asset bundle as the rain
    //
    public GameObject RainObject0;
    public GameObject RainObject1;
    public GameObject RainObject2;
    public GameObject RainObject3;
    public GameObject RainObject4;
    public GameObject RainObject5;
    public GameObject RainObject6;
    public GameObject RainObject7;
    public GameObject RainObject8;
    public GameObject RainObject9;

    public GameObject ClickObject0;
    public GameObject ClickObject1;
    public GameObject ClickObject2;
    public GameObject ClickObject3;
    public GameObject ClickObject4;
    public GameObject ClickObject5;
    public GameObject ClickObject6;
    public GameObject ClickObject7;
    public GameObject ClickObject8;
    public GameObject ClickObject9;

    public GameObject TapSound0;
    public GameObject TapSound1;
    public GameObject TapSound2;
    public GameObject TapSound3;
    public GameObject TapSound4;
    public GameObject TapSound5;
    public GameObject TapSound6;
    public GameObject TapSound7;
    public GameObject TapSound8;
    public GameObject TapSound9;

    #region Rain parameters
    public int OffsetX = 0; // Move the center of the rain in X direction
    public int OffsetZ = 0; // Move the center of the rain in Z direction
    public int DropHeight = 5; // Height to drop from, some random component is added below
    public int AreaSize = 10; // The dimensions of the area the rain happens in
    public int StartSecond = 0; // The number of seconds after the load of the layer to start
    public int EndSecond = 600; // The number of seconds to run after the start
    public float NumberOfNewRainObjectsPerSecond = 2.5f; // How many objects are to be created every second
    public int MaxNofRainObjects = -1; // How many rain objects are to be created before we start deleting some
    public int MaxNofClickObjects = -1; // How many click objects are to be created before we start deleting some
    public int MaxNofTapSounds = -1; // How many tap sounds are to be created before we start deleting some
    #endregion

    public void SetParameter(bool setValue, string label, string value)
    {
        if (label.Equals(nameof(OffsetX)))
        {
            OffsetX = SetParameter(setValue, value, OffsetX).Value;
        }
        else if (label.Equals(nameof(OffsetZ)))
        {
            OffsetZ = SetParameter(setValue, value, OffsetZ).Value;
        }
        else if (label.Equals(nameof(DropHeight)))
        {
            DropHeight = SetParameter(setValue, value, DropHeight).Value;
        }
        else if (label.Equals(nameof(AreaSize)))
        {
            AreaSize = SetParameter(setValue, value, AreaSize).Value;
        }
        else if (label.Equals(nameof(StartSecond)))
        {
            StartSecond = SetParameter(setValue, value, StartSecond).Value;
        }
        else if (label.Equals(nameof(EndSecond)))
        {
            EndSecond = SetParameter(setValue, value, EndSecond).Value;
        }
        else if (label.Equals(nameof(NumberOfNewRainObjectsPerSecond)))
        {
            NumberOfNewRainObjectsPerSecond = SetParameter(setValue, value, NumberOfNewRainObjectsPerSecond).Value;
        }
        else if (label.Equals(nameof(MaxNofRainObjects)))
        {
            MaxNofRainObjects = SetParameter(setValue, value, MaxNofRainObjects).Value;
        }
        else if (label.Equals(nameof(MaxNofClickObjects)))
        {
            MaxNofClickObjects = SetParameter(setValue, value, MaxNofClickObjects).Value;
        }
        else if (label.Equals(nameof(MaxNofTapSounds)))
        {
            MaxNofTapSounds = SetParameter(setValue, value, MaxNofTapSounds).Value;
        }
    }
    protected int? SetParameter(bool setValue, string value, int? defaultValue)
    {
        if (setValue && !string.IsNullOrWhiteSpace(value))
        {
            int intValue;
            if (int.TryParse(value, out intValue))
            {
                return intValue;
            }
        }
        return defaultValue;
    }
    protected float? SetParameter(bool setValue, string value, float? defaultValue)
    {
        if (setValue && !string.IsNullOrWhiteSpace(value))
        {
            float floatValue;
            if (float.TryParse(value, out floatValue))
            {
                return floatValue;
            }
        }
        return defaultValue;
    }

    private readonly List<GameObject> _rainObjects = new List<GameObject>();
    private readonly List<GameObject> _instantiatedRainObjects = new List<GameObject>();
    private readonly List<GameObject> _instantiatedClickObjects = new List<GameObject>();
    private readonly List<GameObject> _instantiatedTapSounds = new List<GameObject>();
    private int _numberOfRainObjects = 0;
    private long _millisecondAtStart;

    protected void Start()
    {
        _millisecondAtStart = DateTime.Now.Ticks / 10000;

        foreach (var rainObject in new[] {
            RainObject0, RainObject1, RainObject2, RainObject3, RainObject4,
            RainObject5, RainObject6, RainObject7, RainObject8, RainObject9
        })
        {
            if (rainObject != null)
            {
                _rainObjects.Add(rainObject);
            }
        }
    }

    private string GetFirstWord(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (!char.IsLetterOrDigit(c) && '_' != c && '-' != c)
            {
                return s.Substring(0, i);
            }
        }
        return s;
    }

    private void HandleHit(RaycastHit hit, GameObject rainObject, GameObject clickObject, GameObject tapSound)
    {
        if (rainObject != null)
        {
            var hitTransformName = GetFirstWord(hit.transform.ToString());
            var rainObjectTransformName = GetFirstWord(rainObject.transform.ToString());
            if (hitTransformName == rainObjectTransformName)
            {
                if (clickObject != null)
                {
                    GameObject newClickObject = Instantiate(clickObject, hit.transform.position, Quaternion.identity);
                    newClickObject.SetActive(true);
                    _instantiatedClickObjects.Add(newClickObject);
                }

                if (tapSound != null)
                {
                    GameObject newTapSound = Instantiate(tapSound, hit.transform.position, Quaternion.identity);
                    newTapSound.SetActive(true);
                    _instantiatedTapSounds.Add(newTapSound);
                }
            }
        }
    }

    protected void Update()
    {
        if (_rainObjects.Count < 1)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

            for (int i = 0; i < hits.Length; i++)
            {
                hit = hits[i];

                HandleHit(hit, RainObject0, ClickObject0, TapSound0);
                HandleHit(hit, RainObject1, ClickObject1, TapSound1);
                HandleHit(hit, RainObject2, ClickObject2, TapSound2);
                HandleHit(hit, RainObject3, ClickObject3, TapSound3);
                HandleHit(hit, RainObject4, ClickObject4, TapSound4);
                HandleHit(hit, RainObject5, ClickObject5, TapSound5);
                HandleHit(hit, RainObject6, ClickObject6, TapSound6);
                HandleHit(hit, RainObject7, ClickObject7, TapSound7);
                HandleHit(hit, RainObject8, ClickObject8, TapSound8);
                HandleHit(hit, RainObject9, ClickObject9, TapSound9);
            }
        }
        long millisecond = DateTime.Now.Ticks / 10000 - _millisecondAtStart;
        if (millisecond / 1000 < StartSecond)
        {
            return;
        }
        if (millisecond / 1000 >= EndSecond)
        {
            return;
        }

        while (_numberOfRainObjects < (millisecond - StartSecond * 1000) * NumberOfNewRainObjectsPerSecond / 1000)
        {
            Vector3 position = new Vector3(
                UnityEngine.Random.Range(-1000 * AreaSize / 2, 1000 * AreaSize / 2) / 1000.0f + OffsetX,
                UnityEngine.Random.Range(-1000 * DropHeight / 5, 1000 * DropHeight / 5) / 1000.0f + DropHeight,
                UnityEngine.Random.Range(-1000 * AreaSize / 2, 1000 * AreaSize / 2) / 1000.0f + OffsetZ
                );

            GameObject rainObject = Instantiate(_rainObjects[_numberOfRainObjects++ % _rainObjects.Count], position, UnityEngine.Random.rotation);
            rainObject.SetActive(true);
            _instantiatedRainObjects.Add(rainObject);
            //Console.WriteLine($"----> arpoiseObjectRain new rain object, total {_instantiatedRainObjects.Count}");
        }

        while (MaxNofRainObjects >= 0 && MaxNofRainObjects < _instantiatedRainObjects.Count)
        {
            var rainObject = _instantiatedRainObjects[0];
            _instantiatedRainObjects.Remove(rainObject);
            Destroy(rainObject);
            //Console.WriteLine($"----> arpoiseObjectRain destroyed rain object, total {_instantiatedRainObjects.Count}");
        }

        while (MaxNofClickObjects >= 0 && MaxNofClickObjects < _instantiatedClickObjects.Count)
        {
            var clickObject = _instantiatedClickObjects[0];
            _instantiatedClickObjects.Remove(clickObject);
            Destroy(clickObject);
        }

        while (MaxNofTapSounds >= 0 && MaxNofTapSounds < _instantiatedTapSounds.Count)
        {
            var tapSound = _instantiatedTapSounds[0];
            _instantiatedTapSounds.Remove(tapSound);
            Destroy(tapSound);
        }
    }
}
