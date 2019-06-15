﻿// Copyright (c) ATHellboy (Alireza Tarahomi) Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

using System;
using UnityEngine;

namespace ScriptableObjectMultiSelectDropdown
{
    /// <summary>
    /// Because you can't make a PropertyDrawer for arrays or generic lists themselves,
    /// I had to create parent class as an abstract layer.
    /// </summary>
    [Serializable]
    public class ScriptableObjectReference
    {
        public ScriptableObject[] values;
    }
}