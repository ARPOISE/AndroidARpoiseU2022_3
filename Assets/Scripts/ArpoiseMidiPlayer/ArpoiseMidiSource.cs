/*
ArpoiseMidiSource.cs - MonoBehaviour for ARpoise MIDI source handling.
                       Attached to a prefab that is distributed through an ARpoise AssetBundle.
                       The component describes the MIDI content and playback settings. 
                       Actual playback is handled by ARpoise.

Copyright (C) 2026, Tamiko Thiel and Peter Graf - All Rights Reserved

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

public class ArpoiseMidiSource : MonoBehaviour
{
    [Header("MIDI Content")]
    [Tooltip("MIDI file included in the AssetBundle as a TextAsset.")]
    public TextAsset midiFile;

    [Tooltip("HTTPS URL of the SoundFont (.sf2) to use.")]
    public string soundFontUrl = "";

    [Header("Playback")]
    [Tooltip("Start playback when the prefab is activated.")]
    public bool playOnEnable = false;

    [Tooltip("Restart the MIDI composition when it finishes.")]
    public bool loop = false;

    [Range(0f, 1f)]
    [Tooltip("Playback volume.")]
    public float volume = 1f;
}