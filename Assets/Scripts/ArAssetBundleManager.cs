/*
ArAssetBundleManager.cs - For handling of asset bundles.

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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace com.arpoise.arpoiseapp
{
    public class ArWebRequestAssetBundle
    {
        public string AssetBundleUrl;
        public string AssetBundleUri;
        public UnityWebRequest UnityWebRequest;
        public string ErrorMessage;
    }

    public class ArAssetBundleManager
    {
        private Dictionary<string, AssetBundle> _assetBundles = new Dictionary<string, AssetBundle>();

        private List<ArWebRequestAssetBundle> _webRequests = new List<ArWebRequestAssetBundle>();

        public ArObjectState ArObjectState { get; set; }

        public void SetAssetBundle(string key, AssetBundle assetBundle)
        {
            if (_assetBundles.ContainsKey(key))
            {
                _assetBundles[key].Unload(false);
                _assetBundles[key] = assetBundle;
            }
            else
            {
                _assetBundles.Add(key, assetBundle);
            }
        }

        public GameObject CreateGameObject(GameObject objectToAdd)
        {
            var result = UnityEngine.Object.Instantiate(objectToAdd);
            if (_createdGameObjects.TryGetValue(objectToAdd, out string assetBundleUrl))
            {
                _instantiatedGameObjects[result] = objectToAdd;
                if (!string.IsNullOrWhiteSpace(assetBundleUrl))
                {
                    if (!_instantiatedGameObjectsByUrl.TryGetValue(assetBundleUrl, out List<GameObject> instantiatedObjects))
                    {
                        _instantiatedGameObjectsByUrl[assetBundleUrl] = new List<GameObject> { result };
                    }
                    else
                    {
                        instantiatedObjects.Add(result);
                    }
                }
            }
            return result;
        }

        public bool ExistsAssetBundle(string key)
        {
            return _assetBundles.ContainsKey(key);
        }

        private Dictionary<string, List<GameObject>> _instantiatedGameObjectsByUrl = new Dictionary<string, List<GameObject>>();

        private Dictionary<GameObject, string> _createdGameObjects = new Dictionary<GameObject, string>();

        private Dictionary<GameObject, GameObject> _instantiatedGameObjects = new Dictionary<GameObject, GameObject>();

        public string LoadGameObject(string poiId, string assetBundleUrl, string objectName, out GameObject newObject)
        {
            newObject = null;

            AssetBundle assetBundle = null;
            if (!_assetBundles.TryGetValue(assetBundleUrl, out assetBundle))
            {
                if (_allowDeferredLoadAssetBundles.TryGetValue(assetBundleUrl, out bool allowDeferred) && allowDeferred)
                {
                    newObject = new GameObject(objectName);
                    _createdGameObjects[newObject] = assetBundleUrl;
                    return null;
                }
                return $"Missing asset bundle '{assetBundleUrl}'.";
            }

            newObject = assetBundle.LoadAsset<GameObject>(objectName);
            if (newObject == null)
            {
                return $"Poi with id {poiId}, unknown game object: '{objectName}' in bundle '{assetBundleUrl}'.";
            }

            return null;
        }

        public GameObject TryLoadGameObject(string assetBundleUrl, string objectName)
        {
            AssetBundle assetBundle = null;
            if (_assetBundles.TryGetValue(assetBundleUrl, out assetBundle))
            {
                return assetBundle.LoadAsset<GameObject>(objectName);
            }
            return null;
        }

        public List<ArWebRequestAssetBundle> CreateWebRequests(Dictionary<string, int> assetBundleUrls)
        {
            foreach (var url in assetBundleUrls)
            {
                if (ExistsAssetBundle(url.Key))
                {
                    continue;
                }
                var request = new ArWebRequestAssetBundle();
                request.AssetBundleUrl = url.Key;
                request.AssetBundleUri = ArBehaviourData.FixUrl(ArBehaviourData.GetAssetBundleUrl(url.Key));
                if (url.Value <= 0)
                {
                    request.UnityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(request.AssetBundleUri, 0);
                }
                else
                {
                    request.UnityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(request.AssetBundleUri, (uint)url.Value, 0);
                }
                request.UnityWebRequest.certificateHandler = new ArpoiseCertificateHandler();
                request.UnityWebRequest.timeout = 90;
                _webRequests.Add(request);
            }
            return _webRequests;
        }

        public bool CheckWebRequests(DateTime startTime)
        {
            bool allDone = true;
            foreach (var request in _webRequests)
            {
                if (request.UnityWebRequest is null || ExistsAssetBundle(request.AssetBundleUrl))
                {
                    continue;
                }
                if (_allowDeferredLoadAssetBundles.TryGetValue(request.AssetBundleUrl, out bool allowDeferred) && allowDeferred)
                {
                    continue;
                }
                if (!request.UnityWebRequest.isDone)
                {
                    allDone = false;
                    continue;
                }
                if (request.UnityWebRequest.result != UnityWebRequest.Result.Success)
                {
                    request.ErrorMessage = $"Bundle '{request.AssetBundleUri}' {request.UnityWebRequest.result}, {request.UnityWebRequest.error}";
                    request.UnityWebRequest.Dispose();
                    request.UnityWebRequest = null;
                }
                else
                {
                    var assetBundle = DownloadHandlerAssetBundle.GetContent(request.UnityWebRequest);
                    if (assetBundle == null)
                    {
                        request.ErrorMessage = $"Bundle '{request.AssetBundleUrl}' is null.";
                    }
                    request.UnityWebRequest.Dispose();
                    request.UnityWebRequest = null;
                    SetAssetBundle(request.AssetBundleUrl, assetBundle);
                }
            }
            return allDone;
        }

        public List<ArWebRequestAssetBundle> GetWebRequests()
        {
            return _webRequests;
        }

        private Dictionary<string, bool> _allowDeferredLoadAssetBundles = new Dictionary<string, bool>();

        public void SetAllowDeferredLoadAssetBundle(string assetBundleUrl, bool allow)
        {
            _allowDeferredLoadAssetBundles.Remove(assetBundleUrl);
            _allowDeferredLoadAssetBundles.Add(assetBundleUrl, allow);
        }

        public void CheckWebRequests()
        {
            foreach (var request in _webRequests)
            {
                if (request.UnityWebRequest is null || ExistsAssetBundle(request.AssetBundleUrl))
                {
                    continue;
                }
                if (!request.UnityWebRequest.isDone)
                {
                    continue;
                }
                if (request.UnityWebRequest.result != UnityWebRequest.Result.Success)
                {
                    request.UnityWebRequest.Dispose();
                    request.UnityWebRequest = null;
                    continue;
                }
                var assetBundle = DownloadHandlerAssetBundle.GetContent(request.UnityWebRequest);
                if (assetBundle == null)
                {
                    request.UnityWebRequest.Dispose();
                    request.UnityWebRequest = null;
                    continue;
                }
                request.UnityWebRequest.Dispose();
                request.UnityWebRequest = null;
                SetAssetBundle(request.AssetBundleUrl, assetBundle);

                if (_instantiatedGameObjectsByUrl.TryGetValue(request.AssetBundleUrl, out List<GameObject> instantiatedObjects))
                {
                    foreach (var instantiatedObject in instantiatedObjects)
                    {
                        if (_instantiatedGameObjects.TryGetValue(instantiatedObject, out GameObject originalObject))
                        {
                            LoadGameObject(originalObject.name, request.AssetBundleUrl, originalObject.name, out GameObject loadedObject);
                            if (loadedObject != null)
                            {
                                var newObject = CreateGameObject(loadedObject);
                                if (newObject != null)
                                {
                                    ArObjectState.Replace(instantiatedObject, newObject);
                                    newObject.SetActive(instantiatedObject.activeSelf);
                                    instantiatedObject.transform.SetParent(null);
                                }
                            }
                        }
                    }
                    _instantiatedGameObjectsByUrl.Remove(request.AssetBundleUrl);
                }
            }
        }
    }
}
