/*
ArFish.cs - Flocking behaviour of a single fish for ARpoise.

    The code is derived from the video
    https://www.youtube.com/watch?v=a7GkPNMGz8Y
    by Holistic3d, aka Professor Penny de Byl.

Copyright (C) 2019, Tamiko Thiel and Peter Graf - All Rights Reserved

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

using UnityEngine;

public class ArFish : MonoBehaviour
{
    private ArFlock _flock;
    public ArFlock Flock { set { _flock = value; } }

    private float _speed = 0.001f;

    protected void Start()
    {
        if (_flock == null)
        {
            return;
        }
        _speed = Random.Range(_flock.MinimumSpeed, _flock.MaximumSpeed);
    }

    protected void Update()
    {
        if (_flock == null)
        {
            return;
        }

        if (_flock.AllFish == null)
        {
            return;
        }

        // determine the bounding box of the manager cube
        var b = new Bounds(_flock.transform.position, _flock.SwimLimits * 2);

        // if fish is outside the bounds of the cube then start turning around
        if (!b.Contains(transform.position))
        {
            // turn towards the centre of the manager cube
            var direction = _flock.transform.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  Quaternion.LookRotation(direction),
                                                  _flock.RotationSpeed * Time.deltaTime);
            _speed = Random.Range(_flock.MinimumSpeed, _flock.MaximumSpeed) * _flock.SpeedFactor;
        }
        else
        {
            if (Random.Range(0, 100) < _flock.ApplyRulesPercentage)
            {
                ApplyRules(_flock, _flock.AllFish);
            }
        }
        transform.Translate(0, 0, Time.deltaTime * _speed * _flock.SpeedFactor);
    }

    private void ApplyRules(ArFlock flock, GameObject[] allFish)
    {
        var centerDirection = Vector3.zero;
        var avoidDirection = Vector3.zero;
        float groupSpeed = 0.01f;

        int groupSize = 0;
        foreach (var fish in allFish)
        {
            if (fish != gameObject)
            {
                var distance = Vector3.Distance(fish.transform.position, transform.position);
                if (distance <= _flock.NeighbourDistance)
                {
                    centerDirection += fish.transform.position;
                    groupSize++;

                    if (distance < _flock.MinNeighbourDistance)
                    {
                        avoidDirection += transform.position - fish.transform.position;
                    }

                    var otherFish = fish.GetComponent<ArFish>();
                    groupSpeed += otherFish._speed;
                }
            }
        }

        if (groupSize > 0)
        {
            centerDirection = centerDirection / groupSize + (flock.GoalPosition - transform.position);
            _speed = groupSpeed / groupSize * _flock.SpeedFactor;
            if (_speed < _flock.MinimumSpeed)
            {
                _speed = _flock.MinimumSpeed;
            }
            if (_speed > _flock.MaximumSpeed)
            {
                _speed = _flock.MaximumSpeed;
            }
            var direction = (centerDirection + avoidDirection) - transform.position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      Quaternion.LookRotation(direction),
                                                      _flock.RotationSpeed * Time.deltaTime);
            }
        }
    }
}
