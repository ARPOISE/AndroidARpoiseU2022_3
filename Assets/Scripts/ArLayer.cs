/*
ArLayer.cs - Data description for an ARpoise layer.

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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace com.arpoise.arpoiseapp
{
    [Serializable]
    public class PoiVector3
    {
        public float x = 0;
        public float y = 0;
        public float z = 0;
    }

    [Serializable]
    public class PoiTransform
    {
        public bool rel = false;
        public float angle = 0;
        public float scale = 0;
    }

    [Serializable]
    public class PoiAction
    {
        public float autoTriggerRange = 0;
        public bool autoTriggerOnly = false;
        public string uri = string.Empty;
        public string label = string.Empty;
        public string contentType = string.Empty;
        public int activityType = 0;
        public string method = "GET";
        public string[] poiParams = null;
        public bool closeBiw = false;
        public bool showActivity = true;
        public string activityMessage = string.Empty;
    }

    [Serializable]
    public class PoiAnimation
    {
        public string name = string.Empty;
        public string type = string.Empty;
        public float length = 0;
        public float delay = 0;
        public string interpolation = string.Empty;
        public float interpolationParam = 0;
        public bool persist = false;
        public bool repeat = false;
        public float from = 0;
        public float to = 0;
        public PoiVector3 axis = null;
        public string followedBy = string.Empty;
    }

    [Serializable]
    public class PoiAnimations
    {
        public PoiAnimation[] onCreate = null;
        public PoiAnimation[] onFollow = null;
        public PoiAnimation[] onFocus = null;
        public PoiAnimation[] inFocus = null;
        public PoiAnimation[] onClick = null;
        public PoiAnimation[] inMinutes = null;
        public PoiAnimation[] whenActive = null;
        public PoiAnimation[] whenActivated = null;
        public PoiAnimation[] whenDeactivated = null;
    }

    [Serializable]
    public class PoiObject
    {
        public string baseURL = string.Empty;
        public string full = string.Empty;
        public string poiLayerName = string.Empty;
        public string relativeLocation = string.Empty;
        public string icon = string.Empty;
        public float size = 0;
        public string triggerImageURL = string.Empty;
        public float triggerImageWidth = 0;

        public float[] RelativeLocation
        {
            get
            {
                if (string.IsNullOrWhiteSpace(relativeLocation))
                {
                    return new float[] { 0, 0, 0 };
                }
                var parts = relativeLocation.Split(',');

                float value;
                var xOffset = (float)(parts.Length > 0 && float.TryParse(parts[0].Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : 0);
                var yOffset = (float)(parts.Length > 1 && float.TryParse(parts[1].Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : 0);
                var zOffset = (float)(parts.Length > 2 && float.TryParse(parts[2].Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : 0);
                return new float[] { xOffset, yOffset, zOffset };
            }
            set
            {
                if (value != null)
                {
                    relativeLocation = $"{(value.Length > 0 ? value[0].ToString(CultureInfo.InvariantCulture) : "0")},{(value.Length > 1 ? value[1].ToString(CultureInfo.InvariantCulture) : "0")},{(value.Length > 2 ? value[2].ToString(CultureInfo.InvariantCulture) : "0")}";
                }
                else
                {
                    relativeLocation = string.Empty;
                }
            }
        }
    }

    [Serializable]
    public class Poi
    {
        public long id = 0;
        public int dimension = 0;
        public bool showSmallBiw = true;
        public bool isVisible = true;
        public PoiTransform transform = null;
        public PoiObject poiObject = null;
        public PoiAction[] actions = Array.Empty<PoiAction>();
        public PoiAnimations animations = null;
        public string attribution = string.Empty;
        public float distance = 0;
        public int visibilityRange = 0;
        public float relativeAlt = 0;
        public string imageURL = string.Empty;
        public int lat = 0;
        public int lon = 0;

        public string line1 = string.Empty;
        public string line2 = string.Empty;
        public string line3 = string.Empty;
        public string line4 = string.Empty;
        public string title = string.Empty;
        public int type = 0;

        public float Latitude { get { return lat / 1000000f; } }
        public float Longitude { get { return lon / 1000000f; } }

        [NonSerialized]
        public ArLayer ArLayer;

        public string BaseUrl => poiObject?.baseURL?.Trim();

        public string TriggerImageURL => poiObject?.triggerImageURL?.Trim();

        public string GameObjectName => poiObject?.full?.Trim();

        public string InnerLayerName => poiObject?.poiLayerName?.Trim();

        public Poi Clone()
        {
            var s = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<Poi>(s);
        }

        [NonSerialized]
        private int? _assetBundleCacheVersion = null;
        public int AssetBundleCacheVersion
        {
            get
            {
                if (!_assetBundleCacheVersion.HasValue)
                {
                    _assetBundleCacheVersion = -1;
                    var layerAssetBundleCacheVersion = ArLayer?.AssetBundleCacheVersion;
                    if (layerAssetBundleCacheVersion.HasValue && layerAssetBundleCacheVersion > _assetBundleCacheVersion)
                    {
                        _assetBundleCacheVersion = layerAssetBundleCacheVersion;
                    }
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(AssetBundleCacheVersion).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        int value;
                        if (int.TryParse(action.activityMessage, out value))
                        {
                            if (value > _assetBundleCacheVersion)
                            {
                                _assetBundleCacheVersion = value;
                            }
                        }
                    }
                }
                return _assetBundleCacheVersion.Value;
            }
        }

        [NonSerialized]
        private int? _maximumCount = null;
        public int MaximumCount
        {
            get
            {
                if (!_maximumCount.HasValue)
                {
                    _maximumCount = 0;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(MaximumCount).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        int value;
                        if (int.TryParse(action.activityMessage, out value))
                        {
                            _maximumCount = value;
                        }
                    }
                }
                return _maximumCount.Value;
            }
        }

        [NonSerialized]
        private string _allAugmentsPlaced = null;
        public string AllAugmentsPlaced
        {
            get
            {
                if (_allAugmentsPlaced is null)
                {
                    _allAugmentsPlaced = string.Empty;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(AllAugmentsPlaced).Equals(x.label?.Trim()));
                    if (action != null)
                    {
                        _allAugmentsPlaced = action.activityMessage ?? string.Empty;
                    }
                }
                return _allAugmentsPlaced;
            }
        }

        [NonSerialized]
        private string _requestedDetectionMode = null;
        public string RequestedDetectionMode
        {
            get
            {
                if (_requestedDetectionMode is null)
                {
                    _requestedDetectionMode = string.Empty;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(RequestedDetectionMode).Equals(x.label?.Trim()));
                    if (action != null)
                    {
                        _requestedDetectionMode = action.activityMessage ?? string.Empty;
                    }
                }
                return _requestedDetectionMode;
            }
        }

        [NonSerialized]
        private double? _trackingTimeout = null;
        public double TrackingTimeout
        {
            get
            {
                if (!_trackingTimeout.HasValue)
                {
                    _trackingTimeout = 0;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(TrackingTimeout).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        double value = 0;
                        if (double.TryParse(action.activityMessage.Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            _trackingTimeout = value;
                        }
                    }
                }
                return _trackingTimeout.Value;
            }
        }

        [NonSerialized]
        private float? _positionLerpFactor = null;
        public float PositionLerpFactor
        {
            get
            {
                if (!_positionLerpFactor.HasValue)
                {
                    _positionLerpFactor = 0;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(PositionLerpFactor).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value = 0;
                        if (float.TryParse(action.activityMessage.Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            _positionLerpFactor = value;
                        }
                    }
                }
                return _positionLerpFactor.Value;
            }
        }

        [NonSerialized]
        private float? _rotationLerpFactor = null;
        public float RotationLerpFactor
        {
            get
            {
                if (!_rotationLerpFactor.HasValue)
                {
                    _rotationLerpFactor = 0;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(RotationLerpFactor).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value = 0;
                        if (float.TryParse(action.activityMessage.Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            _rotationLerpFactor = value;
                        }
                    }
                }
                return _rotationLerpFactor.Value;
            }
        }

        [NonSerialized]
        private float? _targetPositionFactor = null;
        public float TargetPositionFactor
        {
            get
            {
                if (!_targetPositionFactor.HasValue)
                {
                    _targetPositionFactor = 0;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(TargetPositionFactor).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value = 0;
                        if (float.TryParse(action.activityMessage.Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            _targetPositionFactor = value;
                        }
                    }
                }
                return _targetPositionFactor.Value;
            }
        }

        [NonSerialized]
        private string _lindenmayerString = null;
        public string LindenmayerString
        {
            get
            {
                if (_lindenmayerString == null)
                {
                    _lindenmayerString = string.Empty;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(LindenmayerString).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        _lindenmayerString = action.activityMessage;
                        if (_lindenmayerString != null)
                        {
                            _lindenmayerString = Regex.Replace(_lindenmayerString, @"\s+", string.Empty);
                        }
                    }
                }
                return _lindenmayerString;
            }
        }

        [NonSerialized]
        private string _leafPrefab = null;
        public string LeafPrefab
        {
            get
            {
                if (_leafPrefab == null)
                {
                    _leafPrefab = string.Empty;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(LeafPrefab).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        _leafPrefab = action.activityMessage;
                        if (_leafPrefab != null)
                        {
                            _leafPrefab = Regex.Replace(_leafPrefab, @"\s+", string.Empty);
                        }
                    }
                }
                return _leafPrefab;
            }
        }

        [NonSerialized]
        private float? _lindenmayerAngle = null;
        public float LindenmayerAngle
        {
            get
            {
                if (!_lindenmayerAngle.HasValue)
                {
                    _lindenmayerAngle = 22.5f;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(LindenmayerAngle).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value = 0;
                        if (float.TryParse(action.activityMessage.Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            _lindenmayerAngle = value;
                        }
                    }
                }
                return _lindenmayerAngle.Value;
            }
        }

        [NonSerialized]
        private float? _lindenmayerFactor = null;
        public float LindenmayerFactor
        {
            get
            {
                if (!_lindenmayerFactor.HasValue)
                {
                    _lindenmayerFactor = 0.8f;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(LindenmayerFactor).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value = 0;
                        if (float.TryParse(action.activityMessage.Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            _lindenmayerFactor = value;
                        }
                    }
                }
                return _lindenmayerFactor.Value;
            }
        }

        [NonSerialized]
        private int? _derivations = null;
        public int LindenmayerDerivations
        {
            get
            {
                if (!_derivations.HasValue)
                {
                    _derivations = 1;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(LindenmayerDerivations).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        int value = 0;
                        if (int.TryParse(action.activityMessage, out value))
                        {
                            _derivations = value;
                        }
                    }
                }
                return _derivations.Value;
            }
        }
    }

    // This class defines the Json message returned by porpoise to the client side, allowing to parse the message
    [Serializable]
    public class ArLayer
    {
        public Poi[] hotspots = Array.Empty<Poi>();
        public float radius = 0;
        public float refreshInterval = 0;
        public float refreshDistance = 0;
        public string redirectionUrl = string.Empty;
        public string redirectionLayer = string.Empty;
        public string noPoisMessage = string.Empty;
        public string layerTitle = string.Empty;
        public string showMessage = string.Empty;
        public bool morePages = false;
        public string nextPageKey = string.Empty;
        public string layer = string.Empty;
        public int errorCode = 0;
        public string errorString = string.Empty;
        public int bleachingValue = 0;
        public int areaSize = 0;
        public int areaWidth = 0;
        public int visibilityRange = 1500;
        public bool applyKalmanFilter = true;
        public bool isDefaultLayer = false;
        public bool showMenuButton = true;

        public PoiAction[] actions = Array.Empty<PoiAction>();

        public static ArLayer Create(string json)
        {
            // 'params' and 'object' are reserved words in C#, we have to replace them before we parse the json
            json = json.Replace("\"params\"", "\"poiParams\"").Replace("\"object\"", "\"poiObject\"");

            return JsonUtility.FromJson<ArLayer>(json);
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }

        [NonSerialized]
        private float _latitude = float.MinValue;
        public float Latitude
        {
            get
            {
                if (_latitude == float.MinValue)
                {
                    if (hotspots.Any())
                    {
                        _latitude = hotspots.Select(x => x.Latitude).Average();
                    }
                    else
                    {
                        _latitude = 0;
                    }
                }
                return _latitude;
            }
        }

        [NonSerialized]
        private float _longitude = float.MinValue;
        public float Longitude
        {
            get
            {
                if (_longitude == float.MinValue)
                {
                    if (hotspots.Any())
                    {
                        _longitude = hotspots.Select(x => x.Longitude).Average();
                    }
                    else
                    {
                        _longitude = 0;
                    }
                }
                return _longitude;
            }
        }

        [NonSerialized]
        private static readonly HashSet<string> _actionLabels = new HashSet<string>(new string[] // Layer ActionLabel
        {
            nameof(AllowTakeScreenshot),
            nameof(ApplicationSleepInterval),
            nameof(AssetBundleCacheVersion),
            nameof(AudioRolloffMode),
            nameof(AudioSpatialBlend),
            nameof(AudioSpatialize),
            nameof(AudioVolume),
            nameof(DirectionalLightN_Intensity),
            nameof(DirectionalLightN_IsActive),
            nameof(DirectionalLightSEE_Intensity),
            nameof(DirectionalLightSEE_IsActive),
            nameof(DirectionalLightSWW_Intensity),
            nameof(DirectionalLightSWW_IsActive),
            nameof(MaximumActiveTriggerObjects),
            nameof(OcclusionEnvironmentDepthMode),
            nameof(OcclusionHumanSegmentationDepthMode),
            nameof(OcclusionHumanSegmentationStencilMode),
            nameof(OcclusionPreferenceMode),
            nameof(PositionUpdateInterval),
            nameof(RemoteServerUrl),
            nameof(SceneUrl),
            nameof(TimeSync),
        });

        [NonSerialized]
        private bool? _showInfo;
        public bool ShowInfo
        {
            get
            {
                if (!_showInfo.HasValue)
                {
                    _showInfo = (actions?.FirstOrDefault(x => !_actionLabels.Contains(x.label?.Trim()) && x.showActivity)) != null;
                    if (_showInfo == null)
                    {
                        _showInfo = false;
                    }
                }
                return _showInfo.Value;
            }
        }

        [NonSerialized]
        private string _informationMessage;
        public string InformationMessage
        {
            get
            {
                if (_informationMessage == null)
                {
                    _informationMessage = actions.Where(x => !_actionLabels.Contains(x.label?.Trim()) && x.showActivity).Select(x => x.activityMessage).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                    if (_informationMessage == null)
                    {
                        _informationMessage = string.Empty;
                    }
                }
                return _informationMessage;
            }
        }

        #region ActionLabels

        [NonSerialized]
        private int? _allowTakeScreenshot;
        public int AllowTakeScreenshot // Layer ActionLabel
        {
            get
            {
                if (_allowTakeScreenshot == null)
                {
                    _allowTakeScreenshot = 0;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(AllowTakeScreenshot).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        int value;
                        if (int.TryParse(action.activityMessage.Trim(), out value))
                        {
                            _allowTakeScreenshot = value;
                        }
                    }
                    //Console.WriteLine($"----> AllowTakeScreenshot is {_allowTakeScreenshot}");
                }
                return _allowTakeScreenshot.Value;
            }
        }

        [NonSerialized]
        private string _applicationSleepInterval;
        public string ApplicationSleepInterval // Layer ActionLabel
        {
            get
            {
                if (_applicationSleepInterval == null)
                {
                    _applicationSleepInterval = string.Empty;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(ApplicationSleepInterval).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        _applicationSleepInterval = action.activityMessage.Trim();
                    }
                    //Console.WriteLine($"----> ApplicationSleepInterval is {_applicationSleepInterval}");
                }
                return _applicationSleepInterval;
            }
        }

        [NonSerialized]
        private AudioRolloffMode? _audioRolloffMode;
        public AudioRolloffMode? AudioRolloffMode // Layer ActionLabel
        {
            get
            {
                if (!_audioRolloffMode.HasValue)
                {
                    _audioRolloffMode = (AudioRolloffMode)(-1);
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(AudioRolloffMode).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        AudioRolloffMode value;
                        if (Enum.TryParse(action.activityMessage.Trim(), out value))
                        {
                            _audioRolloffMode = value;
                        }
                    }
                    //Console.WriteLine($"----> AudioAudioRolloffMode is {_audioAudioRolloffMode}");
                }
                return _audioRolloffMode.Value >= 0 ? _audioRolloffMode.Value : null;
            }
        }

        [NonSerialized]
        private float? _audioSpatialBlend;
        public float? AudioSpatialBlend // Layer ActionLabel
        {
            get
            {
                if (_audioSpatialBlend == null)
                {
                    _audioSpatialBlend = -1;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(AudioSpatialBlend).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value;
                        if (float.TryParse(action.activityMessage.Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            _audioSpatialBlend = value;
                        }
                    }
                    //Console.WriteLine($"----> AudioSpatialBlend is {_audioSpatialBlend}");
                }
                return _audioSpatialBlend.Value >= 0 ? _audioSpatialBlend.Value : null;
            }
        }

        [NonSerialized]
        private int? _audioSpatialize;
        public bool? AudioSpatialize // Layer ActionLabel
        {
            get
            {
                if (_audioSpatialize == null)
                {
                    _audioSpatialize = -1;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(AudioSpatialize).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        bool value;
                        if (bool.TryParse(action.activityMessage.Trim(), out value))
                        {
                            _audioSpatialize = value ? 1 : 0;
                        }
                    }
                    //Console.WriteLine($"----> AudioSpatialize is {_audioSpatialize}");
                }
                return _audioSpatialize.Value >= 0 ? _audioSpatialize.Value == 1 : null;
            }
        }

        [NonSerialized]
        private float? _audioVolume;
        public float AudioVolume // Layer ActionLabel
        {
            get
            {
                if (_audioVolume == null)
                {
                    _audioVolume = -1;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(AudioVolume).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value;
                        if (float.TryParse(action.activityMessage.Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            _audioVolume = value;
                        }
                    }
                    //Console.WriteLine($"----> AudioVolume is {_audioVolume}");
                }
                return _audioVolume.Value;
            }
        }

        [NonSerialized]
        private float? _directionalLightN_Intensity;
        public float DirectionalLightN_Intensity // Layer ActionLabel
        {
            get
            {
                if (_directionalLightN_Intensity == null)
                {
                    _directionalLightN_Intensity = 1;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(DirectionalLightN_Intensity).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value;
                        if (float.TryParse(action.activityMessage.Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            _directionalLightN_Intensity = value;
                        }
                    }
                    //Console.WriteLine($"----> DirectionalLightN_Intensity is {_directionalLightN_Intensity}");
                }
                return _directionalLightN_Intensity.Value;
            }
        }

        [NonSerialized]
        private bool? _directionalLightN_IsActive;
        public bool DirectionalLightN_IsActive // Layer ActionLabel
        {
            get
            {
                if (_directionalLightN_IsActive == null)
                {
                    _directionalLightN_IsActive = true;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(DirectionalLightN_IsActive).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        bool value;
                        if (bool.TryParse(action.activityMessage.Trim(), out value))
                        {
                            _directionalLightN_IsActive = value;
                        }
                    }
                    //Console.WriteLine($"----> DirectionalLightN_IsActive is {_directionalLightN_IsActive}");
                }
                return _directionalLightN_IsActive.Value;
            }
        }

        [NonSerialized]
        private float? _directionalLightSEE_Intensity;
        public float DirectionalLightSEE_Intensity // Layer ActionLabel
        {
            get
            {
                if (_directionalLightSEE_Intensity == null)
                {
                    _directionalLightSEE_Intensity = 1;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(DirectionalLightSEE_Intensity).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value;
                        if (float.TryParse(action.activityMessage.Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            _directionalLightSEE_Intensity = value;
                        }
                    }
                    //Console.WriteLine($"----> DirectionalLightSEE_Intensity is {_directionalLightSEE_Intensity}");
                }
                return _directionalLightSEE_Intensity.Value;
            }
        }

        [NonSerialized]
        private bool? _directionalLightSEE_IsActive;
        public bool DirectionalLightSEE_IsActive // Layer ActionLabel
        {
            get
            {
                if (_directionalLightSEE_IsActive == null)
                {
                    _directionalLightSEE_IsActive = true;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(DirectionalLightSEE_IsActive).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        bool value;
                        if (bool.TryParse(action.activityMessage.Trim(), out value))
                        {
                            _directionalLightSEE_IsActive = value;
                        }
                    }
                    //Console.WriteLine($"----> DirectionalLightSEE_IsActive is {_directionalLightSEE_IsActive}");
                }
                return _directionalLightSEE_IsActive.Value;
            }
        }

        [NonSerialized]
        private float? _directionalLightSWW_Intensity;
        public float DirectionalLightSWW_Intensity // Layer ActionLabel
        {
            get
            {
                if (_directionalLightSWW_Intensity == null)
                {
                    _directionalLightSWW_Intensity = 1;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(DirectionalLightSWW_Intensity).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value;
                        if (float.TryParse(action.activityMessage.Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            _directionalLightSWW_Intensity = value;
                        }
                    }
                    //Console.WriteLine($"----> DirectionalLightSWW_Intensity is {_directionalLightSWW_Intensity}");
                }
                return _directionalLightSWW_Intensity.Value;
            }
        }

        [NonSerialized]
        private bool? _directionalLightSWW_IsActive;
        public bool DirectionalLightSWW_IsActive // Layer ActionLabel
        {
            get
            {
                if (_directionalLightSWW_IsActive == null)
                {
                    _directionalLightSWW_IsActive = true;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(DirectionalLightSWW_IsActive).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        bool value;
                        if (bool.TryParse(action.activityMessage.Trim(), out value))
                        {
                            _directionalLightSWW_IsActive = value;
                        }
                    }
                    //Console.WriteLine($"----> DirectionalLightSWW_IsActive is {_directionalLightSWW_IsActive}");
                }
                return _directionalLightSWW_IsActive.Value;
            }
        }

        [NonSerialized]
        private int? _maximumActiveTriggerObjects;
        public int MaximumActiveTriggerObjects // Layer ActionLabel
        {
            get
            {
                if (_maximumActiveTriggerObjects == null)
                {
                    _maximumActiveTriggerObjects = -1;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(MaximumActiveTriggerObjects).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        int value;
                        if (int.TryParse(action.activityMessage.Trim(), out value))
                        {
                            _maximumActiveTriggerObjects = value;
                        }
                    }
                }
                return _maximumActiveTriggerObjects.Value;
            }
        }

        [NonSerialized]
        private int? _assetBundleCacheVersion;
        public int AssetBundleCacheVersion // Layer ActionLabel
        {
            get
            {
                if (_assetBundleCacheVersion == null)
                {
                    _assetBundleCacheVersion = -1;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(AssetBundleCacheVersion).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        int value;
                        if (int.TryParse(action.activityMessage.Trim(), out value))
                        {
                            _assetBundleCacheVersion = value;
                        }
                    }
                }
                return _assetBundleCacheVersion.Value;
            }
        }

        [NonSerialized]
        private EnvironmentDepthMode? _occlusionEnvironmentDepthMode = null;
        public EnvironmentDepthMode OcclusionEnvironmentDepthMode // Layer ActionLabel
        {
            get
            {
                if (_occlusionEnvironmentDepthMode == null)
                {
                    _occlusionEnvironmentDepthMode = EnvironmentDepthMode.Disabled;
                    var value = _occlusionEnvironmentDepthMode.Value;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(OcclusionEnvironmentDepthMode).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null && Enum.TryParse(action.activityMessage, out value))
                    {
                        _occlusionEnvironmentDepthMode = value;
                    }
                    //Console.WriteLine($"----> OcclusionEnvironmentDepthMode is {_occlusionEnvironmentDepthMode}");

                }
                return _occlusionEnvironmentDepthMode.Value;
            }
        }

        [NonSerialized]
        private HumanSegmentationDepthMode? _occlusionHumanSegmentationDepthMode = null;
        public HumanSegmentationDepthMode OcclusionHumanSegmentationDepthMode // Layer ActionLabel
        {
            get
            {
                if (_occlusionHumanSegmentationDepthMode == null)
                {
                    _occlusionHumanSegmentationDepthMode = HumanSegmentationDepthMode.Disabled;
                    var value = _occlusionHumanSegmentationDepthMode.Value;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(OcclusionHumanSegmentationDepthMode).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null && Enum.TryParse(action.activityMessage, out value))
                    {
                        _occlusionHumanSegmentationDepthMode = value;
                    }
                    //Console.WriteLine($"----> OcclusionHumanSegmentationDepthMode is {_occlusionHumanSegmentationDepthMode}");
                }
                return _occlusionHumanSegmentationDepthMode.Value;
            }
        }

        [NonSerialized]
        private HumanSegmentationStencilMode? _occlusionHumanSegmentationStencilMode = null;
        public HumanSegmentationStencilMode OcclusionHumanSegmentationStencilMode // Layer ActionLabel
        {
            get
            {
                if (_occlusionHumanSegmentationStencilMode == null)
                {
                    _occlusionHumanSegmentationStencilMode = HumanSegmentationStencilMode.Disabled;
                    var value = _occlusionHumanSegmentationStencilMode.Value;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(OcclusionHumanSegmentationStencilMode).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null && Enum.TryParse(action.activityMessage, out value))
                    {
                        _occlusionHumanSegmentationStencilMode = value;
                    }
                    //Console.WriteLine($"----> OcclusionHumanSegmentationStencilMode is {_occlusionHumanSegmentationStencilMode}");
                }
                return _occlusionHumanSegmentationStencilMode.Value;
            }
        }

        [NonSerialized]
        private OcclusionPreferenceMode? _occlusionPreferenceMode = null;
        public OcclusionPreferenceMode OcclusionPreferenceMode // Layer ActionLabel
        {
            get
            {
                if (_occlusionPreferenceMode == null)
                {
                    _occlusionPreferenceMode = OcclusionPreferenceMode.NoOcclusion;
                    var value = _occlusionPreferenceMode.Value;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(OcclusionPreferenceMode).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null && Enum.TryParse(action.activityMessage, out value))
                    {
                        _occlusionPreferenceMode = value;
                    }
                    //Console.WriteLine($"----> OcclusionPreferenceMode is {_occlusionPreferenceMode}");
                }
                return _occlusionPreferenceMode.Value;
            }
        }

        [NonSerialized]
        private float? _positionUpdateInterval;
        public float PositionUpdateInterval // Layer ActionLabel
        {
            get
            {
                if (_positionUpdateInterval == null)
                {
                    _positionUpdateInterval = 0;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(PositionUpdateInterval).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value;
                        if (float.TryParse(action.activityMessage.Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            _positionUpdateInterval = value;
                        }
                    }
                    //Console.WriteLine($"----> PositionUpdateInterval is {_positionUpdateInterval}");
                }
                return _positionUpdateInterval.Value;
            }
        }

        [NonSerialized]
        private string _remoteServerUrl = null;
        public string RemoteServerUrl // Layer ActionLabel
        {
            get
            {
                if (_remoteServerUrl == null)
                {
                    _remoteServerUrl = string.Empty;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(RemoteServerUrl).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        _remoteServerUrl = action.activityMessage.Trim();
                    }
                    //Console.WriteLine($"----> RemoteServerUrl is {_remoteServerUrl}");
                }
                return _remoteServerUrl;
            }
        }

        [NonSerialized]
        private string _sceneUrl = null;
        public string SceneUrl // Layer ActionLabel
        {
            get
            {
                if (_sceneUrl == null)
                {
                    _sceneUrl = layerTitle;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(SceneUrl).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        _sceneUrl = action.activityMessage.Trim();
                    }
                    //Console.WriteLine($"----> SceneUrl is {_sceneUrl}");
                }
                return _sceneUrl;
            }
        }

        [NonSerialized]
        private float? _timeSync;
        public float TimeSync // Layer ActionLabel
        {
            get
            {
                if (_timeSync == null)
                {
                    _timeSync = 0;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(TimeSync).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        float value;
                        if (float.TryParse(action.activityMessage.Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            _timeSync = value;
                        }
                    }
                    //Console.WriteLine($"----> TimeSync is {_timeSync}");
                }
                return _timeSync.Value;
            }
        }

        #endregion

        [NonSerialized]
        private int? _applicationSleepStartMinute;
        public int ApplicationSleepStartMinute
        {
            get
            {
                if (_applicationSleepStartMinute == null)
                {
                    _applicationSleepStartMinute = -1;

                    var applicationSleepInterval = ApplicationSleepInterval;
                    if (!string.IsNullOrWhiteSpace(applicationSleepInterval))
                    {
                        var parts = applicationSleepInterval.Split('-');
                        if (parts.Length > 1)
                        {
                            int value;
                            if (ArAnimation.TryParseMinutes(parts[0], out value))
                            {
                                _applicationSleepStartMinute = value;
                            }
                        }
                    }
                }
                return _applicationSleepStartMinute.Value;
            }
        }

        [NonSerialized]
        private int? _applicationSleepEndMinute;
        public int ApplicationSleepEndMinute
        {
            get
            {
                if (_applicationSleepEndMinute == null)
                {
                    _applicationSleepEndMinute = -1;

                    var applicationSleepInterval = ApplicationSleepInterval;
                    if (!string.IsNullOrWhiteSpace(applicationSleepInterval))
                    {
                        var parts = applicationSleepInterval.Split('-');
                        if (parts.Length > 1)
                        {
                            int value;
                            if (ArAnimation.TryParseMinutes(parts[1], out value))
                            {
                                _applicationSleepEndMinute = value;
                            }
                        }
                    }
                }
                return _applicationSleepEndMinute.Value;
            }
        }
    }
}
