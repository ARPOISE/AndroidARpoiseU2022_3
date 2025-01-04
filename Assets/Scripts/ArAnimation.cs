/*
ArAnimation.cs - Handling porpoise level animations for ARpoise.

Copyright (C) 2018, Tamiko Thiel and Peter Graf - All Rights Reserved

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
using System.Linq;
using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    public enum ArInterpolation
    {
        Linear = 0,
        Cyclic = 1,
        Sine = 2,
        Halfsine = 3,
        Smooth = 4
    }

    public enum ArAnimationType
    {
        Transform = 0,
        Rotate = 1,
        Scale = 2,
        Destroy = 3,
        Duplicate = 4,
        Fade = 5,
        Grow = 6,
        Volume = 7,
        Buzz = 8,
        SpatialBlend = 9
    }

    public enum ArEventType
    {
        Billboard = 0,
        OnCreate = 1,
        OnFocus = 2,
        InFocus = 3,
        InMinutes = 4,
        WhenActive = 5,
        OnClick = 6,
        OnFollow = 7,
        WhenActivated = 8,
        WhenDeactivated = 9
    }

    public class ArAnimation
    {
        public readonly long PoiId;
        public readonly GameObject Wrapper;
        public readonly GameObject GameObject;
        public readonly string Name = string.Empty;
        public readonly string[] FollowedBy = Array.Empty<string>();
        public readonly ArEventType ArEventType;

        private readonly ArCreature _creature;
        private readonly Transform _transform;
        private readonly long _lengthTicks;
        private readonly long _delayTicks;
        private readonly ArAnimationType _animationType;
        private readonly ArInterpolation _interpolation;
        private readonly bool _persisting;
        private readonly bool _repeating;
        private readonly float _from;
        private readonly float _to;
        private readonly Vector3 _axis;
        private readonly IArpoiseBehaviour _behaviour;
        private readonly bool _isTimeSync;

        private static readonly string _rotate = nameof(ArAnimationType.Rotate).ToLower();
        private static readonly string _scale = nameof(ArAnimationType.Scale).ToLower();
        private static readonly string _destroy = nameof(ArAnimationType.Destroy).ToLower();
        private static readonly string _duplicate = nameof(ArAnimationType.Duplicate).ToLower();
        private static readonly string _fade = nameof(ArAnimationType.Fade).ToLower();
        private static readonly string _grow = nameof(ArAnimationType.Grow).ToLower();
        private static readonly string _volume = nameof(ArAnimationType.Volume).ToLower();
        private static readonly string _spatialBlend = nameof(ArAnimationType.SpatialBlend).ToLower();
        private static readonly string _buzz = nameof(ArAnimationType.Buzz).ToLower();
        private static readonly string _cyclic = nameof(ArInterpolation.Cyclic).ToLower();
        private static readonly string _halfsine = nameof(ArInterpolation.Halfsine).ToLower();
        private static readonly string _sine = nameof(ArInterpolation.Sine).ToLower();
        private static readonly string _smooth = nameof(ArInterpolation.Smooth).ToLower();

        private float? _durationStretchFactor;
        private float? _initialA = null;
        private float? _initialVolume = null;
        private float? _initialSpatialBlend = null;
        private long _startTicks = 0;
        private List<Material> _materialsToFade = null;
        private AudioSource _audioSource = null;

        public bool IsToBeDestroyed { get; private set; }
        public bool IsToBeDuplicated { get; set; }

        public AudioRolloffMode? AudioRolloffMode { get; set; }
        public float? AudioSpatialBlend { get; set; }
        public bool? AudioSpatialize { get; set; }
        public float? AudioVolume { get; set; }

        public ArAnimation(
            long poiId,
            GameObject wrapper,
            GameObject gameObject,
            PoiAnimation poiAnimation,
            ArEventType arEventType,
            bool isActive,
            IArpoiseBehaviour behaviour,
            AudioRolloffMode? audioRolloffMode = null,
            float? audioSpatialBlend = null,
            bool? audioSpatialize = null,
            float? audioVolume = null
            )
        {
            AudioRolloffMode = audioRolloffMode;
            AudioSpatialBlend = audioSpatialBlend;
            AudioSpatialize = audioSpatialize;
            AudioVolume = audioVolume;

            PoiId = poiId;
            ArEventType = arEventType;
            GameObject = gameObject;
            IsActive = isActive;
            if ((Wrapper = wrapper) != null)
            {
                _transform = wrapper.transform;
            }
            if (poiAnimation != null)
            {
                Name = poiAnimation.name?.Trim();
                _isTimeSync = Name.Contains(nameof(_behaviour.TimeSync));
                FollowedBy = !string.IsNullOrWhiteSpace(poiAnimation.followedBy)
                    ? poiAnimation.followedBy.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray()
                    : FollowedBy;
                _lengthTicks = (long)(TimeSpan.TicksPerSecond * poiAnimation.length);
                _delayTicks = (long)(TimeSpan.TicksPerSecond * poiAnimation.delay);
                _behaviour = behaviour;
                if (poiAnimation.type != null)
                {
                    var type = poiAnimation.type.ToLower();
                    _animationType = type.Contains(_rotate) ? ArAnimationType.Rotate
                        : type.Contains(_scale) ? ArAnimationType.Scale
                        : type.Contains(_destroy) ? ArAnimationType.Destroy
                        : type.Contains(_duplicate) ? ArAnimationType.Duplicate
                        : type.Contains(_fade) ? ArAnimationType.Fade
                        : type.Contains(_grow) ? ArAnimationType.Grow
                        : type.Contains(_volume) ? ArAnimationType.Volume
                        : type.Contains(_spatialBlend) ? ArAnimationType.SpatialBlend
                        : type.Contains(_buzz) ? ArAnimationType.Buzz
                        : ArAnimationType.Transform;
                }
                if (poiAnimation.interpolation != null)
                {
                    var interpolation = poiAnimation.interpolation.ToLower();
                    _interpolation = interpolation.Contains(_cyclic) ? ArInterpolation.Cyclic
                        : interpolation.Contains(_halfsine) ? ArInterpolation.Halfsine
                        : interpolation.Contains(_smooth) ? ArInterpolation.Smooth
                        : interpolation.Contains(_sine) ? ArInterpolation.Sine
                        : ArInterpolation.Linear;
                }
                _persisting = poiAnimation.persist;
                _repeating = poiAnimation.repeat;
                _from = poiAnimation.from;
                _to = poiAnimation.to;
                _axis = poiAnimation.axis == null ? Vector3.zero
                    : new Vector3(poiAnimation.axis.x, poiAnimation.axis.y, poiAnimation.axis.z);
                if (_animationType == ArAnimationType.Grow && GameObject != null)
                {
                    _creature = GameObject.GetComponent(typeof(ArCreature)) as ArCreature;
                }
            }
        }

        public bool IsActive { get; private set; }
        public bool JustActivated { get; private set; }
        public bool JustStopped { get; private set; }

        public void Activate(long startTicks, long nowTicks, bool isRemote = false)
        {
            if (!isRemote && _behaviour != null && !string.IsNullOrWhiteSpace(Name) && Name.Contains("Remoted"))
            {
                if (_behaviour.SendAnimationToRemote(Name))
                {
                    return;
                }
            }

            if (ArEventType == ArEventType.WhenActive && !GameObject.activeSelf)
            {
                return;
            }

            IsActive = true;
            _startTicks = 0;
            Animate(startTicks, nowTicks);
        }

        public void Animate(long startTicks, long nowTicks)
        {
            JustStopped = JustActivated = false;

            if (startTicks <= 0 || !IsActive || _lengthTicks < 1 || _delayTicks < 0)
            {
                return;
            }

            _durationStretchFactor = _behaviour.DurationStretchFactor;
            var delayTicks = (long)(_durationStretchFactor.HasValue ? _durationStretchFactor * _delayTicks : _delayTicks);
            if (delayTicks > 0 && startTicks + delayTicks > nowTicks)
            {
                return;
            }

            float animationValue = 0;
            if (_startTicks == 0)
            {
                _startTicks = nowTicks;
                JustActivated = true;
            }
            else
            {
                var lengthTicks = (long)(_durationStretchFactor.HasValue ? _durationStretchFactor * _lengthTicks : _lengthTicks);
                var endTicks = _startTicks + lengthTicks;
                if (endTicks < nowTicks)
                {
                    if (!_repeating || (ArEventType == ArEventType.WhenActive && !GameObject.activeSelf))
                    {
                        Stop(startTicks, endTicks);
                        return;
                    }
                    _startTicks = endTicks + lengthTicks >= nowTicks ? endTicks : nowTicks;
                    JustActivated = true;
                }
                animationValue = (nowTicks - _startTicks) / ((float)lengthTicks);
            }

            if (_isTimeSync && JustActivated && _behaviour.DurationStretchFactor.HasValue)
            {
                _behaviour.TimeSync();
            }

            var from = _from;
            var to = _to;

            switch (_interpolation)
            {
                case ArInterpolation.Cyclic:
                    if (animationValue >= .5f)
                    {
                        animationValue -= .5f;
                        var temp = from;
                        from = to;
                        to = temp;
                    }
                    animationValue *= 2;
                    break;

                case ArInterpolation.Halfsine:
                    animationValue = (float)Math.Sin(Math.PI * animationValue);
                    break;

                case ArInterpolation.Smooth:
                    animationValue = (-1f + (float)Math.Cos(Math.PI * animationValue)) / 2;
                    break;

                case ArInterpolation.Sine:
                    animationValue = (-1f + (float)Math.Cos(2 * Math.PI * animationValue)) / 2;
                    break;
            }

            var animationFactor = from + (to - from) * Math.Abs(animationValue);
            switch (_animationType)
            {
                case ArAnimationType.Destroy:
                    IsToBeDestroyed = true;
                    break;

                case ArAnimationType.Duplicate:
                    IsToBeDuplicated |= JustActivated;
                    break;

                case ArAnimationType.Fade:
                    Fade(animationFactor);
                    break;

                case ArAnimationType.Grow:
                    Grow(animationFactor);
                    break;

                case ArAnimationType.Rotate:
                    Rotate(animationFactor);
                    break;

                case ArAnimationType.Scale:
                    Scale(animationFactor);
                    break;

                case ArAnimationType.Transform:
                    Transform(animationFactor);
                    break;

                case ArAnimationType.Volume:
                    SetVolume(animationFactor);
                    break;

                case ArAnimationType.SpatialBlend:
                    SetSpatialBlend(animationFactor);
                    break;

                case ArAnimationType.Buzz:
                    Buzz(animationFactor);
                    break;
            }

            if (JustActivated)
            {
                HandleOpenUrl(Name);
                HandleSetActive(Name, false);
                HandleAudioSource();
            }
        }

        public void Stop(long startTicks, long nowTicks)
        {
            Animate(startTicks, nowTicks);

            JustStopped = true;
            IsActive = false;
            if (!_persisting)
            {
                HandleSetActive(Name, false);
                switch (_animationType)
                {
                    case ArAnimationType.Fade:
                        if (_initialA.HasValue)
                        {
                            Fade(_initialA.Value);
                        }
                        break;

                    case ArAnimationType.Rotate:
                        if (_transform != null)
                        {
                            _transform.localEulerAngles = Vector3.zero;
                        }
                        break;

                    case ArAnimationType.Scale:
                        if (_transform != null)
                        {
                            _transform.localScale = Vector3.one;
                        }
                        break;

                    case ArAnimationType.Transform:
                        if (_transform != null)
                        {
                            _transform.localPosition = Vector3.zero;
                        }
                        break;

                    case ArAnimationType.Volume:
                        if (_initialVolume.HasValue)
                        {
                            SetVolume(_initialVolume.Value);
                        }
                        break;

                    case ArAnimationType.SpatialBlend:
                        if (_initialSpatialBlend.HasValue)
                        {
                            SetSpatialBlend(_initialSpatialBlend.Value);
                        }
                        break;
                }
            }
        }

        private static readonly string _openUrl = "openUrl:";
        public bool HandleOpenUrl(string name)
        {
            if (!string.IsNullOrWhiteSpace(name) && name.StartsWith(_openUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                var url = name.Substring(_openUrl.Length);
                if (!string.IsNullOrWhiteSpace(url))
                {
                    Application.OpenURL(url);
                    return true;
                }
            }
            return false;
        }

        protected static readonly string SetInactive;
        public bool HandleSetActive(string name, bool onFollow)
        {
            var gameObject = GameObject;
            if (gameObject != null && !string.IsNullOrWhiteSpace(name))
            {
                if (!onFollow || Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (name.EndsWith(nameof(gameObject.SetActive), StringComparison.InvariantCultureIgnoreCase))
                    {
                        SetActive(gameObject, true);
                        //Debug.Log($"GO '{gameObject.name}', animation {name}, is active {gameObject.activeSelf}");
                        return true;
                    }
                    if (name.EndsWith(nameof(SetInactive), StringComparison.InvariantCultureIgnoreCase))
                    {
                        SetActive(gameObject, false);
                        //Debug.Log($"GO '{gameObject.name}', animation {name}, is active {gameObject.activeSelf}");
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ApplicationIsSleeping { get; private set; }

        public void SetActive(GameObject gameObject, bool active)
        {
            if (ApplicationIsSleeping)
            {
                active = false;
            }

            if (gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
                //Debug.Log($"GO '{gameObject.name}', is active {gameObject.activeSelf}");
            }
        }

        public void HandleApplicationSleep(bool shouldSleep)
        {
            ApplicationIsSleeping = shouldSleep;
            var gameObject = GameObject;
            if (gameObject != null)
            {
                SetActive(gameObject, !shouldSleep);
            }
        }

        private int? _startMinute;
        public int StartMinute
        {
            get
            {
                if (_startMinute == null)
                {
                    _startMinute = -1;

                    var interval = Name?.Replace("Time:", string.Empty).Trim(); ;
                    if (!string.IsNullOrWhiteSpace(interval))
                    {
                        var parts = interval.Split('-');
                        if (parts.Length > 1)
                        {
                            int value;
                            if (TryParseMinutes(parts[0], out value))
                            {
                                _startMinute = value;
                            }
                        }
                    }
                }
                return _startMinute.Value;
            }
        }

        private int? _endMinute;
        public int EndMinute
        {
            get
            {
                if (_endMinute == null)
                {
                    _endMinute = -1;

                    var interval = Name?.Replace("Time:", string.Empty).Trim(); ;
                    if (!string.IsNullOrWhiteSpace(interval))
                    {
                        var parts = interval.Split('-');
                        if (parts.Length > 1)
                        {
                            int value;
                            if (TryParseMinutes(parts[1], out value))
                            {
                                _endMinute = value;
                            }
                        }
                    }
                }
                return _endMinute.Value;
            }
        }

        public bool ShouldBeActive()
        {
            var startMinute = StartMinute;
            var endMinute = EndMinute;
            if (startMinute < 0 || startMinute > 24 * 60 || endMinute < 0 || endMinute > 24 * 60)
            {
                return false;
            }
            var minute = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            var shouldNotBeActive = startMinute < 0 || endMinute < 0
                   || (startMinute <= endMinute && (minute < startMinute || minute >= endMinute))
                   || (startMinute > endMinute && (minute < startMinute && minute >= endMinute));

            return !shouldNotBeActive;
        }

        private void HandleAudioSource()
        {
            var gameObject = GameObject;
            if (gameObject != null)
            {
                if (_audioSource == null)
                {
                    _audioSource = gameObject.GetComponent<AudioSource>();
                }
                if (_audioSource != null)
                {
                    if (AudioRolloffMode.HasValue)
                    {
                        _audioSource.rolloffMode = AudioRolloffMode.Value;
                    }
                    if (AudioSpatialBlend.HasValue)
                    {
                        _audioSource.spatialBlend = AudioSpatialBlend.Value;
                    }
                    if (AudioSpatialize.HasValue)
                    {
                        _audioSource.spatialize = AudioSpatialize.Value;
                    }
                    if (AudioVolume.HasValue && AudioVolume.Value >= 0)
                    {
                        _audioSource.volume = AudioVolume.Value;
                    }

                    _audioSource.Play();
                }
            }
        }
        private void Rotate(float value)
        {
            if (_transform != null)
            {
                _transform.localEulerAngles = new Vector3(_axis.x * value, _axis.y * value, _axis.z * value);
            }
        }

        private void Scale(float value)
        {
            if (_transform != null)
            {
                _transform.localScale = new Vector3(
                    _axis.x == 0 ? 1 : _axis.x * value, _axis.y == 0 ? 1 : _axis.y * value, _axis.z == 0 ? 1 : _axis.z * value);
            }
        }

        private void Transform(float value)
        {
            if (_transform != null)
            {
                _transform.localPosition = new Vector3(_axis.x * value, _axis.y * value, _axis.z * value);
            }
        }

        private void Grow(float value)
        {
            if (_creature != null)
            {
                _creature.Grow(value);
            }
        }

        private void Fade(float value)
        {
            if (_materialsToFade == null)
            {
                GetMaterialsToFade(GameObject, _materialsToFade = new List<Material>());
            }
            foreach (var material in _materialsToFade)
            {
                var color = material.color;
                if (!_initialA.HasValue)
                {
                    _initialA = color.a;
                }
                material.color = new Color(color.r, color.g, color.b, value);
            }
        }

        private void Buzz(float value)
        {
            _behaviour.DoBuzz = true;
        }

        private void SetVolume(float value)
        {
            if (_audioSource == null && GameObject != null)
            {
                _audioSource = GameObject.GetComponent<AudioSource>();
            }
            if (_audioSource != null)
            {
                if (!_initialVolume.HasValue)
                {
                    _initialVolume = _audioSource.volume;
                }
                if (AudioVolume.HasValue && AudioVolume.Value >= 0)
                {
                    _audioSource.volume = AudioVolume.Value;
                }
                else
                {
                    _audioSource.volume = value;
                }
            }
        }

        private void SetSpatialBlend(float value)
        {
            if (_audioSource == null && GameObject != null)
            {
                _audioSource = GameObject.GetComponent<AudioSource>();
            }
            if (_audioSource != null)
            {
                if (!_initialVolume.HasValue)
                {
                    _initialSpatialBlend = _audioSource.spatialBlend;
                }
                if (AudioSpatialBlend.HasValue && AudioSpatialBlend.Value >= 0)
                {
                    _audioSource.spatialBlend = AudioSpatialBlend.Value;
                }
                else
                {
                    _audioSource.spatialBlend = value;
                }
            }
        }

        private void GetMaterialsToFade(GameObject gameObject, List<Material> materials)
        {
            if (gameObject != null)
            {
                //particle system also have renderers that could be accessed
                //var ps = gameObject.GetComponent<ParticleSystem>();
                //var x = ps.shape.meshRenderer.material.color;
                //var y = ps.shape.spriteRenderer.material.color;
                //var z = ps.shape.skinnedMeshRenderer.material.color;

                var renderer = gameObject.GetComponent<MeshRenderer>();
                if (renderer != null && renderer.material != null)
                {
                    materials.Add(renderer.material);
                }
                foreach (var child in gameObject.GetComponentsInChildren<Transform>().Select(x => x.gameObject))
                {
                    if (child != null)
                    {
                        renderer = child.GetComponent<MeshRenderer>();
                        if (renderer != null && renderer.material != null)
                        {
                            materials.Add(renderer.material);
                        }
                    }
                    // Making this fully recursive is too slow for example in AyCorona
                    // GetMaterialsToFade(transform.gameObject, materials);
                }
            }
        }

        public static bool TryParseMinutes(string s, out int result)
        {
            result = -1;
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }
            if (s.Contains(':'))
            {
                var parts = s.Split(':');
                if (parts.Length > 0)
                {
                    int hours;
                    if (!int.TryParse(parts[0], out hours))
                    {
                        return false;
                    }
                    if (parts.Length > 1)
                    {
                        int minutes;
                        if (!int.TryParse(parts[1], out minutes))
                        {
                            return false;
                        }
                        result = hours * 60 + minutes;
                        return true;
                    }
                    result = hours * 60;
                    return true;
                }
                return false;
            }

            if (int.TryParse(s, out result))
            {
                result *= 60;
                return true;
            }
            return false;
        }

        // This does not really work, it was work in progress from October 2020, a version of Nothing of Him
        //
        //private void SetBleachingValue(float value)
        //{
        //    var gameObject = GameObject;
        //    if (gameObject == null)
        //    {
        //        return;
        //    }

        //    Renderer objectRenderer;
        //    var rendererColorPairs = new List<KeyValuePair<Renderer, Color>>();
        //    objectRenderer = gameObject.GetComponent<MeshRenderer>();
        //    if (objectRenderer != null && objectRenderer.material != null)
        //    {
        //        rendererColorPairs.Add(new KeyValuePair<Renderer, Color>(objectRenderer, objectRenderer.material.color));
        //    }
        //    else
        //    {
        //        foreach (var child in gameObject.GetComponentsInChildren<Transform>().Select(x => x.gameObject))
        //        {
        //            if (child != null)
        //            {
        //                objectRenderer = child.GetComponent<MeshRenderer>();
        //                if (objectRenderer != null && objectRenderer.material != null)
        //                {
        //                    rendererColorPairs.Add(new KeyValuePair<Renderer, Color>(objectRenderer, objectRenderer.material.color));
        //                }
        //            }
        //        }
        //    }
        //    if (rendererColorPairs.Any())
        //    {
        //        foreach (var pair in rendererColorPairs)
        //        {
        //            var color = pair.Value;
        //            if (_initialR == null)
        //            {
        //                _initialR = color.r;
        //            }
        //            if (_initialG == null)
        //            {
        //                _initialG = color.g;
        //            }
        //            if (_initialB == null)
        //            {
        //                _initialB = color.b;
        //            }
        //            pair.Key.material.color = new Color(
        //                _initialR.Value + value * ((1f - _initialR.Value) / 100f),
        //                _initialG.Value + value * ((1f - _initialG.Value) / 100f),
        //                _initialB.Value + value * ((1f - _initialB.Value) / 100f),
        //                color.a
        //                );
        //        }
        //    }
        //}
    }
}