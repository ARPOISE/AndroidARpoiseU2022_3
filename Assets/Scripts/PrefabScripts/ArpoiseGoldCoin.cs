/*
ArpoiseGoldCoin.cs - A script handling a 'gold coin' like in 'Revolution and Return' for ARpoise.

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
using UnityEngine;

public class ArpoiseGoldCoin : MonoBehaviour
{
    AudioSource coinFall = null;
    private void OnCollisionEnter(Collision collision)
    {
        if (coinFall != null)
        {
            coinFall.Play();
            coinFall = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        coinFall = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
