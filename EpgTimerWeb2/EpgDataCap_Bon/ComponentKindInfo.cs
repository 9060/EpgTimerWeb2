﻿/*
 *  EpgTimerWeb2
 *  Copyright (C) 2015  YukiBoard 0X7hT.k8kU <yuki@yukiboard.tk>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;

namespace EpgTimer
{
    public class ComponentKindInfo
    {
        public ComponentKindInfo()
        {
        }
        public ComponentKindInfo(byte streamContent, byte componentType, string componentName)
        {
            StreamContent = streamContent;
            ComponentType = componentType;
            ComponentName = componentName;
        }
        public byte StreamContent
        {
            get;
            set;
        }
        public byte ComponentType
        {
            get;
            set;
        }
        public string ComponentName
        {
            get;
            set;
        }
        public override string ToString()
        {
            return ComponentName;
        }
    }
}
