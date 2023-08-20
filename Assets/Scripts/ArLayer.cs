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
#if HAS_AR_FOUNDATION
using UnityEngine.XR.ARSubsystems;
#endif

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

                double value;
                var xOffset = (float)(parts.Length > 0 && double.TryParse(parts[0].Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : 0);
                var yOffset = (float)(parts.Length > 1 && double.TryParse(parts[1].Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : 0);
                var zOffset = (float)(parts.Length > 2 && double.TryParse(parts[2].Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : 0);
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

        public string BaseUrl
        {
            get
            {
                string baseUrl = poiObject?.baseURL;
                if (baseUrl != null)
                {
                    baseUrl = baseUrl.Trim();
                }
                return baseUrl;
            }
        }

        public string TriggerImageURL
        {
            get
            {
                string triggerImageURL = poiObject?.triggerImageURL;
                if (triggerImageURL != null)
                {
                    triggerImageURL = triggerImageURL.Trim();
                }
                return triggerImageURL;
            }
        }

        public string GameObjectName
        {
            get
            {
                string name = poiObject?.full;
                if (name != null)
                {
                    name = name.Trim();
                }
                return name;
            }
        }

        public string InnerLayerName
        {
            get
            {
                string name = poiObject?.poiLayerName;
                if (name != null)
                {
                    name = name.Trim();
                }
                return name;
            }
        }

        public Poi Clone()
        {
            var s = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<Poi>(s);
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
        private int _triggerObjectKeepAlive = -1;
        public int TriggerObjectKeepAlive
        {
            get
            {
                if (_triggerObjectKeepAlive < 0)
                {
                    _triggerObjectKeepAlive = 0;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(TriggerObjectKeepAlive).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        var message = action.activityMessage;
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            int.TryParse(message, out _triggerObjectKeepAlive);
                            if (_triggerObjectKeepAlive < 0)
                            {
                                _triggerObjectKeepAlive = 0;
                            }
                        }
                    }
                }
                return _triggerObjectKeepAlive;
            }
        }

        [NonSerialized]
        private bool? _keepLastKnownPose = null;
        public bool KeepLastKnownPose
        {
            get
            {
                if (!_keepLastKnownPose.HasValue)
                {
                    _keepLastKnownPose = false;
                    var action = actions?.FirstOrDefault(x => x.showActivity && nameof(KeepLastKnownPose).Equals(x.label?.Trim()) && !string.IsNullOrWhiteSpace(x.activityMessage));
                    if (action != null)
                    {
                        _keepLastKnownPose = "true".Equals(action.activityMessage.Trim().ToLower());
                    }
                }
                return _keepLastKnownPose.Value;
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
                        if (double.TryParse(action.activityMessage, out value))
                        {
                            _trackingTimeout = value;
                        }
                    }
                }
                return _trackingTimeout.Value;
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
                        if (float.TryParse(action.activityMessage, out value))
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
                        if (float.TryParse(action.activityMessage, out value))
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
        private readonly HashSet<string> _actionLabels = new HashSet<string>(new string[] // Layer ActionLabel
        {
#if HAS_AR_FOUNDATION
            nameof(OcclusionEnvironmentDepthMode),
            nameof(OcclusionPreferenceMode),
            nameof(OcclusionHumanSegmentationStencilMode),
            nameof(OcclusionHumanSegmentationDepthMode),
#endif
            nameof(PositionUpdateInterval),
            nameof(TimeSync),
            nameof(ApplicationSleepInterval),
            nameof(AllowTakeScreenshot),
            nameof(RemoteServerUrl),
            nameof(SceneUrl)
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
                        if (float.TryParse(action.activityMessage.Trim(), out value))
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
                        if (float.TryParse(action.activityMessage.Trim(), out value))
                        {
                            _timeSync = value;
                        }
                    }
                    //Console.WriteLine($"----> TimeSync is {_timeSync}");
                }
                return _timeSync.Value;
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

#if HAS_AR_FOUNDATION
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
#endif
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
                            if (TryParseMinutes(parts[0], out value))
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
                            if (TryParseMinutes(parts[1], out value))
                            {
                                _applicationSleepEndMinute = value;
                            }
                        }
                    }
                }
                return _applicationSleepEndMinute.Value;
            }
        }

        private bool TryParseMinutes(string s, out int result)
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
    }
}
