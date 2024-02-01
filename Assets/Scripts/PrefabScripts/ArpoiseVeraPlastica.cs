/*
ArpoiseVeraPlastica.cs - A script handling an 'plastic object' like in Vera Plastica' for ARpoise.

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

using com.arpoise.arpoiseapp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ArpoiseVeraPlastica : MonoBehaviour, IRemoteCallback
{
    public class VeraPlastic
    {
        public GameObject Bottle;
        public readonly int PlasticType;
        public readonly float PositionX;
        public readonly float PositionZ;
        public readonly float RotationEulerX;
        public readonly float RotationEulerY;
        public readonly float RotationEulerZ;
        public DateTime? DestroyDate;
        public readonly DateTime CreationDate;

        public VeraPlastic(int plasticType, float millisX, float millisZ, float rotationEulerX, float rotationEulerY, float rotationEulerZ)
        {
            CreationDate = DateTime.Now;
            PlasticType = plasticType;
            PositionX = millisX;
            PositionZ = millisZ;
            RotationEulerX = rotationEulerX;
            RotationEulerY = rotationEulerY;
            RotationEulerZ = rotationEulerZ;
        }

        public VeraPlastic(string s)
        {
            CreationDate = DateTime.Now;
            var parts = s?.Split("|");
            if (parts == null || parts.Length < 1)
            {
                return;
            }

            _key = s;
            int value;
            if (parts.Length > 0 && int.TryParse(parts[0], out value))
            {
                PlasticType = value;
            }
            if (parts.Length > 1 && int.TryParse(parts[1], out value))
            {
                PositionX = value / 1000f;
            }
            if (parts.Length > 2 && int.TryParse(parts[2], out value))
            {
                PositionZ = value / 1000f;
            }
            if (parts.Length > 3 && int.TryParse(parts[3], out value))
            {
                RotationEulerX = value;
            }
            if (parts.Length > 4 && int.TryParse(parts[4], out value))
            {
                RotationEulerY = value;
            }
            if (parts.Length > 5 && int.TryParse(parts[5], out value))
            {
                RotationEulerZ = value;
            }
        }

        private string _key;
        public string Key { get { return _key ??= ToString(); } }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(PlasticType);
            sb.Append('|');
            sb.Append((int)(PositionX * 1000));
            sb.Append('|');
            sb.Append((int)(PositionZ * 1000));
            sb.Append('|');
            sb.Append((int)RotationEulerX);
            sb.Append('|');
            sb.Append((int)RotationEulerY);
            sb.Append('|');
            sb.Append((int)RotationEulerZ);
            return sb.ToString();
        }
    }

    public GameObject Plastic1;
    public GameObject Plastic2;
    public GameObject Plastic3;
    public GameObject Plastic4;
    public GameObject Plastic5;

    public int StartSecond = 0;
    public int EndSecond = -1;
    public float NumberOfNewPlasticsPerSecond = 0.25f;
    public int MaxNumberOfPlastics = 70;
    public int MinNumberOfPlastics = 10;

    private readonly List<GameObject> _plastics = new List<GameObject>();
    private readonly List<VeraPlastic> _veraPlasticsList = new List<VeraPlastic>();
    private readonly List<VeraPlastic> _veraPlasticsToDestroy = new List<VeraPlastic>();
    private readonly Dictionary<string, VeraPlastic> _veraPlastics = new Dictionary<string, VeraPlastic>();
    private readonly Dictionary<string, VeraPlastic> _visibleVeraPlastics = new Dictionary<string, VeraPlastic>();
    private string _myLock = string.Empty;
    private string _serverLock = string.Empty;
    private DateTime _connectionStartTime = DateTime.MinValue;
    private DateTime _lastReceiveTime = DateTime.MinValue;
    private DateTime _lastSendTime = DateTime.MinValue;
    private DateTime _dateAtStart;
    private DateTime _lastCreationTime = DateTime.MinValue;
    private DateTime _lastRemoveTime = DateTime.MinValue;
    private int _direction = 1;

    public ArBehaviourArObject ArBehavior { get; set; }

    protected void Start()
    {
        _dateAtStart = DateTime.Now;
        _myLock = ArBehaviourMultiUser.RandomString(8);
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        ArBehavior.SetRemoteCallback(this);

        if (Plastic1 != null)
        {
            _plastics.Add(Plastic1);
        }
        if (Plastic2 != null)
        {
            _plastics.Add(Plastic2);
        }
        if (Plastic3 != null)
        {
            _plastics.Add(Plastic3);
        }
        if (Plastic4 != null)
        {
            _plastics.Add(Plastic4);
        }
        if (Plastic5 != null)
        {
            _plastics.Add(Plastic5);
        }
    }

    private bool SendToRemote(string tag, string value)
    {
        _lastSendTime = DateTime.Now;
        return ArBehavior.SendToRemote(tag, value);
    }

    private void OnLockReceived(string tag, string lockFromServer)
    {
        if (ArBehaviourMultiUser.LockTag == tag && lockFromServer == _myLock && _serverLock != _myLock)
        {
            _veraPlastics.Clear();
            _veraPlasticsList.Clear();
            foreach (var veraPlastic in _visibleVeraPlastics.Values)
            {
                _veraPlastics[veraPlastic.Key] = veraPlastic;
                _veraPlasticsList.Add(veraPlastic);
            }
            _serverLock = lockFromServer;
            return;
        }
        if (lockFromServer != _myLock)
        {
            if (_veraPlastics.Any())
            {
                _veraPlastics.Clear();
            }
            if (_veraPlasticsList.Any())
            {
                _veraPlasticsList.Clear();
            }
        }
        if (ArBehaviourMultiUser.LockTag == tag || lockFromServer != _myLock)
        {
            _serverLock = lockFromServer;
        }
    }

    protected void Update()
    {
        var now = DateTime.Now;
        if (_plastics.Count < 1)
        {
            return;
        }

        long millisecond = now.Ticks / 10000 - _dateAtStart.Ticks / 10000;
        if (millisecond / 1000 < StartSecond)
        {
            return;
        }
        if (EndSecond >= 0 && millisecond / 1000 > EndSecond)
        {
            return;
        }

        if (_connectionStartTime != DateTime.MinValue || !ArBehavior.IsRemotingActivated)
        {
            if (_myLock == _serverLock)
            {
                // Add NumberOfNewPlasticsPerSecond bottles
                while (NumberOfNewPlasticsPerSecond > 0
                    && (now - _lastCreationTime).TotalMilliseconds > 1000f / NumberOfNewPlasticsPerSecond
                    && _veraPlastics.Count <= MaxNumberOfPlastics)
                {
                    AddOneBottleToGrid();
                    _lastCreationTime = now;
                }

                // If in removal phase, remove 2 * NumberOfNewPlasticsPerSecond bottles
                while (_direction != 1
                    && NumberOfNewPlasticsPerSecond > 0
                    && (now - _lastRemoveTime).TotalMilliseconds > 1000f / (2 * NumberOfNewPlasticsPerSecond)
                    && _veraPlastics.Count >= MinNumberOfPlastics)
                {
                    var veraPlastic = _veraPlasticsList[0];
                    _veraPlasticsList.RemoveAt(0);
                    _veraPlastics.Remove(veraPlastic.Key);
                    _lastRemoveTime = now;
                    if(_veraPlastics.Count < MinNumberOfPlastics)
                    {
                        _direction = 1; // we reached the minimum, the removal phase is over
                    }
                }

                if ((now - _lastSendTime).TotalMilliseconds > 1000)
                {
                    while (_veraPlasticsList.Count > MaxNumberOfPlastics)
                    {
                        var veraPlastic = _veraPlasticsList[0];
                        _veraPlasticsList.RemoveAt(0);
                        _veraPlastics.Remove(veraPlastic.Key);
                        _lastRemoveTime = now;
                        _direction = -1; // we reached the maximum, the removal phase starts
                    }

                    var sb = new StringBuilder();
                    sb.Append(_myLock);
                    foreach (var plastic in _veraPlastics.Keys)
                    {
                        sb.Append(sb.Length == 0 ? plastic : ";" + plastic);
                    }
                    var message = sb.ToString();
                    if (!SendToRemote(ArBehaviourMultiUser.VeraPlasticTag, message))
                    {
                        Set(ArBehaviourMultiUser.VeraPlasticTag, message,
                            _connectionStartTime == DateTime.MinValue ? now : _connectionStartTime, now);
                    }
                }
            }
            else
            {
                var waitMilliseconds = WaitMillisecondsDefault + UnityEngine.Random.Range(0f, 500f);
                if ((now - _connectionStartTime).TotalMilliseconds > waitMilliseconds
                    && (now - _lastSendTime).TotalMilliseconds > waitMilliseconds
                    && (now - _lastReceiveTime).TotalMilliseconds > waitMilliseconds)
                {
                    if (!SendToRemote(ArBehaviourMultiUser.LockTag, _myLock))
                    {
                        Set(ArBehaviourMultiUser.LockTag, _myLock,
                            _connectionStartTime == DateTime.MinValue ? now : _connectionStartTime, now);
                    }
                }
            }
        }

        foreach (var veraPlastic in _veraPlasticsToDestroy.Where(x => x.DestroyDate.HasValue && x.DestroyDate < now).ToArray())
        {
            _veraPlasticsToDestroy.Remove(veraPlastic);
            DestroyPlastic(veraPlastic);
        }
    }

    public float WaitMillisecondsDefault = 2000f;
    public float RandomOffsetInX = 0.005f; // +/- 5mm random offset + 7cm wide column * 13 columns
    public float ColumnWidth = 0.07f;
    public float RandomColumnIndex = 7f;
    public float PlasticOffsetInY = 0f;
    public float RandomOffsetInZ = 0.004f; // +/- 4mm random offset + 6cm wide row * 7 rows
    public float RowWidth = 0.06f;
    public float RandomRowIndex = 4f;

    private Vector3 RandomSpawnPositionInGridPlusCm
    {
        get
        {
            return new Vector3(
                UnityEngine.Random.Range(-RandomOffsetInX, RandomOffsetInX) + ColumnWidth * (int)UnityEngine.Random.Range(-RandomColumnIndex, RandomColumnIndex - 0.001f),
                0,
                UnityEngine.Random.Range(-RandomOffsetInZ, RandomOffsetInZ) + RowWidth * (int)UnityEngine.Random.Range(-RandomRowIndex, RandomRowIndex - 0.001f));
        }
    }

    public float RandomDegreesInX = 7f;
    public float RandomDegreesInY = 360f;
    public float RandomDegreesInZ = 7f;

    private void AddOneBottleToGrid()
    {
        VeraPlastic veraPlastic;
        for (; ; )
        {
            Vector3 randomSpawnPosition = RandomSpawnPositionInGridPlusCm;
            veraPlastic = new VeraPlastic(
                (int)UnityEngine.Random.Range(0.0f, 6.99f),
                randomSpawnPosition.x,
                randomSpawnPosition.z,
                UnityEngine.Random.Range(-RandomDegreesInX, RandomDegreesInX),
                UnityEngine.Random.Range(0f, RandomDegreesInY - 0.001f),
                UnityEngine.Random.Range(-RandomDegreesInZ, RandomDegreesInZ)
                );
            if (_veraPlastics.ContainsKey(veraPlastic.Key))
            {
                continue;
            }
            break;
        }
        _veraPlastics[veraPlastic.Key] = veraPlastic;
        _veraPlasticsList.Add(veraPlastic);
    }

    private GameObject GetPlastic(int plasticType)
    {
        GameObject plastic;
        switch (plasticType)
        {
            case 0:
                plastic = Plastic1;
                break;
            case 1:
                plastic = Plastic1;
                break;
            case 2:
                plastic = Plastic2;
                break;
            case 3:
                plastic = Plastic2;
                break;
            case 4:
                plastic = Plastic3;
                break;
            case 5:
                plastic = Plastic4;
                break;
            default:
                plastic = Plastic5;
                break;
        }
        return plastic;
    }

    public void Set(string tag, string value, DateTime start, DateTime now)
    {
        _lastReceiveTime = now;
        if (_connectionStartTime == DateTime.MinValue)
        {
            _connectionStartTime = start;
        }
        if (ArBehaviourMultiUser.LockTag == tag)
        {
            OnLockReceived(tag, value);
            return;
        }
        if (ArBehaviourMultiUser.VeraPlasticTag == tag)
        {
            var parts = value?.Split(";");
            if (parts == null || parts.Length < 1)
            {
                return;
            }

            var currentVeraPlastics = new HashSet<string>();
            for (int i = 1; i < parts.Length; i++)
            {
                var veraPlastic = new VeraPlastic(parts[i]);
                currentVeraPlastics.Add(veraPlastic.Key);

                if (!_visibleVeraPlastics.ContainsKey(veraPlastic.Key))
                {
                    var plastic = GetPlastic(veraPlastic.PlasticType);

                    var position = new Vector3(veraPlastic.PositionX, PlasticOffsetInY, veraPlastic.PositionZ);
                    var rotation = Quaternion.Euler(veraPlastic.RotationEulerX, veraPlastic.RotationEulerY, veraPlastic.RotationEulerZ);
                    var bottle = veraPlastic.Bottle = Instantiate(plastic, this.transform);
                    bottle.transform.localPosition = position;
                    bottle.transform.localRotation = rotation;
                    _visibleVeraPlastics[veraPlastic.Key] = veraPlastic;
                    if (_myLock != _serverLock)
                    {
                        _lastCreationTime = DateTime.Now;
                    }
                }
            }

            foreach (var key in _visibleVeraPlastics.Keys.ToList())
            {
                if (!currentVeraPlastics.Contains(key))
                {
                    VeraPlastic veraPlastic;
                    if (_visibleVeraPlastics.TryGetValue(key, out veraPlastic))
                    {
                        _visibleVeraPlastics.Remove(key);
                        StartScaleDownPlastic(veraPlastic);
                    }
                }
            }
            OnLockReceived(tag, parts[0]);
        }
    }

    private void DestroyPlastic(VeraPlastic veraPlastic)
    {
        if (veraPlastic.Bottle != null)
        {
            Destroy(veraPlastic.Bottle);
        }
    }

    private void StartScaleDownPlastic(VeraPlastic veraPlastic)
    {
        if (veraPlastic.CreationDate.AddSeconds(15) > DateTime.Now)
        {
            DestroyPlastic(veraPlastic);
            return;
        }
        veraPlastic.DestroyDate = DateTime.Now.AddSeconds(11);
        _veraPlasticsToDestroy.Add(veraPlastic);

        if (veraPlastic.Bottle != null)
        {
            StartScaleDownBottle(veraPlastic.Bottle);
        }
    }

    private static string[] _downAnimations =
    {
        "VP_bottleDownAnimation", "VP_DownAnimation1",  "VP_DownAnimation2" ,  "VP_DownAnimation3",  "VP_DownAnimation4",  "VP_DownAnimation5"
    };

    private void StartScaleDownBottle(GameObject bottle)
    {
        foreach (var child in bottle.GetComponentsInChildren<Transform>().Select(x => x.gameObject))
        {
            var animator = child.GetComponent<Animator>();
            if (animator != null)
            {
                foreach (var animation in _downAnimations)
                {
                    animator.Play(animation);
                }
            }
        }
    }

    public void SetParameter(bool setValue, string label, string value)
    {
        switch (label)
        {
            case nameof(StartSecond):
                StartSecond = SetParameter(setValue, value, StartSecond).Value;
                break;
            case nameof(EndSecond):
                EndSecond = SetParameter(setValue, value, EndSecond).Value;
                break;
            case nameof(NumberOfNewPlasticsPerSecond):
                NumberOfNewPlasticsPerSecond = SetParameter(setValue, value, NumberOfNewPlasticsPerSecond).Value;
                break;
            case nameof(MaxNumberOfPlastics):
                MaxNumberOfPlastics = SetParameter(setValue, value, MaxNumberOfPlastics).Value;
                break;
            case nameof(MinNumberOfPlastics):
                MinNumberOfPlastics = SetParameter(setValue, value, MinNumberOfPlastics).Value;
                break;
            case nameof(RandomOffsetInX):
                RandomOffsetInX = SetParameter(setValue, value, RandomOffsetInX).Value;
                break;
            case nameof(PlasticOffsetInY):
                PlasticOffsetInY = SetParameter(setValue, value, PlasticOffsetInY).Value;
                break;
            case nameof(ColumnWidth):
                ColumnWidth = SetParameter(setValue, value, ColumnWidth).Value;
                break;
            case nameof(RandomColumnIndex):
                RandomColumnIndex = SetParameter(setValue, value, RandomColumnIndex).Value;
                break;
            case nameof(RandomOffsetInZ):
                RandomOffsetInZ = SetParameter(setValue, value, RandomOffsetInZ).Value;
                break;
            case nameof(RowWidth):
                RowWidth = SetParameter(setValue, value, RowWidth).Value;
                break;
            case nameof(RandomRowIndex):
                RandomRowIndex = SetParameter(setValue, value, RandomRowIndex).Value;
                break;
            case nameof(RandomDegreesInX):
                RandomDegreesInX = SetParameter(setValue, value, RandomDegreesInX).Value;
                break;
            case nameof(RandomDegreesInY):
                RandomDegreesInY = SetParameter(setValue, value, RandomDegreesInY).Value;
                break;
            case nameof(RandomDegreesInZ):
                RandomDegreesInZ = SetParameter(setValue, value, RandomDegreesInZ).Value;
                break;
            case nameof(WaitMillisecondsDefault):
                WaitMillisecondsDefault = SetParameter(setValue, value, WaitMillisecondsDefault).Value;
                break;
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
}
